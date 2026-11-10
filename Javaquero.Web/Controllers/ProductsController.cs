using Javaquero.Web.Models;
using Javaquero.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Javaquero.Web.Controllers;

[Authorize(Roles = "ADMIN")]
[AutoValidateAntiforgeryToken]
[Route("Admin/Productos")]
public sealed class ProductsController(SupabaseAdmin supabase) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken ct) =>
        View(await supabase.ListProductsAsync(null, ct));

    [HttpGet("Nuevo")]
    public IActionResult Create() => View("Edit", new Product());

    [HttpPost("Nuevo")]
    public Task<IActionResult> Create(Product product, string? notasTexto, CancellationToken ct) =>
        Save(null, product, notasTexto, ct);

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Edit(long id, CancellationToken ct)
    {
        var product = (await supabase.ListProductsAsync(id, ct)).SingleOrDefault();
        return product is null ? NotFound() : View(product);
    }

    [HttpPost("{id:long}")]
    public Task<IActionResult> Edit(long id, Product product, string? notasTexto, CancellationToken ct) =>
        Save(id, product, notasTexto, ct);

    [HttpPost("{id:long}/Eliminar")]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        try
        {
            return await supabase.WriteProductAsync(HttpMethod.Delete, id, null, ct)
                ? RedirectToAction(nameof(Index))
                : NotFound();
        }
        catch (HttpRequestException)
        {
            TempData["ProductError"] = "No se pudo eliminar el producto.";
            return RedirectToAction(nameof(Index));
        }
    }

    private async Task<IActionResult> Save(long? id, Product product, string? notasTexto, CancellationToken ct)
    {
        product.Id = id ?? 0;
        product.Notas = (notasTexto ?? "").Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (!ModelState.IsValid)
            return View("Edit", product);

        try
        {
            return await supabase.WriteProductAsync(id is null ? HttpMethod.Post : HttpMethod.Patch, id, product, ct)
                ? RedirectToAction(nameof(Index))
                : NotFound();
        }
        catch (HttpRequestException ex)
        {
            ModelState.AddModelError(string.Empty, ex.StatusCode == System.Net.HttpStatusCode.Conflict
                ? "Ya existe un producto con ese slug."
                : "No se pudo guardar el producto en Supabase.");
            return View("Edit", product);
        }
    }
}
