# Changelog

All notable changes to **MudShadcn** are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project
adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html). Moving to a new MudBlazor
major version is a new MudShadcn major version.

## [Unreleased]

### Changed
- **A shorter README:** what MudShadcn is, the set-up, the theme options in short and where to read
  more, with a clip of the Theme Generator. The details it had (the provider's parameters, switching
  looks, charts, the palette mapping, where MudShadcn differs from shadcn/ui, versions) are now in
  [`docs/reference.md`](https://github.com/sardar97/MudShadcn/blob/master/docs/reference.md), which
  is also what `/llms-full.txt` serves.

## [1.2.0] — 2026-11-11

Written against MudBlazor 9.12.0, for .NET 10.

### Added
- **shadcn's styles:** the eight styles of [shadcn/create](https://ui.shadcn.com/create), under
  shadcn's names, next to the original look: `ShadcnThemeOptions.Style` is `NewYork` (the default,
  MudShadcn's look so far), `Vega`, `Nova`, `Maia`, `Lyra`, `Mira`, `Luma`, `Sera` or `Rhea`. A
  style changes the shape of every component (sizes, spacing, corners, focus rings, shadows) and is
  a small stylesheet of its own in the package (`_content/MudShadcn/styles/<name>.min.css`) that
  `MudShadcnProvider` links after `MudShadcn.css` when the theme names it. The page's set-up does
  not change, only the style in use is downloaded, and passing the provider another theme switches
  the style while the app runs. `StyleHrefFormat` on the provider is for stylesheets served from
  another address.
- **shadcn's menu options:** `ShadcnThemeOptions.MenuAccent` (`Bold` paints the item under the
  pointer in menus, selects and autocompletes in the primary colour) and `MenuColor` (`Inverted`
  makes them dark on a light page, `DefaultTranslucent` and `InvertedTranslucent` let the page show
  through, blurred). They are stylesheets the provider links too.
- **shadcn's icon libraries:** `ShadcnThemeOptions.IconLibrary` draws the icons MudBlazor's
  components use by default with `Lucide`, `Tabler`, `HugeIcons`, `Phosphor` or `RemixIcon`, the
  five of [shadcn/create](https://ui.shadcn.com/create), instead of Material: the close button of
  a dialog, an alert and a chip, the arrows of a pager, a date picker and a tree view, the icons of
  an alert and a snackbar, the data grid's filter, the stars of a rating (some fifty icons). A
  library is a stylesheet the provider links (`_content/MudShadcn/styles/icons-<name>.min.css`),
  so no component and no markup changes. It recognises a Material icon by its shape, so the same
  icon in your own markup is redrawn too; any other icon stays as it is. `null`, the default,
  leaves MudBlazor's icons, so an existing app looks the same. The check, the dash and the select
  arrow that `MudShadcn.css` draws itself are now the variables `--shadcn-icon-check`, `-minus`,
  `-chevron-down` and `-chevron-up`, which a library sets.
- **shadcn's colour themes:** `MudShadcnTheme.Create(new ShadcnThemeOptions { … })` makes a theme
  from what [shadcn/create](https://ui.shadcn.com/create) lets you choose, under shadcn's names: a
  `BaseColor` (Neutral, Stone, Zinc, Mauve, Olive, Mist, Taupe), a `Theme` colour for primary
  buttons and selected controls (Amber to Yellow, 17 of them), a `ChartColor`, a `Radius` (None,
  Small, Medium, Large), a font and a heading font. The palettes are computed from shadcn's OKLCH
  tokens, and the tokens MudBlazor has no slot for (ring, input, sidebar, charts) follow the theme
  too: the provider emits the `--shadcn-*` set of the theme it is given. The values come from
  shadcn's registry and Tailwind's palette through `tools/generate-shadcn-themes.mjs`.
- **`ShadcnTheme`:** the themes `MudShadcnTheme` makes are a `MudTheme` subclass that carries the
  shadcn tokens (`TokensLight`, `TokensDark`), the `Radius` and the `Style`. Change a token there
  the way you change a palette colour.
- **A switch back to MudBlazor's own look:** `MudShadcnProvider` has an `Enabled` parameter. Set it
  to `false` while the app runs and the provider applies MudBlazor's default theme (or the one
  passed as the new `MudBlazorTheme` parameter), switches MudShadcn's stylesheet off in the page
  and stops emitting the `--shadcn-*` tokens; `true` restores the shadcn look. The set-up does not
  change: the stylesheet `<link>` stays where it is. A stylesheet served from another address
  needs `data-mudshadcn` on its link.
- **Showcase:** the pages and tabs that are new in a version have a "New" badge in the navigation
  (the Theme Generator, Use Cases and the Charts page in this one). The badge goes away by itself
  with the version after.
- **Showcase:** a Theme Generator page (`/customization/theme-generator`, a tab in the top bar) with
  shadcn/create's options (preset, style, base colour, theme, chart colour, heading font, font,
  icon library, radius, menu and menu accent), shuffle and reset, a live preview of stock MudBlazor components,
  the code to paste, and a link that opens the site in the same design. A choice applies to the
  whole site and is remembered, the Theme page prints the active theme's tokens, and the font list
  has shadcn's 24 fonts, loaded from Google Fonts only when chosen.
- **Showcase:** a Use Cases page (under Getting Started) with the set-up for each situation: a new
  app, an app that already uses MudBlazor, a newer or older MudBlazor version, the look switch,
  your own providers and each hosting model.
- **Showcase:** a dropdown in the top bar switches every page between the shadcn look and
  MudBlazor's own, to compare them, and the package version is shown next to the name (it opens
  the release notes).
- **Showcase:** a button in the top bar switches the whole site between left-to-right and
  right-to-left, as mudblazor.com's does, so every component and example can be seen in an RTL
  layout (remembered, and `?dir=rtl` in the address opens a page that way). A new RTL Languages
  page (under Customization) shows the set-up: `MudShadcnProvider` inside a `MudRTLProvider`.
- **Stacked bar charts** have shadcn's rounded stacks: the two ends of each stack are rounded and
  the joins between its segments stay square. The stylesheet tells the series apart by MudBlazor's
  default chart palette, so a palette of your own (or a stack of more than ten series) keeps
  square segments, and it assumes values that are not negative.
- **`--shadcn-chart-bar-radius`** sets the corner radius of chart bars (4px by default) from your
  own CSS or a chart's `Style`; `0` gives square bars.
- **Showcase:** an Icons page (under Customization) explains the icon libraries: the one line that
  sets one, the components whose icons change by themselves, what happens to the icons in your own
  markup, how to use any other icon of a library, and the list of every icon a library redraws,
  with Material's drawing next to it. Its buttons switch the library for the whole site.
- **Showcase:** a Charts page (under Customization) shows `MudShadcnChartOptions` as shadcn's chart
  cards (bar, stacked bar, line, line with dots, area and donut) with the code of each.

### Changed
- **`MudShadcnTheme.Create()`, `Create(string[])` and `Default` return `ShadcnTheme`** instead of
  `MudTheme`. It is a `MudTheme`, so code that assigns or passes the result keeps working; code
  compiled against an earlier MudShadcn has to be rebuilt.
- **Documentation on MudBlazor versions:** the README, the Installation page and the agent skill now
  say plainly that MudBlazor is a separate package NuGet installs alongside MudShadcn, and that the
  MudBlazor version MudShadcn names is a minimum, not a lock. They no longer tell you to remove
  your own `MudBlazor` reference: referencing a newer MudBlazor yourself is how you move ahead of
  MudShadcn. The package's dependency itself has not changed.
- **Chart data markers** (`ShowDataMarkers`) are dots filled with the series colour, as in shadcn's
  "Line Chart - Dots", instead of rings on the surface colour.
- **`MudShadcnChartOptions.StackedBar()`** sets `ShowZeroValues = false`: a zero value draws
  nothing, where MudBlazor's default is a hairline segment that would end up as the rounded end
  of its stack.
- **Data grid:** the sort, filter and column-options buttons in a column header are 24px instead of
  28px and the sort order number sits between their icons, so a column is about 20px narrower
  and a grid with several sortable, filterable columns fits where it used to scroll sideways. In
  the filter row the field keeps a usable width next to its two buttons, and the fields of the
  filter panel are 8px apart.

### Fixed
- **Numeric field:** the spin buttons are inside the field again. Since 1.1.1 the lower one hung
  below it.
- **Date range picker**, while the second date is being picked: in a picker that shows several
  months, the first pick now turns into the end of the range when the pointer is on a day in an
  earlier month, and the previewed band ends rounded next to the hidden days of the adjacent month.
  The first pick also follows a finger resting on a day on a touch screen, and keeps its rounded
  corners at the end of a week row while the pointer is still on it.
- **Charts in Firefox:** the legend no longer hangs out of the chart's box and over whatever
  follows it. MudBlazor relies on `min-height: fit-content` to let the box grow by the legend,
  which Firefox does not apply; the box now grows there too, in every legend position.
- **Card and dialog actions** wrap onto a second row on a narrow card instead of sticking out of
  it (buttons keep their size since 1.1.1). Actions pushed to the end with a `MudSpacer`, such as
  the stepper's Next, stay at the end on the row they wrap to.
- **Chip in a button group:** the chip's close button keeps its chip size, icon size, spacing and
  round shape instead of taking the group's 36px icon-button size and joined corners.
- **Showcase on phones:** the example frame has 8px of side padding instead of 24px, so a static
  picker fits a 360px screen; the chart examples' card is styled as on mudblazor.com, so a Sankey
  chart scrolls inside it instead of sticking out; the previous/next page buttons wrap; and the
  few examples laid out for a desktop keep their width and scroll inside the frame instead of
  piling their controls on top of each other.

## [1.1.1] — 2026-11-08

Written against MudBlazor 9.12.0, for .NET 10.

### Fixed
- **Buttons** no longer shrink in a crowded flex row and cut off their label: like shadcn's
  (`shrink-0`), a button keeps its size and the row has to make room.
- **Toggle group:** a label longer than its equal-width column wraps instead of being cut off.
- **Color picker:** squeezed to MudBlazor's 310px minimum (a card on a phone), the inputs are no
  longer clipped; the inset around the colour field and the controls' extra width give way.
- **Showcase on phones:** an example wider than the screen scrolls inside its frame instead of
  widening the page; the code toolbar and file tabs wrap, and long code lines scroll.

## [1.1.0] — 2026-11-01

Written against MudBlazor 9.12.0, for .NET 10.

### Changed
- **MudBlazor 9.12.0** (was 9.10.0). The showcase is re-ported from MudBlazor 9.12.0's docs, with
  its new examples (divider with content, avatar text variant, culture formats, async switch).
- Built with the .NET 10.0.403 SDK; the showcase runs on .NET 10.0.14 and FluentValidation 12.2.0.

### Added
- **`MudShadcnChartOptions`**: `Bar()`, `StackedBar()`, `Line()`, `Area()` and `Donut()` return
  MudBlazor's chart options set up like shadcn's charts, for what only options can change: wide bars
  4px apart, smooth lines, a filled area, about five axis ticks and a thicker donut ring.
- **Charts closer to shadcn's** with no options: bars have rounded corners (`radius={4}`) and lines
  are 2px instead of MudBlazor's 3px.
- **Divider with content** (`<MudDivider>or</MudDivider>`, new in MudBlazor 9.12): the label is
  shadcn's separator text, `text-sm text-muted-foreground px-2`, horizontal and vertical.

### Fixed
- **Autocomplete:** the chevron sat 8px further right than the select's; icon buttons at the end of
  an input (password toggle, clear, picker icons) no longer hang over the border.
- **Date range picker:** hovering a day before the first pick (MudBlazor 9.12 previews backwards
  ranges) drew the hovered day as the end of the range and the first pick as its start, with a gap
  and the band on the wrong side. They now swap sides, also right to left and at row edges.

## [1.0.0] — 2026-10-03

The first public release. Written against MudBlazor 9.10.0, for .NET 10.

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

[Unreleased]: https://github.com/sardar97/MudShadcn/compare/v1.2.0...HEAD
[1.2.0]: https://github.com/sardar97/MudShadcn/compare/v1.1.1...v1.2.0
[1.1.1]: https://github.com/sardar97/MudShadcn/compare/v1.1.0...v1.1.1
[1.1.0]: https://github.com/sardar97/MudShadcn/compare/v1.0.0...v1.1.0
[1.0.0]: https://github.com/sardar97/MudShadcn/releases/tag/v1.0.0
