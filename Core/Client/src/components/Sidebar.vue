<script setup lang="ts">
import { useUserStore } from '@/store/userStore';
import NavList, { type NavListItem } from '@/components/ui/NavList.vue';
import { computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';

const router = useRouter();
const route = useRoute();

const items = computed<NavListItem[]>(() => [
    { label: 'Home', icon: 'material-symbols:home-rounded', active: route.path === '/', command: () => router.push('/') },
    { label: 'Card management', icon: 'material-symbols:badge-rounded', active: route.path === '/cards', command: () => router.push('/cards') },
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
