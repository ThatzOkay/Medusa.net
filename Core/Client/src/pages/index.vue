<script setup lang="ts">
import Card from '@/volt/Card.vue';
</script>

<template>
    <div class="w-full flex justify-center">
        <Card class="p-4 pt-6 pb-6">
            <template #title>Welcome to Medusa.net</template>

            <template #content>
                <p class="mb-16">This is the home page. Use the sidebar to navigate through the application.</p>
            </template>
        </Card>
    </div>
</template>

<style scoped></style>