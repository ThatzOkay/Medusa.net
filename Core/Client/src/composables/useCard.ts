import { GetMyCardsDocument, type GetMyCardsQuery, useAddCardMutation, useGetMyCardsQuery } from "@/gql/graphql"

export const useCard = () => {
    const myCards = useGetMyCardsQuery();

    const { mutate: addCardMutate } = useAddCardMutation({
        update: (cache, { data }) => {
            const newCard = data?.addCard?.[0];
            if (!newCard) return;

            const existing = cache.readQuery<GetMyCardsQuery>({ query: GetMyCardsDocument });
            if (!existing?.myCards) return;

            cache.writeQuery<GetMyCardsQuery>({
                query: GetMyCardsDocument,
                data: {
                    myCards: {
                        ...existing.myCards,
                        edges: [
                            ...(existing.myCards.edges ?? []),
                            { node: newCard },
                        ],
                    },
                },
            });
        },
    });

    const addCard = async (cardNumber: string) => {
        try {
            const result = await addCardMutate({ cardNumber });
            return { data: result?.data?.addCard ?? null, error: null };
        } catch (error) {
            return { data: null, error };
        }
    };

    return {
        myCards,
        addCard,
    };
};
