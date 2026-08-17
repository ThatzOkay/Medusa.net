import type * as Vue from 'vue'
import type * as VueRouter from 'vue-router'
import type * as PiniaLib from 'pinia'

export interface PluginExportMap {
  [componentKey: string]: Vue.Component
}

export interface MedusaHostHelpers {
  getAccessToken: () => string | null
}

declare global {
  interface Window {
    __medusa_vue__:     typeof Vue
    __medusa_router__:  typeof VueRouter
    __medusa_pinia__:   typeof PiniaLib
    __medusa_plugins__: Record<string, PluginExportMap>
    __medusa_host__:    MedusaHostHelpers
  }
}