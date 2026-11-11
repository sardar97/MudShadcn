# Updating MudShadcn to a new MudBlazor version

Instructions for Claude Code when asked to "update MudShadcn to the latest MudBlazor".
Never run `git commit`. Leave the changes in the working tree for review.

## What can break

MudShadcn depends on three things MudBlazor does not treat as public API, in order of risk:

1. **CSS class names and selector specificity.** `MudShadcn.css`, and each style's stylesheet in
   `wwwroot/styles`, override MudBlazor rules by matching their selectors. If MudBlazor renames a class, restructures a component's markup, or
   raises a rule's specificity, the override silently stops applying. Nothing fails to build.
   This is the main thing to check every time.
2. **CSS custom property names** (`--mud-palette-*`, `--mud-default-borderradius`,
   `--mud-elevation-*`, `--mud-typography-*`, `--mud-ripple-*`). The stylesheet reads these. A
   renamed variable resolves to nothing, also silently.
3. **The theme API** (`MudTheme`, `PaletteLight`/`PaletteDark`, the `*Typography` classes,
   `LayoutProperties`, `Shadow`, `MudThemeProvider` parameters). Changes here fail the build, so
   they are the easy ones.

## Procedure

### 1. Find the versions

```bash
# latest stable MudBlazor
curl -s https://api.nuget.org/v3-flatcontainer/mudblazor/index.json \
  | python3 -c "import json,sys; print([v for v in json.load(sys.stdin)['versions'] if '-' not in v][-1])"

# current
grep MudBlazor src/MudShadcn/MudShadcn.csproj
```

Confirm the new version still ships a `lib/` folder for MudShadcn's target framework (look at the
`<group targetFramework>` entries in
`https://api.nuget.org/v3-flatcontainer/mudblazor/<version>/mudblazor.nuspec`). If MudBlazor has
dropped it, or a newer .NET LTS is out, raise the question of retargeting with the user rather
than deciding alone. Check the .NET side at
`https://dotnetcli.blob.core.windows.net/dotnet/release-metadata/releases-index.json`.

### 2. Read the release notes

Read every release between the current and the new version at
<https://github.com/MudBlazor/MudBlazor/releases>. For a major version also read the migration
guide linked from the release. Note anything touching theming, palette, typography, CSS variables,
and the components styled in `MudShadcn.css`: button, input, select, checkbox, radio, switch,
slider, popover, menu, list, tooltip, dialog, tabs, table, data grid, chip, nav menu, drawer,
app bar, alert, snackbar, progress, skeleton, pickers, expansion panel, card, paper — and since
MudShadcn styles every component, anything else the release notes mention as visual.

### 3. Check the stylesheet against the new version — do not skip this

Release notes rarely mention CSS changes, and nothing fails to build when an override stops
matching. Start with the script, which checks everything that can be checked mechanically:

```bash
tools/verify-mudblazor.sh $NEW
```

It downloads the new MudBlazor and checks that:

- every `--mud-*` variable and `.mud-*` class `MudShadcn.css` uses exists in MudBlazor's
  stylesheet, and the same for every stylesheet in `wwwroot/styles` (the styles and the menu
  options), each reported on a line of its own. Classes MudBlazor renders without styling them are listed in the script
  (`MARKUP_ONLY_CLASSES`); a new one must be confirmed in the rendered HTML before it goes on that
  list (`curl -s http://localhost:5152/pickers | grep -o mud-picker-paper` for the test app; the
  showcase renders in the browser, so use the browser's dev tools there);
- MudBlazor's default chart palette and the five heat-map shades built from it are still the colours
  the charts section recolours (they are matched by attribute value, not class);
- the default checkbox, radio and select/autocomplete icons still have the SVG paths the inputs
  sections match (`path[d^="…"]`) to redraw them as shadcn's controls. If they changed, the
  Material glyphs come back.
- every icon library's stylesheet (`wwwroot/styles/icons-*.css`) still matches the Material icons
  it names: each selector's path data is that of MudBlazor's icon, and no other icon of MudBlazor's
  has the same paths. If an icon was redrawn, run `node tools/generate-shadcn-icons.mjs` after the
  bump (step 4), which reads the icons of the version in the project file.
- MudBlazor's default `LineStrokeWidth` is still the value the chart section's 2px line rule
  matches (`path.mud-chart-line[stroke-width="3"]`).
- a bar, a stacked bar and a line chart still render the SVG the chart rules are written for. The
  script renders them to a string: a bar is one stroked `path.mud-chart-bar`, stacked segments
  have `fill="none"` and come stack after stack in series order (the rounding of a stack's two
  ends depends on that order and on the palette's order), and a data marker is a
  `circle.mud-chart-point` without a `fill` attribute.

The script cannot tell which icons MudBlazor's components use by default. List them from the
new version's source (in the scratchpad) and compare with the `ICONS` table in
`tools/generate-shadcn-icons.mjs`: an icon that is new gets a row there, one that is gone loses its
name.

```bash
git clone -q --depth 1 --branch v$NEW --filter=blob:none --sparse https://github.com/MudBlazor/MudBlazor.git mud-src
git -C mud-src sparse-checkout set src/MudBlazor/Components src/MudBlazor/Base
grep -rhoE 'Icons\.(Material|Custom)\.[A-Za-z]+\.[A-Za-z0-9_]+' mud-src/src/MudBlazor/Components mud-src/src/MudBlazor/Base | sort -u
```

It reads the defaults, and renders the charts, through MudBlazor's API with a small C# probe. If that no longer compiles, the
API changed: update the probe in the script as well.

A missing class or variable is an override that no longer applies: find its replacement in the new
stylesheet and update the selector. A selector that changes in `MudShadcn.css` almost always changes
in the styles that override the same rule: search `wwwroot/styles` for it and change them together. Existence is not enough, though. Diff the shipped rules too,
working in the scratchpad, not the repo:

```bash
OLD=9.12.0; NEW=<new version>
for v in $OLD $NEW; do
  curl -sL https://api.nuget.org/v3-flatcontainer/mudblazor/$v/mudblazor.$v.nupkg -o mud-$v.nupkg
  unzip -o -q mud-$v.nupkg 'staticwebassets/MudBlazor.min.css' -d mud-$v
  tr '}' '\n' < mud-$v/staticwebassets/MudBlazor.min.css | sort > rules-$v.txt
done
diff rules-$OLD.txt rules-$NEW.txt
```

Look for changed rules on selectors MudShadcn overrides. Watch for a rule that gained specificity
(an extra modifier class such as `.mud-card-header.mud-card-header-padding`) or moved a value
inline. An override only wins at equal or higher specificity, because `MudShadcn.css` loads second.

### 4. Bump

- `src/MudShadcn/MudShadcn.csproj`: the `MudBlazor` `PackageReference` version.
- The `Written against MudBlazor x.y.z` line at the top of `MudShadcn.css`.
- `node tools/generate-shadcn-icons.mjs`, which rewrites the icon libraries' stylesheets from the
  new version's icons (and the version in their first comment). Then run the verify script again.
- The Versions table in `README.md`, and `OLD=` in this file.
- The MudBlazor version in `samples/MudShadcn.Showcase/wwwroot/skill.md` (frontmatter and first
  paragraph) and `wwwroot/llms.txt`.
- An entry under `## [Unreleased]` in `CHANGELOG.md` naming the new MudBlazor version and anything
  that changed for consumers. Do not set MudShadcn's own `<Version>` or tag a release: that is
  `RELEASE.md`, and Sardar does it. Note in the report that a MudBlazor major change means a
  MudShadcn major version.

### 5. Re-port the showcase

The showcase (`samples/MudShadcn.Showcase`) is a port of mudblazor.com's component pages. Bring
them to the new version:

```bash
samples/MudShadcn.Showcase/sync-mudblazor-docs.sh $NEW
```

Then compare `samples/MudShadcn.Showcase/Services/MenuService.cs` with
`src/MudBlazor.Docs/Services/Menu/MenuService.cs` at the new tag, add or rename entries, and update
the version in `samples/MudShadcn.Showcase/THIRD-PARTY-NOTICES.md`. If a new docs page uses a
MudBlazor.Docs component or parameter the showcase's `Docs/` folder does not have, add it there
with the same name and parameters; do not edit the ported page.

### 6. Build

```bash
dotnet build MudShadcn.sln
```

Must finish with 0 errors and 0 warnings. `tools/test-package.sh` then checks that the package
still works as a single reference. MudBlazor ships analyzers (`MUD0002` and friends) that
report renamed or removed component parameters in the test app; treat those as errors. The test
app has no MudBlazor reference of its own, so a successful build also re-proves that MudBlazor
flows through MudShadcn transitively. Keep it that way.

### 7. Look at it

A green build says nothing about the styling. Run the test app and go through every page in both
light and dark mode, then do the same with the showcase, which has every example MudBlazor
documents (a missing docs component or parameter shows up there as a page that fails to render).
That is the New York look. Then go through the test app's pages once more in each of the eight
styles (`?style=vega`, `nova`, `maia`, `lyra`, `mira`, `luma`, `sera`, `rhea` in the address, with
`&mode=dark` for the dark mode), looking at the same fragile places as below at that style's sizes:

```bash
dotnet run --project samples/MudShadcn.TestApp --launch-profile http    # http://localhost:5152
dotnet run --project samples/MudShadcn.Showcase --launch-profile http   # http://localhost:5153
```

`tools/smoke-test.mjs` finds the pages that fail to render without opening each one: it loads every
URL of a published showcase's sitemap in a browser (the header of the script has the commands; CI
runs it on every push). It says nothing about how a page looks.

Check in particular, because these have the most fragile overrides:

- Outlined inputs: label sits above the field, no notch in the border, 36px tall, ring on focus.
- Date picker: seven days per row, header aligned with the columns. MudBlazor's calendar is a
  wrapping flex row that depends on a 40px cell pitch.
- Time picker: clock face and hand visible in dark mode.
- Menu, select and autocomplete popovers: border, 4px list padding, rounded items, 16px icons.
- Tabs: segmented look, no slider line.
- Cards and dialogs: 24px padding, not MudBlazor's 16px/8px.
- Snackbar: bordered neutral surface, action button legible.
- Buttons: 36px, no shadow, no ripple, ring on keyboard focus.
- Numeric field: both spin buttons inside the field. Chips in a button group (the file upload
  page's "Interactive file chips"): a 14px close icon with a gap before it.
- Date range picker, with one date picked: the band and both ends when the pointer is on a later
  day, on an earlier day, on a day in the other month of a two-month picker, and at the start and
  end of a week row.
- Data grid: 24px sort, filter and options buttons in the column headers; the test app's grid fits
  its card at 1280px without scrolling sideways.
- Charts: shadcn's chart colours (orange, teal, dark blue in light mode; blue, green, amber in
  dark), muted axis labels, bordered tooltips, rounded bars (a stacked bar at the two ends of the
  stack only), 2px lines and data markers as dots in the series colour. The test app's chart
  cards use `MudShadcnChartOptions`: check that the option names it sets (`BarWidthRatio`,
  `BarSpacingRatio`, `MaxNumYAxisTicks`, `InterpolationOption`, `LineDisplayType`,
  `DonutRingRatio`) still exist and still give wide bars, smooth lines and a 40% donut ring.
- The page's inline `<style>` still contains `--mud-palette-primary`, `--mud-elevation-1` and
  `--mud-default-borderradius` with MudShadcn's values
  (`curl -s http://localhost:5152/ | grep -oE -- '--mud-elevation-1:[^;]+'`).

### 8. Check shadcn too

If the user asks for it, or it has been a while, follow "Updating to shadcn's changes" below.

### 9. Report

Tell the user the old and new versions, which overrides needed changing and why, anything in
MudBlazor's release notes that affects consumers, and anything that could not be verified.

# Updating to shadcn's changes

shadcn changes its themes and styles without a version number: the sources are files in
<https://github.com/shadcn-ui/ui>. MudShadcn was last brought up to them on 10 November 2026:
`apps/v4/registry/themes.ts` at commit `4101ec9` and `apps/v4/registry/styles/` at commit `4233ed1`.
Update those two lines when you finish. Like a MudBlazor update, this is left in the working tree
for review: do not commit.

## The colour themes

```bash
node tools/generate-shadcn-themes.mjs      # rewrites src/MudShadcn/ShadcnThemeData.g.cs
git diff --stat src/MudShadcn/ShadcnThemeData.g.cs
```

The script takes the base colours and theme colours from `themes.ts` and each base colour's grey
scale from Tailwind's `theme.css`. If shadcn added a colour, the build then fails on a missing enum
member: add it to `ShadcnBaseColor` or `ShadcnColor` in `ShadcnThemeOptions.cs` (at the end, so the
existing values keep their numbers), and to `BASE_COLORS` in the script if it is a base colour. The
showcase's Theme Generator lists the enums, so it needs nothing.

The default theme is not generated. Compare `ShadcnTokens.cs` and the `:root` block of
`MudShadcn.css`, which must agree, against the Default Theme CSS in
<https://raw.githubusercontent.com/shadcn-ui/ui/main/apps/v4/content/docs/(root)/theming.mdx>, and
the shadow scale against
<https://raw.githubusercontent.com/tailwindlabs/tailwindcss/main/packages/tailwindcss/theme.css>.
Its five chart colours are shadcn's original ones on purpose (`ChartColor = null`); the registry's
neutral theme now has grey charts, which is `ChartColor = ShadcnColor.Base`.

After any change to the theme code or data, `MudShadcnTheme.Create()` must still give the same
palettes and tokens as before: dump them before and after and diff.

## The styles

Each `wwwroot/styles/<name>.css` was written from `apps/v4/registry/styles/style-<name>.css`. See
what changed upstream since the commit named above:

```bash
# in the scratchpad, not the repo
git clone --filter=blob:none --no-checkout https://github.com/shadcn-ui/ui shadcn && cd shadcn
git diff 4233ed1 origin/main -- apps/v4/registry/styles/
```

- **A changed rule.** The diff names a `.cn-*` class. Search the style's stylesheet for it: every
  block starts with a comment naming the class it comes from, and every declaration has its
  Tailwind utility in a trailing comment. Change the declarations the utilities map to (Tailwind's
  values are in its `theme.css`: the spacing unit is 4px, the text sizes have their line heights).
  If the rule has no block, the style had the base's value until now: find the base rule in
  `MudShadcn.css` that stands for the same shadcn element in another style's stylesheet, or by the
  shadcn component named in the base's comments, and add a block with the same selector. If the new
  value equals the base's, delete the declaration instead: a style holds only what differs.
- **A new rule** for a component MudBlazor has: as above. For one it does not have: nothing.
- **A new style.** Add it to `ShadcnStyle` (at the end) with shadcn's description, copy
  `wwwroot/styles/vega.css` to the new name and go through shadcn's file for the new style against
  `style-vega.css`, rule by rule, as above. `MudShadcnProvider` links the file by the enum
  member's name in lower case, and the samples' look menus and the Theme Generator list the enum.
  Add the style to the lists in `README.md`, the agent skill and `llms.txt`, and to the styles
  named in step 7 above.
- **The menu options.** `.cn-menu-translucent` is the source of `menu-translucent.css`, and
  `buildRegistryTheme` in `apps/v4/registry/config.ts` is how shadcn puts a theme together
  (`ShadcnTokens.Build` follows it) and what its bold menu accent does (`menu-bold.css`).

Then build, run `tools/verify-mudblazor.sh`, and look at the changed styles in the test app, light
and dark (`CLAUDE.md`, "Styles").

## The icon libraries

`wwwroot/styles/icons-*.css` are generated:

```bash
node tools/generate-shadcn-icons.mjs       # rewrites the five stylesheets and the showcase's table of them
tools/verify-mudblazor.sh                  # every selector still matches the icon it names, and no other
```

The tool's `ICONS` table has one row per icon: the Material icons MudBlazor draws it with, what
MudBlazor uses it for (the showcase's Icons page shows that), then its name in each library. The names were last taken from shadcn at commit `8ab2771`: where
shadcn's own components have the icon, an `IconPlaceholder` call under `apps/v4/registry` names it
in all five libraries (`lucide="XIcon" tabler="IconX" hugeicons="Cancel01Icon" …`), and the row
follows it. The other rows are the library's icon of the same shape, chosen by eye: look at a new
or changed row in the test app (`?icons=tabler`) in each library.

- **A library's newer version:** change `version` in `LIBRARIES` and run the tool. Read the
  package's licence file first, and update `THIRD-PARTY-NOTICES.md` with it. Remix Icon stays at
  4.8.0, its last version under Apache-2.0: from 4.9.0 the package ships the "Remix Icon License",
  which limits how the icons may be redistributed.
- **A new library in shadcn/create** (`packages/registry/src/icons/libraries.ts`): add it to
  `LIBRARIES` and to `ShadcnIconLibrary` (at the end), give every row its icon, and add it to the
  Theme Generator's list, `README.md`, the agent skill and the notices.
