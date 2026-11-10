# TODO

Open work. Highest value first. Tick an item off (or delete it) in the same commit that does it.

Ground rules, so a fix does not break something else:

- `src/MudShadcn/wwwroot/MudShadcn.css` is the only stylesheet to edit; `MudShadcn.min.css` is
  generated on build. Never set `--mud-palette-*`, `--mud-elevation-*`, `--mud-typography-*` or
  `--mud-default-borderradius` in it (they belong in `MudShadcnTheme.cs`). Read colours from
  `--mud-palette-*`; use `--shadcn-*` only where MudBlazor has no slot.
- Look a MudBlazor rule up before overriding it, and match or beat its specificity (see "Writing
  overrides" in the project notes and step 3 of `UPDATING.md`).
- Take values from shadcn's `new-york-v4` source and Tailwind's `theme.css`, not from memory, and
  keep the Tailwind utility in a trailing comment (`/* h-9 px-4 */`).
- `samples/MudShadcn.Showcase/Pages/Components/` is a copy of MudBlazor's docs: never edit it by hand.
- After every change: `dotnet build MudShadcn.sln` (0 warnings), `tools/verify-mudblazor.sh`, and a
  look at the change in the test app (`:5152`) and the showcase (`:5153`), in light **and** dark.
- New user-facing changes go under `## [Unreleased]` in `CHANGELOG.md`.

The list written while moving to MudBlazor 9.12.0 (v1.1.0) is done: the visual pass over every
showcase page in both modes, the date range picker's backwards preview, the chart items, the data
grid's width and the two CI checks. What that work left open:

## 1. Safari

Everything was checked in Chrome and, for the parts named here, Firefox. Not in Safari:

- [ ] Date range picker, one date picked: the first pick's sides are computed from MudBlazor's
      `--selected-day` / `--day-id` with `clamp()` inside `calc()`, in border widths, corner radii
      and `background-size` (section 18 of the stylesheet). Check a forwards and a backwards
      preview, also across two months.
- [ ] Charts: data markers set the circle's radius from CSS (`r: 2px`), and the stacked bar
      rounding uses `:has(+ …)` between SVG paths. Check "Line with dots" and "Stacked bar" on the
      showcase's Charts page.

## 2. Charts

- [ ] Stacked bars are rounded at the wrong ends when a stack has negative values (it hangs below
      the axis): the rules know the series order, not the direction. `--shadcn-chart-bar-radius: 0`
      is the way out today. A fix needs something in the markup that tells a negative segment
      apart; MudBlazor draws both as `M x y1 L x y2`.
- [ ] Stacked bars with a palette of your own stay square, because the series is read from the
      default palette's colours. The same would be needed for any stroke colour.

## 3. Date range picker

- [ ] After the pointer leaves the calendar (or while a finger rests on a day), the previewed band
      stays, as in MudBlazor, but the day it runs to is no longer drawn as the range's other end,
      so the band ends square there. It could be rounded from `--selected-day` like the first pick.

## 4. Showcase examples laid out for Material's sizes

MudBlazor's docs examples are copied as they are, and a few rely on MudBlazor's larger controls.
They look wrong in the showcase although the components are fine:

- [ ] Hotkey, "Child content": two checkboxes pulled together with `mt-n3` / `mt-n5` overlap,
      because a MudShadcn checkbox row is 20px tall, not 42px.
- [ ] Text Field, "Input Types": in a narrow frame the Color field is squeezed to a few pixels by
      the native date inputs next to it.

Decide whether `showcase.css` should correct these by example name, as it already does for the
examples that are too wide for a phone.
