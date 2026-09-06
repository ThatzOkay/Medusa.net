/** Internal type. DO NOT USE DIRECTLY. */
type Exact<T extends { [key: string]: unknown }> = { [K in keyof T]: T[K] };
/** Internal type. DO NOT USE DIRECTLY. */
export type Incremental<T> = T | { [P in keyof T]?: P extends ' $fragmentName' | '__typename' ? T[P] : never };
import gql from 'graphql-tag';
import * as VueApolloComposable from '@vue/apollo-composable';
import * as VueCompositionApi from 'vue';
export type Maybe<T> = T | null;
export type InputMaybe<T> = Maybe<T>;
export type ReactiveFunction<TParam> = () => TParam;
/** All built-in and custom scalars, mapped to their actual values */
export type Scalars = {
  ID: { input: string; output: string; }
  String: { input: string; output: string; }
  Boolean: { input: boolean; output: boolean; }
  Int: { input: number; output: number; }
  Float: { input: number; output: number; }
  DateTime: { input: Date; output: Date; }
  Long: { input: unknown; output: unknown; }
};

/** Defines when a policy shall be executed. */
export enum ApplyPolicy {
  /** After the resolver was executed. */
  AFTER_RESOLVER = 'AFTER_RESOLVER',
  /** Before the resolver was executed. */
  BEFORE_RESOLVER = 'BEFORE_RESOLVER',
  /** The policy is applied in the validation step before the execution. */
  VALIDATION = 'VALIDATION'
}

export type BooleanOperationFilterInput = {
  eq?: InputMaybe<Scalars['Boolean']['input']>;
  neq?: InputMaybe<Scalars['Boolean']['input']>;
};

/** The scope of a cache hint. */
export enum CacheControlScope {
  /** The value to cache is specific to a single user. */
  PRIVATE = 'PRIVATE',
  /** The value to cache is not tied to a single user. */
  PUBLIC = 'PUBLIC'
}

export type Card = {
  __typename?: 'Card';
  id: Scalars['Long']['output'];
  konamiId: Scalars['String']['output'];
  rawId: Scalars['String']['output'];
  user: User;
  userId: Scalars['Long']['output'];
};

export type CardFilterInput = {
  and?: InputMaybe<Array<CardFilterInput>>;
  id?: InputMaybe<LongOperationFilterInput>;
  konamiId?: InputMaybe<StringOperationFilterInput>;
  or?: InputMaybe<Array<CardFilterInput>>;
  rawId?: InputMaybe<StringOperationFilterInput>;
  user?: InputMaybe<UserFilterInput>;
  userId?: InputMaybe<LongOperationFilterInput>;
};

export type CardSortInput = {
  id?: InputMaybe<SortEnumType>;
  konamiId?: InputMaybe<SortEnumType>;
  rawId?: InputMaybe<SortEnumType>;
  user?: InputMaybe<UserSortInput>;
  userId?: InputMaybe<SortEnumType>;
};

export type DateTimeOperationFilterInput = {
  eq?: InputMaybe<Scalars['DateTime']['input']>;
  gt?: InputMaybe<Scalars['DateTime']['input']>;
  gte?: InputMaybe<Scalars['DateTime']['input']>;
  in?: InputMaybe<Array<InputMaybe<Scalars['DateTime']['input']>>>;
  lt?: InputMaybe<Scalars['DateTime']['input']>;
  lte?: InputMaybe<Scalars['DateTime']['input']>;
  neq?: InputMaybe<Scalars['DateTime']['input']>;
  ngt?: InputMaybe<Scalars['DateTime']['input']>;
  ngte?: InputMaybe<Scalars['DateTime']['input']>;
  nin?: InputMaybe<Array<InputMaybe<Scalars['DateTime']['input']>>>;
  nlt?: InputMaybe<Scalars['DateTime']['input']>;
  nlte?: InputMaybe<Scalars['DateTime']['input']>;
};

export type IntOperationFilterInput = {
  eq?: InputMaybe<Scalars['Int']['input']>;
  gt?: InputMaybe<Scalars['Int']['input']>;
  gte?: InputMaybe<Scalars['Int']['input']>;
  in?: InputMaybe<Array<InputMaybe<Scalars['Int']['input']>>>;
  lt?: InputMaybe<Scalars['Int']['input']>;
  lte?: InputMaybe<Scalars['Int']['input']>;
  neq?: InputMaybe<Scalars['Int']['input']>;
  ngt?: InputMaybe<Scalars['Int']['input']>;
  ngte?: InputMaybe<Scalars['Int']['input']>;
  nin?: InputMaybe<Array<InputMaybe<Scalars['Int']['input']>>>;
  nlt?: InputMaybe<Scalars['Int']['input']>;
  nlte?: InputMaybe<Scalars['Int']['input']>;
};

export type ListFilterInputTypeOfCardFilterInput = {
  all?: InputMaybe<CardFilterInput>;
  any?: InputMaybe<Scalars['Boolean']['input']>;
  none?: InputMaybe<CardFilterInput>;
  some?: InputMaybe<CardFilterInput>;
};

export type LongOperationFilterInput = {
  eq?: InputMaybe<Scalars['Long']['input']>;
  gt?: InputMaybe<Scalars['Long']['input']>;
  gte?: InputMaybe<Scalars['Long']['input']>;
  in?: InputMaybe<Array<InputMaybe<Scalars['Long']['input']>>>;
  lt?: InputMaybe<Scalars['Long']['input']>;
  lte?: InputMaybe<Scalars['Long']['input']>;
  neq?: InputMaybe<Scalars['Long']['input']>;
  ngt?: InputMaybe<Scalars['Long']['input']>;
  ngte?: InputMaybe<Scalars['Long']['input']>;
  nin?: InputMaybe<Array<InputMaybe<Scalars['Long']['input']>>>;
  nlt?: InputMaybe<Scalars['Long']['input']>;
  nlte?: InputMaybe<Scalars['Long']['input']>;
};

export type Mutation = {
  __typename?: 'Mutation';
  addCard: Array<Card>;
};


export type MutationAddCardArgs = {
  cardNumber: Scalars['String']['input'];
};

/** A connection to a list of items. */
export type MyCardsConnection = {
  __typename?: 'MyCardsConnection';
  /** A list of edges. */
  edges?: Maybe<Array<MyCardsEdge>>;
  /** A flattened list of the nodes. */
  nodes?: Maybe<Array<Card>>;
  /** Information to aid in pagination. */
  pageInfo: PageInfo;
  /** Identifies the total count of items in the connection. */
  totalCount: Scalars['Int']['output'];
};

/** An edge in a connection. */
export type MyCardsEdge = {
  __typename?: 'MyCardsEdge';
  /** A cursor for use in pagination. */
  cursor: Scalars['String']['output'];
  /** The item at the end of the edge. */
  node: Card;
};

/** Information about pagination in a connection. */
export type PageInfo = {
  __typename?: 'PageInfo';
  /** When paginating forwards, the cursor to continue. */
  endCursor?: Maybe<Scalars['String']['output']>;
  /** Indicates whether more edges exist following the set defined by the clients arguments. */
  hasNextPage: Scalars['Boolean']['output'];
  /** Indicates whether more edges exist prior the set defined by the clients arguments. */
  hasPreviousPage: Scalars['Boolean']['output'];
  /** When paginating backwards, the cursor to continue. */
  startCursor?: Maybe<Scalars['String']['output']>;
};

export type Query = {
  __typename?: 'Query';
  myCards?: Maybe<MyCardsConnection>;
  publicUrl: Scalars['String']['output'];
};


export type QueryMyCardsArgs = {
  after?: InputMaybe<Scalars['String']['input']>;
  before?: InputMaybe<Scalars['String']['input']>;
  first?: InputMaybe<Scalars['Int']['input']>;
  last?: InputMaybe<Scalars['Int']['input']>;
  order?: InputMaybe<Array<CardSortInput>>;
  where?: InputMaybe<CardFilterInput>;
};

export enum SortEnumType {
  ASC = 'ASC',
  DESC = 'DESC'
}

export type StringOperationFilterInput = {
  and?: InputMaybe<Array<StringOperationFilterInput>>;
  contains?: InputMaybe<Scalars['String']['input']>;
  endsWith?: InputMaybe<Scalars['String']['input']>;
  eq?: InputMaybe<Scalars['String']['input']>;
  in?: InputMaybe<Array<InputMaybe<Scalars['String']['input']>>>;
  ncontains?: InputMaybe<Scalars['String']['input']>;
  nendsWith?: InputMaybe<Scalars['String']['input']>;
  neq?: InputMaybe<Scalars['String']['input']>;
  nin?: InputMaybe<Array<InputMaybe<Scalars['String']['input']>>>;
  nstartsWith?: InputMaybe<Scalars['String']['input']>;
  or?: InputMaybe<Array<StringOperationFilterInput>>;
  startsWith?: InputMaybe<Scalars['String']['input']>;
};

export type User = {
  __typename?: 'User';
  accessFailedCount: Scalars['Int']['output'];
  cards: Array<Card>;
  concurrencyStamp?: Maybe<Scalars['String']['output']>;
  email?: Maybe<Scalars['String']['output']>;
  emailConfirmed: Scalars['Boolean']['output'];
  id: Scalars['Long']['output'];
  lockoutEnabled: Scalars['Boolean']['output'];
  lockoutEnd?: Maybe<Scalars['DateTime']['output']>;
  normalizedEmail?: Maybe<Scalars['String']['output']>;
  normalizedUserName?: Maybe<Scalars['String']['output']>;
  paseliAmount: Scalars['Int']['output'];
  passwordHash?: Maybe<Scalars['String']['output']>;
  phoneNumber?: Maybe<Scalars['String']['output']>;
  phoneNumberConfirmed: Scalars['Boolean']['output'];
  pin: Scalars['String']['output'];
  securityStamp?: Maybe<Scalars['String']['output']>;
  twoFactorEnabled: Scalars['Boolean']['output'];
  userName?: Maybe<Scalars['String']['output']>;
};

export type UserFilterInput = {
  accessFailedCount?: InputMaybe<IntOperationFilterInput>;
  and?: InputMaybe<Array<UserFilterInput>>;
  cards?: InputMaybe<ListFilterInputTypeOfCardFilterInput>;
  concurrencyStamp?: InputMaybe<StringOperationFilterInput>;
  email?: InputMaybe<StringOperationFilterInput>;
  emailConfirmed?: InputMaybe<BooleanOperationFilterInput>;
  id?: InputMaybe<LongOperationFilterInput>;
  lockoutEnabled?: InputMaybe<BooleanOperationFilterInput>;
  lockoutEnd?: InputMaybe<DateTimeOperationFilterInput>;
  normalizedEmail?: InputMaybe<StringOperationFilterInput>;
  normalizedUserName?: InputMaybe<StringOperationFilterInput>;
  or?: InputMaybe<Array<UserFilterInput>>;
  paseliAmount?: InputMaybe<IntOperationFilterInput>;
  passwordHash?: InputMaybe<StringOperationFilterInput>;
  phoneNumber?: InputMaybe<StringOperationFilterInput>;
  phoneNumberConfirmed?: InputMaybe<BooleanOperationFilterInput>;
  pin?: InputMaybe<StringOperationFilterInput>;
  securityStamp?: InputMaybe<StringOperationFilterInput>;
  twoFactorEnabled?: InputMaybe<BooleanOperationFilterInput>;
  userName?: InputMaybe<StringOperationFilterInput>;
};

export type UserSortInput = {
  accessFailedCount?: InputMaybe<SortEnumType>;
  concurrencyStamp?: InputMaybe<SortEnumType>;
  email?: InputMaybe<SortEnumType>;
  emailConfirmed?: InputMaybe<SortEnumType>;
  id?: InputMaybe<SortEnumType>;
  lockoutEnabled?: InputMaybe<SortEnumType>;
  lockoutEnd?: InputMaybe<SortEnumType>;
  normalizedEmail?: InputMaybe<SortEnumType>;
  normalizedUserName?: InputMaybe<SortEnumType>;
  paseliAmount?: InputMaybe<SortEnumType>;
  passwordHash?: InputMaybe<SortEnumType>;
  phoneNumber?: InputMaybe<SortEnumType>;
  phoneNumberConfirmed?: InputMaybe<SortEnumType>;
  pin?: InputMaybe<SortEnumType>;
  securityStamp?: InputMaybe<SortEnumType>;
  twoFactorEnabled?: InputMaybe<SortEnumType>;
  userName?: InputMaybe<SortEnumType>;
};

export type AddCardMutationVariables = Exact<{
  cardNumber: string;
}>;


export type AddCardMutation = { addCard: Array<{ konamiId: string, rawId: string }> };

export type GetConfigQueryVariables = Exact<{ [key: string]: never; }>;


export type GetConfigQuery = { publicUrl: string };

export type GetMyCardsQueryVariables = Exact<{ [key: string]: never; }>;


export type GetMyCardsQuery = { myCards: { edges: Array<{ node: { konamiId: string, rawId: string } }> | null } | null };


export const AddCardDocument = gql`
    mutation AddCard($cardNumber: String!) {
  addCard(cardNumber: $cardNumber) {
    konamiId
    rawId
  }
}
    `;

/**
 * __useAddCardMutation__
 *
 * To run a mutation, you first call `useAddCardMutation` within a Vue component and pass it any options that fit your needs.
 * When your component renders, `useAddCardMutation` returns an object that includes:
 * - A mutate function that you can call at any time to execute the mutation
 * - Several other properties: https://v4.apollo.vuejs.org/api/use-mutation.html#return
 *
 * @param options that will be passed into the mutation, supported options are listed on: https://v4.apollo.vuejs.org/guide-composable/mutation.html#options;
 *
 * @example
 * const { mutate, loading, error, onDone } = useAddCardMutation({
 *   variables: {
 *     cardNumber: // value for 'cardNumber'
 *   },
 * });
 */
export function useAddCardMutation(options: VueApolloComposable.UseMutationOptions<AddCardMutation, AddCardMutationVariables> | ReactiveFunction<VueApolloComposable.UseMutationOptions<AddCardMutation, AddCardMutationVariables>> = {}) {
  return VueApolloComposable.useMutation<AddCardMutation, AddCardMutationVariables>(AddCardDocument, options);
}
export type AddCardMutationCompositionFunctionResult = VueApolloComposable.UseMutationReturn<AddCardMutation, AddCardMutationVariables>;
export const GetConfigDocument = gql`
    query GetConfig {
  publicUrl
}
    `;

/**
 * __useGetConfigQuery__
 *
 * To run a query within a Vue component, call `useGetConfigQuery` and pass it any options that fit your needs.
 * When your component renders, `useGetConfigQuery` returns an object from Apollo Client that contains result, loading and error properties
 * you can use to render your UI.
 *
 * @param options that will be passed into the query, supported options are listed on: https://v4.apollo.vuejs.org/guide-composable/query.html#options;
 *
 * @example
 * const { result, loading, error } = useGetConfigQuery();
 */
export function useGetConfigQuery(options: VueApolloComposable.UseQueryOptions<GetConfigQuery, GetConfigQueryVariables> | VueCompositionApi.Ref<VueApolloComposable.UseQueryOptions<GetConfigQuery, GetConfigQueryVariables>> | ReactiveFunction<VueApolloComposable.UseQueryOptions<GetConfigQuery, GetConfigQueryVariables>> = {}) {
  return VueApolloComposable.useQuery<GetConfigQuery, GetConfigQueryVariables>(GetConfigDocument, {}, options);
}
export function useGetConfigLazyQuery(options: VueApolloComposable.UseQueryOptions<GetConfigQuery, GetConfigQueryVariables> | VueCompositionApi.Ref<VueApolloComposable.UseQueryOptions<GetConfigQuery, GetConfigQueryVariables>> | ReactiveFunction<VueApolloComposable.UseQueryOptions<GetConfigQuery, GetConfigQueryVariables>> = {}) {
  return VueApolloComposable.useLazyQuery<GetConfigQuery, GetConfigQueryVariables>(GetConfigDocument, {}, options);
}
export type GetConfigQueryCompositionFunctionResult = VueApolloComposable.UseQueryReturn<GetConfigQuery, GetConfigQueryVariables>;
export const GetMyCardsDocument = gql`
    query GetMyCards {
  myCards {
    edges {
      node {
        konamiId
        rawId
      }
    }
  }
}
    `;

/**
 * __useGetMyCardsQuery__
 *
 * To run a query within a Vue component, call `useGetMyCardsQuery` and pass it any options that fit your needs.
 * When your component renders, `useGetMyCardsQuery` returns an object from Apollo Client that contains result, loading and error properties
 * you can use to render your UI.
 *
 * @param options that will be passed into the query, supported options are listed on: https://v4.apollo.vuejs.org/guide-composable/query.html#options;
 *
 * @example
 * const { result, loading, error } = useGetMyCardsQuery();
 */
export function useGetMyCardsQuery(options: VueApolloComposable.UseQueryOptions<GetMyCardsQuery, GetMyCardsQueryVariables> | VueCompositionApi.Ref<VueApolloComposable.UseQueryOptions<GetMyCardsQuery, GetMyCardsQueryVariables>> | ReactiveFunction<VueApolloComposable.UseQueryOptions<GetMyCardsQuery, GetMyCardsQueryVariables>> = {}) {
  return VueApolloComposable.useQuery<GetMyCardsQuery, GetMyCardsQueryVariables>(GetMyCardsDocument, {}, options);
}
export function useGetMyCardsLazyQuery(options: VueApolloComposable.UseQueryOptions<GetMyCardsQuery, GetMyCardsQueryVariables> | VueCompositionApi.Ref<VueApolloComposable.UseQueryOptions<GetMyCardsQuery, GetMyCardsQueryVariables>> | ReactiveFunction<VueApolloComposable.UseQueryOptions<GetMyCardsQuery, GetMyCardsQueryVariables>> = {}) {
  return VueApolloComposable.useLazyQuery<GetMyCardsQuery, GetMyCardsQueryVariables>(GetMyCardsDocument, {}, options);
}
export type GetMyCardsQueryCompositionFunctionResult = VueApolloComposable.UseQueryReturn<GetMyCardsQuery, GetMyCardsQueryVariables>;