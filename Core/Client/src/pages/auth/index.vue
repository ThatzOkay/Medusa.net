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
import { AuthenticationService } from '@/data/authenticationService';

definePage({
  meta: {
    requiresAuth: false,
  },
})

const router = useRouter();

const loading = ref(false);

const responseErrorMessage = ref('');

const schema = toTypedSchema(
  yup.object({
    email: yup.string().required('Username or Email is required'),
    password: yup.string().required('Password is required'),
  })
);

const { handleSubmit } = useForm({ validationSchema: schema });

const onFormSubmit = handleSubmit(async (values) => {
  loading.value = true;
  responseErrorMessage.value = '';

  try {
    const result = await AuthenticationService.login({ email: values.email, password: values.password, twoFactorCode: undefined, twoFactorRecoveryCode: undefined });

    if (!result.incorrectCredentials && !result.twoFactorRequired) {
      router.push('/');
    } else {
      responseErrorMessage.value = result.errorMessage ?? 'Incorrect username or password.';
    }
  } catch (_) {
  }
  finally {
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
      <template #title>Login</template>

      <template #content>
        <Message v-if="responseErrorMessage !== ''" severity="error" variant="simple" class="mt-1">{{ responseErrorMessage }}</Message>
        <form class="flex flex-col gap-4" @submit="onFormSubmit">
          <div class="flex flex-col gap-2">
            <label for="email">Username or Email</label>
            <Field name="email" v-slot="{ field, errorMessage }" as="div">
              <TextField id="email" v-bind="field" :invalid="!!errorMessage" />
              <Message v-if="errorMessage" severity="error" size="small" variant="simple" class="mt-1">{{ errorMessage }}</Message>
            </Field>
          </div>
          <div class="flex flex-col gap-2">
            <label for="password">Password</label>
            <Field name="password" v-slot="{ field, errorMessage }" as="div">
              <TextField id="password" type="password" v-bind="field" :invalid="!!errorMessage" />
              <Message v-if="errorMessage" severity="error" size="small" variant="simple" class="mt-1">{{ errorMessage }}</Message>
            </Field>
          </div>
          <div class="flex justify-between w-full">
            <div class="flex gap-4 mt-1 w-full">
              <Button type="button" variant="outlined" class="w-full" @click="() => router.push('/auth/register')">Register</Button>
              <Button type="submit" class="w-full" :loading="loading">Login</Button>
            </div>
          </div>
          <Button type="button" variant="text" class="w-full" @click="() => router.push('/auth/forgotPassword')">Forgot password?</Button>
        </form>
      </template>
    </Card>
  </div>
</template>
