// tailwind-material-3 still requires a JS/TS config file even under Tailwind
// v4's CSS-first setup (see its README) — loaded via `@config` in main.css.
import { createMaterialConfig, getLightThemeColors, getDarkThemeColors } from 'tailwind-material-3';
import plugin from 'tailwindcss/plugin';
import type { Config } from 'tailwindcss';
import { colorTokens } from './scripts/generate-material-theme';

// `createMaterialConfig` shallow-spreads whatever config we pass over its
// own defaults (`{ ...defaults, ...userConfig }`), so passing a `theme` or
// `plugins` key here would silently wipe out its typography/shape/elevation/
// component plugins instead of merging with them. Call it with no
// theme/plugin overrides first, then layer our seeded palette on top of the
// result ourselves below.
const base = createMaterialConfig({
    content: ['./index.html', './src/**/*.{vue,js,ts,jsx,tsx}'],
});

// `getLightThemeColors`/`getDarkThemeColors` are tailwind-material-3's own
// token->utility mapping functions, fed with our D81B7A-seeded ColorTokens
// (see scripts/generate-material-theme.ts) — this guarantees the CSS
// variable names line up exactly with the `md-*` classes its component
// plugins already emit (e.g. `bg-md-primary`), without us hand-deriving
// kebab-case key names ourselves.
const lightColors = getLightThemeColors(colorTokens);
const darkColors = getDarkThemeColors(colorTokens);

const toCssVars = (colors: Record<string, string>) => Object.fromEntries(Object.entries(colors).map(([key, hex]) => [`--${key}`, hex]));

// tailwind-material-3 declares `darkMode: "class"` but never actually wires
// up dark-mode colors itself (createMaterialConfig only ever bakes in the
// light hex values). Fix that here: point every `md-*` utility at a CSS
// variable, then swap that variable's value under `:root` vs `.dark`.
const seededColors = Object.fromEntries(Object.keys(lightColors).map((key) => [key, `var(--${key})`]));

const materialThemeVarsPlugin = plugin(({ addBase }) => {
    addBase({
        ':root': toCssVars(lightColors),
        '.dark': toCssVars(darkColors),
    });
});

const config: Config = {
    ...base,
    theme: {
        ...base.theme,
        extend: {
            ...base.theme!.extend,
            colors: seededColors,
        },
    },
    plugins: [...(base.plugins ?? []), materialThemeVarsPlugin],
};

export default config;
