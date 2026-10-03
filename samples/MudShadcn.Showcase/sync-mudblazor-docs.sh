#!/usr/bin/env bash
# Re-ports the component pages and examples of mudblazor.com into this showcase.
#
#   ./sync-mudblazor-docs.sh 9.10.0
#
# Run it after moving MudShadcn to a new MudBlazor version (see UPDATING.md), then build: the
# showcase must build with 0 warnings and every page must render. It replaces Pages/Components,
# ExamplesData and wwwroot/images wholesale and re-applies the few edits the port needs, all listed
# below; nothing else in the showcase is touched. Requires git and network access to github.com.
set -euo pipefail

VERSION="${1:?usage: $0 <mudblazor version, e.g. 9.10.0>}"
HERE="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
WORK="$(mktemp -d)"
trap 'rm -rf "$WORK"' EXIT

git clone --quiet --depth 1 --branch "v$VERSION" --filter=blob:none --sparse \
    https://github.com/MudBlazor/MudBlazor.git "$WORK/mud"
git -C "$WORK/mud" sparse-checkout set \
    src/MudBlazor.Docs/Pages/Components \
    src/MudBlazor.Docs/Pages/Features/Masking/Examples \
    src/MudBlazor.Examples.Data \
    src/MudBlazor.Docs.Wasm/wwwroot/images
SRC="$WORK/mud/src"

# 1. Pages and examples, verbatim. TemplateComponent is MudBlazor's template for new pages.
rm -rf "$HERE/Pages/Components"
cp -r "$SRC/MudBlazor.Docs/Pages/Components" "$HERE/Pages/Components"
rm -rf "$HERE/Pages/Components/TemplateComponent"

# 2. The text field page shows an example that lives with the masking feature page.
cp "$SRC/MudBlazor.Docs/Pages/Features/Masking/Examples/PatternMaskExample.razor" "$HERE/Pages/Components/TextField/Examples/"

# 3. A static code sample that sits outside the Examples folders takes the showcase's namespace.
sed -i 's/@namespace MudBlazor\.Docs\.Pages\.Components\.Snackbar/@namespace MudShadcn.Showcase.Pages.Components.Snackbar/' \
    "$HERE/Pages/Components/Snackbar/ManualMarkdown/MarkdownSnackbarConfigurationService.razor"

# 4. Links in the page prose: relative links so the site works under a sub-path (GitHub Pages),
#    and mudblazor.com for sections this site does not have. Examples are left alone.
find "$HERE/Pages/Components" -name '*Page.razor' -print0 | xargs -0 sed -i -E \
    -e 's#Href="/(components|getting-started)/#Href="\1/#g' \
    -e 's#Href="/(features|customization|utilities|api|mud)/#Href="https://mudblazor.com/\1/#g' \
    -e 's#src="/iframe/#src="iframe/#g'

# 5. The Explore page injects the docs' menu service; the showcase has its own.
sed -i 's/\[Inject\] IMenuService MenuService/[Inject] MenuService MenuService/' "$HERE/Pages/Components/Overview/OverviewPage.razor"

# 6. One example injects MudBlazor's internal localizer, which only the docs project can see.
#    The public ILocalizationInterceptor returns the same string.
sed -i -e 's/@inject InternalMudLocalizer Localizer/@inject ILocalizationInterceptor LocalizationInterceptor/' \
       -e 's/@Localizer\[LanguageResource\.MudInput_Clear\]/@LocalizationInterceptor.Handle("MudInput_Clear").Value/' \
    "$HERE/Pages/Components/Field/Examples/FieldLabelPlaceholderExample.razor"

# 7. Sample data (MudBlazor.Examples.Data) and the photos the examples show.
rm -rf "$HERE/ExamplesData"
mkdir -p "$HERE/ExamplesData/Models"
cp "$SRC/MudBlazor.Examples.Data/"*.cs "$SRC/MudBlazor.Examples.Data/Elements.json" "$HERE/ExamplesData/"
cp "$SRC/MudBlazor.Examples.Data/Models/"*.cs "$HERE/ExamplesData/Models/"
for f in "$HERE/ExamplesData/"*.cs "$HERE/ExamplesData/Models/"*.cs; do
    { printf '// Ported from MudBlazor.Examples.Data (MudBlazor %s, MIT). See THIRD-PARTY-NOTICES.md.\n#nullable enable\n\n' "$VERSION"
      sed '1s/^\xEF\xBB\xBF//' "$f"; } > "$f.tmp" && mv "$f.tmp" "$f"
done
rm -rf "$HERE/wwwroot/images"
cp -r "$SRC/MudBlazor.Docs.Wasm/wwwroot/images" "$HERE/wwwroot/images"

# 8. The sitemap lists every page, so it changes with them.
"$HERE/generate-sitemap.sh"

# 9. Remind about the version-specific parts that need a human eye.
echo "Ported MudBlazor $VERSION docs. Now:"
echo "  - update the version in THIRD-PARTY-NOTICES.md and Services/MenuService.cs (diff MudBlazor.Docs/Services/Menu/MenuService.cs),"
echo "  - dotnet build MudShadcn.sln (0 warnings), and fix any example that no longer compiles,"
echo "  - open every page of the showcase in light and dark mode."
