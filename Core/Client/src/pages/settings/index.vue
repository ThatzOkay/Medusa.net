<script setup lang="ts">
import { Field, Form, type SubmissionContext } from 'vee-validate';
import { toTypedSchema } from '@vee-validate/yup';
import * as yup from 'yup';
import { computed, ref } from 'vue';
import { Icon } from '@iconify/vue';
import Card from '@/components/ui/Card.vue';
import Button from '@/components/ui/Button.vue';
import Message from '@/components/ui/Message.vue';
import TextField from '@/components/ui/TextField.vue';
import {
  useGetApiAuthManageInfo,
  useGetApiAuthManagePin,
  usePostApiAuthManageInfo,
  usePostApiAuthManagePin,
  usePostApiAuthManageUsername,
} from '@/data/apiClient';
import type { HttpValidationProblemDetails } from '@/types/api';
import { useTheme, type ThemeMode } from '@/composables/useTheme';
import { useUserStore } from '@/store/userStore';

const userStore = useUserStore();


const extractErrorMessage = (data: HttpValidationProblemDetails | undefined) => {
  const messages = Object.values(data?.errors ?? {}).flat();
  return messages.length > 0 ? messages.join(' ') : 'Something went wrong.';
};

const authFetchOptions = (): RequestInit => ({
  headers: { Authorization: `Bearer ${sessionStorage.getItem('accessToken')}` },
});

type FormMessage = { severity: 'success' | 'error'; text: string };

const { mode, seedColor, defaultSeedColor, setMode, setSeedColor, resetSeedColor } = useTheme();

const modeOptions: { value: ThemeMode; label: string; icon: string }[] = [
  { value: 'light', label: 'Light', icon: 'material-symbols:light-mode-rounded' },
  { value: 'dark', label: 'Dark', icon: 'material-symbols:dark-mode-rounded' },
  { value: 'system', label: 'System', icon: 'material-symbols:brightness-auto-rounded' },
];

const onSeedColorInput = (event: Event) => setSeedColor((event.target as HTMLInputElement).value);
const isDefaultSeedColor = computed(() => seedColor.value.toLowerCase() === defaultSeedColor.toLowerCase());

const { data: infoResult, refetch: refetchInfo } = useGetApiAuthManageInfo({ request: authFetchOptions() });
const currentEmail = computed(() => (infoResult.value?.status === 200 ? infoResult.value.data.email : undefined));

const usernameMutation = usePostApiAuthManageUsername({ request: authFetchOptions() });
const usernameLoading = ref(false);
const usernameMessage = ref<FormMessage | null>(null);

const usernameSchema = toTypedSchema(
  yup.object({
    newUsername: yup.string().min(3, 'Username must be at least 3 characters').required('Username is required'),
  }),
);
const onUsernameSubmit = (async (values: unknown, { resetForm }: SubmissionContext) => {
  usernameLoading.value = true;
  usernameMessage.value = null;

  const formValues = values as { [key: string]: string };

  try {
    const result = await usernameMutation.mutateAsync({ data: { newUsername: formValues.newUsername } });
    if (result.status === 200) {
      userStore.setName(formValues.newUsername);
      usernameMessage.value = { severity: 'success', text: 'Username updated.' };
      resetForm();
    } else {
      usernameMessage.value = { severity: 'error', text: extractErrorMessage(result.status === 400 ? result.data : undefined) };
    }
  } catch {
    // A malformed/non-JSON response (e.g. a stale API server 404ing with an
    // HTML error page) throws inside the generated client rather than
    // resolving with a status — make sure that still surfaces feedback
    // instead of silently leaving the button spinning forever.
    usernameMessage.value = { severity: 'error', text: 'Something went wrong. Please try again.' };
  }

  usernameLoading.value = false;
});

const emailMutation = usePostApiAuthManageInfo({ request: authFetchOptions() });
const emailLoading = ref(false);
const emailMessage = ref<FormMessage | null>(null);

const emailSchema = toTypedSchema(
  yup.object({
    newEmail: yup.string().email('Invalid email format').required('Email is required'),
  }),
);
const onEmailSubmit = async (values: unknown, { resetForm }: SubmissionContext) => {
  emailLoading.value = true;
  emailMessage.value = null;

  const formValues = values as { [key: string]: string };

  try {
    const result = await emailMutation.mutateAsync({ data: { newEmail: formValues.newEmail } });
    if (result.status === 200) {
      await refetchInfo();
      emailMessage.value = { severity: 'success', text: 'Check your inbox to confirm the new email address.' };
      resetForm();
    } else {
      emailMessage.value = { severity: 'error', text: extractErrorMessage(result.status === 400 ? result.data : undefined) };
    }
  } catch {
    emailMessage.value = { severity: 'error', text: 'Something went wrong. Please try again.' };
  }

  emailLoading.value = false;
};

const passwordMutation = usePostApiAuthManageInfo({ request: authFetchOptions() });
const passwordLoading = ref(false);
const passwordMessage = ref<FormMessage | null>(null);

const passwordSchema = toTypedSchema(
  yup.object({
    oldPassword: yup.string().required('Current password is required'),
    newPassword: yup.string().required('New password is required').min(8, 'Password must be at least 8 characters')
      .matches(/(?=.*\d)/, 'Password must contain a number')
      .matches(/(?=.*[a-z])/, 'Password must contain a lowercase letter')
      .matches(/(?=.*[A-Z])/, 'Password must contain an uppercase letter')
      .matches(/(?=.*\W)/, 'Password must contain a special character'),
    newPasswordConfirmation: yup.string()
      .oneOf([yup.ref('newPassword')], 'Passwords must match')
      .required('Password confirmation is required'),
  }),
);
const onPasswordSubmit = async (values: unknown, { resetForm }: SubmissionContext) => {
  passwordLoading.value = true;
  passwordMessage.value = null;

  const formValues = values as { [key: string]: string };

  try {
    const result = await passwordMutation.mutateAsync({
      data: { oldPassword: formValues.oldPassword, newPassword: formValues.newPassword },
    });
    if (result.status === 200) {
      passwordMessage.value = { severity: 'success', text: 'Password updated.' };
      resetForm();
    } else {
      passwordMessage.value = { severity: 'error', text: extractErrorMessage(result.status === 400 ? result.data : undefined) };
    }
  } catch {
    passwordMessage.value = { severity: 'error', text: 'Something went wrong. Please try again.' };
  }

  passwordLoading.value = false;
};

const { data: pinResult, refetch: refetchPin } = useGetApiAuthManagePin({ request: authFetchOptions() });
const currentPin = computed(() => (pinResult.value?.status === 200 ? pinResult.value.data.pin : undefined));

const pinMutation = usePostApiAuthManagePin({ request: authFetchOptions() });
const pinLoading = ref(false);
const pinMessage = ref<FormMessage | null>(null);

const pinSchema = toTypedSchema(
  yup.object({
    oldPin: yup.string().required('Current pin is required').matches(/^\d{4}$/, 'Pin must be exactly 4 digits'),
    newPin: yup.string().required('New pin is required').matches(/^\d{4}$/, 'Pin must be exactly 4 digits'),
  }),
);
const onPinSubmit = async (values: unknown, { resetForm }: SubmissionContext) => {
  pinLoading.value = true;
  pinMessage.value = null;

  const formValues = values as { [key: string]: string };

  try {
    const result = await pinMutation.mutateAsync({ data: { oldPin: formValues.oldPin, newPin: formValues.newPin } });
    if (result.status === 200) {
      await refetchPin();
      pinMessage.value = { severity: 'success', text: 'Pin updated.' };
      resetForm();
    } else {
      pinMessage.value = { severity: 'error', text: extractErrorMessage(result.status === 400 ? result.data : undefined) };
    }
  } catch {
    pinMessage.value = { severity: 'error', text: 'Something went wrong. Please try again.' };
  }

  pinLoading.value = false;
};
</script>

<template>
  <div class="w-full max-w-2xl mx-auto flex flex-col gap-4">
    <Card class="p-4 pt-6 pb-6">
      <template #title>Appearance</template>
      <template #content>
        <div class="flex flex-col gap-6">
          <div class="flex flex-col gap-2">
            <span class="text-md-label-large text-md-on-surface">Theme</span>
            <div class="inline-flex h-10 self-start rounded-full border border-md-outline overflow-hidden">
              <button
                v-for="(option, index) in modeOptions"
                :key="option.value"
                type="button"
                class="flex items-center gap-1.5 px-4 h-full text-md-label-large cursor-pointer transition-colors"
                :class="[
                  index > 0 ? 'border-l border-md-outline' : '',
                  mode === option.value
                    ? 'bg-md-secondary-container text-md-on-secondary-container'
                    : 'text-md-on-surface hover:bg-md-on-surface/8',
                ]"
                @click="setMode(option.value)"
              >
                <Icon :icon="mode === option.value ? 'material-symbols:check-rounded' : option.icon" class="w-[18px] h-[18px]" />
                {{ option.label }}
              </button>
            </div>
          </div>

          <div class="flex items-center gap-4">
            <label
              class="relative w-10 h-10 shrink-0 rounded-full overflow-hidden border border-md-outline-variant cursor-pointer"
              :style="{ backgroundColor: seedColor }"
            >
              <input
                type="color"
                :value="seedColor"
                class="absolute inset-0 w-full h-full opacity-0 cursor-pointer"
                @input="onSeedColorInput"
              >
            </label>
            <div class="flex-1 flex flex-col">
              <span class="text-md-body-large text-md-on-surface">Accent color</span>
              <span class="text-md-body-medium text-md-on-surface-variant uppercase">{{ seedColor }}</span>
            </div>
            <Button v-if="!isDefaultSeedColor" variant="text" @click="resetSeedColor">Reset</Button>
          </div>
        </div>
      </template>
    </Card>

    <Card class="p-4 pt-6 pb-6">
      <template #title>Username</template>
      <template #content>
        <Form class="flex flex-col gap-4" :validation-schema="usernameSchema" @submit="onUsernameSubmit">
          <p v-if="userStore.name" class="text-md-body-medium">Current: {{ userStore.name }}</p>
          <div class="flex flex-col gap-2">
            <label for="newUsername">New username</label>
            <Field name="newUsername" v-slot="{ field, errorMessage }" as="div">
              <TextField id="newUsername" v-bind="field" :invalid="!!errorMessage" />
              <Message v-if="errorMessage" severity="error" size="small" variant="simple" class="mt-1">{{ errorMessage }}</Message>
            </Field>
          </div>
          <Message v-if="usernameMessage" :severity="usernameMessage.severity" size="small" variant="simple">{{ usernameMessage.text }}</Message>
          <Button type="submit" class="w-fit" :loading="usernameLoading">Update username</Button>
        </Form>
      </template>
    </Card>

    <Card class="p-4 pt-6 pb-6">
      <template #title>Email</template>
      <template #content>
        <Form class="flex flex-col gap-4" :validation-schema="emailSchema" @submit="onEmailSubmit">
          <p v-if="currentEmail" class="text-md-body-medium">Current: {{ currentEmail }}</p>
          <div class="flex flex-col gap-2">
            <label for="newEmail">New email address</label>
            <Field name="newEmail" v-slot="{ field, errorMessage }" as="div">
              <TextField id="newEmail" v-bind="field" :invalid="!!errorMessage" />
              <Message v-if="errorMessage" severity="error" size="small" variant="simple" class="mt-1">{{ errorMessage }}</Message>
            </Field>
          </div>
          <Message v-if="emailMessage" :severity="emailMessage.severity" size="small" variant="simple">{{ emailMessage.text }}</Message>
          <Button type="submit" class="w-fit" :loading="emailLoading">Update email</Button>
        </Form>
      </template>
    </Card>

    <Card class="p-4 pt-6 pb-6">
      <template #title>Password</template>
      <template #content>
        <Form class="flex flex-col gap-4" :validation-schema="passwordSchema" @submit="onPasswordSubmit">
          <div class="flex flex-col gap-2">
            <label for="oldPassword">Current password</label>
            <Field name="oldPassword" v-slot="{ field, errorMessage }" as="div">
              <TextField id="oldPassword" type="password" v-bind="field" :invalid="!!errorMessage" />
              <Message v-if="errorMessage" severity="error" size="small" variant="simple" class="mt-1">{{ errorMessage }}</Message>
            </Field>
          </div>
          <div class="flex gap-4">
            <div class="flex flex-col gap-2 flex-1 min-w-0">
              <label for="newPassword">New password</label>
              <Field name="newPassword" v-slot="{ field, errorMessage }" as="div">
                <TextField id="newPassword" type="password" v-bind="field" :invalid="!!errorMessage" />
                <Message v-if="errorMessage" severity="error" size="small" variant="simple" class="mt-1">{{ errorMessage }}</Message>
              </Field>
            </div>
            <div class="flex flex-col gap-2 flex-1 min-w-0">
              <label for="newPasswordConfirmation">Confirm new password</label>
              <Field name="newPasswordConfirmation" v-slot="{ field, errorMessage }" as="div">
                <TextField id="newPasswordConfirmation" type="password" v-bind="field" :invalid="!!errorMessage" />
                <Message v-if="errorMessage" severity="error" size="small" variant="simple" class="mt-1">{{ errorMessage }}</Message>
              </Field>
            </div>
          </div>
          <Message v-if="passwordMessage" :severity="passwordMessage.severity" size="small" variant="simple">{{ passwordMessage.text }}</Message>
          <Button type="submit" class="w-fit" :loading="passwordLoading">Update password</Button>
        </Form>
      </template>
    </Card>

    <Card class="p-4 pt-6 pb-6">
      <template #title>Pin code</template>
      <template #content>
        <Form class="flex flex-col gap-4" :validation-schema="pinSchema" @submit="onPinSubmit">
          <p v-if="currentPin" class="text-md-body-medium">Current: {{ currentPin }}</p>
          <div class="flex gap-4">
            <div class="flex flex-col gap-2 flex-1 min-w-0">
              <label for="oldPin">Current pin</label>
              <Field name="oldPin" v-slot="{ field, errorMessage }" as="div">
                <TextField id="oldPin" v-bind="field" :invalid="!!errorMessage" />
                <Message v-if="errorMessage" severity="error" size="small" variant="simple" class="mt-1">{{ errorMessage }}</Message>
              </Field>
            </div>
            <div class="flex flex-col gap-2 flex-1 min-w-0">
              <label for="newPin">New pin</label>
              <Field name="newPin" v-slot="{ field, errorMessage }" as="div">
                <TextField id="newPin" v-bind="field" :invalid="!!errorMessage" />
                <Message v-if="errorMessage" severity="error" size="small" variant="simple" class="mt-1">{{ errorMessage }}</Message>
              </Field>
            </div>
          </div>
          <Message v-if="pinMessage" :severity="pinMessage.severity" size="small" variant="simple">{{ pinMessage.text }}</Message>
          <Button type="submit" class="w-fit" :loading="pinLoading">Update pin</Button>
        </Form>
      </template>
    </Card>
  </div>
</template>
