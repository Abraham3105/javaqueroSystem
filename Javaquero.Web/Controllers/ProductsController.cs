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
    public Task<IActionResult> Create(Product product, string? notasTexto, IFormFile? foto, CancellationToken ct) =>
        Save(null, product, notasTexto, foto, ct);

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Edit(long id, CancellationToken ct)
    {
        var product = (await supabase.ListProductsAsync(id, ct)).SingleOrDefault();
        return product is null ? NotFound() : View(product);
    }

    [HttpPost("{id:long}")]
    public Task<IActionResult> Edit(long id, Product product, string? notasTexto, IFormFile? foto, CancellationToken ct) =>
        Save(id, product, notasTexto, foto, ct);

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

    private async Task<IActionResult> Save(long? id, Product product, string? notasTexto, IFormFile? foto, CancellationToken ct)
    {
        product.Id = id ?? 0;
        product.Notas = (notasTexto ?? "").Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        product.Imagen = "";
        if (id is long productId)
        {
            var current = (await supabase.ListProductsAsync(productId, ct)).SingleOrDefault();
            if (current is null)
                return NotFound();
            product.Imagen = current.Imagen;
        }

        if (id is null && foto is null)
            ModelState.AddModelError("foto", "Selecciona una imagen del café.");

        if (!ModelState.IsValid)
            return View("Edit", product);

        try
        {
            if (foto is not null)
            {
                product.Imagen = await supabase.UploadProductImageAsync(foto, ct);
            }

            return await supabase.WriteProductAsync(id is null ? HttpMethod.Post : HttpMethod.Patch, id, product, ct, writeImage: foto is not null)
                ? RedirectToAction(nameof(Index))
                : NotFound();
        }
        catch (InvalidDataException ex)
        {
            ModelState.AddModelError("foto", ex.Message);
            return View("Edit", product);
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
