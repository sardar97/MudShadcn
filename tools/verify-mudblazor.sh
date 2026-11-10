#!/usr/bin/env bash
# Checks the assumptions MudShadcn.css makes about MudBlazor, none of which a build would catch:
#
#   1. every --mud-* variable and .mud-* class the stylesheet uses exists in MudBlazor's stylesheet,
#      or is one of the classes MudBlazor puts in its markup without styling them itself;
#   2. the chart colours the stylesheet recolours are still MudBlazor's defaults: the 20-colour
#      ChartOptions.ChartPalette and the five heat-map shades built from it;
#   3. the default checkbox, radio and select/autocomplete icons the stylesheet redraws still have
#      the SVG paths its selectors match;
#   4. the default line width the chart rules match (stroke-width="3") is still MudBlazor's;
#   5. bar, stacked bar and line charts still render the SVG the chart rules are written for: one
#      stroked path per bar, stacked segments in stack and series order, data markers without a fill.
#
# Usage:
#   tools/verify-mudblazor.sh           # the MudBlazor version MudShadcn references
#   tools/verify-mudblazor.sh 9.11.0    # a version you are about to move to (see UPDATING.md)
#
# Needs the .NET 10 SDK, python3, curl and unzip, and network access to nuget.org. Exits non-zero
# if anything does not hold; the output says what to look at. MUDSHADCN_CSS=<path> checks another
# copy of the stylesheet instead of the one in the repository.
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
CSS="${MUDSHADCN_CSS:-$ROOT/src/MudShadcn/wwwroot/MudShadcn.css}"
VERSION="${1:-$(sed -nE 's/.*Include="MudBlazor" Version="([^"]+)".*/\1/p' "$ROOT/src/MudShadcn/MudShadcn.csproj")}"
WORK="$(mktemp -d)"
trap 'rm -rf "$WORK"' EXIT

echo "Checking ${CSS#"$ROOT"/} against MudBlazor $VERSION"

# MudBlazor's stylesheet, from the package.
curl -fsSL "https://api.nuget.org/v3-flatcontainer/mudblazor/$VERSION/mudblazor.$VERSION.nupkg" -o "$WORK/mudblazor.nupkg"
unzip -q -o "$WORK/mudblazor.nupkg" 'staticwebassets/MudBlazor.min.css' -d "$WORK"

# MudBlazor's defaults, read from the library itself rather than from its source.
mkdir -p "$WORK/probe"
sed "s/@VERSION@/$VERSION/" > "$WORK/probe/probe.cs" <<'EOF'
#:sdk Microsoft.NET.Sdk.Razor
#:package MudBlazor@@VERSION@
#:property TargetFramework=net10.0
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;
using MudBlazor;
using MudBlazor.Services;
using MudBlazor.Utilities;

var palette = new BarChartOptions().ChartPalette;
var heat = MudColor.GenerateMultiGradientPalette(palette.Select(c => new MudColor(c)).ToArray(), 5)
    .Select(c => c.ToString(MudColorOutputFormats.RGB)).ToArray();
var icons = new Dictionary<string, string>
{
    ["MudCheckBox.CheckedIcon"] = new MudCheckBox<bool>().CheckedIcon,
    ["MudCheckBox.UncheckedIcon"] = new MudCheckBox<bool>().UncheckedIcon,
    ["MudCheckBox.IndeterminateIcon"] = new MudCheckBox<bool>().IndeterminateIcon,
    ["MudRadio.CheckedIcon"] = new MudRadio<string>().CheckedIcon,
    ["MudRadio.UncheckedIcon"] = new MudRadio<string>().UncheckedIcon,
    ["MudSelect.OpenIcon"] = new MudSelect<string>().OpenIcon,
    ["MudSelect.CloseIcon"] = new MudSelect<string>().CloseIcon,
    ["MudAutocomplete.OpenIcon"] = new MudAutocomplete<string>().OpenIcon,
    ["MudAutocomplete.CloseIcon"] = new MudAutocomplete<string>().CloseIcon,
};

for (var i = 0; i < palette.Length; i++) Console.WriteLine($"palette\t{i + 1}\t{palette[i]}");
for (var i = 0; i < heat.Length; i++) Console.WriteLine($"heat\t{i + 1}\t{heat[i]}");
foreach (var (name, svg) in icons) Console.WriteLine($"icon\t{name}\t{svg}");
Console.WriteLine(FormattableString.Invariant($"chart\tLineStrokeWidth\t{new LineChartOptions().LineStrokeWidth}"));

// The SVG of a bar, a stacked bar and a line chart with data markers, rendered to a string. Three
// series over three labels; the second series has a zero in the middle.
var services = new ServiceCollection();
services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
services.AddMudServices();
services.AddSingleton<IJSRuntime, NoJsRuntime>();
await using var provider = services.BuildServiceProvider();
await using var renderer = new HtmlRenderer(provider, NullLoggerFactory.Instance);
foreach (var type in new[] { ChartType.Bar, ChartType.StackedBar, ChartType.Line })
{
    var parameters = new Dictionary<string, object?>
    {
        ["ChartType"] = type,
        ["ChartLabels"] = new[] { "x", "y", "z" },
        ["ChartSeries"] = new List<ChartSeries<double>>
        {
            new() { Name = "A", Data = new double[] { 1, 2, 3 } },
            new() { Name = "B", Data = new double[] { 3, 0, 1 } },
            new() { Name = "C", Data = new double[] { 2, 2, 2 } },
        },
    };
    if (type == ChartType.Line)
    {
        parameters["ChartOptions"] = new LineChartOptions { ShowDataMarkers = true };
    }

    var html = await renderer.Dispatcher.InvokeAsync(async () =>
        (await renderer.RenderComponentAsync<MudChart<double>>(ParameterView.FromDictionary(parameters))).ToHtmlString());
    Console.WriteLine($"markup\t{type}\t{html.ReplaceLineEndings(" ").Replace('\t', ' ')}");
}

// Static rendering never calls JavaScript, but the chart components ask for the service.
sealed class NoJsRuntime : IJSRuntime
{
    private const DynamicallyAccessedMemberTypes Json = DynamicallyAccessedMemberTypes.PublicConstructors
        | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties;

    public ValueTask<TValue> InvokeAsync<[DynamicallyAccessedMembers(Json)] TValue>(string identifier, object?[]? args) => default;

    public ValueTask<TValue> InvokeAsync<[DynamicallyAccessedMembers(Json)] TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) => default;
}
EOF
if ! (cd "$WORK/probe" && dotnet run probe.cs) > "$WORK/defaults.tsv" 2> "$WORK/probe.log"; then
    # The defaults are read through MudBlazor's API (BarChartOptions.ChartPalette,
    # MudColor.GenerateMultiGradientPalette, the components' icon properties, MudChart's
    # parameters). If that API changed, the probe above needs updating along with the stylesheet.
    cat "$WORK/defaults.tsv" "$WORK/probe.log" >&2
    echo "Could not read MudBlazor $VERSION's defaults: the probe in this script no longer compiles or runs." >&2
    : > "$WORK/defaults.tsv"
fi

python3 - "$CSS" "$WORK/staticwebassets/MudBlazor.min.css" "$WORK/defaults.tsv" <<'EOF'
import re
import sys

css_path, mud_css_path, defaults_path = sys.argv[1:]
css = re.sub(r"/\*.*?\*/", "", open(css_path, encoding="utf-8").read(), flags=re.S)
mud_css = open(mud_css_path, encoding="utf-8").read()

# Classes MudBlazor renders in its markup without styling them in MudBlazor.min.css. Each one was
# confirmed in the rendered HTML; re-confirm any that the check below starts reporting as missing.
MARKUP_ONLY_CLASSES = {
    ".mud-avatar-filled-default", ".mud-chart-axis-value", ".mud-chart-heat", ".mud-chart-label-value",
    ".mud-chart-point", ".mud-chart-serie", ".mud-chart-serie-hovered", ".mud-charts-bar-series",
    ".mud-charts-gridlines-xaxis-lines", ".mud-charts-gridlines-yaxis", ".mud-checkbox-true",
    ".mud-chip-color-default", ".mud-fab-filled-default", ".mud-file-upload-filelist",
    ".mud-pagination-text", ".mud-picker-paper", ".mud-picker-popover", ".mud-snackbar-action-button",
    ".mud-step-label-content-secondary-text", ".mud-toggle-item-selected",
}
# Variables MudThemeProvider writes that MudBlazor's own stylesheet never reads.
PROVIDER_ONLY_VARIABLES = {"--mud-palette-black"}

defaults = {"palette": {}, "heat": {}, "icon": {}, "chart": {}, "markup": {}}
for line in open(defaults_path, encoding="utf-8"):
    kind, key, value = line.rstrip("\n").split("\t", 2)
    defaults[kind][key] = value
have_defaults = all(defaults.values())

failures = []
def check(ok, message):
    print(("  ok    " if ok else "  FAIL  ") + message)
    if not ok:
        failures.append(message)

print("1. Variables and classes")
mud_vars = set(re.findall(r"--mud-[a-zA-Z0-9-]+", mud_css))
mud_classes = set(re.findall(r"\.mud-[a-zA-Z0-9_-]+", mud_css))
missing_vars = sorted(set(re.findall(r"--mud-[a-zA-Z0-9-]+", css)) - mud_vars - PROVIDER_ONLY_VARIABLES)
missing_classes = sorted(set(re.findall(r"\.mud-[a-zA-Z0-9_-]+", css)) - mud_classes - MARKUP_ONLY_CLASSES)
check(not missing_vars, "every --mud-* variable exists" + (": missing " + ", ".join(missing_vars) if missing_vars else ""))
check(not missing_classes, "every .mud-* class exists" + (": missing " + ", ".join(missing_classes) if missing_classes else ""))
gone = sorted(c for c in MARKUP_ONLY_CLASSES if c in mud_classes)
if gone:
    print("  note  now styled by MudBlazor too (can leave the markup-only list): " + ", ".join(gone))

if not have_defaults:
    check(False, "MudBlazor's chart and icon defaults could be read (see the probe error above)")
    print(f"\n{len(failures)} check(s) failed. See UPDATING.md, step 3.")
    sys.exit(1)

print("2. Chart colours")
mapped = {int(n): hex_.lower() for hex_, n in re.findall(
    r'\[fill="(#[0-9a-fA-F]{6})" i\]\s*\{\s*fill:\s*var\(--shadcn-chart-series-(\d+)\)', css)}
palette = {int(k): v.lower() for k, v in defaults["palette"].items()}
check(mapped == palette, f"the {len(mapped)} recoloured series colours are MudBlazor's {len(palette)}-colour default palette"
      + ("" if mapped == palette else f": stylesheet {mapped}, MudBlazor {palette}"))
heat_css = list(dict.fromkeys(re.findall(r'\.mud-chart-heat \[fill="(rgb\(\d+,\d+,\d+\))"\]', css)))
heat = [defaults["heat"][k] for k in sorted(defaults["heat"], key=int)]
check(heat_css == heat, "the heat-map ramp matches MudBlazor's default shades"
      + ("" if heat_css == heat else f": stylesheet {heat_css}, MudBlazor {heat}"))

print("3. Redrawn icons")
icon_paths = {name: re.findall(r'\bd="([^"]+)"', svg) for name, svg in defaults["icon"].items()}
all_paths = [p for paths in icon_paths.values() for p in paths]
selectors = sorted(set(re.findall(r'path\[d(\^?)="([^"]+)"\]', css)))
for prefix, value in selectors:
    ok = any(p.startswith(value) if prefix else p == value for p in all_paths)
    check(ok, f'path[d{prefix}="{value[:40]}{"…" if len(value) > 40 else ""}"] matches a default icon')
for name, paths in icon_paths.items():
    drawn = [p for p in paths if p != "M0 0h24v24H0z"]
    ok = any(any(p.startswith(v) if pre else p == v for pre, v in selectors) for p in drawn)
    check(ok, f"{name} is matched by a selector")

print("4. Chart line width")
line_widths = sorted(set(re.findall(r'path\.mud-chart-line\[stroke-width="([^"]+)"\]', css)))
default_width = defaults["chart"].get("LineStrokeWidth")
check(line_widths == [default_width], f"the 2px line rule matches MudBlazor's default LineStrokeWidth ({default_width})"
      + ("" if line_widths == [default_width] else f": stylesheet matches {line_widths}"))

print("5. Chart markup")
def attributes(tag):
    return dict(re.findall(r'([a-zA-Z-]+)="([^"]*)"', tag))
def series(chart, group, element):
    """The elements MudBlazor draws for a chart's series, as attribute dictionaries."""
    match = re.search(rf'<g class="{group}">(.*?)</g>', defaults["markup"].get(chart, ""), re.S)
    return [attributes(tag) for tag in re.findall(rf"<{element}\b[^>]*>", match.group(1))] if match else []
def vertical_line(path):
    """M x y1 L x y2: the single stroke MudBlazor draws a bar as. Returns x, or None."""
    match = re.fullmatch(r"M (\S+) \S+ L (\S+) \S+", path.get("d", ""))
    return match.group(1) if match and match.group(1) == match.group(2) else None
order = [palette[n] for n in sorted(palette)]

# Grouped bars: the rounding clips each bar to the box of its stroke.
bars = series("Bar", "mud-charts-bar-series", "path")
check(len(bars) == 9 and all(b.get("class") == "mud-chart-bar" and b.get("stroke-width") and b.get("fill", "none") != "none"
                             and vertical_line(b) for b in bars),
      "a bar is one stroked, filled path.mud-chart-bar in .mud-charts-bar-series")

# Stacked bars: the end rounding reads the series from the stroke colour and relies on the order.
segments = series("StackedBar", "mud-charts-stackedbar-series", "path")
stacks = {}
for segment in segments:
    stacks.setdefault(vertical_line(segment), []).append(segment.get("stroke", "").lower())
contiguous = [vertical_line(a) for a, b in zip(segments, segments[1:] + [{}]) if vertical_line(a) != vertical_line(b)]
check(len(segments) == 9 and None not in stacks and all(s.get("class") == "mud-chart-bar" and s.get("fill") == "none" for s in segments),
      'a stacked segment is one stroked path.mud-chart-bar with fill="none" in .mud-charts-stackedbar-series')
check(len(stacks) == 3 and len(contiguous) == 3 and all(colours == order[:3] for colours in stacks.values()),
      "stacked segments come stack after stack, each stack in series order, in the palette's colours"
      + ("" if segments else ": no segments found"))

# The stylesheet's stack rules name the palette's colours in order.
def colours(selector_list):
    return [c.lower() for c in re.findall(r'\[stroke="(#[0-9a-fA-F]{6})" i\]', selector_list)]
rounded = re.search(r'path\.mud-chart-bar\[fill="none"\]:is\(([^)]*)\)\s*\{\s*--_top', css)
rounded = colours(rounded.group(1)) if rounded else []
beyond = re.search(r'g:not\(:has\(> path:is\(([^)]*)\)\)\) > path\.mud-chart-bar\[fill="none"\]', css)
beyond = colours(beyond.group(1)) if beyond else []
tops = {c.lower(): colours(later) for c, later in re.findall(
    r'\[fill="none"\]\[stroke="(#[0-9a-fA-F]{6})" i\]:has\(\+ path:is\(([^)]*)\)\)', css)}
bottoms = {c.lower(): colours(earlier) for earlier, c in re.findall(
    r'path:is\(([^)]*)\) \+ path\.mud-chart-bar\[fill="none"\]\[stroke="(#[0-9a-fA-F]{6})" i\]', css)}
count = len(rounded)
check(count > 1 and rounded == order[:count] and beyond == order[count:]
      and tops == {order[i]: order[i + 1:count] for i in range(count - 1)}
      and bottoms == {order[j]: order[:j] for j in range(1, count)},
      f"the stack rounding rules follow the palette's order (its first {count} colours)")

# Data markers: the marker has no fill attribute, the hover target over it has one.
points = series("Line", "mud-charts-line-series", "circle")
markers = [p for p in points if "fill" not in p]
check(len(points) == 18 and len(markers) == 9 and all("mud-chart-point" in p.get("class", "") and p.get("stroke") for p in points)
      and all(p.get("opacity") == "0" for p in points if "fill" in p),
      "a data marker is a circle.mud-chart-point with a stroke and no fill, under a transparent hover target")

if failures:
    print(f"\n{len(failures)} check(s) failed. See UPDATING.md, step 3.")
    sys.exit(1)
print("\nAll checks passed.")
EOF
