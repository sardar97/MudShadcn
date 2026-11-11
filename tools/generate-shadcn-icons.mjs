#!/usr/bin/env node
// Generates src/MudShadcn/wwwroot/styles/icons-<library>.css: one stylesheet for each icon library
// of ui.shadcn.com/create (Lucide, Tabler Icons, HugeIcons, Phosphor Icons, Remix Icon). A sheet
// recognises the Material icons MudBlazor's components use by default, by their path data, and
// draws the library's icon in their place. MudShadcnProvider links the sheet of the theme's
// IconLibrary. It also writes the same table as C#, for the showcase's Icons page
// (samples/MudShadcn.Showcase/Services/IconLibraryData.g.cs).
//
//   node tools/generate-shadcn-icons.mjs
//
// It reads MudBlazor's icons from MudBlazor's source on GitHub, at the version MudShadcn
// references, and the libraries' icons from their npm packages on jsDelivr, at the versions pinned
// in LIBRARIES. Run it after moving to a new MudBlazor version (UPDATING.md), after changing a row
// of ICONS, and to take a newer version of a library. tools/verify-mudblazor.sh then checks every
// selector it wrote against the MudBlazor the package references.

import { readFile, writeFile } from "node:fs/promises";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const ROOT = join(dirname(fileURLToPath(import.meta.url)), "..");
const STYLES = join(ROOT, "src/MudShadcn/wwwroot/styles");
const SHOWCASE_DATA = join(ROOT, "samples/MudShadcn.Showcase/Services/IconLibraryData.g.cs");

// shadcn's icon libraries, by its names and in its order (packages/registry/src/icons/libraries.ts
// in shadcn-ui/ui). "member" is the library in ShadcnIconLibrary; the stylesheet's name is that in
// lower case. "file" is where an icon of ICONS is in the package: each library in the set shadcn
// uses, which is the outline one.
const LIBRARIES = [
    {
        name: "lucide", member: "Lucide", title: "Lucide", site: "https://lucide.dev",
        package: "lucide-static", version: "1.71.0", licence: "ISC",
        file: (icon) => `icons/${icon}.svg`,
    },
    {
        name: "tabler", member: "Tabler", title: "Tabler Icons", site: "https://tabler.io/icons",
        package: "@tabler/icons", version: "3.51.0", licence: "MIT",
        file: (icon) => `icons/${icon.includes("/") ? icon : `outline/${icon}`}.svg`,
    },
    {
        name: "hugeicons", member: "HugeIcons", title: "HugeIcons", site: "https://hugeicons.com",
        package: "@hugeicons/core-free-icons", version: "4.4.0", licence: "MIT",
        file: (icon) => `dist/esm/${icon}.js`,
    },
    {
        name: "phosphor", member: "Phosphor", title: "Phosphor Icons", site: "https://phosphoricons.com",
        package: "@phosphor-icons/core", version: "2.1.1", licence: "MIT",
        file: (icon) => `assets/${icon.endsWith("-fill") ? "fill" : "regular"}/${icon}.svg`,
    },
    {
        // 4.8.0 is the last version under Apache-2.0. From 4.9.0 the package ships the "Remix Icon
        // License", which limits how the icons may be redistributed (its package.json still says
        // Apache-2.0): read it, and update THIRD-PARTY-NOTICES.md, before moving past 4.8.0.
        name: "remixicon", member: "RemixIcon", title: "Remix Icon", site: "https://remixicon.com",
        package: "remixicon", version: "4.8.0", licence: "Apache-2.0",
        file: (icon) => `icons/${icon}.svg`,
    },
];

// One row per icon: the Material icons MudBlazor draws it with (names under MudBlazor.Icons), what
// MudBlazor uses it for (the showcase's Icons page shows that), then the icon of each library, in
// the order of LIBRARIES.
//
// - The names are shadcn's where its own components have the icon (the IconPlaceholder calls under
//   apps/v4/registry in shadcn-ui/ui name it in all five libraries). The others are the library's
//   icon of the same shape, because an icon is recognised wherever it is on the page, also in the
//   application's own markup.
// - ":filled" paints the inside of an outline icon too, for a library with no filled one (the full
//   star of a rating). The others have a filled icon of their own: "filled/…", "…-fill".
// - null: the library has no such icon, and MudBlazor's stays.
//
// The checkbox and radio icons are not here: MudShadcn.css draws those as shadcn's controls.
const ICONS = [
    [["Material.Filled.Close", "Material.Filled.Clear", "Material.Outlined.Close", "Material.Filled.Cancel"],
        "The close button of a dialog, an alert, a snackbar, a tab and a chip; the clear button of an input.",
        "x", "x", "Cancel01Icon", "x", "System/close-line"],
    [["Material.Filled.Check", "Material.Filled.Done", "Material.Outlined.Done"],
        "A selected chip and toggle item, a finished step, saving an edited table row.",
        "check", "check", "Tick02Icon", "check", "System/check-line"],
    [["Material.Filled.Add", "Material.Outlined.Add"],
        "Adding a tab or a data grid filter; the button of a FAB menu.",
        "plus", "plus", "PlusSignIcon", "plus", "System/add-line"],
    [["Material.Filled.Remove"],
        "Clearing the data grid's filters.",
        "minus", "minus", "MinusSignIcon", "minus", "System/subtract-line"],
    [["Material.Filled.ArrowDropDown", "Material.Filled.ExpandMore", "Material.Filled.KeyboardArrowDown"],
        "The arrow of a select, a nav group, an expansion panel and a grouped row; the down button of a numeric field.",
        "chevron-down", "chevron-down", "ArrowDown01Icon", "caret-down", "Arrows/arrow-down-s-line"],
    [["Material.Filled.ArrowDropUp", "Material.Filled.ExpandLess", "Material.Filled.KeyboardArrowUp"],
        "The arrow of an open select and an open list group; the up button of a numeric field.",
        "chevron-up", "chevron-up", "ArrowUp01Icon", "caret-up", "Arrows/arrow-up-s-line"],
    [["Material.Filled.ChevronLeft", "Material.Filled.NavigateBefore", "Material.Filled.ArrowLeft"],
        "Previous: pagers, the date picker, the carousel, tabs that scroll.",
        "chevron-left", "chevron-left", "ArrowLeft01Icon", "caret-left", "Arrows/arrow-left-s-line"],
    [["Material.Filled.ChevronRight", "Material.Filled.NavigateNext", "Material.Filled.ArrowRight"],
        "Next; the arrow of a tree view item, of a row that expands and of a nested menu.",
        "chevron-right", "chevron-right", "ArrowRight01Icon", "caret-right", "Arrows/arrow-right-s-line"],
    [["Material.Filled.FirstPage"],
        "First page; the stepper's reset button.",
        "chevrons-left", "chevrons-left", "ArrowLeftDoubleIcon", "caret-double-left", "Arrows/skip-left-line"],
    [["Material.Filled.LastPage"],
        "Last page.",
        "chevrons-right", "chevrons-right", "ArrowRightDoubleIcon", "caret-double-right", "Arrows/skip-right-line"],
    [["Material.Filled.ArrowRightAlt"],
        "Between the two dates of a date range picker.",
        "arrow-right", "arrow-right", "ArrowRight02Icon", "arrow-right", "Arrows/arrow-right-line"],
    [["Material.Filled.ArrowUpward"],
        "The sort direction of a table or data grid column.",
        "arrow-up", "arrow-up", "ArrowUp02Icon", "arrow-up", "Arrows/arrow-up-line"],
    [["Material.Filled.AccessTime"],
        "The time picker's field.",
        "clock", "clock", "Clock01Icon", "clock", "System/time-line"],
    [["Material.Filled.Event"],
        "The date picker's field.",
        "calendar", "calendar", "Calendar03Icon", "calendar-blank", "Business/calendar-line"],
    [["Material.Outlined.Info"],
        "An info alert or snackbar.",
        "info", "info-circle", "InformationCircleIcon", "info", "System/information-line"],
    [["Custom.Uncategorized.AlertSuccess"],
        "A success alert or snackbar.",
        "circle-check", "circle-check", "CheckmarkCircle02Icon", "check-circle", "System/checkbox-circle-line"],
    [["Material.Outlined.ReportProblem"],
        "A warning alert or snackbar.",
        "triangle-alert", "alert-triangle", "Alert02Icon", "warning", "System/alert-line"],
    [["Material.Filled.ErrorOutline"],
        "An error alert or snackbar.",
        "circle-alert", "exclamation-circle", "AlertCircleIcon", "warning-circle", "System/error-warning-line"],
    [["Material.Outlined.EventNote"],
        "An alert or snackbar of the normal severity.",
        "calendar-days", "calendar-event", "Calendar02Icon", "calendar-dots", "Business/calendar-event-line"],
    [["Material.Filled.StarBorder"],
        "An empty star of a rating.",
        "star", "star", "StarIcon", "star", "System/star-line"],
    [["Material.Filled.Star"],
        "A full star of a rating.",
        "star:filled", "filled/star", "StarIcon:filled", "star-fill", "System/star-fill"],
    [["Material.Filled.Search"],
        "The data grid's search field.",
        "search", "search", "Search01Icon", "magnifying-glass", "System/search-line"],
    [["Material.Outlined.FilterAlt"],
        "A data grid column without a filter.",
        "funnel", "filter", "FilterIcon", "funnel", "System/filter-line"],
    [["Material.Filled.FilterAlt"],
        "A data grid column with a filter.",
        "funnel:filled", "filled/filter", "FilterIcon:filled", "funnel-fill", "System/filter-fill"],
    [["Material.Filled.FilterAltOff"],
        "Clearing a data grid column's filter.",
        "funnel-x", "filter-x", "FilterRemoveIcon", "funnel-x", "System/filter-off-line"],
    [["Material.Filled.MoreVert"],
        "A data grid column's menu.",
        "ellipsis-vertical", "dots-vertical", "MoreVerticalCircle01Icon", "dots-three-vertical", "System/more-2-line"],
    [["Material.Outlined.Settings"],
        "The data grid's columns menu.",
        "settings", "settings", "Settings01Icon", "gear", "System/settings-line"],
    [["Material.Filled.AccountTree"],
        "The data grid's columns panel: group by a column.",
        "network", "sitemap", "HierarchyIcon", "tree-structure", "Editor/organization-chart"],
    [["Material.Filled.TableRows"],
        "The data grid's columns panel: stop grouping.",
        "rows-3", "layout-rows", "Rows3Icon", "rows", "Design/layout-row-line"],
    [["Material.Filled.DragIndicator"],
        "A drag handle.",
        "grip-vertical", "grip-vertical", "DragDropVerticalIcon", "dots-six-vertical", "Editor/draggable"],
    [["Material.Outlined.Edit"],
        "Editing a table row.",
        "pencil", "pencil", "Edit03Icon", "pencil", "Design/pencil-line"],
    [["Material.Filled.SettingsEthernet"],
        "The collapsed items of a breadcrumb.",
        "ellipsis", "dots", "MoreHorizontalCircle01Icon", "dots-three", "System/more-line"],
    [["Material.Filled.Loop"],
        "A tree view item that is loading.",
        "refresh-cw", "refresh", "RefreshIcon", "arrows-clockwise", "System/refresh-line"],
    [["Material.Filled.CloudUpload"],
        "The file upload's button.",
        "cloud-upload", "cloud-upload", "CloudUploadIcon", "cloud-arrow-up", "System/upload-cloud-line"],
    [["Material.Filled.Inbox"],
        "The file upload's drop zone.",
        "inbox", "inbox", "InboxIcon", "tray", "Business/inbox-line"],
    [["Material.Filled.Palette", "Material.Outlined.Palette"],
        "The colour picker's field and its palette button.",
        "palette", "palette", "PaintBoardIcon", "palette", "Design/palette-line"],
    [["Material.Filled.Apps"],
        "The colour picker's grid button.",
        "layout-grid", "layout-grid", "GridViewIcon", "grid-four", "Design/grid-line"],
    [["Material.Filled.Tune"],
        "The colour picker's sliders button.",
        "sliders-horizontal", "adjustments-horizontal", "SlidersHorizontalIcon", "sliders-horizontal", "Media/equalizer-line"],
    [["Material.Filled.ImportExport"],
        "The colour picker's button that switches between RGB and HSL.",
        "arrow-up-down", "arrows-up-down", "ArrowUpDownIcon", "arrows-down-up", "Arrows/arrow-up-down-line"],
    [["Material.Outlined.SkipNext"],
        "Skipping a step.",
        "skip-forward", "player-skip-forward", "NextIcon", "skip-forward", "Media/skip-forward-line"],
    [["Material.Outlined.PriorityHigh"],
        "A step with an error.",
        null, "exclamation-mark", "ExclamationMarkIcon", "exclamation-mark", null],
];

// The glyphs MudShadcn.css draws itself, as --shadcn-icon-<name>: the check and the dash of a
// checkbox, the check of a selected option, the arrow of a select. Each is the Lucide name of a
// row of ICONS, which gives the other libraries' icon.
const BASE_GLYPHS = ["check", "minus", "chevron-down", "chevron-up"];

async function fetchText(url) {
    const response = await fetch(url);
    if (!response.ok) {
        throw new Error(`${url}: ${response.status} ${response.statusText}`);
    }
    return response.text();
}

// ---------------------------------------------------------------------------------------------
// MudBlazor's icons: public const string Close = "<path d=\"M0 0h24v24H0z\" fill=\"none\"/><path …/>";
// ---------------------------------------------------------------------------------------------

const csproj = await readFile(join(ROOT, "src/MudShadcn/MudShadcn.csproj"), "utf8");
const mudVersion = /Include="MudBlazor" Version="([^"]+)"/.exec(csproj)?.[1];
if (!mudVersion) {
    throw new Error("MudShadcn.csproj: could not find the MudBlazor version");
}

const mudIconFiles = new Map();
async function mudIcon(name) {
    const [set, group, constant] = name.split(".");
    const file = `${set}/${group}.cs`;
    if (!mudIconFiles.has(file)) {
        const source = await fetchText(`https://raw.githubusercontent.com/MudBlazor/MudBlazor/v${mudVersion}/src/MudBlazor/Icons/${file}`);
        mudIconFiles.set(file, new Map([...source.matchAll(/public const string (\w+) = "((?:[^"\\]|\\.)*)";/g)]
            .map(([, key, value]) => [key, value.replace(/\\(.)/g, "$1")])));
    }
    const markup = mudIconFiles.get(file).get(constant);
    if (markup === undefined) {
        throw new Error(`MudBlazor ${mudVersion} has no Icons.${name}`);
    }
    return markup;
}

// The selector of a Material icon: the <svg> MudIcon renders, with every path the icon draws and no
// path more than the icon has (Material's ZoomIn is Search with a plus in it). A path without a fill
// is the icon's empty 24px box; a path inside a <g> is not a child of the <svg>. Everything after
// the first path is in :where(), so every selector has the same specificity.
function selector(name, markup) {
    const paths = [];
    let depth = 0;
    let children = 0;
    for (const [, closing, element, attributes, selfClosing] of markup.matchAll(/<(\/?)([a-zA-Z]+)([^>]*?)(\/?)>/g)) {
        if (closing) {
            depth--;
            continue;
        }
        if (element === "path" && depth === 0) {
            children++;
        }
        const d = /(?:^|\s)d="([^"]*)"/.exec(attributes)?.[1];
        if (element === "path" && d !== undefined && !/(?:^|\s)fill="none"/.test(attributes)) {
            paths.push(`:has(${depth === 0 ? "> " : ""}path[d="${d.replace(/[\\"]/g, "\\$&")}"])`);
        }
        if (!selfClosing) {
            depth++;
        }
    }
    if (paths.length === 0) {
        throw new Error(`Icons.${name} draws no path a selector could match: ${markup}`);
    }
    const [first, ...others] = paths;
    return `svg.mud-icon-root${first}:where(${others.join("")}:not(:has(> path:nth-of-type(${children + 1}))))`;
}

// ---------------------------------------------------------------------------------------------
// The libraries' icons, each as the attributes of its <svg> and the elements in it.
// ---------------------------------------------------------------------------------------------

const PRESENTATION = ["fill", "stroke", "stroke-width", "stroke-linecap", "stroke-linejoin"];
const attribute = (attributes, name) => new RegExp(`(?:^|\\s)${name}="([^"]*)"`).exec(attributes)?.[1];

// An SVG file. The <svg> carries how the icon is painted; an empty box (Tabler's first path) goes.
function parseSvg(source, url) {
    const svg = /<svg\b([^>]*)>([\s\S]*)<\/svg>/.exec(source.replace(/<!--[\s\S]*?-->/g, ""));
    if (!svg) {
        throw new Error(`${url}: not an SVG`);
    }
    const elements = [...svg[2].matchAll(/<[a-zA-Z][^>]*?\/?>/g)]
        .map(([element]) => element.replace(/\s+/g, " ").replace(/\s*\/?>$/, "/>"))
        .filter((element) => !/\sfill="none"/.test(element) || /\sstroke="(?!none)/.test(element));
    return {
        viewBox: attribute(svg[1], "viewBox"),
        paint: Object.fromEntries(PRESENTATION.map((name) => [name, attribute(svg[1], name)]).filter(([, value]) => value)),
        elements,
    };
}

// A HugeIcons module: const Tick02Icon = [["path", { d: "…", stroke: "currentColor", strokeWidth: "1.5", key: "0" }]];
function parseHugeicons(source, url) {
    const elements = [...source.matchAll(/\[\s*"([a-zA-Z]+)"\s*,\s*\{([^}]*)\}\s*\]/g)].map(([, element, properties]) => {
        const attributes = [...properties.matchAll(/([a-zA-Z]+):\s*"([^"]*)"/g)]
            .filter(([, key]) => key !== "key")
            .map(([, key, value]) => `${key.replace(/[A-Z]/g, (c) => `-${c.toLowerCase()}`)}="${value}"`);
        return `<${element} ${attributes.join(" ")}/>`;
    });
    if (elements.length === 0) {
        throw new Error(`${url}: no elements found`);
    }
    return { viewBox: "0 0 24 24", paint: { fill: "none" }, elements };
}

// What every element says the same way moves to the <svg>, which keeps the address short.
function hoist(icon) {
    for (const name of PRESENTATION) {
        const values = new Set(icon.elements.map((element) => attribute(element, name)));
        if (values.size === 1 && !values.has(undefined) && (icon.paint[name] ?? [...values][0]) === [...values][0]) {
            icon.paint[name] = [...values][0];
            icon.elements = icon.elements.map((element) => element.replace(new RegExp(`\\s${name}="[^"]*"`), ""));
        }
    }
    return icon;
}

// The icon as the address of a mask: a mask only reads the shape, so the colour is black.
function maskUrl(icon, filled) {
    const paint = { ...icon.paint, ...(filled ? { fill: "currentColor" } : {}) };
    const attributes = [`viewBox="${icon.viewBox}"`, ...Object.entries(paint).map(([name, value]) => `${name}="${value}"`)];
    const svg = `<svg xmlns="http://www.w3.org/2000/svg" ${attributes.join(" ")}>${icon.elements.join("")}</svg>`;
    return `url("data:image/svg+xml,${svg
        .replaceAll("currentColor", "#000")
        .replaceAll('"', "'")
        .replaceAll("%", "%25")
        .replaceAll("<", "%3C")
        .replaceAll(">", "%3E")
        .replaceAll("#", "%23")}")`;
}

async function libraryIcon(library, spec) {
    const [icon, filled] = spec.split(":");
    const url = `https://cdn.jsdelivr.net/npm/${library.package}@${library.version}/${library.file(icon)}`;
    const source = await fetchText(url);
    return maskUrl(hoist(library.name === "hugeicons" ? parseHugeicons(source, url) : parseSvg(source, url)), filled === "filled");
}

// ---------------------------------------------------------------------------------------------
// The stylesheets.
// ---------------------------------------------------------------------------------------------

// Material icons that are drawn the same way (Close and Clear) share a selector.
const rows = [];
for (const [names, use, ...icons] of ICONS) {
    const shapes = new Map();
    for (const name of names) {
        const shape = selector(name, await mudIcon(name));
        shapes.set(shape, [...(shapes.get(shape) ?? []), `Icons.${name}`]);
    }
    rows.push({ names, use, shapes, icons });
}

for (const [index, library] of LIBRARIES.entries()) {
    const drawn = [];
    for (const row of rows) {
        if (row.icons[index] !== null) {
            drawn.push({ ...row, icon: row.icons[index], mask: await libraryIcon(library, row.icons[index]) });
        }
    }
    const selectors = drawn.flatMap((row) => [...row.shapes.keys()]);
    const left = rows.filter((row) => row.icons[index] === null).flatMap((row) => [...row.shapes.values()].flat());

    const css = [`/*
 * icons-${library.name}.css — shadcn/create's "${library.title}" icon library for MudShadcn: the icons
 * MudBlazor's components draw by default (the close button of a dialog, the arrows of a pager and
 * of a date picker, the icons of an alert, …), drawn with ${library.title} instead of Material.
 * MudShadcnProvider links it, after MudShadcn.css, the style's stylesheet and the menu options,
 * when the theme's IconLibrary is ${library.member}. Written against MudBlazor ${mudVersion}.
 *
 * Generated by tools/generate-shadcn-icons.mjs from ${library.package} ${library.version}
 * (${library.site}, ${library.licence} licence: see THIRD-PARTY-NOTICES.md). Do not edit it by hand:
 * change the table in the tool and run it again.
 *
 * In MudBlazor an icon is a string of SVG paths, not a name, so an icon is recognised by its path
 * data, wherever it is: the same Material icon in the application's own markup is redrawn too. An
 * icon that is not listed here stays as it is.${left.length ? `
 * ${library.title} has no icon for ${left.join(", ")}: MudBlazor's stays.` : ""}
 */
`];

    if (library.name === "lucide") {
        css.push(`/* The check and the dash of a checkbox and the arrow of a select are Lucide's in MudShadcn.css
   already (--shadcn-icon-check, -minus, -chevron-down, -chevron-up). */
`);
    } else {
        css.push(`/* The glyphs MudShadcn.css draws itself: the check and the dash of a checkbox, the check of a
   selected option, the arrow of a select. */
:root {
${BASE_GLYPHS.map((glyph) => {
        const row = drawn.find((r) => r.icons[0] === glyph);
        return `    --shadcn-icon-${glyph}: ${row.mask};    /* ${row.icon} */`;
    }).join("\n")}
}
`);
    }

    css.push(`/* Every icon below: the library's icon is a mask over the icon's own colour, at the icon's own
   size, and MudBlazor's paths are not drawn. */
${selectors.join(",\n")} {
    background-color: currentColor;
    -webkit-mask: var(--shadcn-icon) center / contain no-repeat;
    mask: var(--shadcn-icon) center / contain no-repeat;
}

${selectors.map((s) => `${s} > *`).join(",\n")} {
    display: none;
}
`);

    for (const row of drawn) {
        const shapes = [...row.shapes].map(([shape, names]) => `/* ${names.join(", ")} */\n${shape}`);
        css.push(`/* ${row.icon} */
${shapes.join(",\n")} {
    --shadcn-icon: ${row.mask};
}
`);
    }

    const path = join(STYLES, `icons-${library.name}.css`);
    await writeFile(path, css.join("\n"));
    console.log(`Wrote styles/icons-${library.name}.css: ${drawn.length} icons for ${selectors.length} Material shapes (${library.package} ${library.version})`);
}

// ---------------------------------------------------------------------------------------------
// The same table for the showcase's Icons page.
// ---------------------------------------------------------------------------------------------

// An icon's name as its library's site has it: without this tool's marks and Remix Icon's folder.
const displayName = (icon) => icon === null ? "null"
    : `"${icon.replace(/^filled\/(.*)$/, "$1 (filled)").replace(/:filled$/, " (filled)").replace(/^[A-Z][a-z]+\//, "")}"`;
const csharp = (text) => `"${text.replace(/[\\"]/g, "\\$&")}"`;

await writeFile(SHOWCASE_DATA, `// <auto-generated>
// Generated by tools/generate-shadcn-icons.mjs, from the table the icon libraries' stylesheets are
// made of. Do not edit it by hand: change the table in the tool and run it again.
// </auto-generated>
#nullable enable

namespace MudShadcn.Showcase.Services;

/// <summary>One of shadcn's icon libraries, as the package it is taken from.</summary>
public sealed record IconLibraryInfo(ShadcnIconLibrary Library, string Title, string Site, string Package, string Version, string Licence);

/// <summary>
/// An icon the icon libraries redraw: what MudBlazor uses it for, the Material icons MudBlazor draws
/// it with (name and SVG), and its name in each library, in the order of
/// <see cref="ShadcnIconLibrary"/> (<c>null</c> where the library has no such icon).
/// </summary>
public sealed record IconLibraryIcon(string Use, IReadOnlyList<(string Name, string Svg)> Material, IReadOnlyList<string?> Names);

/// <summary>What the icon libraries' stylesheets (styles/icons-*.css) are made of, for the Icons page.</summary>
public static class IconLibraryData
{
    public static IReadOnlyList<IconLibraryInfo> Libraries { get; } =
    [
${LIBRARIES.map((l) => `        new(ShadcnIconLibrary.${l.member}, ${csharp(l.title)}, ${csharp(l.site)}, ${csharp(l.package)}, ${csharp(l.version)}, ${csharp(l.licence)}),`).join("\n")}
    ];

    public static IReadOnlyList<IconLibraryIcon> Rows { get; } =
    [
${rows.map((row) => `        new(${csharp(row.use)},
            [${row.names.map((name) => `("Icons.${name}", MudBlazor.Icons.${name})`).join(", ")}],
            [${row.icons.map(displayName).join(", ")}]),`).join("\n")}
    ];
}
`);
console.log(`Wrote ${SHOWCASE_DATA.slice(ROOT.length + 1)}: ${rows.length} icons`);
