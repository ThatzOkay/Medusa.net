<script setup lang="ts">
import { onMounted, onUnmounted } from 'vue'
import { useRouter } from 'vue-router'
import { usePluginLoader } from '@/composables/usePluginLoader'

const router = useRouter()
const { loadAllPlugins, startEventStream } = usePluginLoader()
let es: EventSource | null = null

onMounted(async () => {
  await loadAllPlugins()
  es = startEventStream()

  // main.ts mounts the app after router.isReady(), so the initial navigation
  // (e.g. a hard refresh on a plugin page) resolves before loadAllPlugins()
  // has registered that plugin's routes. vue-router won't retroactively
  // rematch an already-resolved location against routes added afterwards, so
  // force a re-resolve now that every plugin route exists.
  await router.replace(router.currentRoute.value.fullPath)
})

onUnmounted(() => es?.close())
</script>

<template>
  <div class="bg-md-background text-md-on-background min-h-screen flex flex-col gap-6">
    <RouterView />
  </div>
</template>