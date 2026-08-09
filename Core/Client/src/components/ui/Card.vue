 <script setup lang="ts">

const slots = defineSlots<{
  title?(props: {}): unknown;
  content?(props: {}): unknown;
  default?(props: {}): unknown;
}>();
</script>

<template>
  <div class="md-card md-card-elevated">
    <h2 v-if="slots.title" class="text-md-title-large text-md-on-surface mb-4">
      <slot name="title" />
    </h2>
    <div class="text-md-body-medium text-md-on-surface-variant">
      <slot name="content"><slot /></slot>
    </div>
  </div>
</template>
