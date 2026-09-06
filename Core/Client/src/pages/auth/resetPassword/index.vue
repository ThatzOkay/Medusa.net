<script setup lang="ts">
import { useForm, Field } from 'vee-validate';
import { toTypedSchema } from '@vee-validate/yup';
import Card from '@/components/ui/Card.vue';
import Button from '@/components/ui/Button.vue';
import Message from '@/components/ui/Message.vue';
import TextField from '@/components/ui/TextField.vue';
import { useRoute, useRouter } from 'vue-router';
import * as yup from 'yup';
import { ref } from 'vue';
import { usePostApiAuthResetPassword } from '@/data/apiClient.ts';

definePage({
  meta: {
    requiresAuth: false,
  },
})

const route = useRoute();
const router = useRouter();
const resetPasswordMutation = usePostApiAuthResetPassword();

const email = String(route.query.email ?? '');
const code = String(route.query.code ?? '');
const linkIsValid = email !== '' && code !== '';

const loading = ref(false);
const responseErrorMessage = ref('');

const schema = toTypedSchema(
  yup.object({
    newPassword: yup.string().required('Password is required').min(8, 'Password must be at least 8 characters')
      .matches(/(?=.*\d)/, 'Password must contain a number')
      .matches(/(?=.*[a-z])/, 'Password must contain a lowercase letter')
      .matches(/(?=.*[A-Z])/, 'Password must contain an uppercase letter')
      .matches(/(?=.*\W)/, 'Password must contain a special character'),
    newPasswordConfirmation: yup.string()
      .oneOf([yup.ref('newPassword')], 'Passwords must match')
      .required('Password confirmation is required'),
  })
);

const { handleSubmit } = useForm({ validationSchema: schema });

const onFormSubmit = handleSubmit(async (values) => {
  loading.value = true;
  responseErrorMessage.value = '';

  try {
    const result = await resetPasswordMutation.mutateAsync({
      data: { email, resetCode: code, newPassword: values.newPassword },
    });

    if (result.status === 200) {
      await router.push('/auth/resetPassword/confirmation');
    } else {
      responseErrorMessage.value = 'This reset link is invalid or has expired.';
    }
  } catch {
    responseErrorMessage.value = 'Something went wrong. Please try again.';
  } finally {
    loading.value = false;
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
      <template #title>Reset Password</template>

      <template #content>
        <Message v-if="!linkIsValid" severity="error" variant="simple">
          This reset link is missing or invalid. Request a new one from the forgot password page.
        </Message>
        <form v-else class="flex flex-col gap-4" @submit="onFormSubmit">
          <Message v-if="responseErrorMessage !== ''" severity="error" variant="simple">{{ responseErrorMessage }}</Message>
          <div class="flex flex-col gap-2">
            <label for="newPassword">New Password</label>
            <Field name="newPassword" v-slot="{ field, errorMessage }" as="div">
              <TextField id="newPassword" type="password" v-bind="field" :invalid="!!errorMessage" />
              <Message v-if="errorMessage" severity="error" size="small" variant="simple" class="mt-1">{{ errorMessage }}</Message>
            </Field>
          </div>
          <div class="flex flex-col gap-2">
            <label for="newPasswordConfirmation">Confirm New Password</label>
            <Field name="newPasswordConfirmation" v-slot="{ field, errorMessage }" as="div">
              <TextField id="newPasswordConfirmation" type="password" v-bind="field" :invalid="!!errorMessage" />
              <Message v-if="errorMessage" severity="error" size="small" variant="simple" class="mt-1">{{ errorMessage }}</Message>
            </Field>
          </div>
          <div class="flex justify-between w-full">
            <div class="flex gap-4 mt-1 w-full">
              <Button type="button" variant="outlined" class="w-full" @click="() => router.push('/auth')">Back to Login</Button>
              <Button type="submit" class="w-full" :loading="loading">Reset Password</Button>
            </div>
          </div>
        </form>
      </template>
    </Card>
  </div>
</template>
