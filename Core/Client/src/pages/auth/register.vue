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
  const cardResponse = ref<{ success: boolean; message: string }>({ success: false, message: 'Waiting for card' });

  const resolver = yupResolver(
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

  const onFormSubmit = async (event: FormSubmitEvent<Record<string, any>>) => {
    loading.value = true;
    console.log('Form Submitted', event);
    if (!event.valid) {
      loading.value = false;
      return;
    }
    const result = await AuthenticationService.registerUser(event.values.cardNumber, event.values.pinCode, event.values.username, event.values.email, event.values.password);
    loading.value = false;
    if (result) {
      router.push('/auth/login');
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
        <template #title>Register</template>

        <template #content>
          <Form v-slot="$form" class="flex flex-col gap-4" :resolver="resolver" :validate-on-value-update="true"
            @submit="onFormSubmit">
            <div class="flex gap-4">
              <div class="flex flex-col gap-2">
                <label for="cardNumber">16 digit Card Number</label>
                <InputText id="cardNumber" name="cardNumber" />
                <Message v-if="$form.cardNumber?.invalid" severity="error" size="small" variant="simple">{{
                  $form.cardNumber.error.message }}</Message>
              </div>
              <div class="flex flex-col gap-2">
                <label for="pinCode">Pin code</label>
                <InputText id="pinCode" name="pinCode" :format="false" />
                <Message v-if="$form.pinCode?.invalid" severity="error" size="small" variant="simple">{{
                  $form.pinCode.error.message }}</Message>
              </div>
            </div>
            <Message :severity="cardResponse?.success ? 'success' : 'warn'">{{ cardResponse?.message }}</Message>
            <div class="flex flex-col gap-2">
              <label for="username">Username</label>
              <InputText id="username" name="username" />
              <Message v-if="$form.username?.invalid" severity="error" size="small" variant="simple">{{
                $form.username.error.message }}</Message>
            </div>
            <div class="flex flex-col gap-2">
              <label for="email">Email</label>
              <InputText id="email" name="email" />
              <Message v-if="$form.email?.invalid" severity="error" size="small" variant="simple">{{
                $form.email.error.message }}</Message>
            </div>
            <div class="flex gap-4">
              <div class="flex flex-col gap-2">
                <label for="password">Password</label>
                <InputText id="password" type="password" name="password" />
                <Message v-if="$form.password?.invalid" severity="error" size="small" variant="simple">{{
                  $form.password.error.message }}</Message>
              </div>
              <div class="flex flex-col gap-2">
                <label for="passwordConfirmation">Confirm Password</label>
                <InputText type="password" id="passwordConfirmation" name="passwordConfirmation" />
                <Message v-if="$form.passwordConfirmation?.invalid" severity="error" size="small" variant="simple">{{
                  $form.passwordConfirmation.error.message }}</Message>
              </div>
            </div>
            <div class="flex justify-between w-full">
              <div class="flex gap-4 mt-1 w-full">
                <Button @click="() => router.push('/auth/login')" label="Login" severity="secondary" variant="outlined"
                  class="w-full" />
                <Button :loading="loading" type="submit" label="Register" class="w-full" loading-icon="ProgressSpinner">Register</Button>
              </div>
            </div>
          </Form>
        </template>

      </Card>
    </div>
  </template>