using System.Globalization;
using Javaquero.Web.Models;
using Javaquero.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Javaquero.Web.Controllers;

[Authorize(Roles = "ADMIN")]
[AutoValidateAntiforgeryToken]
[ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
[Route("Admin/Documentos")]
public sealed class DocumentsController(SupabaseAdmin supabase) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        try
        {
            var data = await LoadCatalogAsync(ct);
            var quotes = await supabase.ReadDocumentsAsync<Quote[]>(
                "JV_COTIZACION_TB?select=id,cliente,creado_en,total&order=id.desc&limit=50", ct);
            return View(data with { Quotes = quotes });
        }
        catch (HttpRequestException)
        {
            return View(null);
        }
    }

    [HttpPost("Precio")]
    public async Task<IActionResult> Price(long productId, int grams, string amount, CancellationToken ct)
    {
        if (productId <= 0 || !DocumentData.Sizes.Contains(grams) ||
            !decimal.TryParse(amount, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var price) ||
            price <= 0 || price > 9999999999.99m || decimal.Round(price, 2) != price)
        {
            TempData["DocumentError"] = "Ingresa un precio positivo con hasta dos decimales.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            await supabase.SavePresentationPriceAsync(new(productId, grams, price), ct);
            TempData["DocumentStatus"] = "Precio guardado.";
        }
        catch (HttpRequestException)
        {
            TempData["DocumentError"] = "No se pudo guardar el precio.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("Lista")]
    public async Task<IActionResult> PriceList(string? harvest, CancellationToken ct)
    {
        ViewData["Harvest"] = harvest is { Length: <= 40 } ? harvest.Trim() : null;
        return View(await LoadCatalogAsync(ct));
    }

    [HttpPost("Cotizacion")]
    public async Task<IActionResult> CreateQuote(Guid requestId, string? customer, string[] items, int[] quantities, CancellationToken ct)
    {
        customer = customer?.Trim();
        if (!ModelState.IsValid || requestId == Guid.Empty || customer is not { Length: >= 1 and <= 120 } ||
            items.Length != quantities.Length || items.Length is < 1 or > 50)
        {
            TempData["DocumentError"] = "Indica el cliente y agrega al menos un producto válido.";
            return RedirectToAction(nameof(Index));
        }

        var lines = new List<object>();
        for (var i = 0; i < items.Length; i++)
        {
            var parts = items[i].Split(':');
            if (parts.Length != 2 || !long.TryParse(parts[0], out var productId) ||
                !int.TryParse(parts[1], out var grams) || quantities[i] is < 1 or > 999)
            {
                TempData["DocumentError"] = "Revisa los productos y las cantidades.";
                return RedirectToAction(nameof(Index));
            }

            lines.Add(new { producto_id = productId, gramos = grams, cantidad = quantities[i], posicion = i });
        }

        try
        {
            var id = await supabase.CreateQuoteAsync(requestId, customer, lines, ct);
            return RedirectToAction(nameof(QuotePreview), new { id });
        }
        catch (HttpRequestException)
        {
            TempData["DocumentError"] = "No se pudo guardar la cotización. Revisa los precios y vuelve a intentar.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet("Cotizacion/{id:long}")]
    public async Task<IActionResult> QuotePreview(long id, CancellationToken ct)
    {
        var quotes = await supabase.ReadDocumentsAsync<Quote[]>($"JV_COTIZACION_TB?id=eq.{id}", ct);
        return quotes.Length == 0 ? NotFound() : View(quotes[0]);
    }

    private async Task<DocumentData> LoadCatalogAsync(CancellationToken ct)
    {
        var products = supabase.ListProductsAsync(null, ct, activeOnly: true);
        var prices = supabase.ReadDocumentsAsync<PresentationPrice[]>("JV_PRODUCTO_PRECIO_TB", ct);
        await Task.WhenAll(products, prices);
        return new(await products, await prices, []);
    }
}
