<script setup lang="ts">
import { Icon } from '@iconify/vue';
import { reactive } from 'vue';

export interface NavListItem {
  label: string;
  /** Iconify icon identifier, e.g. "material-symbols:home-rounded". */
  icon?: string;
  active?: boolean;
  command?: () => void;
  /**
   * When set, this item renders as a non-navigable group header that expands
   * to reveal its children instead of calling `command` on click.
   */
  children?: NavListItem[];
}

const props = defineProps<{
  items: NavListItem[];
}>();

// Explicit user overrides of a group's expanded state, keyed by label.
// Groups with no override default to expanded when one of their children is active.
const expandedOverrides = reactive(new Map<string, boolean>());

const isExpanded = (item: NavListItem): boolean =>
  expandedOverrides.get(item.label) ?? (item.children?.some((c) => c.active) ?? false);

const toggle = (item: NavListItem) => expandedOverrides.set(item.label, !isExpanded(item));
</script>

<template>
  <nav class="flex flex-col gap-1">
    <template v-for="item in items" :key="item.label">
      <a
        v-if="!item.children"
        class="flex items-center gap-3 px-4 py-3 min-h-12 rounded-full cursor-pointer text-md-label-large transition-colors"
        :class="item.active ? 'bg-md-primary-container text-md-on-primary-container' : 'text-md-on-surface-variant hover:bg-md-on-surface/8'"
        @click="item.command?.()"
      >
        <Icon v-if="item.icon" :icon="item.icon" class="w-6 h-6 shrink-0" />
        <span>{{ item.label }}</span>
      </a>

      <div v-else class="flex flex-col gap-1">
        <button
          type="button"
          class="flex items-center gap-3 px-4 py-3 min-h-12 rounded-full cursor-pointer text-md-label-large text-md-on-surface-variant hover:bg-md-on-surface/8 transition-colors"
          @click="toggle(item)"
        >
          <Icon v-if="item.icon" :icon="item.icon" class="w-6 h-6 shrink-0" />
          <span class="flex-1 text-left">{{ item.label }}</span>
          <Icon
            icon="material-symbols:expand-more-rounded"
            class="w-5 h-5 shrink-0 transition-transform duration-300 ease-[cubic-bezier(0.2,0,0,1)]"
            :class="{ 'rotate-180': isExpanded(item) }"
          />
        </button>
        <div
          class="grid transition-[grid-template-rows] duration-300 ease-[cubic-bezier(0.2,0,0,1)]"
          :class="isExpanded(item) ? 'grid-rows-[1fr]' : 'grid-rows-[0fr]'"
        >
          <div class="overflow-hidden">
            <div
              class="flex flex-col gap-1 pl-6 transition-opacity ease-[cubic-bezier(0.2,0,0,1)]"
              :class="isExpanded(item) ? 'opacity-100 duration-300 delay-100' : 'opacity-0 duration-150'"
            >
              <a
                v-for="child in item.children"
                :key="child.label"
                class="flex items-center gap-3 px-4 py-2.5 min-h-10 rounded-full cursor-pointer text-md-label-large transition-colors"
                :class="child.active ? 'bg-md-primary-container text-md-on-primary-container' : 'text-md-on-surface-variant hover:bg-md-on-surface/8'"
                @click="child.command?.()"
              >
                <Icon v-if="child.icon" :icon="child.icon" class="w-5 h-5 shrink-0" />
                <span>{{ child.label }}</span>
              </a>
            </div>
          </div>
        </div>
      </div>
    </template>
  </nav>
</template>
