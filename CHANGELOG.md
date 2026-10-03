# Changelog

All notable changes to **MudShadcn** are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project
adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html). Moving to a new MudBlazor
major version is a new MudShadcn major version.

## [Unreleased]

The first public release, planned as 1.0.0. Written against MudBlazor 9.10.0, for .NET 10.

### Added
- **`MudShadcnTheme`**: shadcn/ui's default neutral theme as a `MudTheme`, light and dark. Palette,
  type scale, radius and flat elevations come from shadcn's tokens and Tailwind's scales;
  `MudShadcnTheme.Create(fontFamily)` builds it with your own font.
- **`MudShadcn.css`**: one stylesheet that restyles every MudBlazor component the way shadcn/ui draws
  it: 36px controls, bordered flat surfaces, 3px focus rings, labels above outlined inputs, no ripple
  and no uppercase buttons. Checkboxes, radios, switches, sliders and the select arrow are redrawn as
  shadcn's controls. It reads MudBlazor's palette variables, so dark mode and palette changes flow
  through.
- **`MudShadcn.min.css`**: the same stylesheet without comments and whitespace (about 14 KB instead of
  26 KB over Brotli), for production.
- **Charts in shadcn's colours** with no options: MudBlazor's default chart palette becomes shadcn's
  `chart-1` to `chart-5` (light and dark) plus tints and shades for up to 20 series, heat maps get a
  ramp of the primary colour, and axes, grid lines, legends and tooltips follow shadcn's chart.
- **`MudShadcnProvider`**: `MudThemeProvider` plus the popover, dialog and snackbar providers in one
  component. It emits the `--shadcn-*` tokens for the active mode, sets `color-scheme` on the root
  element, and starts in (and follows) the operating system's colour scheme.
- **`AddMudShadcn()`**: registers MudBlazor's services with sonner-style snackbar defaults
  (bottom-right, outlined, 150ms transitions).
- **`ShadcnTokens`**: shadcn's original OKLCH tokens, light and dark, for your own code; the same
  tokens are CSS variables (`--shadcn-border`, `--shadcn-ring`, `--shadcn-radius-lg`, …).
- MudBlazor comes along as a dependency, so MudShadcn is the only package an app references.

### Documentation
- **Showcase** at <https://mudshadcn.pages.dev>: every page and example of mudblazor.com's component
  documentation, in the same menu and order, running on MudShadcn, in light and dark mode.

[Unreleased]: https://github.com/sardar97/MudShadcn/commits/master
