<script setup lang="ts">
import Card from '@/components/ui/Card.vue';
import Button from '@/components/ui/Button.vue';
import { useRouter } from 'vue-router';

definePage({
  meta: {
    requiresAuth: false,
  },
})

const router = useRouter();
</script>

<route lang="yaml">
meta:
  layout: auth
</route>

<template>
  <div>
    <Card class="p-24 pt-16 pb-16 w-3xl">
      <template #title>Forgot Password</template>

      <template #content>
        <div class="flex flex-col gap-4">
          <p>If an account exists for that email, a password reset link has been sent.</p>
          <Button type="button" class="w-full" @click="() => router.push('/auth')">Back to Login</Button>
        </div>
      </template>
    </Card>
  </div>
</template>
