/**
 * Derives the app's full Material 3 color palette from a single seed color
 * using Google's own color-science library, so tailwind.config.ts and the
 * generated `md-*` Tailwind utilities always agree with each other.
 *
 * Uses the "Tonal Spot" scheme variant (low-to-medium colorfulness, the
 * default Material You look on Android) at standard contrast.
 */
import {
    Hct,
    SchemeTonalSpot,
    MaterialDynamicColors,
    argbFromHex,
    hexFromArgb,
    type DynamicScheme,
} from '@material/material-color-utilities';
import type { ColorTokens } from 'tailwind-material-3';

export const SEED_COLOR = '#D81B7A';

const dynamicColors = new MaterialDynamicColors();
const seed = Hct.fromInt(argbFromHex(SEED_COLOR));
const lightScheme = new SchemeTonalSpot(seed, false, 0);
const darkScheme = new SchemeTonalSpot(seed, true, 0);

type DynamicColorLike = { getArgb(scheme: DynamicScheme): number };

const role = (pick: (colors: MaterialDynamicColors) => DynamicColorLike) => ({
    light: hexFromArgb(pick(dynamicColors).getArgb(lightScheme)),
    dark: hexFromArgb(pick(dynamicColors).getArgb(darkScheme)),
});

export const colorTokens: ColorTokens = {
    primary: role((c) => c.primary()),
    onPrimary: role((c) => c.onPrimary()),
    primaryContainer: role((c) => c.primaryContainer()),
    onPrimaryContainer: role((c) => c.onPrimaryContainer()),
    inversePrimary: role((c) => c.inversePrimary()),
    secondary: role((c) => c.secondary()),
    onSecondary: role((c) => c.onSecondary()),
    secondaryContainer: role((c) => c.secondaryContainer()),
    onSecondaryContainer: role((c) => c.onSecondaryContainer()),
    tertiary: role((c) => c.tertiary()),
    onTertiary: role((c) => c.onTertiary()),
    tertiaryContainer: role((c) => c.tertiaryContainer()),
    onTertiaryContainer: role((c) => c.onTertiaryContainer()),
    error: role((c) => c.error()),
    onError: role((c) => c.onError()),
    errorContainer: role((c) => c.errorContainer()),
    onErrorContainer: role((c) => c.onErrorContainer()),
    background: role((c) => c.background()),
    onBackground: role((c) => c.onBackground()),
    surface: role((c) => c.surface()),
    onSurface: role((c) => c.onSurface()),
    surfaceVariant: role((c) => c.surfaceVariant()),
    onSurfaceVariant: role((c) => c.onSurfaceVariant()),
    surfaceDim: role((c) => c.surfaceDim()),
    surfaceBright: role((c) => c.surfaceBright()),
    surfaceContainerLowest: role((c) => c.surfaceContainerLowest()),
    surfaceContainerLow: role((c) => c.surfaceContainerLow()),
    surfaceContainer: role((c) => c.surfaceContainer()),
    surfaceContainerHigh: role((c) => c.surfaceContainerHigh()),
    surfaceContainerHighest: role((c) => c.surfaceContainerHighest()),
    inverseSurface: role((c) => c.inverseSurface()),
    inverseOnSurface: role((c) => c.inverseOnSurface()),
    outline: role((c) => c.outline()),
    outlineVariant: role((c) => c.outlineVariant()),
    shadow: role((c) => c.shadow()),
    scrim: role((c) => c.scrim()),
};
