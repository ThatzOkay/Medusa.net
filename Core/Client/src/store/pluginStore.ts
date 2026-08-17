import { defineStore } from 'pinia'

export interface PluginNavItemDef {
  label: string
  icon: string
  path: string
}

export interface PluginRouteDef {
  path: string
  componentKey: string
}

export interface PluginManifest {
  pluginId: string
  displayName: string
  icon: string
  bundleUrl: string
  sri: string | null
  navItems: PluginNavItemDef[]
  routes: PluginRouteDef[]
}

export const usePluginStore = defineStore('plugins', {
  state: () => ({ manifests: {} as Record<string, PluginManifest> }),
  getters: {
    // Grouped per plugin rather than flattened, so the sidebar can render each
    // plugin as its own collapsible group instead of mixing pages together.
    pluginNavGroups: (state) =>
      Object.values(state.manifests).map((m) => ({
        pluginId:    m.pluginId,
        displayName: m.displayName,
        icon:        m.icon,
        navItems:    m.navItems,
      })),
  },
  actions: {
    setManifest(m: PluginManifest) { this.manifests[m.pluginId] = m },
    removeManifest(id: string)     { delete this.manifests[id] },
  },
})