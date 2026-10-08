<script setup lang="ts">
import Sidebar from '@/components/Sidebar.vue';
import { Icon } from '@iconify/vue';
import { ref } from 'vue';

const sidebarOpen = ref(false);
</script>

<template>
    <div class="flex flex-col md:flex-row md:gap-4 h-full w-full">
        <header class="flex items-center gap-3 p-4 md:hidden">
            <button
                type="button"
                class="p-2 rounded-full text-md-on-surface-variant hover:bg-md-on-surface/8"
                @click="sidebarOpen = true"
            >
                <Icon icon="material-symbols:menu-rounded" class="w-6 h-6" />
            </button>
            <img src="/medusa-icon.svg" alt="" class="w-8 h-8 shrink-0" />
            <span class="text-md-title-large text-md-on-surface">Medusa.net</span>
        </header>

        <Sidebar :open="sidebarOpen" @close="sidebarOpen = false" />

        <main class="w-full flex-1 min-h-0 md:h-dvh p-4 overflow-x-hidden overflow-y-auto">
            <router-view v-slot="{ Component }">
                <transition name="fade" mode="out-in">
                    <component :is="Component" />
                </transition>
            </router-view>
        </main>
    </div>
</template>