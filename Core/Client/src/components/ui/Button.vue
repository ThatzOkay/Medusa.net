<script setup lang="ts">
import {computed} from "vue";
import classNames from "classnames";
import { Icon } from "@iconify/vue";

const variantClasses = {
  filled: 'md-btn-filled',
  outlined: 'md-btn-outlined',
  text: 'md-btn-text',
  tonal: 'md-btn-tonal',
  elevated: 'md-btn-elevated',
} as const;

const props = withDefaults(
  defineProps<{
    variant?: keyof typeof variantClasses;
    type?: 'button' | 'submit' | 'reset';
    loading?: boolean;
    disabled?: boolean;
  }>(),
  {
    variant: 'filled',
    type: 'button',
    loading: false,
    disabled: false,
  },
);

const classes = computed(() => 
  classNames(
      'group',
      variantClasses[props.variant],
      props.loading ? 'is-loading' : ''
  )
);
</script>

<template>
  <button :type="type" :disabled="disabled || loading" class="md-btn" :class="classes">
    <Icon icon="line-md:loading-loop" class="h-4 w-4 absolute hidden group-[.is-loading]:block" />
    <span class="group-[.is-loading]:text-transparent">
      <slot />
    </span>
  </button>
</template>
