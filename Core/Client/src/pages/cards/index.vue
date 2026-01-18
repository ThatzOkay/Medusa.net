<script setup lang="ts">
import Card from '@/volt/Card.vue';
import DataTable from '@/volt/DataTable.vue';
import Column from 'primevue/column';

import type { Card as MedusaCard } from '@/types/card';

import { onMounted, ref } from 'vue';
import { useUserStore } from '@/store/userStore';

const cards = ref<MedusaCard[] | null>(null);

onMounted(async () => {
    const store = useUserStore();
    fetch('/api/cards/my',
        {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${store.accessToken}`,
            },
            credentials: 'include',
        }
    )
        .then(response => response.json())
        .then(data => {
            cards.value = data;
        });
});
</script>

<template>
    <div class="w-full flex justify-center">
        <Card class="p-4 pt-6 pb-6">
            <template #title>Manage your cards</template>
            <template #content>
                <div v-if="cards === null">
                    <p>Loading cards...</p>
                </div>
                <div v-else>
                    <DataTable :value="cards" pt:table="min-w-200">
                        <Column field="konamiId" header="KonamiId"></Column>
                        <Column field="rawId" header="Raw Id"></Column>
                    </DataTable>
                </div>
            </template>
        </Card>
    </div>
</template>