#!/usr/bin/env bash
# Tests the package the way a consumer gets it. The samples reference the MudShadcn project, which
# cannot show a file missing from the .nupkg or MudBlazor no longer flowing through it. This script
# points a copy of the test app at the packed MudShadcn (a PackageReference, restored from a local
# feed), publishes and starts it, and checks that the page renders and that both stylesheets are
# served: _content/MudBlazor/MudBlazor.min.css and _content/MudShadcn/MudShadcn.min.css.
#
# Usage:
#   tools/test-package.sh              # packs src/MudShadcn first
#   tools/test-package.sh artifacts    # tests the MudShadcn.<version>.nupkg already in that folder
#
# Needs the .NET 10 SDK, curl and network access to nuget.org (for MudBlazor). Everything happens
# in a temporary folder, with its own NuGet package cache, so nothing is packed into the repository
# and the test package never lands in ~/.nuget/packages. Exits non-zero if anything does not hold.
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
# The physical path: on macOS the temporary folder is behind a symlink, which the Razor compiler
# does not see through when it derives namespaces from file paths.
WORK="$(cd "$(mktemp -d)" && pwd -P)"
APP_PID=""
cleanup() {
    if [ -n "$APP_PID" ]; then
        kill "$APP_PID" 2> /dev/null || true
        wait "$APP_PID" 2> /dev/null || true
    fi
    rm -rf "$WORK"
}
trap cleanup EXIT

export DOTNET_NOLOGO=1 DOTNET_CLI_TELEMETRY_OPTOUT=1
export NUGET_PACKAGES="$WORK/packages"

# 1. The package: the one given, or a fresh pack.
mkdir -p "$WORK/feed"
if [ $# -ge 1 ]; then
    cp "$1"/MudShadcn.*.nupkg "$WORK/feed/"
else
    echo "==> Packing src/MudShadcn"
    dotnet pack "$ROOT/src/MudShadcn" -c Release -o "$WORK/feed" > "$WORK/pack.log" || { cat "$WORK/pack.log"; exit 1; }
fi
rm -f "$WORK/feed"/*.symbols.nupkg
PACKAGE="$(find "$WORK/feed" -name 'MudShadcn.*.nupkg' | head -n 1)"
[ -n "$PACKAGE" ] || { echo "No MudShadcn.<version>.nupkg found." >&2; exit 1; }
VERSION="$(basename "$PACKAGE" .nupkg)"
VERSION="${VERSION#MudShadcn.}"
echo "==> Testing MudShadcn $VERSION as a package"

# 2. A copy of the test app that references the package instead of the project. MudShadcn may only
#    come from the local feed: nuget.org has the released versions under the same numbers.
mkdir -p "$WORK/app"
(cd "$ROOT/samples/MudShadcn.TestApp" && tar -cf - --exclude ./bin --exclude ./obj .) | tar -xf - -C "$WORK/app"
cp "$ROOT/global.json" "$WORK/"
PROJECT="$WORK/app/MudShadcn.TestApp.csproj"
sed -E "s#<ProjectReference Include=\"[^\"]*MudShadcn\.csproj\" */>#<PackageReference Include=\"MudShadcn\" Version=\"$VERSION\" />#" \
    "$PROJECT" > "$PROJECT.tmp" && mv "$PROJECT.tmp" "$PROJECT"
grep -q "<PackageReference Include=\"MudShadcn\" Version=\"$VERSION\" />" "$PROJECT" \
    || { echo "Could not replace the test app's ProjectReference with a PackageReference." >&2; exit 1; }
if grep -qi 'Include="MudBlazor"' "$PROJECT"; then
    echo "The test app references MudBlazor itself; it must get it through MudShadcn." >&2
    exit 1
fi
cat > "$WORK/nuget.config" <<EOF
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="local" value="$WORK/feed" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
  <packageSourceMapping>
    <packageSource key="local">
      <package pattern="MudShadcn" />
    </packageSource>
    <packageSource key="nuget.org">
      <package pattern="*" />
    </packageSource>
  </packageSourceMapping>
</configuration>
EOF

echo "==> Publishing a copy of the test app against the package"
dotnet publish "$PROJECT" -c Release -o "$WORK/out" > "$WORK/publish.log" || { cat "$WORK/publish.log"; exit 1; }

# 3. Run it and ask for what a consumer's page asks for.
echo "==> Starting it"
(cd "$WORK/out" && exec dotnet MudShadcn.TestApp.dll --urls http://127.0.0.1:0) > "$WORK/app.log" 2>&1 &
APP_PID=$!
URL=""
for _ in $(seq 1 60); do
    URL="$(sed -nE 's#.*Now listening on: (http://[^ ]+).*#\1#p' "$WORK/app.log" | head -n 1)"
    [ -n "$URL" ] && break
    kill -0 "$APP_PID" 2> /dev/null || break
    sleep 0.5
done
[ -n "$URL" ] || { cat "$WORK/app.log"; echo "The app did not start." >&2; exit 1; }

FAILED=0
check() {
    if "${@:2}"; then
        echo "  ok    $1"
    else
        echo "  FAIL  $1"
        FAILED=1
    fi
}
served() {      # <path> <text the file must contain>
    curl -fsS "$URL/$1" -o "$WORK/response" && grep -q -- "$2" "$WORK/response"
}

check "the home page renders MudBlazor components" served "" 'class="mud-'
check "_content/MudBlazor/MudBlazor.min.css is served" served "_content/MudBlazor/MudBlazor.min.css" '\.mud-button-root'
check "_content/MudShadcn/MudShadcn.min.css is served" served "_content/MudShadcn/MudShadcn.min.css" '--shadcn-radius'
check "_content/MudShadcn/MudShadcn.css is served" served "_content/MudShadcn/MudShadcn.css" '--shadcn-radius'
check "_content/MudBlazor/MudBlazor.min.js is served" served "_content/MudBlazor/MudBlazor.min.js" 'mud'

if [ "$FAILED" -ne 0 ]; then
    echo
    echo "The package does not work as a single reference. The app's log:"
    cat "$WORK/app.log"
    exit 1
fi
echo
echo "MudShadcn $VERSION works as a single package reference."
