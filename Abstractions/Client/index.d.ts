import type { Component } from 'vue'

export interface PluginExportMap {
  [componentKey: string]: Component
}

export interface MedusaHostHelpers {
  getAccessToken: () => string | null
}

declare global {
  interface Window {
    __medusa_plugins__: Record<string, PluginExportMap>
    __medusa_host__:    MedusaHostHelpers
  }
}
