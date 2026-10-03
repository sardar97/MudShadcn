# Updating MudShadcn to a new MudBlazor version

Instructions for Claude Code when asked to "update MudShadcn to the latest MudBlazor".
Never run `git commit`. Leave the changes in the working tree for review.

## What can break

MudShadcn depends on three things MudBlazor does not treat as public API, in order of risk:

1. **CSS class names and selector specificity.** `MudShadcn.css` overrides MudBlazor rules by
   matching their selectors. If MudBlazor renames a class, restructures a component's markup, or
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
  stylesheet. Classes MudBlazor renders without styling them are listed in the script
  (`MARKUP_ONLY_CLASSES`); a new one must be confirmed in the rendered HTML before it goes on that
  list (`curl -s http://localhost:5152/pickers | grep -o mud-picker-paper` for the test app; the
  showcase renders in the browser, so use the browser's dev tools there);
- MudBlazor's default chart palette and the five heat-map shades built from it are still the colours
  the charts section recolours (they are matched by attribute value, not class);
- the default checkbox, radio and select/autocomplete icons still have the SVG paths the inputs
  sections match (`path[d^="…"]`) to redraw them as shadcn's controls. If they changed, the
  Material glyphs come back.

It reads the defaults through MudBlazor's API with a small C# probe. If that no longer compiles, the
API changed: update the probe in the script as well.

A missing class or variable is an override that no longer applies: find its replacement in the new
stylesheet and update the selector. Existence is not enough, though. Diff the shipped rules too,
working in the scratchpad, not the repo:

```bash
OLD=9.10.0; NEW=<new version>
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
- The version table in `src/MudShadcn/README.md`, and `OLD=` in this file.
- `<Version>` of MudShadcn itself. Use a major bump if MudBlazor's major changed.

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

Must finish with 0 errors and 0 warnings. MudBlazor ships analyzers (`MUD0002` and friends) that
report renamed or removed component parameters in the test app; treat those as errors. The test
app has no MudBlazor reference of its own, so a successful build also re-proves that MudBlazor
flows through MudShadcn transitively. Keep it that way.

### 7. Look at it

A green build says nothing about the styling. Run the test app and go through every page in both
light and dark mode, then do the same with the showcase, which has every example MudBlazor
documents (a missing docs component or parameter shows up there as a page that fails to render):

```bash
dotnet run --project samples/MudShadcn.TestApp --launch-profile http    # http://localhost:5152
dotnet run --project samples/MudShadcn.Showcase --launch-profile http   # http://localhost:5153
```

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
- Charts: shadcn's chart colours (orange, teal, dark blue in light mode; blue, green, amber in
  dark), muted axis labels, bordered tooltips.
- The page's inline `<style>` still contains `--mud-palette-primary`, `--mud-elevation-1` and
  `--mud-default-borderradius` with MudShadcn's values
  (`curl -s http://localhost:5152/ | grep -oE -- '--mud-elevation-1:[^;]+'`).

### 8. Check shadcn too

If the user asks for it, or it has been a while: compare `ShadcnTokens.cs` and the `:root` block
of `MudShadcn.css` against the Default Theme CSS in
<https://raw.githubusercontent.com/shadcn-ui/ui/main/apps/v4/content/docs/(root)/theming.mdx>, and
the shadow scale against
<https://raw.githubusercontent.com/tailwindlabs/tailwindcss/main/packages/tailwindcss/theme.css>.
The token values live in three places that must agree: `ShadcnTokens.cs`, the `:root` block of
`MudShadcn.css`, and the hex conversions in `MudShadcnTheme.cs`.

### 9. Report

Tell the user the old and new versions, which overrides needed changing and why, anything in
MudBlazor's release notes that affects consumers, and anything that could not be verified.
