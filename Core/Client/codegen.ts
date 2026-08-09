import type { CodegenConfig } from '@graphql-codegen/cli';

const config: CodegenConfig = {
    overwrite: true,
    schema: 'http://localhost:5120/graphql',
    documents: ['src/graphql/**/*.graphql'],
    //ignoreNoDocuments: true, // for better experience with the watcher
    generates: {
        './src/gql/graphql.ts': {
            plugins: [
                "@graphql-codegen/typescript",
                "@graphql-codegen/typescript-operations",
                "@graphql-codegen/typescript-vue-apollo",
            ],
            config: {
                withCompositionFunctions: true,
                vueCompositionApiImportFrom: "vue",
                addDocBlocks: true,
                skipTypename: false,
                namingConvention: {
                    typeNames: "change-case-all#pascalCase",
                    enumValues: "change-case-all#upperCase",
                    transformUnderscore: true,
                },
                scalars: {
                    DateTime: "Date",
                    JSON: "{ [key: string]: any }",
                    Byte: 'Uint8Array',
                },
            },
        },
    },
};

export default config;
