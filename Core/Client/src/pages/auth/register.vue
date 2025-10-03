<script setup lang="ts">
import { Form } from '@primevue/forms';
import Card from '@/volt/Card.vue';
import Button from '@/volt/Button.vue';
import InputText from "@/volt/InputText.vue";
import InputNumber from "@/volt/InputNumber.vue";
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
      <template #title>Register</template>

      <template #content>
        <Form class="flex flex-col gap-4">
          <div class="flex gap-4">
            <div class="flex flex-col gap-2">
              <label for="cardNumber">Card Number</label>
              <InputText id="cardNumber" name="cardNumber" />
            </div>
            <div class="flex flex-col gap-2">
              <label for="pinCode">Pin code</label>
              <InputNumber id="pinCode" name="pinCode" :format="false" max="9999"/>
            </div>
          </div>
          <p>Card status:</p>
          <div class="flex flex-col gap-2">
            <label for="username">Username</label>
            <InputText id="username" name="username" />
          </div>
          <div class="flex flex-col gap-2">
            <label for="email">Email</label>
            <InputText id="email" name="email" />
          </div>
          <div class="flex gap-4">
            <div class="flex flex-col gap-2">
              <label for="password">Password</label>
              <InputText id="password" name="password" />
            </div>
            <div class="flex flex-col gap-2">
              <label for="passwordConfirmation">Confirm Password</label>
              <InputText id="passwordConfirmation" name="passwordConfirmation" />
            </div>
          </div>
        </Form>
      </template>

    </Card>
  </div>
</template>