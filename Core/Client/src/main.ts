import './assets/main.css'

import { createApp } from 'vue'
import App from './App.vue'
import { createPinia } from 'pinia';
import { VueQueryPlugin } from '@tanstack/vue-query';
import { DefaultApolloClient } from '@vue/apollo-composable';
import { graphqlClient } from '@/data/graphqlClient';
import router from './router';

import './types/globals';

const app = createApp(App);

const pinia = createPinia();

app.use(pinia)
  .use(VueQueryPlugin)
  .use(router)
  .provide(DefaultApolloClient, graphqlClient.client);

// tailwind-material-3 uses Tailwind's class-based dark mode strategy but has
// no built-in toggle of its own — follow the system preference, matching the
// app's previous `@media (prefers-color-scheme: dark)`-only behavior.
const darkModeQuery = window.matchMedia('(prefers-color-scheme: dark)');
const syncDarkClass = (isDark: boolean) => document.documentElement.classList.toggle('dark', isDark);
syncDarkClass(darkModeQuery.matches);
darkModeQuery.addEventListener('change', (event) => syncDarkClass(event.matches));

router.isReady().then(() => {
  app.mount('#app');
})
