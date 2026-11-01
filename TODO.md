# TODO

Open work found while moving to MudBlazor 9.12.0 and reworking the charts (v1.1.0). Highest value
first. Tick an item off (or delete it) in the same commit that does it.

Ground rules, so a fix does not break something else:

- `src/MudShadcn/wwwroot/MudShadcn.css` is the only stylesheet to edit; `MudShadcn.min.css` is
  generated on build. Never set `--mud-palette-*`, `--mud-elevation-*`, `--mud-typography-*` or
  `--mud-default-borderradius` in it (they belong in `MudShadcnTheme.cs`). Read colours from
  `--mud-palette-*`; use `--shadcn-*` only where MudBlazor has no slot.
- Look a MudBlazor rule up before overriding it, and match or beat its specificity (see "Writing
  overrides" in the project notes and step 3 of `UPDATING.md`).
- Take values from shadcn's `new-york-v4` source and Tailwind's `theme.css`, not from memory, and
  keep the Tailwind utility in a trailing comment (`/* h-9 px-4 */`).
- `samples/MudShadcn.Showcase/Pages/Components/` is a copy of MudBlazor's docs: never edit it by hand.
- After every change: `dotnet build MudShadcn.sln` (0 warnings), `tools/verify-mudblazor.sh`, and a
  look at the change in the test app (`:5152`) and the showcase (`:5153`), in light **and** dark.
- New user-facing changes go under `## [Unreleased]` in `CHANGELOG.md`.

## 1. Visual pass over the whole showcase on MudBlazor 9.12

Checked so far: every test app page in both modes, the chart pages, the divider, avatar, date range
picker and multi-select. A crawl of all 94 showcase URLs found no render errors (only blocked
third-party CSS and the image page's deliberate fallback 404). Still to do:

- [ ] Open each of the 78 component pages of the showcase in light and dark and fix what is off.
      The ones MudBlazor 9.11/9.12 touched first: Data Grid (new "is any of" enum filter operator,
      `ToolBarWrapContent`, the settings menu and column drag handles), Select (multi-select now
      draws `CheckedIcon`/`UncheckedIcon`; check custom icons too), Switch (new async example),
      Date Picker (new culture-format examples), Tree View, Nav Menu.
- [ ] Required outlined inputs: 9.12 adds a `*` to the (hidden) legend. Confirm the label above the
      field shows one asterisk and the border has no notch, also with `Margin.Dense`.

## 2. Date range picker, backwards preview (new in 9.12)

The hovered day and the first pick now swap sides when hovering a day before the first pick
(section 18 of the stylesheet). Not verified yet:

- [x] Right to left (`dir="rtl"`): checked, the band and corners mirror correctly.
- [ ] `ShowAdjacentMonthDays="false"` with the hovered day or the anchor next to a hidden day, and
      at the start/end of a week row (the "plain square" rules).
- [ ] Touch: the rules key off `:hover`. MudBlazor 9.12 also sets the preview position on click;
      check with touch emulation that the anchor still swaps sides.

## 3. Charts

The stylesheet now rounds grouped bars and draws default lines at 2px; `MudShadcnChartOptions`
supplies bar width, curves, area fill, five ticks and the donut ring.

- [ ] Showcase: add a page (Customization section, or next to the chart pages in the menu) showing
      `MudShadcnChartOptions` as shadcn's chart cards — bar, line, area, donut — with the code. The
      ported docs pages use MudBlazor's own options, so the showcase never shows the presets.
- [ ] Stacked bars: shadcn rounds only the ends of a stack (`radius={[0, 0, 4, 4]}` on the bottom
      series, `[4, 4, 0, 0]` on the top one). Find a way to round the outer segment of each stack
      (MudBlazor draws them as `.mud-charts-stackedbar-series > path`); keep the inner joins square.
- [ ] `tools/verify-mudblazor.sh` now checks the default `LineStrokeWidth` (3) the 2px line rule
      matches. Still unchecked: that grouped bars are single stroked paths in
      `.mud-charts-bar-series`, which the bar rounding (a clip to the stroke box) relies on.
- [ ] Data markers (`ShowDataMarkers`): shadcn's "Line Chart - Dots" draws filled dots in the series
      colour (`r=4`, active `r=6`); ours are rings on the surface colour.
- [ ] Bar corner radius is 4 SVG units, so it scales on a chart without `MatchBoundsToSize`.
      Consider a radius that stays 4px (e.g. computed from the SVG's scale), or document it better.

## 4. Data grid layout in the test app

- [ ] At 1280px the test app's data grid is wider than its card: the Amount column is cut off and
      the grid scrolls sideways. Compare header and cell padding, the filter icon buttons and the
      sort icons with shadcn's Data Table (`h-10 px-2` header, `p-2` cells) and tighten what is
      MudBlazor-sized.

## 5. Tooling and CI

- [ ] Add a browser smoke test to CI: publish the showcase, serve it, load every URL in
      `wwwroot/sitemap.xml` with Playwright and fail on a page error or a visible
      `#blazor-error-ui`. (A local version of this crawl took about two minutes for 94 pages.)
- [ ] Test the real package before a release: `dotnet pack src/MudShadcn -c Release -o <tmp>/feed`,
      point a copy of the test app at it through a local `nuget.config`, and check that
      `_content/MudBlazor/MudBlazor.min.css` and `_content/MudShadcn/MudShadcn.min.css` are served.
