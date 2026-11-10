#!/usr/bin/env node
// Browser smoke test for the published showcase: serves the static site, loads every URL of its
// sitemap.xml in Chromium and fails if a page throws a script error, shows Blazor's error bar
// (#blazor-error-ui), never renders, or ends up on the Not found page. A build cannot catch these:
// a ported docs page that uses a missing Docs/ parameter, or something the publish trimmed away,
// only fails when the page renders.
//
//   dotnet publish samples/MudShadcn.Showcase -c Release -o <dir>
//   npm install --no-save --no-package-lock playwright && npx playwright install chromium
//   node tools/smoke-test.mjs <dir>/wwwroot
//
// SMOKE_BROWSER_CHANNEL=chrome uses an installed Chrome instead of Playwright's Chromium, and
// SMOKE_CONCURRENCY sets how many pages load at once (4). Failed resource loads are not failures:
// the image page requests a missing picture on purpose. Exits non-zero if any page fails.
import fs from 'node:fs';
import http from 'node:http';
import path from 'node:path';
import process from 'node:process';
import { chromium } from 'playwright';

const root = path.resolve(process.argv[2] ?? '');
if (!process.argv[2] || !fs.existsSync(path.join(root, 'index.html')) || !fs.existsSync(path.join(root, 'sitemap.xml'))) {
    console.error('usage: node tools/smoke-test.mjs <published showcase>/wwwroot   (needs index.html and sitemap.xml)');
    process.exit(2);
}

const TYPES = {
    '.html': 'text/html; charset=utf-8', '.js': 'text/javascript', '.mjs': 'text/javascript', '.css': 'text/css',
    '.json': 'application/json', '.wasm': 'application/wasm', '.svg': 'image/svg+xml', '.png': 'image/png',
    '.jpg': 'image/jpeg', '.jpeg': 'image/jpeg', '.webp': 'image/webp', '.ico': 'image/x-icon', '.woff2': 'font/woff2',
    '.txt': 'text/plain; charset=utf-8', '.md': 'text/markdown; charset=utf-8', '.xml': 'application/xml',
    '.webmanifest': 'application/manifest+json',
};

// A static host with the single-page fallback Cloudflare Pages gives the site: a path that is not
// a file gets index.html, except under _framework and _content, where a missing file is a 404.
const server = http.createServer((request, response) => {
    const pathname = decodeURIComponent(new URL(request.url, 'http://localhost').pathname);
    let file = path.join(root, pathname);
    if (!file.startsWith(root)) {
        response.writeHead(403).end();
        return;
    }
    if (!fs.existsSync(file) || fs.statSync(file).isDirectory()) {
        if (/^\/(_framework|_content)\//.test(pathname)) {
            response.writeHead(404).end();
            return;
        }
        file = path.join(root, 'index.html');
    }
    response.writeHead(200, { 'Content-Type': TYPES[path.extname(file).toLowerCase()] ?? 'application/octet-stream' });
    fs.createReadStream(file).pipe(response);
});
await new Promise(resolve => server.listen(0, '127.0.0.1', resolve));
const origin = `http://127.0.0.1:${server.address().port}`;

const routes = [...fs.readFileSync(path.join(root, 'sitemap.xml'), 'utf8').matchAll(/<loc>https?:\/\/[^/<]+([^<]*)<\/loc>/g)]
    .map(match => match[1] || '/');
if (routes.length === 0) {
    console.error('sitemap.xml lists no URLs.');
    process.exit(2);
}

const browser = await chromium.launch({ channel: process.env.SMOKE_BROWSER_CHANNEL || undefined });
const context = await browser.newContext({ viewport: { width: 1280, height: 900 } });
const failures = [];

async function check(route) {
    const page = await context.newPage();
    const problems = [];
    page.on('pageerror', error => problems.push(`script error: ${error.message.split('\n')[0]}`));
    try {
        await page.goto(origin + route, { waitUntil: 'load', timeout: 60_000 });
        // The page's content (the home page has a hero instead) is there once Blazor has rendered.
        await page.locator('.docs-main :is(#docspage, .docs-hero)').first().waitFor({ state: 'visible', timeout: 45_000 });
        await page.waitForLoadState('networkidle', { timeout: 30_000 }).catch(() => { });
        await page.waitForTimeout(300);
        const title = await page.locator('.docs-page-title').first().innerText({ timeout: 1000 }).catch(() => '');
        if (route !== '/not-found' && title.trim() === 'Not found') {
            problems.push('rendered the Not found page');
        }
    } catch (error) {
        problems.push(`did not render: ${error.message.split('\n')[0]}`);
    }
    if (await page.locator('#blazor-error-ui').isVisible().catch(() => false)) {
        problems.push('Blazor reported an unhandled error (#blazor-error-ui is visible)');
    }
    await page.close();
    if (problems.length > 0) {
        failures.push({ route, problems });
    }
    console.log(`${problems.length > 0 ? 'FAIL' : 'ok  '}  ${route}${problems.map(problem => `\n        ${problem}`).join('')}`);
}

const queue = [...routes];
const workers = Math.max(1, Number(process.env.SMOKE_CONCURRENCY) || 4);
await Promise.all(Array.from({ length: workers }, async () => {
    for (let route = queue.shift(); route !== undefined; route = queue.shift()) {
        await check(route);
    }
}));

await browser.close();
server.close();

if (failures.length > 0) {
    console.error(`\n${failures.length} of ${routes.length} pages failed.`);
    process.exit(1);
}
console.log(`\nAll ${routes.length} pages rendered without errors.`);
