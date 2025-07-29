<script setup lang="ts">
import Card from '@/volt/Card.vue';
import Button from '@/volt/Button.vue';

definePage({
    meta: {
        requiresAuth: false,
    },
})

</script>

<route lang="yaml">
meta:
  layout: auth
</route>

<template>
    <Card>
        <template #title>Login</template>

        <template #content>

        </template>

        <template #footer>
            <div class="flex justify-between">
                <div class="flex gap-4 mt-1">
                    <Button label="Register" severity="secondary" variant="outlined" class="w-full" />
                    <Button label="Login" class="w-full">Login</Button>
                </div>
            </div>
        </template>
    </Card>
</template>