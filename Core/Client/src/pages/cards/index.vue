<script setup lang="ts">
import Card from '@/components/ui/Card.vue';
import Message from '@/components/ui/Message.vue';
import TextField from '@/components/ui/TextField.vue';
import Button from '@/components/ui/Button.vue';
import { FlexRender, tableFeatures, useTable } from '@tanstack/vue-table';
import { Field, Form, type SubmissionContext } from 'vee-validate';
import { toTypedSchema } from '@vee-validate/yup';
import * as yup from 'yup';

import { computed, ref } from 'vue';
import { useCard } from '@/composables/useCard';
import type { MedusaCard } from '@/types/card';

const { myCards, addCard } = useCard();

const { result: myCardsResult, loading: cardsLoading } = myCards;
const cards = computed<MedusaCard[]>(() => myCardsResult.value?.myCards?.edges?.map(edge => edge.node as MedusaCard) ?? []);

const features = tableFeatures({});
const columns = [
    { accessorKey: 'konamiId', header: 'KonamiId' },
    { accessorKey: 'rawId', header: 'Raw Id' },
];
const table = useTable({ features, columns, data: cards });

const loading = ref(false);
const addCardMessage = ref<string | null>(null);

const schema = yup.object({
    cardNumber: yup.string().length(16, 'Card number must be 16 digits').required('Card number is required'),
});

const validationSchema = toTypedSchema(schema);

const onFormSubmit = async (values: unknown, { resetForm }: SubmissionContext) => {
    loading.value = true;
    const formValues = values as yup.InferType<typeof schema>;
    const { error } = await addCard(formValues.cardNumber);

    if (!error) {
        addCardMessage.value = 'Card added successfully!';
    } else {
        addCardMessage.value = 'Failed to add card.';
    }

    loading.value = false;
    resetForm();
};
</script>

<template>
    <div class="w-full flex flex-col gap-4 justify-center">
        <Card class="p-4 pt-6 pb-6">
            <template #title>Manage your cards</template>
            <template #content>
                <div v-if="cardsLoading">
                    <p>Loading cards...</p>
                </div>
                <div v-else class="overflow-x-auto">
                    <table class="w-full min-w-lg border-collapse">
                        <thead>
                            <tr v-for="group in table.getHeaderGroups()" :key="group.id" class="border-b border-md-outline-variant">
                                <th
                                    v-for="header in group.headers"
                                    :key="header.id"
                                    class="text-left px-4 py-2 text-md-label-large text-md-on-surface-variant"
                                >
                                    <FlexRender v-if="!header.isPlaceholder" :header="header" />
                                </th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr v-for="row in table.getRowModel().rows" :key="row.id" class="border-b border-md-outline-variant last:border-0">
                                <td
                                    v-for="cell in row.getAllCells()"
                                    :key="cell.id"
                                    class="px-4 py-2 text-md-body-medium text-md-on-surface"
                                >
                                    <FlexRender :cell="cell" />
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </div>
            </template>
        </Card>
      <Card class="p-4 pt-6 pb-6">
        <template #title>Add new card</template>
        <template #content>
          <Form class="flex flex-col gap-4" :validation-schema="validationSchema" @submit="onFormSubmit">
            <div class="flex flex-col gap-2 flex-1 min-w-0">
              <label for="cardNumber">16 digit Card Number</label>
              <Field name="cardNumber" v-slot="{ field, errorMessage }" as="div">
                <TextField id="cardNumber" v-bind="field" :invalid="!!errorMessage" />
                <Message v-if="errorMessage" severity="error" size="small" variant="simple" class="mt-1">{{ errorMessage }}</Message>
              </Field>
            </div>
            <Message v-if="addCardMessage" severity="info" size="small" variant="simple">{{ addCardMessage }}</Message>
            <Button type="submit" class="w-fit" :loading="loading">Add Card</Button>
          </Form>
        </template>
      </Card>
    </div>
</template>
