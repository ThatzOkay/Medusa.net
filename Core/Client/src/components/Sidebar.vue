<script setup lang="ts">
import { useUserStore } from '@/store/userStore';
import NavList, { type NavListItem } from '@/components/ui/NavList.vue';
import { computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { usePluginStore } from '@/store/pluginStore';

const router = useRouter();
const route = useRoute();
const pluginStore = usePluginStore();

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
      command: () => router.push(n.path),
    })),
  })));

const items = computed<NavListItem[]>(() => [
    { label: 'Home', icon: 'material-symbols:home-rounded', active: route.path === '/', command: () => router.push('/') },
    { label: 'Card management', icon: 'material-symbols:badge-rounded', active: route.path === '/cards', command: () => router.push('/cards') },
    ...pluginItems.value,
    { label: 'logout', icon: 'material-symbols:logout-rounded', command: () => {
        const userStore = useUserStore();
        userStore.unsetAccessToken();
        userStore.unsetUser();
        userStore.unsetRefreshToken();
        userStore.unsetLoggedIn();
        sessionStorage.removeItem("accessToken");
        localStorage.removeItem("refreshToken");
        router.push('/auth');
    } },
]);
</script>

<template>
    <div class="flex flex-col w-72 shrink-0 m-4 h-[calc(100dvh-2rem)] rounded-md-xl bg-md-surface-container overflow-y-auto">
        <div class="flex flex-col gap-1 p-4 flex-1">
            <div class="text-md-headline-small text-md-on-surface px-3 pt-2 pb-4">Medusa.net</div>
            <NavList :items="items" />
        </div>
    </div>
</template>
