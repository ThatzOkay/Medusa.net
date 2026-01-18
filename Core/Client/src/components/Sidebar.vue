<script setup lang="ts">
import { useUserStore } from '@/store/userStore';
import Menu from '@/volt/Menu.vue';
import { ref } from 'vue';
import { useRouter } from 'vue-router';

const router = useRouter();

const items = ref([
    { label: 'Home', icon: 'pi pi-home' , command: () => router.push('/') },
    { label: 'Card management', icon: 'pi pi-id-card' , command: () => router.push('/cards') },
    { label: 'logout', icon: 'pi pi-sign-out', command: () => {
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
    <div class="flex flex-col justify-center w-2xs h-dvh rounded-none! bg-surface-0 dark:bg-surface-900 text-surface-700 dark:text-surface-0 shadow-md"
        data-pc-name="card" pc4="" data-pc-section="root">
        <div class="p-5 flex flex-col gap-2 h-full" data-pc-section="body">
            <div class="flex flex-col gap-2" data-pc-section="caption">
                <div class="font-medium text-xl" data-pc-section="title">Medusa.net</div>
            </div>
            <div class="" data-pc-section="content">
                <Menu :model="items" />
            </div>
        </div>
    </div>
</template>