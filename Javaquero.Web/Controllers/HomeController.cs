using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Javaquero.Web.Models;
using Javaquero.Web.Services;

namespace Javaquero.Web.Controllers;

public class HomeController(SupabaseAdmin supabase, ILogger<HomeController> logger) : Controller
{
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        try
        {
            return View(await supabase.ListProductsAsync(null, ct, activeOnly: true));
        }
        catch (Exception ex) when (ex is HttpRequestException or InvalidOperationException or System.Text.Json.JsonException ||
                                  ex is OperationCanceledException && !ct.IsCancellationRequested)
        {
            logger.LogWarning(ex, "No se pudo cargar el catálogo público.");
            return View();
        }
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
