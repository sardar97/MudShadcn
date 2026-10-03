# MudShadcn

A [shadcn/ui](https://ui.shadcn.com) look and feel for [MudBlazor](https://mudblazor.com): neutral
palette, flat bordered surfaces, shadcn's radius scale, 3px focus rings, labels above inputs, no
ripple, no uppercase buttons.

MudShadcn is a theme and a stylesheet, nothing more. Every MudBlazor component is still the
original MudBlazor component with its full API. MudBlazor comes along as a dependency, so
**MudShadcn is the only package you reference**.

| | |
|---|---|
| Target framework | .NET 10 (`net10.0`) |
| MudBlazor | 9.10.0 |

## Install

```
dotnet add package MudShadcn
```

Do not add a separate `MudBlazor` reference. If you already have one, remove it so the version
MudShadcn was written against is the one that gets used.

## Set up

**1. `Program.cs`** — register services. `AddMudShadcn` calls MudBlazor's `AddMudServices` for you
and sets sonner-style snackbar defaults. It accepts the same configuration callback.

```csharp
using MudShadcn;

builder.Services.AddMudShadcn();
```

**2. `App.razor`** — link both stylesheets, MudBlazor's first, and MudBlazor's script.

```html
<link rel="stylesheet" href="@Assets["_content/MudBlazor/MudBlazor.min.css"]" />
<link rel="stylesheet" href="@Assets["_content/MudShadcn/MudShadcn.css"]" />
...
<script src="@Assets["_content/MudBlazor/MudBlazor.min.js"]"></script>
```

The order matters: `MudShadcn.css` overrides MudBlazor rules at equal specificity and has to come
second. You do not need MudBlazor's Roboto font link.

**3. `_Imports.razor`**

```razor
@using MudBlazor
@using MudShadcn
```

**4. Your layout** — one provider replaces `MudThemeProvider`, `MudPopoverProvider`,
`MudDialogProvider` and `MudSnackbarProvider`.

```razor
<MudShadcnProvider @bind-IsDarkMode="_isDarkMode" />

<MudLayout>
    ...
</MudLayout>

@code {
    private bool _isDarkMode;
}
```

The provider has to be rendered interactively, like MudBlazor's own providers. Besides the theme it
emits the `--shadcn-*` tokens for the active mode and sets `color-scheme` on the root element, as
shadcn's `next-themes` does, so native scrollbars, form controls and CSS `light-dark()` follow the
mode too.

| Parameter | Default | |
|---|---|---|
| `Theme` | `MudShadcnTheme.Default` | Any `MudTheme`. |
| `IsDarkMode` | `false` | Bindable. |
| `FollowSystemDarkMode` | `true` | Start in the OS colour scheme and follow changes to it. |
| `IncludeProviders` | `true` | Set `false` if you place the popover, dialog and snackbar providers yourself. |

### Without the provider

`MudShadcnProvider` is a convenience. This works too:

```razor
<MudThemeProvider Theme="MudShadcnTheme.Default" @bind-IsDarkMode="_isDarkMode" />
<MudPopoverProvider />
<MudDialogProvider />
<MudSnackbarProvider />
```

You lose one thing: the provider is what re-emits the `--shadcn-*` tokens for dark mode. Without it
the focus ring, input background and destructive button keep their light-mode values in dark mode.

## Using it

Write ordinary MudBlazor. The mapping to shadcn's vocabulary:

| shadcn | MudBlazor |
|---|---|
| Button `default` | `Variant.Filled` `Color.Primary` |
| Button `secondary` | `Variant.Filled` `Color.Secondary` (or `Color.Default`) |
| Button `destructive` | `Variant.Filled` `Color.Error` |
| Button `outline` / `ghost` | `Variant.Outlined` / `Variant.Text` |
| Input, Select, Textarea | `Variant="Variant.Outlined"` — 36px; `Margin.Dense` gives 32px |
| Badge | `MudChip` |
| Tabs | `MudTabs` with `MinimumTabWidth="0px"` for content-width tabs |
| Sheet / Sidebar | `MudDrawer` temporary / persistent |
| Sonner | `ISnackbar` |
| Accordion | `MudExpansionPanels` |
| Chart | `MudChart` / the chart components, with the default palette |
| muted text | `Color.Tertiary` on `MudText`, or `Typo.body2` with `Color.Tertiary` |

`Variant.Outlined` is the shadcn input: a bordered 36px box with its label above it. `Filled` is
a borderless `bg-muted` box and `Text` a single underline; both keep MudBlazor's floating label,
since shadcn has no such variants. Checkboxes, radios, switches, sliders and the select's dropdown
arrow are redrawn as shadcn's controls, whatever the variant.

### Charts

Charts need no options. MudBlazor's default chart palette is replaced with shadcn's `chart-1` to
`chart-5` (each with its own light and dark value), followed by tints and shades of them for series
6 to 20; heat maps get a ramp of the primary colour. Axis labels are muted, grid lines take the
border colour, tooltips and Sankey labels look like shadcn's chart tooltip, and legend markers are
small squares. If you set `ChartPalette` yourself, your colours are used as given. The colours
series receive are available to your CSS as `--shadcn-chart-series-1` to `--shadcn-chart-series-20`.

### Customising

```csharp
var theme = MudShadcnTheme.Create(["Geist", .. MudShadcnTheme.DefaultFontFamily]);
theme.PaletteLight.Primary = "#2563eb";
theme.LayoutProperties.DefaultBorderRadius = "0.375rem";
```

```razor
<MudShadcnProvider Theme="theme" />
```

The stylesheet reads colours from MudBlazor's `--mud-palette-*` variables, so palette changes flow
through. shadcn's own tokens are available to your CSS as `--shadcn-*` (`--shadcn-border`,
`--shadcn-muted-foreground`, `--shadcn-ring`, `--shadcn-radius-lg`, `--shadcn-shadow-md`, …) and as
`ShadcnTokens.Light` / `ShadcnTokens.Dark` in C#. To change the radius scale:

```css
:root { --shadcn-radius: 0.5rem; }
```

## How the palette is mapped

| shadcn token | MudBlazor palette |
|---|---|
| `background` / `foreground` | `Background` / `TextPrimary` |
| `card`, `popover` | `Surface` |
| `primary` / `primary-foreground` | `Primary` / `PrimaryContrastText` |
| `secondary` / `secondary-foreground` | `Secondary` / `SecondaryContrastText` |
| `muted` / `muted-foreground` | `BackgroundGray` / `TextSecondary`, `Tertiary` |
| `accent` | hover surfaces (in the stylesheet) |
| `destructive` | `Error` |
| `border` / `input` | `LinesDefault`, `Divider`, `TableLines` / `LinesInputs` |
| `sidebar*` | `DrawerBackground`, `DrawerText` |
| `ring` | no MudBlazor slot — `--shadcn-ring` |

`Secondary` is shadcn's light grey surface, not an accent colour. Where MudBlazor uses it as a
foreground (text buttons, icons), the stylesheet substitutes `secondary-foreground` so it stays
legible. shadcn has no info, success or warning; those come from Tailwind's blue, green and amber.

## Where it differs from shadcn

MudShadcn restyles MudBlazor; it does not change what MudBlazor renders. Where the markup, an
inline style or a component parameter decides something, MudBlazor's version stays.

**General**

- Icons are MudBlazor's Material icons, not Lucide. The exceptions are the default checkbox,
  radio and select-arrow glyphs, which are recognised and redrawn as shadcn's controls; custom
  `CheckedIcon`/`UncheckedIcon` render as given. Other Material shapes you can swap with
  parameters: the nav group arrow (`ExpandIcon`), the breadcrumb ellipsis (`ExpanderIcon`) and
  separator (`Separator`).
- `Color.Secondary` is shadcn's light grey surface, not an accent. Where it would be a foreground
  (text, icons, a checked control) the stylesheet uses `secondary-foreground` instead.
- `Color.Dark` is near-black (`#171717`) in light mode and a mid grey (`#737373`) in dark mode: the
  darkest grey that still reads as a text or icon colour on the dark background and behind white
  text. Use `Color.Default` (or no colour) for text that should simply follow the mode.
- shadcn defines no info, success or warning; they come from Tailwind's blue, green and amber, and
  components with a semantic colour keep it (a ghost button with `Color.Info` has blue text).
- `<mark>` (what `MudHighlighter` renders) gets a soft warning tint everywhere on the page.
- The destructive reds are just outside sRGB; the `MudTheme` holds the gamut-clipped hex
  (`#e7000b`, `#ff6467`), which is what an sRGB display shows for the OKLCH original anyway.
- The default font is the system UI stack. To use Geist or Inter, load the font yourself and pass
  it to `MudShadcnTheme.Create(...)`.

**Inputs and pickers**

- **Floating labels are gone** from `Variant.Outlined`: the label is pinned above the field and
  the notch is closed, so a labelled outlined input takes 22px more vertical space than in stock
  MudBlazor. `Text` and `Filled` keep a floating label; shadcn has no such variants.
- `MudSlider` draws a range only with `Variant.Filled`. Its track uses `input` rather than
  `muted` so it stays visible on cards.
- `UncheckedColor` on checkboxes and radios is not shown: unchecked is always the input border.
- The calendar keeps MudBlazor's 40px cell pitch, so days are 32px squares with gaps rather than
  touching cells, and the panel is about 310px wide (shadcn's is about 250px). It always shows six
  weeks unless adjacent-month days are hidden. The toolbar above it has no shadcn counterpart
  (`ShowToolbar="false"` gives the bare calendar), and the month caption opens month and year
  grids rather than dropdowns. Static pickers are always rounded.
- The time picker and colour picker have no shadcn counterpart; they are dressed in its style.
  The clock face and colour field keep MudBlazor's fixed sizes.

**Buttons**

- Icons inside a `MudButton` are always `size-4`; `IconSize` has no visible effect.
- FABs have no shadcn counterpart: they get the button variants on a round button and keep
  MudBlazor's sizes.
- In a toggle group with a semantic colour, the selected item is filled with that colour rather
  than `bg-accent`.
- A disabled `MudLink` keeps MudBlazor's disabled colour (set with `!important`), not 50% opacity.

**Data**

- Row selection is highlighted from the selection checkbox; single-select tables without one need
  `RowClassFunc` to show the selected row.
- The data grid's group header text is bold (an inline style), and its loading bar takes
  `LoadingProgressColor` (default `Color.Info`).
- Pagination previous/next are icon-only buttons, and `Rectangular` looks like the default, since
  both are `rounded-md`.

**Layout, navigation and feedback**

- `MudTabs`' `Color` tints only the active label; the tab list stays neutral.
- Expansion panels are always flat: `Elevation` and `Square` have no visible effect.
- The carousel's arrows sit inside the slide at the 75% opacity MudBlazor sets inline. Only the
  default bullet icons become dots.
- `MudAlert`'s default `Variant.Text` is a tinted alert; `Variant.Outlined` is shadcn's plain
  Alert. Alerts and snackbars have no separate title and description.
- `MudAvatarGroup`'s overlap comes from its `Spacing` parameter, and its ring from `OutlineColor`.
- A selected `MudChip` swaps variant as MudBlazor does; shadcn's badge is not selectable.

**Charts**

- The default palette is recognised by value: a chart is restyled when it uses MudBlazor's default
  colours (a palette of your own that happens to start with `#2979FF` gets that one colour mapped).
- Tooltips are MudBlazor's SVG title and subtitle in shadcn's tooltip box: there is no colour
  indicator, no row per series and no cursor band.
- Bars are drawn as thick strokes, so their corners stay square. Line width, fill opacity and
  marker size are `ChartOptions` parameters (MudBlazor's lines are 3px; shadcn's are 2px).
- Labels on pie, donut and rose segments use the background colour, as in shadcn's examples, which
  is low-contrast on light segments.
