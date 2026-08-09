<script setup lang="ts">
defineOptions({ inheritAttrs: false });

withDefaults(
  defineProps<{
    type?: string;
    invalid?: boolean;
  }>(),
  {
    type: 'text',
    invalid: false,
  },
);
</script>

<template>
  <div class="md-text-field md-text-field-outlined" :class="{ 'md-text-field-error': invalid }">
    <input
      :type="type"
      class="w-full bg-transparent outline-none border-none text-md-on-surface text-md-body-large py-2.5 px-4 placeholder:text-md-on-surface-variant"
      v-bind="$attrs"
    />
  </div>
</template>
