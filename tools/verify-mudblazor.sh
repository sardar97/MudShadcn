#!/usr/bin/env bash
# Checks the assumptions MudShadcn.css makes about MudBlazor, none of which a build would catch:
#
#   1. every --mud-* variable and .mud-* class the stylesheet uses exists in MudBlazor's stylesheet,
#      or is one of the classes MudBlazor puts in its markup without styling them itself. The same
#      goes for every style's stylesheet (styles/*.css next to it);
#   2. the chart colours the stylesheet recolours are still MudBlazor's defaults: the 20-colour
#      ChartOptions.ChartPalette and the five heat-map shades built from it;
#   3. the default checkbox, radio and select/autocomplete icons the stylesheet redraws still have
#      the SVG paths its selectors match;
#   4. the default line width the chart rules match (stroke-width="3") is still MudBlazor's;
#   5. bar, stacked bar and line charts still render the SVG the chart rules are written for: one
#      stroked path per bar, stacked segments in stack and series order, data markers without a fill;
#   6. every icon library's stylesheet (styles/icons-*.css) still matches the Material icons it
#      names, and only those: the path data of every selector is that of MudBlazor's icon.
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

// Every icon MudBlazor ships (Icons.Material.Filled.Close, …): the icon libraries' stylesheets
// recognise an icon by its paths, so a selector has to match the icons it names and no other.
// (The probe is not trimmed, so reflection over the icon classes is safe; a build warning would
// land in the output this script reads.)
#pragma warning disable IL2070, IL2072, IL2075
void WriteIcons(Type type, string name)
{
    foreach (var field in type.GetFields().Where(f => f.IsLiteral && f.FieldType == typeof(string)))
    {
        Console.WriteLine($"constant\t{name}.{field.Name}\t{((string)field.GetRawConstantValue()!).ReplaceLineEndings(" ").Replace('\t', ' ')}");
    }
    foreach (var nested in type.GetNestedTypes())
    {
        WriteIcons(nested, $"{name}.{nested.Name}");
    }
}
#pragma warning restore IL2070, IL2072, IL2075
WriteIcons(typeof(Icons), "Icons");
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
import glob
import os
import re
import sys

css_path, mud_css_path, defaults_path = sys.argv[1:]
def read_css(path):
    return re.sub(r"/\*.*?\*/", "", open(path, encoding="utf-8").read(), flags=re.S)
css = read_css(css_path)
mud_css = open(mud_css_path, encoding="utf-8").read()
# The styles (vega.css, nova.css, …) override the same MudBlazor rules, so the same check applies.
style_paths = sorted(path for path in glob.glob(os.path.join(os.path.dirname(css_path), "styles", "*.css"))
                     if not path.endswith(".min.css"))

# Classes MudBlazor renders in its markup without styling them in MudBlazor.min.css. Each one was
# confirmed in the rendered HTML; re-confirm any that the check below starts reporting as missing.
MARKUP_ONLY_CLASSES = {
    ".mud-avatar-filled-default", ".mud-chart-axis-value", ".mud-chart-heat", ".mud-chart-label-value",
    ".mud-chart-point", ".mud-chart-serie", ".mud-chart-serie-hovered", ".mud-charts-bar-series",
    ".mud-charts-gridlines-xaxis-lines", ".mud-charts-gridlines-yaxis", ".mud-checkbox-true",
    ".mud-chip-color-default", ".mud-fab-filled-default", ".mud-file-upload-filelist", ".mud-message-box",
    ".mud-pagination-text", ".mud-picker-paper", ".mud-picker-popover", ".mud-snackbar-action-button",
    ".mud-step-label-content-secondary-text", ".mud-toggle-item-selected",
}
# Variables MudThemeProvider writes that MudBlazor's own stylesheet never reads.
PROVIDER_ONLY_VARIABLES = {"--mud-palette-black"}

defaults = {"palette": {}, "heat": {}, "icon": {}, "constant": {}, "chart": {}, "markup": {}}
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
for style_path in style_paths:
    style_css = read_css(style_path)
    missing = sorted((set(re.findall(r"--mud-[a-zA-Z0-9-]+", style_css)) - mud_vars - PROVIDER_ONLY_VARIABLES)
                     | (set(re.findall(r"\.mud-[a-zA-Z0-9_-]+", style_css)) - mud_classes - MARKUP_ONLY_CLASSES))
    check(not missing, f"styles/{os.path.basename(style_path)}: every --mud-* variable and .mud-* class exists"
          + (": missing " + ", ".join(missing) if missing else ""))
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

print("6. Icon libraries")
def drawing(svg):
    """What an icon draws: its markup without the empty 24px box some icons start with."""
    return re.sub(r'<(path|rect)\b(?=[^>]*\bfill="none")[^>]*?/>|</?g>', "", svg).strip()
def child_paths(svg):
    """How many <path> elements are children of the icon's <svg>, not inside a <g>."""
    depth = count = 0
    for closing, element, self_closing in re.findall(r"<(/?)([a-zA-Z]+)[^>]*?(/?)>", svg):
        if closing:
            depth -= 1
            continue
        count += element == "path" and depth == 0
        depth += not self_closing
    return count
def matches(svg, paths, limit):
    """Whether a selector matches an icon: every :has(path[d="…"]) finds its path, and the icon has
    fewer child paths than the one :not(:has(> path:nth-of-type(n))) rules out."""
    return all(f'd="{d}"' in svg for d in paths) and child_paths(svg) < limit
icon_sheets = [path for path in style_paths if os.path.basename(path).startswith("icons-")]
check(bool(icon_sheets), "the icon libraries' stylesheets are there (styles/icons-*.css)")
for sheet_path in icon_sheets:
    # One selector per line; the line before it names the icons that are drawn that way:
    # /* Icons.Material.Filled.Close, Icons.Material.Filled.Clear */
    lines = open(sheet_path, encoding="utf-8").read().split("\n")
    def selector_of(line):
        return re.sub(r"( > \*)?(,| \{)?$", "", line) if line.startswith("svg.mud-icon-root:has(") else None
    by_selector = {selector_of(line): re.fullmatch(r"/\* (Icons\..+) \*/", before).group(1).split(", ")
                   for before, line in zip(lines, lines[1:])
                   if selector_of(line) and re.fullmatch(r"/\* (Icons\..+) \*/", before)}
    problems = [f"a selector names no icon: {selector[:60]}…"
                for selector in sorted({selector_of(line) for line in lines if selector_of(line)} - set(by_selector))]
    for selector, names in by_selector.items():
        paths = [d.replace('\\"', '"').replace("\\\\", "\\") for d in re.findall(r'path\[d="((?:[^"\\]|\\.)*)"\]', selector)]
        limit = re.search(r":not\(:has\(> path:nth-of-type\((\d+)\)\)\)", selector)
        limit = int(limit.group(1)) if limit else sys.maxsize
        gone = [n for n in names if n not in defaults["constant"]]
        changed = [n for n in names if n in defaults["constant"] and not matches(defaults["constant"][n], paths, limit)]
        # Another icon may be matched too if it is the same drawing, or the icon of the same name in
        # another of Material's sets (TwoTone.FilterAlt is Outlined.FilterAlt with a tint inside).
        drawings = {drawing(defaults["constant"][n]) for n in names if n in defaults["constant"]}
        same_icon = {n.rsplit(".", 1)[1] for n in names}
        others = sorted(n for n, svg in defaults["constant"].items()
                        if n not in names and n.rsplit(".", 1)[1] not in same_icon
                        and matches(svg, paths, limit) and drawing(svg) not in drawings)
        if gone:
            problems.append("MudBlazor no longer has " + ", ".join(gone))
        if changed:
            problems.append("no longer drawn with the selector's paths: " + ", ".join(changed))
        if others:
            problems.append(f"the selector of {names[0]} also matches " + ", ".join(others[:5]))
    check(not problems and bool(by_selector),
          f"styles/{os.path.basename(sheet_path)}: its {len(by_selector)} selectors match the Material icons they name, and only those"
          + ("" if not problems else ": " + "; ".join(problems)
             + ". Run tools/generate-shadcn-icons.mjs (UPDATING.md)"))

if failures:
    print(f"\n{len(failures)} check(s) failed. See UPDATING.md, step 3.")
    sys.exit(1)
print("\nAll checks passed.")
EOF
