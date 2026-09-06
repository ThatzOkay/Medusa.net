<script setup lang="ts">
import { onBeforeUnmount, onMounted, ref } from 'vue';
import QrScanner from 'qr-scanner';
import Card from '@/components/ui/Card.vue';
import Message from '@/components/ui/Message.vue';
import Button from '@/components/ui/Button.vue';
import { useCardlessAuth } from '@/composables/useCardlessAuth';

const videoRef = ref<HTMLVideoElement | null>(null);
let scanner: QrScanner | null = null;

type Status = 'scanning' | 'approving' | 'success' | 'error';
const status = ref<Status>('scanning');
const message = ref('Point your camera at the QR code on the cabinet.');

const { approveSession } = useCardlessAuth();

// The cabinet's QR encodes the full URL sppass.open handed it
// (e.g. http://host/cardless/<token>), so a plain phone camera app scanning
// it still works by opening that page directly - this scanner just reads
// the token back out of the same string instead of expecting a bare token.
const extractToken = (decoded: string): string | null => {
    try {
        const url = new URL(decoded);
        const segments = url.pathname.split('/').filter(Boolean);
        return segments.at(-1) ?? null;
    } catch {
        const trimmed = decoded.trim();
        return trimmed.length > 0 ? trimmed : null;
    }
};

const handleDecode = async (decoded: string) => {
    if (status.value !== 'scanning') return;

    const token = extractToken(decoded);
    if (!token) return;

    status.value = 'approving';
    scanner?.stop();

    // No card picker here on purpose - the server always approves with the
    // signed-in player's first/default card (see CardlessApi.ApproveSession).
    const result = await approveSession(token);

    status.value = result.success ? 'success' : 'error';
    message.value = result.message;
};

const retry = () => {
    status.value = 'scanning';
    message.value = 'Point your camera at the QR code on the cabinet.';
    scanner?.start();
};

onMounted(() => {
    if (!videoRef.value) return;

    scanner = new QrScanner(
        videoRef.value,
        (result) => handleDecode(result.data),
        { highlightScanRegion: true, highlightCodeOutline: true },
    );

    scanner.start().catch(() => {
        status.value = 'error';
        message.value = 'Could not access the camera. Check your browser permissions and try again.';
    });
});

onBeforeUnmount(() => {
    scanner?.stop();
    scanner?.destroy();
    scanner = null;
});
</script>

<template>
    <div class="w-full flex flex-col gap-4 justify-center items-center">
        <Card class="p-4 pt-6 pb-6 w-full max-w-md">
            <template #title>Scan cabinet QR code</template>
            <template #content>
                <div class="flex flex-col gap-4 items-center">
                    <div class="relative w-full aspect-square overflow-hidden rounded-md-lg bg-black">
                        <video ref="videoRef" class="w-full h-full object-cover" muted playsinline></video>
                        <div
                            v-if="status !== 'scanning'"
                            class="absolute inset-0 flex items-center justify-center bg-black/60"
                        >
                            <span class="text-white text-md-title-large">
                                {{ status === 'approving' ? 'Approving…' : status === 'success' ? '✓' : '✕' }}
                            </span>
                        </div>
                    </div>

                    <Message
                        :severity="status === 'success' ? 'success' : status === 'error' ? 'error' : 'info'"
                        variant="simple"
                    >
                        {{ message }}
                    </Message>

                    <Button v-if="status === 'error'" @click="retry">Try again</Button>
                </div>
            </template>
        </Card>
    </div>
</template>
