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
  import {usePostApiAuthRegister} from "@/data/apiClient.ts";

  definePage({
    meta: {
      requiresAuth: false,
    },
  })

  const router = useRouter();
  const registerMutation = usePostApiAuthRegister();

  const loading = ref(false);
  const cardResponse = ref<{ success: boolean; message: string }>({ success: false, message: 'Waiting for card' });

  const schema = toTypedSchema(
    yup.object({
      cardNumber: yup.string().length(16, 'Card number must be 16 digits').required('Card number is required')
        .test('validate-card', '', async function (value) {
          const { pinCode } = this.parent;
          const response = await validateCard(value!, pinCode!);
          cardResponse.value = response;
          return response.success;
        }),
      pinCode: yup
        .string()
        .required('Pin code is required')
        .matches(/^\d{4}$/, 'Pin code must be exactly 4 digits (0000–9999)')
        .test('validate-pin', '', async function (value) {
          const { cardNumber } = this.parent;
          const response = await validateCard(cardNumber!, value!);
          cardResponse.value = response;
          return response.success;
        }),
      username: yup.string().min(3, 'Username must be at least 3 characters').required('Username is required'),
      email: yup.string().email('Invalid email format').required('Email is required'),
      password: yup.string().required('Password is required').min(8, 'Password must be at least 6 characters')
        .matches(/(?=.*\d)/, 'Password must contain a number')
        .matches(/(?=.*[a-z])/, 'Password must contain a lowercase letter')
        .matches(/(?=.*[A-Z])/, 'Password must contain an uppercase letter')
        .matches(/(?=.*\W)/, 'Password must contain a special character'),
      passwordConfirmation: yup.string()
        .oneOf([yup.ref('password')], 'Passwords must match')
        .required('Password confirmation is required'),
    })
  );

  const validateCard = async (cardNumber: string, pinCode: string) => {
    const response = await fetch('/api/cards/validate', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ cardId: cardNumber, pin: pinCode }),
    });

    if (!response.ok) {
      return { success: false, message: 'Failed to validate card' };
    }

    return response.json();
  };

  const { handleSubmit } = useForm({ validationSchema: schema });

  const onFormSubmit = handleSubmit(async (values) => {
    loading.value = true;
    console.log('Form Submitted', values);

    try {
      await registerMutation.mutateAsync({
        data: {
          konamiId : values.cardNumber,
          pin : values.pinCode,
          username: values.username,
          email: values.email,
          password: values.password
        }
      })

      const loginResult = await AuthenticationService.login({
        email: values.email,
        password: values.password,
        twoFactorCode: undefined,
        twoFactorRecoveryCode: undefined,
      });

      if (loginResult.incorrectCredentials) {
        loading.value = false;
        router.push('/auth');
        return;
      }

    } catch (error) {
      console.log(error);
      loading.value = false;
    }

    loading.value = false;
    router.push('/');
  });

</script>

  <route lang="yaml">
  meta:
    layout: auth
  </route>

  <template>
      <Card class="p-24 pt-16 pb-16 w-3xl">
        <template #title>Register</template>

        <template #content>
          <form class="flex flex-col gap-4" @submit="onFormSubmit">
            <div class="flex gap-4">
              <div class="flex flex-col gap-2 flex-1 min-w-0">
                <label for="cardNumber">16 digit Card Number</label>
                <Field name="cardNumber" v-slot="{ field, errorMessage }" as="div">
                  <TextField id="cardNumber" v-bind="field" :invalid="!!errorMessage" />
                  <Message v-if="errorMessage" severity="error" size="small" variant="simple" class="mt-1">{{ errorMessage }}</Message>
                </Field>
              </div>
              <div class="flex flex-col gap-2 flex-1 min-w-0">
                <label for="pinCode">Pin code</label>
                <Field name="pinCode" v-slot="{ field, errorMessage }" as="div">
                  <TextField id="pinCode" v-bind="field" :invalid="!!errorMessage" />
                  <Message v-if="errorMessage" severity="error" size="small" variant="simple" class="mt-1">{{ errorMessage }}</Message>
                </Field>
              </div>
            </div>
            <Message :severity="cardResponse?.success ? 'success' : 'warn'">{{ cardResponse?.message }}</Message>
            <div class="flex flex-col gap-2">
              <label for="username">Username</label>
              <Field name="username" v-slot="{ field, errorMessage }" as="div">
                <TextField id="username" v-bind="field" :invalid="!!errorMessage" />
                <Message v-if="errorMessage" severity="error" size="small" variant="simple" class="mt-1">{{ errorMessage }}</Message>
              </Field>
            </div>
            <div class="flex flex-col gap-2">
              <label for="email">Email</label>
              <Field name="email" v-slot="{ field, errorMessage }" as="div">
                <TextField id="email" v-bind="field" :invalid="!!errorMessage" />
                <Message v-if="errorMessage" severity="error" size="small" variant="simple" class="mt-1">{{ errorMessage }}</Message>
              </Field>
            </div>
            <div class="flex gap-4">
              <div class="flex flex-col gap-2 flex-1 min-w-0">
                <label for="password">Password</label>
                <Field name="password" v-slot="{ field, errorMessage }" as="div">
                  <TextField id="password" type="password" v-bind="field" :invalid="!!errorMessage" />
                  <Message v-if="errorMessage" severity="error" size="small" variant="simple" class="mt-1">{{ errorMessage }}</Message>
                </Field>
              </div>
              <div class="flex flex-col gap-2 flex-1 min-w-0">
                <label for="passwordConfirmation">Confirm Password</label>
                <Field name="passwordConfirmation" v-slot="{ field, errorMessage }" as="div">
                  <TextField type="password" id="passwordConfirmation" v-bind="field" :invalid="!!errorMessage" />
                  <Message v-if="errorMessage" severity="error" size="small" variant="simple" class="mt-1">{{ errorMessage }}</Message>
                </Field>
              </div>
            </div>
            <div class="flex justify-between w-full">
              <div class="flex gap-4 mt-1 w-full">
                <Button type="button" variant="outlined" class="w-full" @click="() => router.push('/auth/login')">Login</Button>
                <Button :loading="loading" type="submit" class="w-full">Register</Button>
              </div>
            </div>
          </form>
        </template>
      </Card>
  </template>
