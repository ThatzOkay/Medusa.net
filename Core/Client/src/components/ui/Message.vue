<script setup lang="ts">
import { computed } from 'vue';

// Material 3's baseline color roles only cover primary/secondary/tertiary/
// error — there's no "warning"/"success"/"info" role to seed, so those three
// severities fall back to plain Tailwind palette colors (with dark-mode
// variants) instead of md-* tokens; only `error` maps onto the seeded palette.
const props = withDefaults(
  defineProps<{
    severity?: 'error' | 'warn' | 'success' | 'info';
    variant?: 'filled' | 'simple';
    size?: 'small' | 'normal';
  }>(),
  {
    severity: 'info',
    variant: 'filled',
    size: 'normal',
  },
);

const textColorClasses: Record<string, string> = {
  error: 'text-md-error',
  warn: 'text-amber-600 dark:text-amber-400',
  success: 'text-green-600 dark:text-green-400',
  info: 'text-blue-600 dark:text-blue-400',
};

const bannerColorClasses: Record<string, string> = {
  error: 'bg-md-error-container text-md-on-error-container',
  warn: 'bg-amber-100 text-amber-900 dark:bg-amber-900/40 dark:text-amber-200',
  success: 'bg-green-100 text-green-900 dark:bg-green-900/40 dark:text-green-200',
  info: 'bg-blue-100 text-blue-900 dark:bg-blue-900/40 dark:text-blue-200',
};

const textColorClass = computed(() => textColorClasses[props.severity]);
const bannerColorClass = computed(() => bannerColorClasses[props.severity]);
const sizeClass = computed(() => (props.size === 'small' ? 'text-xs' : 'text-sm'));
</script>

<template>
  <p v-if="variant === 'simple'" :class="[textColorClass, sizeClass]">
    <slot />
  </p>
  <div v-else class="rounded-md-xs px-3 py-2 flex items-center gap-2" :class="[bannerColorClass, sizeClass]">
    <slot />
  </div>
</template>
