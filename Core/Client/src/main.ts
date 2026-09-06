import './assets/main.css'

import { createApp } from 'vue'
import App from './App.vue'
import { createPinia } from 'pinia';
import { VueQueryPlugin } from '@tanstack/vue-query';
import { DefaultApolloClient } from '@vue/apollo-composable';
import { graphqlClient } from '@/data/graphqlClient';
import router from './router';
import { initTheme } from '@/composables/useTheme';

import './types/globals';

// Apply the user's persisted light/dark/system mode and seed color before
// mount, so there's no flash of the default theme.
initTheme();

const app = createApp(App);

const pinia = createPinia();

app.use(pinia)
  .use(VueQueryPlugin)
  .use(router)
  .provide(DefaultApolloClient, graphqlClient.client);

router.isReady().then(() => {
  app.mount('#app');
})
