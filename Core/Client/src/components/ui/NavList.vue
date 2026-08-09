<script setup lang="ts">
import { Icon } from '@iconify/vue';

export interface NavListItem {
  label: string;
  /** Iconify icon identifier, e.g. "material-symbols:home-rounded". */
  icon?: string;
  active?: boolean;
  command?: () => void;
}

defineProps<{
  items: NavListItem[];
}>();
</script>

<template>
  <nav class="flex flex-col gap-1">
    <a
      v-for="item in items"
      :key="item.label"
      class="flex items-center gap-3 px-4 py-3 min-h-12 rounded-full cursor-pointer text-md-label-large transition-colors"
      :class="item.active ? 'bg-md-primary-container text-md-on-primary-container' : 'text-md-on-surface-variant hover:bg-md-on-surface/8'"
      @click="item.command?.()"
    >
      <Icon v-if="item.icon" :icon="item.icon" class="w-6 h-6 shrink-0" />
      <span>{{ item.label }}</span>
    </a>
  </nav>
</template>
