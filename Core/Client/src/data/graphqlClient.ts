import {
  ApolloClient,
  HttpLink,
  InMemoryCache,
  type InteropApolloQueryResult,
  type OperationVariables,
  type QueryOptions,
} from '@apollo/client/core';
import { setContext } from '@apollo/client/link/context';
import { useUserStore } from '@/store/userStore';
import type { PageInfo } from '@/gql/graphql';

export class GraphQLClient {
  public client: ApolloClient;

  constructor(baseUrl?: string) {
    // Read the store fresh on every request rather than once at construction time —
    // this client is built as a module-level singleton (see `graphqlClient` below),
    // which runs before Pinia is installed, so `useUserStore()` can't be called here.
    const authLink = setContext((_operation, prevContext) => {
      const token = useUserStore().accessToken;
      return {
        headers: {
          ...prevContext.headers,
          Authorization: token ? `Bearer ${token}` : '',
        },
      };
    });

    const httpLink = new HttpLink({
      // Relative path, proxied to the backend by vite's dev server (see vite.config.ts)
      // the same way the REST client's `/api` calls are — matches that pattern instead
      // of relying on a VITE_API_URL env var that isn't defined anywhere in this project.
      uri: baseUrl ?? '/graphql',
    });

    this.client = new ApolloClient({
      link: authLink.concat(httpLink),
      cache: new InMemoryCache(),
    });
  }

  public async fetchAllPaginated<
    TQuery extends Record<string, unknown>,
    TQueryVariables extends OperationVariables = OperationVariables,
  >(options: QueryOptions<TQueryVariables, TQuery>): Promise<ExtractNode<TQuery>[]> {
    const nodes: ExtractNode<TQuery>[] = [];
    let hasNextPage = true;
    let endCursor: PageInfo['endCursor'] | null = null;

    while (hasNextPage) {
      const variables = {
        ...options.variables,
        ...(endCursor ? { after: endCursor } : {}),
      };

      const result: InteropApolloQueryResult<TQuery> = await this.client.query<TQuery, TQueryVariables>({
        ...options,
        variables: variables as TQueryVariables,
      });

      const connection = GraphQLClient.extractConnection<ExtractNode<TQuery>>(result.data as Record<string, unknown>);
      nodes.push(...connection.edges.map((edge) => edge.node));

      hasNextPage = connection.pageInfo.hasNextPage;
      endCursor = connection.pageInfo.endCursor;
    }

    return nodes;
  }

  private static extractConnection<TNode>(data: Record<string, unknown>): {
    edges: Array<{ node: TNode }>;
    pageInfo: PageInfo;
  } {
    const connection = Object.values(data).find(
      (value): value is { edges: Array<{ node: TNode }>; pageInfo: PageInfo } =>
        value !== null &&
        typeof value === 'object' &&
        'edges' in value &&
        Array.isArray((value as Record<string, unknown>)['edges']),
    );

    if (!connection) throw new Error('No connection field found in query result');

    return connection;
  }
}

type ExtractNode<TQuery> = {
  [K in keyof TQuery]: TQuery[K] extends { edges?: Array<{ node: infer TNode }> | null } | null | undefined
    ? TNode
    : never;
}[keyof TQuery];

// Single shared instance for the whole app. Anything that needs the Apollo client
// outside of a component's `setup()` (main.ts's `provide` call, the one-off helpers in
// useGraphqlOnce.ts) imports this directly instead of going through Vue's `inject()`,
// which only resolves synchronously during setup.
export const graphqlClient = new GraphQLClient();
