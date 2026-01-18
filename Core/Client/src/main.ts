import './assets/main.css'
import 'primeicons/primeicons.css'

import { createApp } from 'vue'
import PrimeVue from 'primevue/config';
import App from './App.vue'
import { createPinia } from 'pinia';
import router from './router';
import { addIcons, OhVueIcon } from 'oh-vue-icons';

const app = createApp(App);

const pinia = createPinia();

app.use(PrimeVue, {
    unstyled: true
}).use(pinia)
  .use(router);


addIcons();

app.component("oh-vue-icon", OhVueIcon);

router.isReady().then(() => {
  app.mount('#app');
})
