using MudBlazor;

namespace MudShadcn;

/// <summary>
/// Chart options with the proportions of shadcn/ui's charts (Recharts), for what MudShadcn.css
/// cannot change because MudBlazor computes it into the SVG: bar width and spacing, line curves,
/// the area fill, the number of axis ticks and the donut ring.
/// </summary>
/// <remarks>
/// Charts look like shadcn's without these too: the stylesheet gives every chart shadcn's colours,
/// axes, grid, legend, tooltip, rounded bars and 2px lines. Each method returns a new instance, so
/// set anything else on it as usual. MudChart's own <c>MatchBoundsToSize</c> parameter keeps the
/// axis text at 12px however wide the chart is drawn, as Recharts does:
/// <code>
/// &lt;MudChart ChartType="ChartType.Bar" ChartOptions="@_options" MatchBoundsToSize="true" ... /&gt;
///
/// private readonly BarChartOptions _options = MudShadcnChartOptions.Bar();
/// </code>
/// </remarks>
public static class MudShadcnChartOptions
{
    // Recharts' default tickCount: about five values on the axis, and as many grid lines.
    private const int AxisTicks = 5;

    /// <summary>
    /// <c>&lt;BarChart&gt;</c>: each category's bars fill 80% of it (Recharts' default
    /// <c>barCategoryGap="10%"</c>), about 4px apart (<c>barGap={4}</c>).
    /// </summary>
    public static BarChartOptions Bar() => new()
    {
        MaxNumYAxisTicks = AxisTicks,
        BarWidthRatio = 0.8,
        BarSpacingRatio = 0.08,
    };

    /// <summary>
    /// <c>&lt;BarChart&gt;</c> with stacked bars (<c>stackId</c>): the stack fills 80% of its category.
    /// A zero value draws nothing, as in Recharts. MudBlazor's default is a hairline segment, which
    /// the stylesheet would take for the end of its stack when it rounds the stack's two ends.
    /// </summary>
    public static StackedBarChartOptions StackedBar() => new()
    {
        MaxNumYAxisTicks = AxisTicks,
        BarWidthRatio = 0.8,
        ShowZeroValues = false,
    };

    /// <summary>
    /// <c>&lt;Line type="natural" strokeWidth={2} dot={false}&gt;</c>: smooth 2px lines.
    /// </summary>
    public static LineChartOptions Line() => new()
    {
        MaxNumYAxisTicks = AxisTicks,
        LineStrokeWidth = 2,
        InterpolationOption = InterpolationOption.NaturalSpline,
    };

    /// <summary>
    /// <c>&lt;Area type="natural" fillOpacity={0.4}&gt;</c>: smooth lines over a fill in the series
    /// colour.
    /// </summary>
    public static LineChartOptions Area() => new()
    {
        MaxNumYAxisTicks = AxisTicks,
        LineStrokeWidth = 1,
        InterpolationOption = InterpolationOption.NaturalSpline,
        LineDisplayType = LineDisplayType.Area,
    };

    /// <summary>
    /// <c>&lt;Pie innerRadius={60}&gt;</c> in shadcn's donut examples: a ring 40% of the radius
    /// (MudBlazor's default is 25%).
    /// </summary>
    public static DonutChartOptions Donut() => new()
    {
        DonutRingRatio = 0.4,
    };
}
