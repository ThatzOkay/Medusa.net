import { ref } from 'vue';
import { getDarkThemeColors, getLightThemeColors } from 'tailwind-material-3';
import { generateColorTokens, SEED_COLOR } from '@/theme/colorTokens';

export type ThemeMode = 'light' | 'dark' | 'system';

const MODE_STORAGE_KEY = 'theme-mode';
const SEED_COLOR_STORAGE_KEY = 'theme-seed-color';
const OVERRIDE_STYLE_ELEMENT_ID = 'theme-seed-overrides';

const isThemeMode = (value: string | null): value is ThemeMode =>
  value === 'light' || value === 'dark' || value === 'system';

const storedMode = localStorage.getItem(MODE_STORAGE_KEY);
const mode = ref<ThemeMode>(isThemeMode(storedMode) ? storedMode : 'system');
const seedColor = ref<string>(localStorage.getItem(SEED_COLOR_STORAGE_KEY) ?? SEED_COLOR);

const systemDarkQuery = window.matchMedia('(prefers-color-scheme: dark)');

const applyDarkClass = () => {
  const isDark = mode.value === 'system' ? systemDarkQuery.matches : mode.value === 'dark';
  document.documentElement.classList.toggle('dark', isDark);
};

const toCssVarsBlock = (selector: string, colors: Record<string, string>) => {
  const declarations = Object.entries(colors).map(([key, hex]) => `  --${key}: ${hex};`).join('\n');
  return `${selector} {\n${declarations}\n}`;
};

// tailwind.config.ts already bakes SEED_COLOR's palette into `:root`/`.dark`
// at build time. For any other seed we inject a `<style>` tag appended after
// that stylesheet in <head> — same `:root`/`.dark` selectors, so it wins on
// cascade order alone — to override just the `--md-*` custom properties.
// Picking SEED_COLOR again removes the tag so the build-time output (the
// single source of truth for the default look) applies with zero drift.
const applySeedColor = () => {
  const existing = document.getElementById(OVERRIDE_STYLE_ELEMENT_ID);

  if (seedColor.value.toLowerCase() === SEED_COLOR.toLowerCase()) {
    existing?.remove();
    return;
  }

  const styleEl = (existing as HTMLStyleElement | null) ?? document.createElement('style');
  styleEl.id = OVERRIDE_STYLE_ELEMENT_ID;

  const tokens = generateColorTokens(seedColor.value);
  styleEl.textContent = [
    toCssVarsBlock(':root', getLightThemeColors(tokens) as unknown as Record<string, string>),
    toCssVarsBlock('.dark', getDarkThemeColors(tokens) as unknown as Record<string, string>),
  ].join('\n');

  if (!existing) {
    document.head.appendChild(styleEl);
  }
};

systemDarkQuery.addEventListener('change', () => {
  if (mode.value === 'system') {
    applyDarkClass();
  }
});

const setMode = (value: ThemeMode) => {
  mode.value = value;
  localStorage.setItem(MODE_STORAGE_KEY, value);
  applyDarkClass();
};

const setSeedColor = (value: string) => {
  seedColor.value = value;
  localStorage.setItem(SEED_COLOR_STORAGE_KEY, value);
  applySeedColor();
};

let initialized = false;

/** Applies the persisted theme immediately. Call once, before mount, to avoid a flash of the default theme. */
export const initTheme = () => {
  if (initialized) return;
  initialized = true;
  applyDarkClass();
  applySeedColor();
};

export const useTheme = () => ({
  mode,
  seedColor,
  defaultSeedColor: SEED_COLOR,
  setMode,
  setSeedColor,
  resetSeedColor: () => setSeedColor(SEED_COLOR),
});
