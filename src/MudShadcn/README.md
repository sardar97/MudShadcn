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

The provider has to be rendered interactively, like MudBlazor's own providers.

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
| muted text | `Color.Tertiary` on `MudText`, or `Typo.body2` with `Color.Tertiary` |

`Variant.Outlined` is the variant that gets the full shadcn treatment for inputs. `Text` and
`Filled` keep MudBlazor's structure and only pick up the neutral colours.

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

- **Floating labels are gone.** MudBlazor's outlined label is pinned above the field and the
  notch is closed. A labelled outlined input therefore takes 22px more vertical space than in
  stock MudBlazor.
- Icons are MudBlazor's Material icons, not Lucide. The select arrow and checkbox glyphs are
  Material shapes.
- `MudSlider` has no filled-range element to style, so the track is a single muted colour.
- The destructive reds are just outside sRGB; the `MudTheme` holds the gamut-clipped hex
  (`#e7000b`, `#ff6467`), which is what an sRGB display shows for the OKLCH original anyway.
- The default font is the system UI stack. To use Geist or Inter, load the font yourself and pass
  it to `MudShadcnTheme.Create(...)`.
