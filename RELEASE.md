# Releasing MudShadcn

Two things ship from this repository, on separate triggers, so a showcase change never publishes a
package and a package release never forces a showcase rebuild.

| What | Trigger | Pipeline | Result |
| --- | --- | --- | --- |
| **NuGet package** | push a `v*` **tag** | `.github/workflows/publish.yml` | package on nuget.org + a GitHub Release |
| **Showcase website** | push to the **`docs`** branch | Cloudflare Pages runs `build.sh` | live site at <https://mudshadcn.pages.dev> |
| _(every commit)_ | push / pull request to **`master`** | `.github/workflows/ci.yml` | build and checks only, no deploy or publish |

## Branches and tags

- **`master`**: development. CI builds the solution with warnings as errors, runs
  `tools/verify-mudblazor.sh`, packs the library and publishes the showcase, on every push and pull
  request. It never deploys or publishes.
- **`docs`**: deploy-only; it is what the live site shows. Never commit to it directly: fast-forward
  it from `master` when the site should be updated. Cloudflare Pages watches this branch.
- **`v*` tags** (`v1.0.0`, `v1.1.0-beta.1`, …): each tag is one NuGet release. The tag **is** the
  published version (`publish.yml` passes it to `dotnet pack` as `-p:Version`).

---

## One-time setup

Do these once. After that, a release is only the steps further down.

### 0. Make the repository public

The repository is private at the moment. Make it public before the first release
(**Settings → General → Danger Zone → Change repository visibility**): the README that nuget.org
shows loads its logo and screenshots from `raw.githubusercontent.com`, the CI badge and the
package's "Source repository" link point at GitHub, and none of them work for a private
repository. Public repositories also get GitHub Actions minutes for free.

### 1. Create the `docs` branch

```bash
git switch master
git pull
git switch -c docs
git push -u origin docs
git switch master
```

### 2. Create the Cloudflare Pages project

In the Cloudflare dashboard, go to **Workers & Pages** and select **Create application → Pages →
Connect to Git**. Pick `sardar97/MudShadcn` (if it is not listed, give the Cloudflare Pages GitHub
app access to it through **Install & Authorize**), select **Begin setup**, and set:

- **Project name:** `mudshadcn`. It becomes `https://mudshadcn.pages.dev`; if that name is taken,
  Cloudflare adds a suffix, and the addresses under "Site address" below need changing.
- **Production branch:** `docs` (the branch must already be on GitHub, step 1).
- **Framework preset:** None.
- **Build command:** `bash build.sh`
- **Build output directory:** `publish/wwwroot`
- **Root directory (advanced):** leave empty.

Select **Save and Deploy**. Then, in the project's **Settings → Builds & deployments → Configure
Preview deployments**, choose **None**, so pushes to `master` and other branches do not build the
site. Keep **Enable automatic production branch deployments** ticked.

No GitHub secrets are needed for this: Cloudflare pulls from GitHub, and `build.sh` installs the
.NET SDK named in `global.json` itself.

To use a domain of your own later, buy one (Cloudflare Registrar sells at cost) and add it under the
project's **Custom domains**. `mudshadcn.pages.dev` keeps working.

### 3. Add the NuGet API key

1. Sign in at <https://www.nuget.org>, open **API Keys → Create**.
2. Key name `MudShadcn GitHub Actions`, expiry 365 days, scope **Push new packages and package
   versions**, glob pattern `MudShadcn`.
3. Copy the key, then in GitHub: **Settings → Secrets and variables → Actions → New repository
   secret**, name `NUGET_API_KEY`.

The key expires; when it does, create a new one and replace the secret. `GITHUB_TOKEN`, which
creates the GitHub Release, is provided automatically.

### 4. Fill in the repository's About box

On the GitHub repository page, the cog next to **About**:

- **Description:** `shadcn/ui look and feel for MudBlazor: a MudTheme and one stylesheet that restyle every MudBlazor component.`
- **Website:** `https://mudshadcn.pages.dev`
- **Topics:** `blazor`, `mudblazor`, `shadcn`, `shadcn-ui`, `theme`, `dotnet`, `csharp`,
  `blazor-webassembly`, `blazor-server`, `css`, `ui-components`, `nuget`
- Tick **Releases** and untick **Packages** and **Deployments** if you do not want them shown.

### Site address

The site's own address appears in a few files that search engines and AI tools read. If the site
does not end up at `https://mudshadcn.pages.dev` (a suffixed name or a custom domain), replace it
in `samples/MudShadcn.Showcase/wwwroot/` (`index.html`, `robots.txt`, `sitemap.xml`, `llms.txt`,
`skill.md`), in `README.md`, and in `PackageProjectUrl` in `src/MudShadcn/MudShadcn.csproj`:

```bash
grep -rl 'mudshadcn.pages.dev' --exclude-dir=bin --exclude-dir=obj .
```

---

## Releasing the showcase

The site redeploys whenever the `docs` branch moves. To publish the current `master`:

```bash
git fetch origin
git push origin origin/master:docs    # fast-forward docs to master
```

Watch it in Cloudflare under the project's **Deployments**.

- `docs` must only ever be fast-forwarded from `master`, never committed to.
- The site and the package are independent: redeploy the site any time without a package release,
  and release a package without touching the site. The site shows the changelog and the package
  version from `master` at the time it was built.

---

## Releasing the NuGet package

Versions follow [Semantic Versioning](https://semver.org). Moving to a new MudBlazor major version
is a new MudShadcn major version (see `UPDATING.md`). The git tag is the published version.

1. **Pick the version**, e.g. `1.1.0`. Pre-releases have a hyphen (`1.1.0-beta.1`, `1.2.0-rc.1`)
   and are marked as pre-releases on GitHub automatically.

2. **Update `CHANGELOG.md`.** Rename `## [Unreleased]` to `## [1.1.0] — YYYY-MM-DD` (today's date),
   start a new empty `## [Unreleased]` above it, and add the compare link at the bottom of the
   file. The GitHub Release notes are this version's section, verbatim, and the showcase's
   Releases page renders the same file.

3. **Set `<Version>` in `src/MudShadcn/MudShadcn.csproj`** to the same number. The tag overrides it
   in CI, but keeping them equal means a local `dotnet pack` gives the same version, and the
   showcase footer reads it from there.

4. **Commit to `master`** and wait for CI to pass:

   ```bash
   git add CHANGELOG.md src/MudShadcn/MudShadcn.csproj
   git commit -m "release: v1.1.0"
   git push
   ```

5. **Tag and push the tag:**

   ```bash
   git tag -a v1.1.0 -m "v1.1.0"
   git push origin v1.1.0
   ```

`publish.yml` then builds and checks everything again, packs the library with the tag's version,
pushes the `.nupkg` and its symbols (`.snupkg`) to nuget.org, and creates a GitHub Release with the
notes from `CHANGELOG.md` and the package files attached.

**Check:** <https://www.nuget.org/packages/MudShadcn> shows the new version (indexing takes a few
minutes) and the repository's **Releases** page shows `v1.1.0`.

### If something goes wrong

- **NuGet versions are permanent.** You cannot replace `1.1.0`. If a bad package ships,
  *unlist* it on nuget.org and release `1.1.1`.
- The push uses `--skip-duplicate`, so **re-running the failed workflow** (Actions → *Re-run
  jobs*) after a transient error does not fail on a version that is already up, and the release
  step updates the existing GitHub Release.
- To **drop a mistaken tag** before it published:

  ```bash
  git push origin :refs/tags/v1.1.0    # delete the remote tag
  git tag -d v1.1.0                    # delete the local tag
  ```

  Once the package is on nuget.org, bump the version instead.

---

## A full release (package and site together)

1. Merge the work into `master`; CI is green.
2. **Package:** update `CHANGELOG.md` and `<Version>`, commit, tag `vX.Y.Z`, push the tag.
3. **Site:** `git push origin origin/master:docs`.

Steps 2 and 3 are independent: do either, both, or neither, in any order.
