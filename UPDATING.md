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
app bar, alert, snackbar, progress, skeleton, pickers, expansion panel, card, paper.

### 3. Diff the stylesheets — do not skip this

Release notes rarely mention CSS changes. Compare the shipped CSS directly. Work in the
scratchpad, not the repo.

```bash
OLD=9.10.0; NEW=<new version>
for v in $OLD $NEW; do
  curl -sL https://api.nuget.org/v3-flatcontainer/mudblazor/$v/mudblazor.$v.nupkg -o mud-$v.nupkg
  unzip -o -q mud-$v.nupkg 'staticwebassets/MudBlazor.min.css' -d mud-$v
  tr '}' '\n' < mud-$v/staticwebassets/MudBlazor.min.css | sort > rules-$v.txt
  grep -oE -- '--mud-[a-zA-Z0-9-]+' mud-$v/staticwebassets/MudBlazor.min.css | sort -u > vars-$v.txt
  grep -oE '\.mud-[a-zA-Z0-9_-]+' mud-$v/staticwebassets/MudBlazor.min.css | sort -u > classes-$v.txt
done

diff vars-$OLD.txt vars-$NEW.txt        # renamed or removed custom properties
diff classes-$OLD.txt classes-$NEW.txt  # renamed or removed classes
```

Then check what MudShadcn actually uses against the new version:

```bash
CSS=src/MudShadcn/wwwroot/MudShadcn.css
# every --mud-* variable we read must still exist
grep -oE -- '--mud-[a-zA-Z0-9-]+' $CSS | sort -u | comm -23 - vars-$NEW.txt
# every .mud-* class we target must still exist
grep -oE '\.mud-[a-zA-Z0-9_-]+' $CSS | sort -u | comm -23 - classes-$NEW.txt
```

The first should print only the three wildcard mentions in the header comment (`--mud-palette-`,
`--mud-elevation-`, `--mud-typography-`). The second has known false positives: classes MudBlazor
puts in its markup without styling them itself. As of 9.10.0 those are
`.mud-avatar-filled-default`, `.mud-button-month`, `.mud-chip-color-default`, `.mud-picker-paper`,
`.mud-snackbar-action-button` and `.mud-toggle-item-selected`. Confirm each still appears in the
rendered HTML of the test app (`curl -s http://localhost:5152/pickers | grep -o mud-picker-paper`).
Anything else either command prints is an override that no longer applies: find the replacement
in `rules-$NEW.txt` and update the selector.

Existence is not enough: also `diff rules-$OLD.txt rules-$NEW.txt` and look for changed rules on
selectors MudShadcn overrides. Watch for a rule that gained specificity (an extra modifier class
such as `.mud-card-header.mud-card-header-padding`) or moved a value inline. An override only wins
at equal or higher specificity, because `MudShadcn.css` loads second.

### 4. Bump

- `src/MudShadcn/MudShadcn.csproj`: the `MudBlazor` `PackageReference` version.
- The `Written against MudBlazor x.y.z` line at the top of `MudShadcn.css`.
- The version table in `src/MudShadcn/README.md`, and `OLD=` in this file.
- `<Version>` of MudShadcn itself. Use a major bump if MudBlazor's major changed.

### 5. Build

```bash
dotnet build MudShadcn.sln
```

Must finish with 0 errors and 0 warnings. MudBlazor ships analyzers (`MUD0002` and friends) that
report renamed or removed component parameters in the test app; treat those as errors. The test
app has no MudBlazor reference of its own, so a successful build also re-proves that MudBlazor
flows through MudShadcn transitively. Keep it that way.

### 6. Look at it

A green build says nothing about the styling. Run the test app and go through every page in both
light and dark mode:

```bash
dotnet run --project samples/MudShadcn.TestApp --launch-profile http   # http://localhost:5152
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
- The page's inline `<style>` still contains `--mud-palette-primary`, `--mud-elevation-1` and
  `--mud-default-borderradius` with MudShadcn's values
  (`curl -s http://localhost:5152/ | grep -oE -- '--mud-elevation-1:[^;]+'`).

### 7. Check shadcn too

If the user asks for it, or it has been a while: compare `ShadcnTokens.cs` and the `:root` block
of `MudShadcn.css` against the Default Theme CSS in
<https://raw.githubusercontent.com/shadcn-ui/ui/main/apps/v4/content/docs/(root)/theming.mdx>, and
the shadow scale against
<https://raw.githubusercontent.com/tailwindlabs/tailwindcss/main/packages/tailwindcss/theme.css>.
The token values live in three places that must agree: `ShadcnTokens.cs`, the `:root` block of
`MudShadcn.css`, and the hex conversions in `MudShadcnTheme.cs`.

### 8. Report

Tell the user the old and new versions, which overrides needed changing and why, anything in
MudBlazor's release notes that affects consumers, and anything that could not be verified.
