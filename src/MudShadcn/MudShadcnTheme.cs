using MudBlazor;

namespace MudShadcn;

/// <summary>
/// The shadcn/ui default (neutral) theme expressed as a <see cref="MudTheme"/>.
/// </summary>
/// <remarks>
/// Hex values are the sRGB conversions of shadcn's OKLCH tokens (see <see cref="ShadcnTokens"/>).
/// The neutral greys convert exactly; the two destructive reds sit marginally outside sRGB and
/// are gamut-clipped, which is also what an sRGB display shows for the original OKLCH value.
/// </remarks>
public static class MudShadcnTheme
{
    /// <summary>
    /// Tailwind's default <c>--font-sans</c> stack, which is what a stock shadcn/ui project
    /// renders with. Put a web font such as "Geist" or "Inter" in front via
    /// <see cref="Create(string[])"/> if your app loads one.
    /// </summary>
    public static readonly string[] DefaultFontFamily =
    [
        "-apple-system", "BlinkMacSystemFont", "Segoe UI", "Roboto", "Helvetica Neue", "Noto Sans",
        "Arial", "sans-serif", "Apple Color Emoji", "Segoe UI Emoji", "Segoe UI Symbol", "Noto Color Emoji",
    ];

    /// <summary>A shared default instance. Use <see cref="Create()"/> if you intend to mutate it.</summary>
    public static MudTheme Default { get; } = Create();

    /// <summary>Creates a fresh shadcn-styled theme you can customise further.</summary>
    public static MudTheme Create() => Create(DefaultFontFamily);

    /// <summary>Creates a fresh shadcn-styled theme using the given font stack.</summary>
    public static MudTheme Create(string[] fontFamily) => new()
    {
        PaletteLight = CreatePaletteLight(),
        PaletteDark = CreatePaletteDark(),
        Typography = CreateTypography(fontFamily),
        LayoutProperties = new LayoutProperties
        {
            // rounded-md = calc(var(--radius) * 0.8) = 0.5rem: buttons, inputs, menus.
            // Larger surfaces (cards, dialogs) get their own radius in MudShadcn.css.
            DefaultBorderRadius = "0.5rem",
            AppbarHeight = "56px",
            DrawerWidthLeft = "256px",  // shadcn --sidebar-width: 16rem
            DrawerWidthRight = "256px",
        },
        Shadows = CreateShadows(),
    };

    public static PaletteLight CreatePaletteLight() => new()
    {
        Black = "#0a0a0a",
        White = "#ffffff",

        Primary = "#171717",                 // --primary
        PrimaryContrastText = "#fafafa",     // --primary-foreground
        PrimaryDarken = "#2e2e2e",           // primary/90 over background (shadcn hover)
        PrimaryLighten = "#404040",
        Secondary = "#f5f5f5",               // --secondary
        SecondaryContrastText = "#171717",   // --secondary-foreground
        SecondaryDarken = "#ebebeb",
        SecondaryLighten = "#fafafa",
        Tertiary = "#737373",                // --muted-foreground, as a usable neutral accent
        TertiaryContrastText = "#fafafa",
        Error = "#e7000b",                   // --destructive
        ErrorContrastText = "#ffffff",
        // shadcn defines no info/success/warning. These are Tailwind's blue/green/amber-600/500,
        // the scale shadcn's own docs draw from when adding such tokens.
        Info = "#155dfc",
        InfoContrastText = "#ffffff",
        Success = "#00a63e",
        SuccessContrastText = "#ffffff",
        Warning = "#fe9a00",
        WarningContrastText = "#0a0a0a",
        Dark = "#171717",
        DarkContrastText = "#fafafa",

        TextPrimary = "#0a0a0a",             // --foreground
        TextSecondary = "#737373",           // --muted-foreground
        TextDisabled = "#0a0a0a80",          // disabled:opacity-50
        ActionDefault = "#737373",
        ActionDisabled = "#0a0a0a4d",
        ActionDisabledBackground = "#0a0a0a14",

        Background = "#ffffff",              // --background
        BackgroundGray = "#f5f5f5",          // --muted
        Surface = "#ffffff",                 // --card / --popover
        DrawerBackground = "#fafafa",        // --sidebar
        DrawerText = "#0a0a0a",              // --sidebar-foreground
        DrawerIcon = "#0a0a0a",
        AppbarBackground = "#ffffff",
        AppbarText = "#0a0a0a",

        LinesDefault = "#e5e5e5",            // --border
        LinesInputs = "#e5e5e5",             // --input
        TableLines = "#e5e5e5",
        TableStriped = "#f5f5f580",          // bg-muted/50
        TableHover = "#f5f5f580",            // hover:bg-muted/50
        Divider = "#e5e5e5",
        DividerLight = "#e5e5e5",
        Skeleton = "#f5f5f5",                // bg-accent
        OverlayDark = "rgba(0,0,0,0.5)",     // bg-black/50
        OverlayLight = "rgba(255,255,255,0.5)",

        GrayLighter = "#e5e5e5",
        GrayLight = "#d4d4d4",
        GrayDefault = "#a1a1a1",
        GrayDark = "#737373",
        GrayDarker = "#404040",

        // shadcn has no ripple.
        RippleOpacity = 0,
        RippleOpacitySecondary = 0,
    };

    public static PaletteDark CreatePaletteDark() => new()
    {
        Black = "#0a0a0a",
        White = "#ffffff",

        Primary = "#e5e5e5",
        PrimaryContrastText = "#171717",
        PrimaryDarken = "#cfcfcf",           // primary/90 over background
        PrimaryLighten = "#f5f5f5",
        Secondary = "#262626",
        SecondaryContrastText = "#fafafa",
        SecondaryDarken = "#202020",
        SecondaryLighten = "#404040",
        Tertiary = "#a1a1a1",
        TertiaryContrastText = "#171717",
        Error = "#ff6467",
        ErrorContrastText = "#ffffff",
        Info = "#2b7fff",
        InfoContrastText = "#ffffff",
        Success = "#00c950",
        SuccessContrastText = "#0a0a0a",
        Warning = "#ffb900",
        WarningContrastText = "#0a0a0a",
        Dark = "#262626",
        DarkContrastText = "#fafafa",

        TextPrimary = "#fafafa",
        TextSecondary = "#a1a1a1",
        TextDisabled = "#fafafa80",
        ActionDefault = "#a1a1a1",
        ActionDisabled = "#fafafa4d",
        ActionDisabledBackground = "#fafafa14",

        Background = "#0a0a0a",
        BackgroundGray = "#262626",
        Surface = "#171717",
        DrawerBackground = "#171717",
        DrawerText = "#fafafa",
        DrawerIcon = "#fafafa",
        AppbarBackground = "#0a0a0a",
        AppbarText = "#fafafa",

        LinesDefault = "#ffffff1a",          // oklch(1 0 0 / 10%)
        LinesInputs = "#ffffff26",           // oklch(1 0 0 / 15%)
        TableLines = "#ffffff1a",
        TableStriped = "#26262680",
        TableHover = "#26262680",
        Divider = "#ffffff1a",
        DividerLight = "#ffffff1a",
        Skeleton = "#262626",
        OverlayDark = "rgba(0,0,0,0.5)",
        OverlayLight = "rgba(255,255,255,0.1)",

        GrayLighter = "#404040",
        GrayLight = "#525252",
        GrayDefault = "#737373",
        GrayDark = "#a1a1a1",
        GrayDarker = "#d4d4d4",

        RippleOpacity = 0,
        RippleOpacitySecondary = 0,
    };

    /// <summary>
    /// Tailwind's type scale as shadcn uses it: <c>text-sm</c> (0.875rem / 1.25rem) is the
    /// default body size, headings follow shadcn's Typography page.
    /// </summary>
    public static Typography CreateTypography(string[] fontFamily)
    {
        const string tight = "-0.025em"; // tracking-tight

        return new Typography
        {
            Default = new DefaultTypography { FontFamily = fontFamily, FontSize = "0.875rem", FontWeight = "400", LineHeight = "1.4286", LetterSpacing = "normal" },
            H1 = new H1Typography { FontFamily = fontFamily, FontSize = "2.25rem", FontWeight = "800", LineHeight = "1.1111", LetterSpacing = tight },
            H2 = new H2Typography { FontFamily = fontFamily, FontSize = "1.875rem", FontWeight = "600", LineHeight = "1.2", LetterSpacing = tight },
            H3 = new H3Typography { FontFamily = fontFamily, FontSize = "1.5rem", FontWeight = "600", LineHeight = "1.3333", LetterSpacing = tight },
            H4 = new H4Typography { FontFamily = fontFamily, FontSize = "1.25rem", FontWeight = "600", LineHeight = "1.4", LetterSpacing = tight },
            H5 = new H5Typography { FontFamily = fontFamily, FontSize = "1.125rem", FontWeight = "600", LineHeight = "1.5556", LetterSpacing = "normal" },
            H6 = new H6Typography { FontFamily = fontFamily, FontSize = "1rem", FontWeight = "600", LineHeight = "1.5", LetterSpacing = "normal" },
            Subtitle1 = new Subtitle1Typography { FontFamily = fontFamily, FontSize = "1rem", FontWeight = "500", LineHeight = "1.5", LetterSpacing = "normal" },
            Subtitle2 = new Subtitle2Typography { FontFamily = fontFamily, FontSize = "0.875rem", FontWeight = "500", LineHeight = "1.4286", LetterSpacing = "normal" },
            Body1 = new Body1Typography { FontFamily = fontFamily, FontSize = "0.875rem", FontWeight = "400", LineHeight = "1.4286", LetterSpacing = "normal" },
            Body2 = new Body2Typography { FontFamily = fontFamily, FontSize = "0.875rem", FontWeight = "400", LineHeight = "1.4286", LetterSpacing = "normal" },
            Button = new ButtonTypography { FontFamily = fontFamily, FontSize = "0.875rem", FontWeight = "500", LineHeight = "1.4286", LetterSpacing = "normal", TextTransform = "none" },
            Caption = new CaptionTypography { FontFamily = fontFamily, FontSize = "0.75rem", FontWeight = "400", LineHeight = "1.3333", LetterSpacing = "normal" },
            Overline = new OverlineTypography { FontFamily = fontFamily, FontSize = "0.75rem", FontWeight = "500", LineHeight = "1.3333", LetterSpacing = "0.05em", TextTransform = "uppercase" },
        };
    }

    /// <summary>
    /// MudBlazor's 26 elevation levels collapsed onto Tailwind's shadow scale, which is all
    /// shadcn uses: sm for cards (MudPaper's default elevation 1), md for popovers, lg for dialogs.
    /// (shadow-xs, used on inputs and outline buttons, is applied in MudShadcn.css.)
    /// </summary>
    public static Shadow CreateShadows()
    {
        const string sm = "0 1px 3px 0 rgb(0 0 0 / 0.1), 0 1px 2px -1px rgb(0 0 0 / 0.1)";
        const string md = "0 4px 6px -1px rgb(0 0 0 / 0.1), 0 2px 4px -2px rgb(0 0 0 / 0.1)";
        const string lg = "0 10px 15px -3px rgb(0 0 0 / 0.1), 0 4px 6px -4px rgb(0 0 0 / 0.1)";

        var elevation = new string[26];
        for (var i = 0; i < elevation.Length; i++)
        {
            elevation[i] = i switch
            {
                0 => "none",
                <= 4 => sm,
                <= 12 => md,
                _ => lg,
            };
        }

        return new Shadow { Elevation = elevation };
    }
}
