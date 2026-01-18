<script setup lang="ts">
import { Form, type FormSubmitEvent } from '@primevue/forms';
import Card from '@/volt/Card.vue';
import Button from '@/volt/Button.vue';
import Message from '@/volt/Message.vue';
import InputText from "@/volt/InputText.vue";
import { useRouter } from 'vue-router';
import * as yup from 'yup';
import { yupResolver } from '@primevue/forms/resolvers/yup';
import { ref } from 'vue';
import { AuthenticationService } from '@/data/authenticationService';

definePage({
  meta: {
    requiresAuth: false,
  },
})

const router = useRouter();

const loading = ref(false);

const resolver = yupResolver(
  yup.object({
    email: yup.string().required('Username or Email is required'),
    password: yup.string().required('Password is required'),
  })
);

const onFormSubmit = async (event: FormSubmitEvent<Record<string, any>>) => {
  loading.value = true;
  if (!event.valid) {
    loading.value = false;
    return;
  }

  const result = await AuthenticationService.login({ email: event.values.email!, password: event.values.password!, twoFactorCode: undefined, twoFactorRecoveryCode: undefined });
  loading.value = false;
  if (!result.incorrectCredentials) {
    router.push('/');
  }
};
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
        <Form v-slot="$form" class="flex flex-col gap-4" :resolver="resolver" :validate-on-value-update="true"
          @submit="onFormSubmit">
          <div class="flex flex-col gap-2">
            <label for="email">Username or Email</label>
            <InputText id="email" name="email" />
            <Message v-if="$form.email?.invalid" severity="error" size="small" variant="simple">{{
              $form.email.error.message }}</Message>
          </div>
          <div class="flex flex-col gap-2">
            <label for="password">Password</label>
            <InputText id="password" type="password" name="password" />
            <Message v-if="$form.password?.invalid" severity="error" size="small" variant="simple">{{
              $form.password.error.message }}</Message>
          </div>
          <div class="flex justify-between w-full">
            <div class="flex gap-4 mt-1 w-full">
              <Button @click="() => router.push('/auth/register')" label="Register" severity="secondary"
                variant="outlined" class="w-full" />
              <Button type="submit" label="Login" class="w-full" :loading="loading" loading-icon="ProgressSpinner">Login</Button>
            </div>
          </div>
        </Form>
      </template>
    </Card>
  </div>
</template>