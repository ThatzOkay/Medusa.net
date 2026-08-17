import * as Vue from 'vue'
import * as VueRouter from 'vue-router'
import * as PiniaLib from 'pinia'
import { useUserStore } from '@/store/userStore'

window.__medusa_vue__     = Vue
window.__medusa_router__  = VueRouter
window.__medusa_pinia__   = PiniaLib

window.__medusa_plugins__ = {}

window.__medusa_host__ = {
  getAccessToken: () =>
    useUserStore().accessToken ?? sessionStorage.getItem('accessToken'),
}