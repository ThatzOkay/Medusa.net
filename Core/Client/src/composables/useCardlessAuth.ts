import { useMutation } from '@tanstack/vue-query';
import { postApiCardlessTokenApprove } from '@/data/apiClient';
import type { ApproveSessionResponse } from '@/types/api';

export type ApproveSessionResult = ApproveSessionResponse;

const approveSessionRequest = async (token: string): Promise<ApproveSessionResult> => {
    const result = await postApiCardlessTokenApprove(token, {
        headers: { Authorization: `Bearer ${sessionStorage.getItem('accessToken')}` },
    });

    if (result.data && typeof result.data.success === 'boolean') {
        return result.data;
    }

    return {
        success: false,
        message: result.status === 200 ? 'Unexpected response from server.' : 'Something went wrong approving this login.',
    };
};

export const useCardlessAuth = () => {
    const { mutateAsync: approveSession, isPending: approving } = useMutation({
        mutationFn: approveSessionRequest,
    });

    return {
        approveSession,
        approving,
    };
};
