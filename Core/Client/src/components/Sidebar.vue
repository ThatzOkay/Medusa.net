<script setup lang="ts">
import { useUserStore } from '@/store/userStore';
import NavList, { type NavListItem } from '@/components/ui/NavList.vue';
import { Icon } from '@iconify/vue';
import { computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { usePluginStore } from '@/store/pluginStore';

const props = defineProps<{
  open?: boolean;
}>();

const emit = defineEmits<{
  close: [];
}>();

const router = useRouter();
const route = useRoute();
const pluginStore = usePluginStore();

const navigateTo = (path: string) => {
  router.push(path);
  emit('close');
};

// Each plugin renders as its own collapsible group: the group header just
// toggles the dropdown (it has no page of its own to navigate to), and its
// nav items render as the clickable sub-nav underneath it.
const pluginItems = computed<NavListItem[]>(() => pluginStore.pluginNavGroups.map((g) => ({
    label: g.displayName,
    icon: g.icon,
    children: g.navItems.map((n) => ({
      label: n.label,
      icon: n.icon,
      active: route.path === n.path || route.path.startsWith(`${n.path}/`),
      command: () => navigateTo(n.path),
    })),
  })));

const items = computed<NavListItem[]>(() => [
    { label: 'Home', icon: 'material-symbols:home-rounded', active: route.path === '/', command: () => navigateTo('/') },
    { label: 'Card management', icon: 'material-symbols:badge-rounded', active: route.path === '/cards', command: () => navigateTo('/cards') },
    { label: 'Cardless login', icon: 'material-symbols:qr-code-scanner-rounded', active: route.path === '/cardless/scan', command: () => navigateTo('/cardless/scan') },
    ...pluginItems.value,
    { label: 'Settings', icon: 'material-symbols:settings-rounded', active: route.path === '/settings', command: () => navigateTo('/settings') },
    { label: 'logout', icon: 'material-symbols:logout-rounded', command: () => {
        const userStore = useUserStore();
        userStore.unsetAccessToken();
        userStore.unsetUser();
        userStore.unsetRefreshToken();
        userStore.unsetLoggedIn();
        sessionStorage.removeItem("accessToken");
        localStorage.removeItem("refreshToken");
        navigateTo('/auth');
    } },
]);
</script>

<template>
    <Transition
        enter-active-class="transition-opacity duration-300"
        leave-active-class="transition-opacity duration-200"
        enter-from-class="opacity-0"
        leave-to-class="opacity-0"
    >
        <div v-if="props.open" class="fixed inset-0 bg-black/50 z-40 md:hidden" @click="emit('close')" />
    </Transition>

    <div
        class="fixed inset-y-0 left-0 z-50 flex flex-col w-72 shrink-0 h-dvh rounded-r-md-xl bg-md-surface-container overflow-y-auto transition-transform duration-300 ease-[cubic-bezier(0.2,0,0,1)] md:static md:z-auto md:m-4 md:h-[calc(100dvh-2rem)] md:rounded-md-xl md:translate-x-0"
        :class="props.open ? 'translate-x-0' : '-translate-x-full'"
    >
        <div class="flex flex-col gap-1 p-4 flex-1">
            <div class="flex items-center justify-between px-3 pt-2 pb-4">
                <span class="text-md-headline-small text-md-on-surface">Medusa.net</span>
                <button
                    type="button"
                    class="md:hidden p-1 rounded-full text-md-on-surface-variant hover:bg-md-on-surface/8"
                    @click="emit('close')"
                >
                    <Icon icon="material-symbols:close-rounded" class="w-6 h-6" />
                </button>
            </div>
            <NavList :items="items" />
        </div>
    </div>
</template>
