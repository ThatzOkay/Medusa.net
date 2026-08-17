import { defineComponent, h, markRaw, type Component } from 'vue'
import { useRouter } from 'vue-router'
import DefaultLayout from '@/layouts/default.vue'
import { usePluginStore, type PluginManifest } from '@/store/pluginStore'

// ── CSS scoping wrapper ───────────────────────────────────────────────────────
// Each plugin page is rendered inside <div data-plugin-{id}> so plugin authors
// can scope global styles with [data-plugin-foo] .card { ... }. <style scoped>
// is preferred but this provides an extra safety net for global styles.
const wrapWithContainer = (pluginId: string, inner: Component): Component =>
  defineComponent({
    name: `PluginContainer-${pluginId}`,
    setup: () => () => h('div', { [`data-plugin-${pluginId}`]: '' }, [h(inner)]),
  })

// ── Script tag injection with SRI ─────────────────────────────────────────────
const injectScript = (pluginId: string, bundleUrl: string, sri: string | null): Promise<void> => {
  // Remove any previously injected script for this plugin (hot-reload path).
  document.querySelector(`script[data-medusa-plugin="${pluginId}"]`)?.remove()

  return new Promise((resolve, reject) => {
    const script = document.createElement('script')
    script.dataset.medusaPlugin = pluginId
    script.src = bundleUrl

    if (sri) {
      script.integrity   = sri
      script.crossOrigin = 'anonymous' // required for SRI to apply
    } else {
      console.warn(`[medusa] Plugin "${pluginId}" has no SRI hash — running unverified bundle.`)
    }

    script.onload  = () => resolve()
    script.onerror = () =>
      reject(new Error(`Plugin "${pluginId}" bundle failed to load or failed SRI check.`))

    document.head.appendChild(script)
  })
}

export const usePluginLoader = () => {
  const router      = useRouter()
  const pluginStore = usePluginStore()

  // ── Load a single plugin ────────────────────────────────────────────────────
  const loadPlugin = async (manifest: PluginManifest): Promise<void> => {
    try {
      await injectScript(manifest.pluginId, manifest.bundleUrl, manifest.sri)
    } catch (err) {
      console.error(`[medusa] Failed to load plugin "${manifest.pluginId}":`, err)
      return
    }

    // Validate exports before touching the router — a bad bundle must not crash the host app.
    const exports = window.__medusa_plugins__[manifest.pluginId]
    if (!exports || Object.keys(exports).length === 0) {
      console.error(`[medusa] Plugin "${manifest.pluginId}" bundle registered no components.`)
      return
    }

    // Build child routes from the manifest's route list.
    // Auth guard note: the existing beforeEach treats any route without
    // meta.requiresAuth === false as auth-required, so omitting meta is fine here.
    const children = manifest.routes.flatMap((r) => {
      const component = exports[r.componentKey]
      if (typeof component !== 'function' && typeof component !== 'object') {
        console.warn(`[medusa] Plugin "${manifest.pluginId}" missing component key "${r.componentKey}" — skipping.`)
        return []
      }
      const parentPrefix = `/plugins/${manifest.pluginId}`
      const relativePath = r.path === parentPrefix ? '' : r.path.slice(parentPrefix.length + 1)
      return [{
        name:      `plugin-${manifest.pluginId}-${r.componentKey}`,
        path:      relativePath,
        component: markRaw(wrapWithContainer(manifest.pluginId, component as Component)),
      }]
    })

    if (children.length === 0) {
      console.warn(`[medusa] Plugin "${manifest.pluginId}" has no valid routes after validation.`)
      return
    }

    // Replicate vite-plugin-vue-layouts-next wrapping: layout is the parent route,
    // page components are children. router.removeRoute on the parent removes all children.
    router.addRoute({
      path:      `/plugins/${manifest.pluginId}`,
      name:      `plugin-${manifest.pluginId}`,
      component: markRaw(DefaultLayout),
      children,
    })

    pluginStore.setManifest(manifest)
  }

  // ── Unload a single plugin ──────────────────────────────────────────────────
  const unloadPlugin = (pluginId: string): void => {
    try { router.removeRoute(`plugin-${pluginId}`) } catch { /* already gone */ }
    document.querySelector(`script[data-medusa-plugin="${pluginId}"]`)?.remove()
    delete window.__medusa_plugins__[pluginId]
    pluginStore.removeManifest(pluginId)
  }

  // ── Load all plugins on startup ─────────────────────────────────────────────
  const loadAllPlugins = async (): Promise<void> => {
    let manifests: PluginManifest[]
    try {
      const res = await fetch('/api/plugins/ui', {
        headers: { Authorization: `Bearer ${sessionStorage.getItem('accessToken')}` },
      })
      if (!res.ok) return
      manifests = await res.json()
    } catch {
      return
    }
    for (const manifest of manifests) {
      await loadPlugin(manifest)
    }
  }

  // ── SSE for hot-reload ──────────────────────────────────────────────────────
  const startEventStream = (): EventSource => {
    const es = new EventSource('/api/plugins/events')

    es.onmessage = async (event) => {
      let data: { type: string; pluginId: string }
      try { data = JSON.parse(event.data) } catch { return }

      if (data.type === 'reloaded') {
        unloadPlugin(data.pluginId)
        try {
          const res = await fetch('/api/plugins/ui', {
            headers: { Authorization: `Bearer ${sessionStorage.getItem('accessToken')}` },
          })
          const manifests: PluginManifest[] = await res.json()
          const updated = manifests.find((m) => m.pluginId === data.pluginId)
          if (updated) await loadPlugin(updated)
        } catch { /* fetch failed — user will see missing nav item, refresh to recover */ }
      } else if (data.type === 'unloaded') {
        unloadPlugin(data.pluginId)
      }
    }

    return es
  }

  return { loadAllPlugins, loadPlugin, unloadPlugin, startEventStream }
}