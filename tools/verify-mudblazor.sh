#!/usr/bin/env bash
# Checks the assumptions MudShadcn.css makes about MudBlazor, none of which a build would catch:
#
#   1. every --mud-* variable and .mud-* class the stylesheet uses exists in MudBlazor's stylesheet,
#      or is one of the classes MudBlazor puts in its markup without styling them itself;
#   2. the chart colours the stylesheet recolours are still MudBlazor's defaults: the 20-colour
#      ChartOptions.ChartPalette and the five heat-map shades built from it;
#   3. the default checkbox, radio and select/autocomplete icons the stylesheet redraws still have
#      the SVG paths its selectors match.
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
using MudBlazor;
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
EOF
if ! (cd "$WORK/probe" && dotnet run probe.cs) > "$WORK/defaults.tsv" 2> "$WORK/probe.log"; then
    # The defaults are read through MudBlazor's API (BarChartOptions.ChartPalette,
    # MudColor.GenerateMultiGradientPalette, the components' icon properties). If that API changed,
    # the probe above needs updating along with the stylesheet.
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
    ".mud-chart-point", ".mud-chart-serie", ".mud-chart-serie-hovered", ".mud-charts-gridlines-xaxis-lines",
    ".mud-charts-gridlines-yaxis", ".mud-checkbox-true", ".mud-chip-color-default", ".mud-fab-filled-default",
    ".mud-file-upload-filelist", ".mud-pagination-text", ".mud-picker-paper", ".mud-picker-popover",
    ".mud-snackbar-action-button", ".mud-step-label-content-secondary-text", ".mud-toggle-item-selected",
}
# Variables MudThemeProvider writes that MudBlazor's own stylesheet never reads.
PROVIDER_ONLY_VARIABLES = {"--mud-palette-black"}

defaults = {"palette": {}, "heat": {}, "icon": {}}
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

if failures:
    print(f"\n{len(failures)} check(s) failed. See UPDATING.md, step 3.")
    sys.exit(1)
print("\nAll checks passed.")
EOF
