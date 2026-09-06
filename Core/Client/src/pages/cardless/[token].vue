<script setup lang="ts">
import { computed, ref } from 'vue';
import { useRoute } from 'vue-router';
import Card from '@/components/ui/Card.vue';
import Button from '@/components/ui/Button.vue';
import Message from '@/components/ui/Message.vue';
import { useCardlessAuth } from '@/composables/useCardlessAuth';
import { useGetConfigQuery } from '@/gql/graphql';

const route = useRoute<'/cardless/[token]'>();
const token = computed(() => route.params.token);

const { approveSession, approving } = useCardlessAuth();

const { result: configResult } = useGetConfigQuery();
const publicUrl = computed(() => configResult.value?.publicUrl ?? '');

const status = ref<'idle' | 'success' | 'error'>('idle');
const message = ref('');

const approve = async () => {
    const result = await approveSession(token.value);
    status.value = result.success ? 'success' : 'error';
    message.value = result.message;
};
</script>

<template>
    <div class="w-full flex flex-col gap-4 justify-center items-center">
        <Card class="p-4 pt-6 pb-6 w-full max-w-md">
            <template #title>Cardless login</template>
            <template #content>
                <div class="flex flex-col gap-4">
                    <p>Approve this login on the arcade cabinet using your account's default card?</p>

                    <Message v-if="publicUrl" severity="info" variant="simple" size="small">
                        Connecting to {{ publicUrl }}
                    </Message>

                    <Message v-if="message" :severity="status === 'success' ? 'success' : 'error'" variant="simple">
                        {{ message }}
                    </Message>

                    <Button v-if="status !== 'success'" :loading="approving" @click="approve">
                        Approve login
                    </Button>
                </div>
            </template>
        </Card>
    </div>
</template>
