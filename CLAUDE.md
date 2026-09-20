# MudShadcn

A NuGet package that makes MudBlazor look like shadcn/ui. It is a `MudTheme` plus one stylesheet;
it adds no components of its own and must never wrap, fork or replace MudBlazor's.

**Never run `git commit` (or anything that writes git history).** Sardar reviews and commits.

## Layout

- `src/MudShadcn/` — the package (Razor class library, `net10.0`, MudBlazor 9.10.0).
  - `MudShadcnTheme.cs` — palette, typography, radius, elevation. Hex values.
  - `ShadcnTokens.cs` — shadcn's original OKLCH tokens, light and dark.
  - `wwwroot/MudShadcn.css` — structural overrides and the `--shadcn-*` tokens.
  - `MudShadcnProvider.razor` — wraps `MudThemeProvider` + popover/dialog/snackbar providers and
    emits the `--shadcn-*` tokens for the active mode.
  - `ServiceCollectionExtensions.cs` — `AddMudShadcn()`.
  - `README.md` — consumer docs; packed into the nupkg.
- `samples/MudShadcn.TestApp/` — Blazor Server, global InteractiveServer. One page per component
  category. This is the only way to see whether a change worked.
- `UPDATING.md` — procedure for moving to a new MudBlazor version. Follow it when asked to update.

## Commands

```bash
dotnet build MudShadcn.sln                                              # must be 0 warnings, 0 errors
dotnet run --project samples/MudShadcn.TestApp --launch-profile http    # http://localhost:5152
```

`MudShadcn.css` is served straight from `src/` in development, so CSS edits only need a browser
reload. `.razor` and `.cs` edits need a rebuild and restart. There are no tests; verification is
visual, in both light and dark mode. The app starts in the OS colour scheme.

## Rules that are easy to break

- **The test app must not reference MudBlazor.** It gets it transitively through MudShadcn, and
  that is the proof the package works as a single reference. Do not add `PrivateAssets` to the
  MudBlazor `PackageReference` in the library either.
- **Do not set `--mud-palette-*`, `--mud-elevation-*`, `--mud-typography-*` or
  `--mud-default-borderradius` in the stylesheet.** `MudThemeProvider` writes them into an inline
  `<style>` in the body, which beats any stylesheet. They are set in `MudShadcnTheme.cs`.
- **In the stylesheet, read colours from `--mud-palette-*`** wherever a mapping exists, so dark
  mode and consumer palette changes flow through. Use `--shadcn-*` only for what MudBlazor has no
  slot for: ring, input background, destructive background, radius scale, shadows.
- **Token values live in three places that must agree:** `ShadcnTokens.cs`, the `:root` block of
  `MudShadcn.css` (light values, the no-provider fallback), and the hex conversions in
  `MudShadcnTheme.cs`.
- **Take values from the source, not from memory.** shadcn tokens from `theming.mdx` and the
  `new-york-v4` component source in the shadcn-ui/ui repo; shadows and type scale from Tailwind's
  `theme.css`. URLs are in `UPDATING.md`. Put the Tailwind utility in a trailing comment
  (`/* h-9 px-4 */`) the way the file already does.

## Writing overrides

Overrides win by load order, so a selector needs specificity equal to or higher than MudBlazor's
rule. Look the rule up before writing one; do not guess class names.

```bash
# one rule per line, greppable
curl -sL https://api.nuget.org/v3-flatcontainer/mudblazor/9.10.0/mudblazor.9.10.0.nupkg -o mud.nupkg
unzip -o -q mud.nupkg 'staticwebassets/MudBlazor.min.css' -d mud
tr '}' '\n' < mud/staticwebassets/MudBlazor.min.css > rules.txt
grep -E '^\.mud-card-header' rules.txt
```

Do this in the scratchpad, not the repo. Things already learned the hard way:

- MudBlazor 9 adds modifier classes that raise specificity: `.mud-card-header.mud-card-header-padding`,
  `.mud-expand-panel-header-gutters`, `.mud-list.mud-list-padding`. A bare `.mud-card-header` loses.
- The date picker is a wrapping flex row that breaks after seven days only because each cell has a
  40px pitch. Keep day width + horizontal margins at 40px.
- Some values are inline styles from component parameters (`MudTabs.MinimumTabWidth`, 160px).
  Do not fight those with `!important`; document the parameter instead.
- Some classes exist only in markup, not in MudBlazor's CSS (`.mud-chip-color-default`,
  `.mud-picker-paper`, `.mud-snackbar-action-button`, …). Confirm those against rendered HTML.
- Outlined input labels are pinned above the field and the legend notch is hidden. Anything that
  touches `.mud-input-label-outlined` or `.mud-input-control` margins has to keep that working,
  including `Margin.Dense`.

After CSS changes, run the class/variable existence check from step 3 of `UPDATING.md`.

## MudBlazor 9 API notes

Differences from older MudBlazor that have already caused build errors here:

- `MudFileUpload` uses `<CustomContent>` with `context.OpenFilePickerAsync`, not `ActivatorContent`.
- `MudSelect` multi-select binds `IReadOnlyCollection<T>`, not `IEnumerable<T>`.
- `IDialogService.ShowAsync<T>(null, options)` is ambiguous; use the overload without a title.
- `MudTabs` panel class parameter is `TabPanelsClass`.
- Nested menus are a `<MudMenu>` inside a `<MudMenu>`, not nested `MudMenuItem`s.
- `MudGlobal` no longer has per-component defaults, so variants cannot be defaulted globally.
- The `MUD0002` analyzer flags unknown parameters; treat it as an error.

## Packaging

`dotnet pack src/MudShadcn -c Release -o <scratchpad>/feed`. To test the real package, copy the
test app into the scratchpad, swap its `ProjectReference` for a `PackageReference` with a local
`nuget.config`, and check that `_content/MudBlazor/MudBlazor.min.css` is served. Never pack into
the repo and never publish to nuget.org; Sardar does that.
