# MudShadcn reference

The details behind the [README](https://github.com/sardar97/MudShadcn#readme): the provider's
parameters, switching between looks, charts, styles and theme options, how the palette is mapped,
where MudShadcn differs from shadcn/ui, and versions. The [showcase](https://mudshadcn.pages.dev)
shows all of it running. This file is also served as
[`/llms-full.txt`](https://mudshadcn.pages.dev/llms-full.txt).

- [Set-up in short](#set-up-in-short)
- [The provider](#the-provider)
- [Using it](#using-it): [charts](#charts), [styles and colour themes](#styles-and-colour-themes),
  [customising](#customising)
- [How the palette is mapped](#how-the-palette-is-mapped)
- [Where it differs from shadcn](#where-it-differs-from-shadcn)
- [Versions](#versions)

## Set-up in short

The [README](https://github.com/sardar97/MudShadcn#set-up) has these steps with their code.

1. `dotnet add package MudShadcn`. That installs MudBlazor too, as the separate NuGet package it
   is, so MudShadcn is the only package you have to reference.
2. `builder.Services.AddMudShadcn();` in `Program.cs`. It calls MudBlazor's `AddMudServices` for
   you, sets sonner-style snackbar defaults and accepts the same configuration callback.
3. Link `_content/MudBlazor/MudBlazor.min.css`, then `_content/MudShadcn/MudShadcn.min.css`, and
   MudBlazor's script. The order matters: MudShadcn's stylesheet overrides MudBlazor rules at equal
   specificity and has to come second. You do not need MudBlazor's Roboto font link.
   `MudShadcn.min.css` is `MudShadcn.css` without comments and whitespace (about 14 KB instead of
   26 KB over Brotli); link `MudShadcn.css` instead when you want to read the rules in the
   browser's dev tools.
4. `@using MudBlazor` and `@using MudShadcn` in `_Imports.razor`.
5. One `<MudShadcnProvider />` in the layout.

## The provider

`MudShadcnProvider` replaces `MudThemeProvider`, `MudPopoverProvider`, `MudDialogProvider` and
`MudSnackbarProvider`:

```razor
<MudShadcnProvider @bind-IsDarkMode="_isDarkMode" />
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
| `Enabled` | `true` | Set `false` for MudBlazor's own look. See [Switching to MudBlazor's own look](#switching-to-mudblazors-own-look). |
| `MudBlazorTheme` | MudBlazor's default theme | The `MudTheme` used while `Enabled` is `false`. |

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
A style, a colour theme and the menu options need the provider as well (see
[Styles and colour themes](#styles-and-colour-themes)).

### Switching to MudBlazor's own look

`Enabled` turns MudShadcn off and on while the app runs, so you can let your users choose between
the shadcn look and MudBlazor's, or compare the two. Nothing else in the set-up changes.

```razor
<MudShadcnProvider Enabled="_shadcn" @bind-IsDarkMode="_isDarkMode" />

<MudSwitch @bind-Value="_shadcn" Label="shadcn look" Color="Color.Primary" />

@code {
    private bool _shadcn = true;
    private bool _isDarkMode;
}
```

With `Enabled="false"` the provider applies MudBlazor's default theme, switches MudShadcn's
stylesheet off in the page and stops emitting the `--shadcn-*` tokens: the app looks as it does
without MudShadcn. Dark mode and the three MudBlazor providers keep working. Back to `true`
restores the shadcn look at once, since the stylesheet stays loaded.

- **Your own MudBlazor theme.** Pass it as `MudBlazorTheme` and it is the one used while `Enabled`
  is `false`: `<MudShadcnProvider Enabled="_shadcn" MudBlazorTheme="_myTheme" />`.
- **Use `Enabled`, not an `@if` around two providers.** Swapping `MudShadcnProvider` for
  `MudThemeProvider` changes the colours only; the stylesheet would still apply.
- **Keep the stylesheet `<link>` from the set-up.** The provider finds it by its address
  (`_content/MudShadcn/MudShadcn.min.css` or `MudShadcn.css`). If you serve a copy from another
  address, mark it: `<link rel="stylesheet" href="..." data-mudshadcn />`. When the provider cannot
  find the link it logs a warning.
- **In your own CSS, use `--mud-palette-*`** for anything that has to work in both looks. The
  `--shadcn-*` variables do not exist while `Enabled` is `false`.
- **The switch happens when the provider becomes interactive**, like following the OS colour
  scheme. A prerendered page that starts with `Enabled="false"` shows MudShadcn's shapes for that
  moment.
- **Snackbar defaults stay.** `AddMudShadcn()` sets them once at start-up (bottom right, outlined),
  for both looks. Call MudBlazor's `AddMudServices()` instead if you want MudBlazor's defaults.
- MudBlazor's own look expects the Roboto font, which MudShadcn does not need. Add MudBlazor's
  [font link](https://mudblazor.com/getting-started/installation) if you offer both looks.

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
small squares. Bars have shadcn's rounded corners (a stacked bar at the two ends of the stack),
lines are 2px and data markers are dots in the series colour. If you set `ChartPalette` yourself,
your colours are used as given. The colours series receive are available to your CSS as
`--shadcn-chart-series-1` to `--shadcn-chart-series-20`.

Some of shadcn's look is computed into the SVG by MudBlazor, so it comes from chart options rather
than CSS: wide bars 4px apart, smooth `type="natural"` lines, a filled area, about five axis ticks
and a thicker donut ring. `MudShadcnChartOptions` returns MudBlazor's own option objects set up that
way, and `MatchBoundsToSize` keeps the axis text at 12px however wide the chart is:

```razor
<MudChart T="double" ChartType="ChartType.Bar" ChartSeries="@_series" ChartLabels="@_months"
          ChartOptions="@_options" MatchBoundsToSize="true" Height="260px" />

@code {
    private readonly BarChartOptions _options = MudShadcnChartOptions.Bar();
}
```

| Method | MudBlazor options | shadcn chart |
| --- | --- | --- |
| `Bar()` | `BarWidthRatio = 0.8`, `BarSpacingRatio = 0.08`, 5 ticks | `<Bar radius={4}>`, `barGap={4}` |
| `StackedBar()` | `BarWidthRatio = 0.8`, `ShowZeroValues = false`, 5 ticks | stacked `<Bar stackId>` |
| `Line()` | `LineStrokeWidth = 2`, `NaturalSpline`, 5 ticks | `<Line type="natural" strokeWidth={2}>` |
| `Area()` | `LineDisplayType.Area`, `LineStrokeWidth = 1`, `NaturalSpline`, 5 ticks | `<Area type="natural">` |
| `Donut()` | `DonutRingRatio = 0.4` | `<Pie innerRadius={60}>` |

Each call returns a new object, so change anything else on it as usual. The showcase's
[Charts page](https://mudshadcn.pages.dev/customization/charts) shows each preset as one of shadcn's
chart cards, with its code.

### Styles and colour themes

`MudShadcnTheme.Create` takes what [shadcn/create](https://ui.shadcn.com/create) lets you choose,
under shadcn's names. Every option has a default, so name only the ones you want:

```csharp
var theme = MudShadcnTheme.Create(new ShadcnThemeOptions
{
    Style = ShadcnStyle.Nova,             // NewYork, Vega, Nova, Maia, Lyra, Mira, Luma, Sera, Rhea
    BaseColor = ShadcnBaseColor.Zinc,     // Neutral, Stone, Zinc, Mauve, Olive, Mist, Taupe
    Theme = ShadcnColor.Blue,             // Base (the base colour itself), Amber, Blue, … Yellow
    ChartColor = ShadcnColor.Blue,        // null keeps shadcn's original five chart colours
    Radius = ShadcnRadius.Small,          // Default, None, Small, Medium, Large
    FontFamily = ["Inter", .. MudShadcnTheme.DefaultFontFamily],
    MenuAccent = ShadcnMenuAccent.Bold,   // Subtle, Bold
    MenuColor = ShadcnMenuColor.Inverted, // Default, Inverted, DefaultTranslucent, InvertedTranslucent
    IconLibrary = ShadcnIconLibrary.Lucide, // null (MudBlazor's Material icons), Lucide, Tabler, HugeIcons, Phosphor, RemixIcon
});
```

```razor
<MudShadcnProvider Theme="theme" />
```

The [Theme Generator](https://mudshadcn.pages.dev/customization/theme-generator) has every option
on a live preview and writes this code for you.

| Option | What it sets |
|---|---|
| `Style` | The shape of every component: sizes, spacing, corners, focus rings, shadows. `NewYork` is the default, and MudShadcn's look since 1.0. |
| `BaseColor` | The grey everything is drawn in: backgrounds, borders, muted text. |
| `Theme` | The colour of primary buttons, selected controls and the sidebar's active item. |
| `ChartColor` | The colour the five chart colours are shades of. |
| `Radius` | `--radius`, which every rounded corner is derived from. |
| `FontFamily`, `HeadingFontFamily` | The font stacks. Load a web font in the page, as usual. |
| `MenuAccent`, `MenuColor` | How menus, selects and autocompletes highlight an item, and whether they are dark on a light page or translucent. |
| `IconLibrary` | The icon library MudBlazor's built-in icons are drawn with: Lucide, Tabler Icons, HugeIcons, Phosphor Icons or Remix Icon. `null`, the default, leaves MudBlazor's Material icons. |

- **A style is a small stylesheet of its own.** The package ships one per style under
  `_content/MudShadcn/styles/`, and `MudShadcnProvider` links the one the theme names, after
  `MudShadcn.css`. Your page keeps its two `<link>`s and only the style in use is downloaded. The
  menu options work the same way. `NewYork` needs none.
- **An icon library redraws the icons MudBlazor's components bring themselves:** the close button
  of a dialog, an alert and a chip, the arrows of a pager, a date picker and a tree view, the icons
  of an alert and a snackbar, the data grid's filter, the stars of a rating. Some fifty Material
  icons in all are drawn with the library's icons instead. It is a stylesheet the provider links
  too (`styles/icons-lucide.css`, …), so no component and no line of your code changes. An icon is
  recognised by its shape, wherever it is: one of those Material icons in your own markup
  (`Icons.Material.Filled.Close`) is redrawn as well, in its own colour and size. Any other icon
  (`Icons.Material.Filled.Delete`) stays Material's.
- **Switching while the app runs** is passing another theme to the provider:

  ```razor
  <MudShadcnProvider Theme="_theme" />

  <MudSelect T="ShadcnStyle" Value="_theme.Style" ValueChanged="SetStyle" Label="Style" Variant="Variant.Outlined">
      @foreach (var style in Enum.GetValues<ShadcnStyle>())
      {
          <MudSelectItem Value="style">@style</MudSelectItem>
      }
  </MudSelect>

  @code {
      private ShadcnTheme _theme = MudShadcnTheme.Create();

      private void SetStyle(ShadcnStyle style) =>
          _theme = MudShadcnTheme.Create(_theme.Options with { Style = style });
  }
  ```

- **A style does not choose the font.** shadcn's presets pair some styles with one (Lyra with a mono
  font, Sera with serif headings): set `FontFamily` and `HeadingFontFamily` for that.
- **In a WebAssembly app** the provider renders after the app has started, so the style's stylesheet
  is requested then. To have it ready, preload it in the page's `<head>`:
  `<link rel="preload" as="style" href="_content/MudShadcn/styles/nova.min.css" />`.
- **If you serve the stylesheets from another address,** set the provider's `StyleHrefFormat`
  (default `_content/MudShadcn/styles/{0}.min.css`).
- **The theme options need `MudShadcnProvider`:** it emits the theme's `--shadcn-*` tokens and
  links its stylesheets. With MudBlazor's own providers you get the palette, but the tokens stay
  the default theme's and the style is not linked.

### Customising

A theme made by `MudShadcnTheme` is an ordinary `MudTheme`, so change it as usual:

```csharp
var theme = MudShadcnTheme.Create(["Geist", .. MudShadcnTheme.DefaultFontFamily]);
theme.PaletteLight.Primary = "#2563eb";
theme.LayoutProperties.DefaultBorderRadius = "0.375rem";
theme.TokensLight["ring"] = "oklch(0.623 0.214 259.815)";   // a shadcn token MudBlazor has no slot for
```

```razor
<MudShadcnProvider Theme="theme" />
```

The stylesheet reads colours from MudBlazor's `--mud-palette-*` variables, so palette changes flow
through. shadcn's own tokens are available to your CSS as `--shadcn-*` (`--shadcn-border`,
`--shadcn-muted-foreground`, `--shadcn-ring`, `--shadcn-radius-lg`, `--shadcn-shadow-md`, …) and in
C# as the theme's `TokensLight` / `TokensDark` (`ShadcnTokens.Light` / `ShadcnTokens.Dark` are the
default theme's). For a radius that is not one of shadcn's four, set the variable the scale is
derived from:

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

- Icons are MudBlazor's Material icons, not Lucide, unless the theme names an `IconLibrary`
  ("Styles and colour themes" above). Without one, the exceptions are the default checkbox, radio
  and select-arrow glyphs, which are recognised and redrawn as shadcn's controls; custom
  `CheckedIcon`/`UncheckedIcon` render as given. Other Material shapes you can swap with
  parameters: the nav group arrow (`ExpandIcon`), the breadcrumb ellipsis (`ExpanderIcon`) and
  separator (`Separator`).
- An `IconLibrary` covers the icons MudBlazor's components use by default, not the whole Material
  set: an icon you chose yourself stays Material's unless it is one of those, so a page can show
  both families. The library's check and arrow also replace the ones in the checkbox and the
  select. Lucide and Remix Icon have no plain exclamation mark, so a step with an error keeps
  Material's there.
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
- Bars are drawn as thick strokes and rounded by clipping. The radius is 4 of the chart's own
  units, like its 12px axis text: without `MatchBoundsToSize` MudBlazor draws a chart on a fixed
  650 × 350 canvas and scales it to its box, corners included; with it, a unit is a pixel.
  `--shadcn-chart-bar-radius` (on the chart's `Style`, or anywhere above it) changes the radius,
  and `0` gives square bars.
- A stacked bar is rounded at the two ends of the stack. The stylesheet finds them by the default
  palette's colours, so a palette of your own, or more than ten series, keeps square segments. It
  assumes values that are not negative (a stack below the axis is rounded at the wrong ends; set
  the radius to `0` there), and a zero value is a hairline that counts as the end of its stack
  unless `ShowZeroValues` is `false`, which `MudShadcnChartOptions.StackedBar()` sets.
- Data markers are dots with a radius of 4, whatever the line width.
- The 2px line width replaces MudBlazor's default `LineStrokeWidth` (3) only; any other width you
  set is kept.
- Labels on pie, donut and rose segments use the background colour, as in shadcn's examples, which
  is low-contrast on light segments.

**Styles and theme options**

- A style restyles the MudBlazor components that have a shadcn counterpart. shadcn components
  MudBlazor does not have (Command, Menubar, Navigation Menu, Hover Card, Input OTP, Item, Empty,
  Kbd and the chat components) have nothing to restyle, and MudBlazor components shadcn does not
  have (the time and colour pickers, the stepper, the timeline, the tree view) follow the shadcn
  component they are modelled on.
- A style does not choose the font or the icons. shadcn's presets pair Lyra with a mono font and
  Phosphor's icons, Maia and Mira with HugeIcons, and Sera with serif headings: set `FontFamily`,
  `HeadingFontFamily` and `IconLibrary` for that. `MudLink` keeps the
  size of the text around it, also in the styles whose buttons are smaller or uppercase.
- A dialog's width is MudBlazor's `MaxWidth`, not the style's. Menus, selects and autocompletes
  open in the same kind of popover, so a style gives them one shape where shadcn's differ slightly
  between a dropdown, a context menu and a select.
- The drawer's header and the sizes MudBlazor has and shadcn does not (`Margin.Dense`, `Size.Small`
  and `Size.Large` of some controls, the filled and text input variants) are stepped from the
  style's default where shadcn's file has a counterpart, and keep the New York values otherwise.
- The styles with wider cells or wide letter-spacing (Sera, Maia, Luma) need more room: a data grid
  that fits its container in New York may scroll sideways in them.
- `MenuAccent.Bold` paints the item under the pointer in the primary colour, and the selected
  option of a select with it: MudBlazor marks the selected and the keyboard-highlighted option the
  same way. In a multi-selection select that is every selected option, and its checked box, also
  primary, shows only as its check. `MenuColor.Inverted` has no effect on a dark page, where a menu is dark
  already. A translucent menu has shadcn's own highlight (the foreground at 10%), so the bold accent
  does not apply to it.

## Versions

| MudShadcn | Minimum MudBlazor | .NET |
| --- | --- | --- |
| 1.2.x | 9.12.0 | 10 (`net10.0`) |
| 1.1.x | 9.12.0 | 10 (`net10.0`) |
| 1.0.x | 9.10.0 | 10 (`net10.0`) |

### Which MudBlazor version your app uses

MudBlazor is a separate NuGet package that MudShadcn depends on. It is not inside MudShadcn, and
the version in the table is the lowest one MudShadcn accepts, not the only one.

| Your project references | MudBlazor your app gets |
| --- | --- |
| `MudShadcn` only | The minimum from the table. NuGet does not pick a newer one by itself, even when one exists. |
| `MudShadcn` and a **newer** `MudBlazor` | The version you named. NuGet lets a direct reference win, with no warning. |
| `MudShadcn` and an **older** `MudBlazor` | None: the restore stops with error `NU1605` (package downgrade). Raise MudBlazor to the minimum, or use the MudShadcn release that matches yours. |

So to take a new MudBlazor release on the day it ships, add or raise your own reference next to
MudShadcn's:

```xml
<PackageReference Include="MudShadcn" Version="..." />
<!-- Optional: only to use a MudBlazor newer than the minimum. -->
<PackageReference Include="MudBlazor" Version="..." />
```

What to expect on a MudBlazor newer than the minimum:

- **Your app keeps working.** MudShadcn does not change MudBlazor's components or their API. It
  uses a small part of that API itself (the theme, the providers, the chart options), and MudBlazor
  keeps its API stable within a major version.
- **Styling can lag for what is new.** The stylesheet matches MudBlazor's CSS class names. A
  component or variant MudBlazor adds after the minimum keeps MudBlazor's own look until the next
  MudShadcn release; everything that existed before stays styled.
- **Each MudShadcn release is tested against its minimum.** CI checks every class, variable, chart
  colour and icon the stylesheet relies on against that version.
- **A new MudBlazor major version is different.** It can rename classes and change the theme API,
  so wait for the MudShadcn major version that names it.

MudShadcn follows [Semantic Versioning](https://semver.org); moving to a new MudBlazor major version is
a new MudShadcn major version. What changed in each release is in the
[changelog](https://github.com/sardar97/MudShadcn/blob/master/CHANGELOG.md) and on the
[Releases page](https://github.com/sardar97/MudShadcn/releases).
