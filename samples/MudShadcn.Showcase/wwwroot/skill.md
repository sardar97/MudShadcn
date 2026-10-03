---
name: mudshadcn
description: >-
  Set up and use MudShadcn, which gives every MudBlazor component the shadcn/ui look (a MudTheme
  plus one stylesheet, light and dark). Covers installing it as the only package, wiring it into a
  Blazor Web App (Server, WebAssembly or Auto) or a standalone WebAssembly app, dark mode, and
  writing ordinary MudBlazor that comes out looking like shadcn/ui: which Variant and Color give
  shadcn's button, input, badge, card, tabs, sidebar and toast, charts, theming with your own
  colours, radius and font, and the mistakes that break the styling. Use it whenever a Blazor or
  MudBlazor app should look like shadcn/ui, or a project references MudShadcn.
license: MIT
metadata:
  package: MudShadcn
  version: "1.0.0"
  mudblazor: "9.10.0"
  homepage: https://mudshadcn.pages.dev
  nuget: https://www.nuget.org/packages/MudShadcn
  repository: https://github.com/sardar97/MudShadcn
  llms_txt: https://mudshadcn.pages.dev/llms.txt
  llms_full_txt: https://mudshadcn.pages.dev/llms-full.txt
---

# MudShadcn: agent skill

MudShadcn (MIT) restyles [MudBlazor](https://mudblazor.com) to look like
[shadcn/ui](https://ui.shadcn.com). It is a `MudTheme` and one stylesheet. It adds **no components**:
the app keeps using MudBlazor's components and API, and they render with shadcn's neutral palette,
flat bordered surfaces, radius scale, focus rings and sizes. Written against MudBlazor 9.10.0, for
.NET 10.

This file is self-contained. The full documentation is the package README, also served as
<https://mudshadcn.pages.dev/llms-full.txt>.

---

## 0. Rules that are easy to break

1. **Reference only `MudShadcn`.** It brings MudBlazor (and its CSS and JS) as a dependency. Remove an
   existing `MudBlazor` `PackageReference` so the version MudShadcn was written against is used.
2. **Link MudBlazor's stylesheet first, then MudShadcn's.** MudShadcn's rules win over MudBlazor's
   only by coming later.
3. **One `<MudShadcnProvider />` replaces** `MudThemeProvider`, `MudPopoverProvider`,
   `MudDialogProvider` and `MudSnackbarProvider`. Do not add those as well (unless you set
   `IncludeProviders="false"`), and render it in an **interactive** layout.
4. **Do not wrap, fork or re-implement MudBlazor components** to get the shadcn look, and do not add
   Tailwind. Choose the right `Variant` and `Color` (section 3); the stylesheet does the rest.
5. **Change colours, radius and fonts through the theme** (`MudShadcnTheme.Create`, `PaletteLight`,
   `PaletteDark`), never by setting `--mud-palette-*` or other `--mud-*` variables in CSS:
   `MudThemeProvider` writes them inline in the page and overrides any stylesheet.
6. **Inputs are `Variant.Outlined`.** That is shadcn's input: a 36px bordered box with the label above
   it. `Filled` and `Text` are MudBlazor-only looks.

---

## 1. Install

```bash
dotnet add package MudShadcn
```

## 2. Wire it up

### `Program.cs` (every hosting model)

```csharp
using MudShadcn;

builder.Services.AddMudShadcn();   // calls AddMudServices() with shadcn-style snackbar defaults
```

`AddMudShadcn` accepts the same configuration callback as `AddMudServices`:
`builder.Services.AddMudShadcn(config => { /* MudServicesConfiguration */ });`.

### Blazor Web App (.NET 8+): `Components/App.razor`

```razor
<head>
    ...
    <link rel="stylesheet" href="@Assets["_content/MudBlazor/MudBlazor.min.css"]" />
    <link rel="stylesheet" href="@Assets["_content/MudShadcn/MudShadcn.min.css"]" />
    <HeadOutlet @rendermode="InteractiveServer" />
</head>
<body>
    <Routes @rendermode="InteractiveServer" />
    <script src="@Assets["_framework/blazor.web.js"]"></script>
    <script src="@Assets["_content/MudBlazor/MudBlazor.min.js"]"></script>
</body>
```

Use `InteractiveWebAssembly` or `InteractiveAuto` the same way. The provider must run interactively,
like MudBlazor's own providers: with global interactivity (above) that is automatic. With per-page
interactivity, the layout that holds the provider is static, so popovers, dialogs, snackbars and the
dark-mode switch do not work; make interactivity global instead.

### Standalone Blazor WebAssembly: `wwwroot/index.html`

```html
<link rel="stylesheet" href="_content/MudBlazor/MudBlazor.min.css" />
<link rel="stylesheet" href="_content/MudShadcn/MudShadcn.min.css" />
...
<script src="_content/MudBlazor/MudBlazor.min.js"></script>
```

### `_Imports.razor`

```razor
@using MudBlazor
@using MudShadcn
```

### The layout (`MainLayout.razor`)

```razor
@inherits LayoutComponentBase

<MudShadcnProvider @bind-IsDarkMode="_isDarkMode" />

<MudLayout>
    <MudAppBar Elevation="0">
        <MudText Typo="Typo.h6">My app</MudText>
        <MudSpacer />
        <MudIconButton Icon="@(_isDarkMode ? Icons.Material.Outlined.LightMode : Icons.Material.Outlined.DarkMode)"
                       OnClick="@(() => _isDarkMode = !_isDarkMode)" aria-label="Toggle dark mode" />
    </MudAppBar>
    <MudMainContent>
        <MudContainer MaxWidth="MaxWidth.Large" Class="py-8">@Body</MudContainer>
    </MudMainContent>
</MudLayout>

@code {
    private bool _isDarkMode;
}
```

`MudShadcnProvider` parameters: `Theme` (default `MudShadcnTheme.Default`), `IsDarkMode` (bindable),
`FollowSystemDarkMode` (default `true`: starts in the OS colour scheme and follows it),
`IncludeProviders` (default `true`), `DefaultScrollbar`. It also emits shadcn's tokens for the active
mode as `--shadcn-*` CSS variables and sets `color-scheme` on the page.

Without the provider (`<MudThemeProvider Theme="MudShadcnTheme.Default" />` plus the three MudBlazor
providers) everything works except that the focus ring, input background and destructive colour keep
their light-mode values in dark mode.

---

## 3. shadcn/ui vocabulary in MudBlazor

| shadcn/ui | Write this |
| --- | --- |
| `<Button>` (default) | `<MudButton Variant="Variant.Filled" Color="Color.Primary">` |
| `<Button variant="secondary">` | `<MudButton Variant="Variant.Filled" Color="Color.Secondary">` (or `Color.Default`) |
| `<Button variant="destructive">` | `<MudButton Variant="Variant.Filled" Color="Color.Error">` |
| `<Button variant="outline">` | `<MudButton Variant="Variant.Outlined">` |
| `<Button variant="ghost">` | `<MudButton Variant="Variant.Text">` |
| `<Button size="icon">` | `<MudIconButton Icon="…" Variant="Variant.Outlined">` (or `Text`) |
| `<Input>`, `<Textarea>` | `<MudTextField Variant="Variant.Outlined" Label="…">` (`Lines="3"` for a textarea; `Margin="Margin.Dense"` for 32px) |
| `<Select>` | `<MudSelect Variant="Variant.Outlined">` with `<MudSelectItem>` |
| `<Combobox>` | `<MudAutocomplete Variant="Variant.Outlined">` |
| `<Checkbox>`, `<RadioGroup>`, `<Switch>`, `<Slider>` | `MudCheckBox`, `MudRadioGroup` + `MudRadio`, `MudSwitch`, `MudSlider` (with `Color="Color.Primary"`) |
| `<Badge>` | `<MudChip T="string" Color="Color.Primary">`; secondary: no colour; outline: `Variant="Variant.Outlined"`; destructive: `Color.Error` |
| `<Card>` | `MudCard` + `MudCardHeader` (`CardHeaderContent`), `MudCardContent`, `MudCardActions` |
| card description, muted text | `<MudText Color="Color.Tertiary">` |
| `<Tabs>` | `<MudTabs MinimumTabWidth="0px">` (content-width segments) |
| `<Table>` / data table | `MudTable` / `MudDataGrid` |
| `<Dialog>`, `<AlertDialog>` | `IDialogService.ShowAsync<T>()` with a `MudDialog` |
| `<Sheet>` / sidebar | `MudDrawer` (`DrawerVariant.Temporary` / `Persistent`) with `MudNavMenu` |
| Sonner / toast | `ISnackbar.Add("…", Severity.Normal)` |
| `<Alert>` | `<MudAlert Variant="Variant.Outlined">` (plain); default `Variant.Text` is tinted |
| `<Accordion>` | `MudExpansionPanels` |
| `<Tooltip>`, `<Popover>`, `<DropdownMenu>` | `MudTooltip`, `MudPopover`, `MudMenu` |
| `<Calendar>`, `<DatePicker>` | `MudDatePicker` (`PickerVariant.Static` for an inline calendar) |
| `<Progress>`, `<Skeleton>` | `MudProgressLinear`, `MudSkeleton` |
| `<Chart>` | `MudChart` / the chart components, with no `ChartPalette` |

Colours, in shadcn's terms: `Color.Primary` is shadcn's primary (near-black in light mode, near-white
in dark). `Color.Secondary` is shadcn's light grey **surface**, not an accent. `Color.Tertiary` is the
muted foreground. `Color.Error` is destructive. Info, success and warning are Tailwind's blue, green
and amber (shadcn has none). Leave text uncoloured (or `Color.Default`) to follow the mode.

### A shadcn "login" card

```razor
<MudCard Style="max-width:24rem">
    <MudCardHeader>
        <CardHeaderContent>
            <MudText Typo="Typo.h6">Login to your account</MudText>
            <MudText Color="Color.Tertiary" Class="mt-2">Enter your email below to log in.</MudText>
        </CardHeaderContent>
    </MudCardHeader>
    <MudCardContent>
        <MudStack Spacing="4">
            <MudTextField @bind-Value="_email" Label="Email" Placeholder="m@example.com" Variant="Variant.Outlined" />
            <MudTextField @bind-Value="_password" Label="Password" InputType="InputType.Password" Variant="Variant.Outlined" />
        </MudStack>
    </MudCardContent>
    <MudCardActions Class="d-flex flex-column gap-2">
        <MudButton Variant="Variant.Filled" Color="Color.Primary" FullWidth="true">Login</MudButton>
        <MudButton Variant="Variant.Outlined" FullWidth="true">Login with Google</MudButton>
    </MudCardActions>
</MudCard>

@code {
    private string _email = "";
    private string _password = "";
}
```

---

## 4. Theme, radius, fonts

```csharp
// Program.cs or a static class: a theme with your font, colour and radius.
public static class AppTheme
{
    public static readonly MudTheme Theme = Create();

    private static MudTheme Create()
    {
        var theme = MudShadcnTheme.Create(["Geist", .. MudShadcnTheme.DefaultFontFamily]);
        theme.PaletteLight.Primary = "#2563eb";
        theme.PaletteDark.Primary = "#3b82f6";
        theme.LayoutProperties.DefaultBorderRadius = "0.375rem";
        return theme;
    }
}
```

```razor
<MudShadcnProvider Theme="AppTheme.Theme" @bind-IsDarkMode="_isDarkMode" />
```

Load the font yourself (a `<link>` to Google Fonts or a self-hosted `@font-face`); the default is the
system UI font stack. shadcn's radius scale follows one variable:
`:root { --shadcn-radius: 0.5rem; }` in your own CSS. shadcn's tokens are available to your CSS as
`--shadcn-*` (`--shadcn-border`, `--shadcn-muted-foreground`, `--shadcn-ring`, `--shadcn-radius-lg`,
`--shadcn-shadow-md`, …) and to C# as `ShadcnTokens.Light` / `ShadcnTokens.Dark`.

## 5. Charts

Use MudBlazor's charts without `ChartOptions.ChartPalette`: the default palette becomes shadcn's
`chart-1` to `chart-5` (with their dark-mode values) plus tints and shades up to 20 series, and heat
maps use a ramp of the primary colour. A palette you set is used as given. Series colours are
available to CSS as `--shadcn-chart-series-1` … `--shadcn-chart-series-20`.

## 6. Things that look wrong but are expected

- Icons are Material icons (MudBlazor's), not Lucide. Checkbox, radio, switch and the select arrow
  are redrawn as shadcn's controls; custom `CheckedIcon`/`UncheckedIcon` render as given.
- `Variant.Outlined` inputs have no floating label: it sits above the field.
- `Color.Dark` is a mid grey in dark mode, so it stays visible.
- `MudTabs` tabs have a 160px minimum width by default (an inline style from `MinimumTabWidth`); set
  `MinimumTabWidth="0px"` for shadcn's content-width tabs.
- Single-select tables without a selection checkbox need `RowClassFunc` to highlight the selected row.

## 7. Checking your work

Every MudBlazor component, with all of MudBlazor's documented examples, is in the showcase at
<https://mudshadcn.pages.dev> (light and dark). Compare a page there with the app when something
looks off.
