import type * as Vue from 'vue'
import type * as VueRouter from 'vue-router'
import type * as PiniaLib from 'pinia'
import 'medusa-plugin-types'

export type { PluginExportMap, MedusaHostHelpers } from 'medusa-plugin-types'

declare global {
  interface Window {
    __medusa_vue__:     typeof Vue
    __medusa_router__:  typeof VueRouter
    __medusa_pinia__:   typeof PiniaLib
  }
}
