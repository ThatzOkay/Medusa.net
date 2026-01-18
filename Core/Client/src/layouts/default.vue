<script setup lang="ts">
import Sidebar from '@/components/Sidebar.vue';
</script>

<template>
    <div class="flex gap-4 h-full w-full">
        <Sidebar />
        <main class="w-full h-dvh flex items-center">
            <router-view v-slot="{ Component }">
                <transition name="fade" mode="out-in">
                    <component :is="Component" />
                </transition>
            </router-view>
        </main>
    </div>
</template>