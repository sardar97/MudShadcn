using MudBlazor;

namespace MudShadcn;

/// <summary>
/// shadcn/ui's themes expressed as a <see cref="MudTheme"/>: the default (neutral) one, or any
/// combination <see href="https://ui.shadcn.com/create">shadcn/create</see> offers
/// (<see cref="ShadcnThemeOptions"/>).
/// </summary>
/// <remarks>
/// The palettes are the sRGB conversions of shadcn's OKLCH tokens (see <see cref="ShadcnTokens"/>),
/// computed when a theme is created. The greys convert exactly; saturated colours that sit outside
/// sRGB are gamut-clipped, which is also what an sRGB display shows for the original OKLCH value.
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

    private static readonly ShadcnThemeOptions DefaultOptions = new();

    /// <summary>A shared default instance. Use <see cref="Create()"/> if you intend to mutate it.</summary>
    public static ShadcnTheme Default { get; } = Create();

    /// <summary>Creates a fresh shadcn-styled theme you can customise further.</summary>
    public static ShadcnTheme Create() => Create(DefaultOptions);

    /// <summary>Creates a fresh shadcn-styled theme using the given font stack.</summary>
    public static ShadcnTheme Create(string[] fontFamily) => Create(new ShadcnThemeOptions { FontFamily = fontFamily });

    /// <summary>
    /// Creates a fresh theme from what shadcn/create lets you choose: style, base colour, theme
    /// colour, chart colour, radius, fonts, the look of menus and the icon library.
    /// </summary>
    public static ShadcnTheme Create(ShadcnThemeOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var radius = RadiusOf(options.Radius);
        var theme = new ShadcnTheme(options, radius.Css)
        {
            Typography = CreateTypography(options.FontFamily ?? DefaultFontFamily, options.HeadingFontFamily),
            LayoutProperties = new LayoutProperties
            {
                // rounded-md = calc(var(--radius) * 0.8), 0.5rem by default: buttons, inputs, menus.
                // Larger surfaces (cards, dialogs) get their own radius in MudShadcn.css.
                DefaultBorderRadius = FormattableString.Invariant($"{radius.Rem * 0.8:0.###}rem"),
                AppbarHeight = "56px",
                DrawerWidthLeft = "256px",  // shadcn --sidebar-width: 16rem
                DrawerWidthRight = "256px",
                DrawerMiniWidthLeft = "48px",   // shadcn --sidebar-width-icon: 3rem
                DrawerMiniWidthRight = "48px",
            },
            Shadows = CreateShadows(),
        };

        theme.PaletteLight = FillPalette(new PaletteLight(), theme.TokensLight, options, dark: false);
        theme.PaletteDark = FillPalette(new PaletteDark(), theme.TokensDark, options, dark: true);
        return theme;
    }

    /// <summary>The default (neutral) light palette.</summary>
    public static PaletteLight CreatePaletteLight() =>
        FillPalette(new PaletteLight(), ShadcnTokens.Light, DefaultOptions, dark: false);

    /// <summary>The default (neutral) dark palette.</summary>
    public static PaletteDark CreatePaletteDark() =>
        FillPalette(new PaletteDark(), ShadcnTokens.Dark, DefaultOptions, dark: true);

    // shadcn/create's RADII (apps/v4/registry/config.ts).
    private static (string Css, double Rem) RadiusOf(ShadcnRadius radius) => radius switch
    {
        ShadcnRadius.None => ("0rem", 0),   // with its unit: the stylesheet uses the scale in calc() and min()
        ShadcnRadius.Small => ("0.45rem", 0.45),
        ShadcnRadius.Large => ("0.875rem", 0.875),
        _ => (ShadcnTokens.Radius, 0.625),
    };

    /// <summary>
    /// shadcn's tokens on MudBlazor's palette. The trailing comments name the token or, where shadcn
    /// has none, the step of the base colour's Tailwind scale (neutral-200, stone-200, …).
    /// </summary>
    private static T FillPalette<T>(T palette, IEnumerable<KeyValuePair<string, string>> tokens, ShadcnThemeOptions options, bool dark)
        where T : Palette
    {
        var token = tokens.ToDictionary(t => t.Key, t => t.Value);
        OklchColor Color(string name) => OklchColor.Parse(token[name]);
        string Hex(string name) => Color(name).ToHex();
        string Scale(int step) => OklchColor.Parse(ShadcnThemeData.Scale(options.BaseColor, step)).ToHex();

        var background = Color("background");
        var foreground = Color("foreground");
        var primary = Color("primary");
        var secondary = Color("secondary");
        var muted = Color("muted");

        palette.Black = Scale(950);
        palette.White = "#ffffff";

        palette.Primary = primary.ToHex();                             // --primary
        palette.PrimaryContrastText = Hex("primary-foreground");       // --primary-foreground
        palette.PrimaryDarken = primary.Over(background, 0.9);         // primary/90 over background (shadcn hover)
        palette.PrimaryLighten = options.Theme != ShadcnColor.Base
            ? primary.Under((255, 255, 255), 0.2)                      // a theme colour: a fifth of the way to white
            : Scale(dark ? 100 : 700);
        palette.Secondary = secondary.ToHex();                         // --secondary
        palette.SecondaryContrastText = Hex("secondary-foreground");   // --secondary-foreground
        palette.SecondaryDarken = dark ? secondary.Over(background, 0.8) : secondary.Under((0, 0, 0), 0.04);
        palette.SecondaryLighten = Scale(dark ? 700 : 50);
        palette.Tertiary = Hex("muted-foreground");                    // --muted-foreground, as a usable neutral accent
        palette.TertiaryContrastText = Scale(dark ? 900 : 50);
        palette.Error = Hex("destructive");                            // --destructive
        palette.ErrorContrastText = "#ffffff";
        // shadcn defines no info/success/warning. These are Tailwind's blue/green/amber-600/500
        // (500/400 in dark mode), the scale shadcn's own docs draw from when adding such tokens.
        palette.Info = dark ? "#2b7fff" : "#155dfc";
        palette.InfoContrastText = "#ffffff";
        palette.Success = dark ? "#00c950" : "#00a63e";
        palette.SuccessContrastText = dark ? "#0a0a0a" : "#ffffff";
        palette.Warning = dark ? "#ffb900" : "#fe9a00";
        palette.WarningContrastText = "#0a0a0a";
        // Dark mode: the 500 step (shadcn's dark --ring for neutral) is the darkest grey that is
        // legible both as a text or icon colour on the dark background (4.2:1) and behind white text (4.5:1).
        palette.Dark = Scale(dark ? 500 : 900);
        palette.DarkContrastText = Scale(50);

        palette.TextPrimary = foreground.ToHex();                      // --foreground
        palette.TextSecondary = Hex("muted-foreground");               // --muted-foreground
        palette.TextDisabled = foreground.ToHex(0.5);                  // disabled:opacity-50
        palette.ActionDefault = Hex("muted-foreground");
        palette.ActionDisabled = foreground.ToHex(0.3);
        palette.ActionDisabledBackground = foreground.ToHex(0.08);

        palette.Background = background.ToHex();                       // --background
        palette.BackgroundGray = muted.ToHex();                        // --muted
        palette.Surface = Hex("card");                                 // --card / --popover
        palette.DrawerBackground = Hex("sidebar");                     // --sidebar
        palette.DrawerText = Hex("sidebar-foreground");                // --sidebar-foreground
        palette.DrawerIcon = Hex("sidebar-foreground");
        palette.AppbarBackground = background.ToHex();
        palette.AppbarText = foreground.ToHex();

        palette.LinesDefault = Hex("border");                          // --border
        palette.LinesInputs = Hex("input");                            // --input
        palette.TableLines = Hex("border");
        palette.TableStriped = muted.ToHex(0.5);                       // bg-muted/50
        palette.TableHover = muted.ToHex(0.5);                         // hover:bg-muted/50
        palette.Divider = Hex("border");
        palette.DividerLight = Hex("border");
        palette.Skeleton = muted.ToHex();                              // bg-muted
        palette.OverlayDark = "rgba(0,0,0,0.5)";                       // bg-black/50
        palette.OverlayLight = dark ? "rgba(255,255,255,0.1)" : "rgba(255,255,255,0.5)";

        palette.GrayLighter = Scale(dark ? 700 : 200);
        palette.GrayLight = Scale(dark ? 600 : 300);
        palette.GrayDefault = Scale(dark ? 500 : 400);
        palette.GrayDark = Scale(dark ? 400 : 500);
        palette.GrayDarker = Scale(dark ? 300 : 700);

        // shadcn has no ripple.
        palette.RippleOpacity = 0;
        palette.RippleOpacitySecondary = 0;

        return palette;
    }

    /// <summary>
    /// Tailwind's type scale as shadcn uses it: <c>text-sm</c> (0.875rem / 1.25rem) is the
    /// default body size, headings follow shadcn's Typography page.
    /// </summary>
    public static Typography CreateTypography(string[] fontFamily) => CreateTypography(fontFamily, null);

    /// <summary>
    /// As <see cref="CreateTypography(string[])"/>, with a font stack of their own for the headings
    /// (shadcn/create's heading font). <c>null</c> uses <paramref name="fontFamily"/>.
    /// </summary>
    public static Typography CreateTypography(string[] fontFamily, string[]? headingFontFamily)
    {
        const string tight = "-0.025em"; // tracking-tight
        var heading = headingFontFamily ?? fontFamily;

        return new Typography
        {
            Default = new DefaultTypography { FontFamily = fontFamily, FontSize = "0.875rem", FontWeight = "400", LineHeight = "1.4286", LetterSpacing = "normal" },
            H1 = new H1Typography { FontFamily = heading, FontSize = "2.25rem", FontWeight = "800", LineHeight = "1.1111", LetterSpacing = tight },
            H2 = new H2Typography { FontFamily = heading, FontSize = "1.875rem", FontWeight = "600", LineHeight = "1.2", LetterSpacing = tight },
            H3 = new H3Typography { FontFamily = heading, FontSize = "1.5rem", FontWeight = "600", LineHeight = "1.3333", LetterSpacing = tight },
            H4 = new H4Typography { FontFamily = heading, FontSize = "1.25rem", FontWeight = "600", LineHeight = "1.4", LetterSpacing = tight },
            H5 = new H5Typography { FontFamily = heading, FontSize = "1.125rem", FontWeight = "600", LineHeight = "1.5556", LetterSpacing = "normal" },
            H6 = new H6Typography { FontFamily = heading, FontSize = "1rem", FontWeight = "600", LineHeight = "1.5", LetterSpacing = "normal" },
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
