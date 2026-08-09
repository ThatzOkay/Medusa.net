import { defineConfig } from 'orval';

export default defineConfig({
    medusa: {
        input: 'http://localhost:5120/openapi/v1.json',
        output: {
            namingConvention: 'camelCase',
            target: './src/data/apiClient.ts',
            schemas: './src/types/api',
            client: 'vue-query',
        }
    }
});