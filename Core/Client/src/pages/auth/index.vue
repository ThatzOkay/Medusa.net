<script setup lang="ts">
import { Form } from '@primevue/forms';
import Card from '@/volt/Card.vue';
import Button from '@/volt/Button.vue';
import InputText from "@/volt/InputText.vue";
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
    <Card class="p-24 pt-16 pb-16">
        <template #title>Login</template>

        <template #content>
          <Form class="flex flex-col gap-2">
            <div class="flex flex-col gap-2">
              <label for="email">Username or Email</label>
              <InputText id="email" name="email" />
            </div>
            <div class="flex flex-col gap-2">
              <label for="password">Password</label>
              <InputText id="password" name="password" />
            </div>
            <div class="flex justify-between">
              <div class="flex gap-4 mt-1">
                <Button @click="() => router.push('/auth/register')" label="Register" severity="secondary" variant="outlined" class="w-full" />
                <Button label="Login" class="w-full">Login</Button>
              </div>
            </div>
          </Form>
        </template>
    </Card>
  </div>
</template>