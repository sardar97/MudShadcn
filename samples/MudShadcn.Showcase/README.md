# MudShadcn showcase

A Blazor WebAssembly site that shows every MudBlazor component styled by MudShadcn. It is laid out
like [mudblazor.com](https://mudblazor.com): the same components menu in the same order, the same
pages, the same examples with their source underneath, and the "On this page" navigation. The pages
and examples are MudBlazor's own (see `THIRD-PARTY-NOTICES.md`), so what you see is exactly what a
MudBlazor app looks like after adding MudShadcn.

Like the test app, it references only the MudShadcn project. MudBlazor arrives transitively.

## Run it

```bash
dotnet run --project samples/MudShadcn.Showcase --launch-profile http   # http://localhost:5153
```

In development `MudShadcn.css` is served straight from `src/`, so stylesheet edits need only a
browser reload.

## Publish

```bash
dotnet publish samples/MudShadcn.Showcase -c Release -o publish
```

`publish/wwwroot` is a static site (about 30 MB, with pre-compressed `.br`/`.gz` copies). It needs
no server code: the sample data the examples request from `webapi/…` is answered in the browser.
Any static host works. Two things to set up for a single-page app:

- **Base path.** `wwwroot/index.html` has `<base href="/" />`. If the site is served from a
  sub-path, such as GitHub Pages at `https://<user>.github.io/MudShadcn/`, change it to
  `<base href="/MudShadcn/" />` before publishing (or rewrite it in the deployment step). All
  links in the site are relative, so nothing else changes.
- **Deep links.** Routes such as `/components/alert` exist only in the app, so the host must serve
  `index.html` for paths it does not know:
  - GitHub Pages: copy `index.html` to `404.html` in the published folder, and add an empty
    `.nojekyll` file so the `_framework` and `_content` folders are served.
  - Azure Static Web Apps: a `staticwebapp.config.json` with
    `"navigationFallback": { "rewrite": "/index.html" }`.
  - Netlify: a `_redirects` file containing `/* /index.html 200`.
  - Cloudflare Pages and Vercel serve `index.html` for unknown paths of a single-page app by
    default.

## Moving to a new MudBlazor version

After the package moves to a new MudBlazor version (`UPDATING.md`), re-port the docs:

```bash
samples/MudShadcn.Showcase/sync-mudblazor-docs.sh <version>
```

It replaces `Pages/Components`, `ExamplesData` and `wwwroot/images` with MudBlazor's versions and
re-applies the handful of edits the port needs. Then compare `Services/MenuService.cs` with
MudBlazor.Docs' `Services/Menu/MenuService.cs` (new components, renamed pages), build, and look at
every page.

## How it is put together

| Path | What |
|---|---|
| `Pages/Components/` | MudBlazor's docs pages and examples, verbatim. Do not edit; re-sync instead. |
| `Docs/` | Stand-ins for MudBlazor.Docs' page components (`DocsPage`, `SectionContent`, …) with the same names and parameters, so the pages compile unchanged. |
| `Services/MenuService.cs` | The components menu, in mudblazor.com's order. |
| `Services/ExampleSource.cs` | Highlights each example's source (embedded at build time) with ColorCode, as mudblazor.com does. |
| `Services/ExamplesApiHandler.cs` | Answers the examples' `webapi/…` requests in the browser. |
| `Services/ApiSummaries.cs` | Page subtitles: the type summaries from MudBlazor's XML docs, extracted at build time. |
| `Layout/`, `Pages/*.razor`, `wwwroot/css/showcase.css` | The site itself: app bar, sidebar, landing, installation and theme pages. |
