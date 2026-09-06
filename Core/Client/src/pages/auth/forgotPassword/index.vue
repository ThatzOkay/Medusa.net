<script setup lang="ts">
import { useForm, Field } from 'vee-validate';
import { toTypedSchema } from '@vee-validate/yup';
import Card from '@/components/ui/Card.vue';
import Button from '@/components/ui/Button.vue';
import Message from '@/components/ui/Message.vue';
import TextField from '@/components/ui/TextField.vue';
import { useRouter } from 'vue-router';
import * as yup from 'yup';
import { ref } from 'vue';
import { usePostApiAuthForgotPassword } from '@/data/apiClient.ts';

definePage({
  meta: {
    requiresAuth: false,
  },
})

const router = useRouter();
const forgotPasswordMutation = usePostApiAuthForgotPassword();

const loading = ref(false);

const schema = toTypedSchema(
  yup.object({
    email: yup.string().email('Invalid email format').required('Email is required'),
  })
);

const { handleSubmit } = useForm({ validationSchema: schema });

const onFormSubmit = handleSubmit(async (values) => {
  loading.value = true;

  try {
    await forgotPasswordMutation.mutateAsync({ data: { email: values.email } });
  } finally {
    loading.value = false;
    // The API never reveals whether the email belongs to an account, so always
    // show the same confirmation regardless of the outcome.
    await router.push('/auth/forgotPassword/confirmation');
  }
});
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
        <form class="flex flex-col gap-4" @submit="onFormSubmit">
          <div class="flex flex-col gap-2">
            <label for="email">Email</label>
            <Field name="email" v-slot="{ field, errorMessage }" as="div">
              <TextField id="email" v-bind="field" :invalid="!!errorMessage" />
              <Message v-if="errorMessage" severity="error" size="small" variant="simple" class="mt-1">{{ errorMessage }}</Message>
            </Field>
          </div>
          <div class="flex justify-between w-full">
            <div class="flex gap-4 mt-1 w-full">
              <Button type="button" variant="outlined" class="w-full" @click="() => router.push('/auth')">Back to Login</Button>
              <Button type="submit" class="w-full" :loading="loading">Send Reset Link</Button>
            </div>
          </div>
        </form>
      </template>
    </Card>
  </div>
</template>
