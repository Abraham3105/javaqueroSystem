import { cp, mkdir, readFile, writeFile } from "node:fs/promises";

const root = new URL("../", import.meta.url);
const web = new URL("Javaquero.Web/", root);
const output = new URL("cloudflare-dist/", root);
const source = await readFile(new URL("Views/Home/Index.cshtml", web), "utf8");
const start = source.indexOf("@if (Model is null)");
const end = source.indexOf("</section>", start);
if (start < 0 || end < 0) throw new Error("No se encontró el catálogo en la vista pública.");

const catalog = `<p id="catalog-status" role="status">Cargando cafés…</p>
    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-space-lg w-full" id="products-list"></div>
  `;
const html = (source.slice(0, start) + catalog + source.slice(end))
  .replace(/^\s*@model Product\[\]\s*\r?\n@\{ Layout = null; \}\s*/, "")
  .replaceAll(' asp-append-version="true"', "")
  .replace('asp-controller="Admin" asp-action="Index"', 'href="https://javaquerosystem.onrender.com/Admin"')
  .replace("</body>", '<script src="/js/catalog.js" defer></script>\n</body>');

await mkdir(new URL("js/", output), { recursive: true });
await Promise.all([
  writeFile(new URL("index.html", output), html),
  cp(new URL("wwwroot/css/style.css", web), new URL("css/style.css", output)),
  cp(new URL("wwwroot/img/", web), new URL("img/", output), { recursive: true }),
  cp(new URL("wwwroot/favicon.ico", web), new URL("favicon.ico", output)),
  cp(new URL("wwwroot/js/tailwind-config.js", web), new URL("js/tailwind-config.js", output)),
  cp(new URL("wwwroot/js/main.js", web), new URL("js/main.js", output)),
  cp(new URL("cloudflare/catalog.js", root), new URL("js/catalog.js", output)),
]);
