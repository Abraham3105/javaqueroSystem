using System.ComponentModel.DataAnnotations;
using Javaquero.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Javaquero.Web.Controllers;

[Authorize(Roles = "ADMIN")]
[AutoValidateAntiforgeryToken]
[Route("Admin/Usuarios")]
public sealed class UsersController(SupabaseAdmin supabase) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        return View(await supabase.ListAsync(ct));
    }

    [HttpGet("Nuevo")]
    public IActionResult Create() => View("Edit", new AdminUserInput());

    [HttpPost("Nuevo")]
    public Task<IActionResult> Create(AdminUserInput input, CancellationToken ct) =>
        Save(input, true, ct);

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
    {
        var user = (await supabase.ListAsync(ct)).SingleOrDefault(user => user.Id == id);
        return user is null ? NotFound() : View(new AdminUserInput
        {
            Id = user.Id,
            Nombre = user.Nombre,
            Activo = user.Activo
        });
    }

    [HttpPost("{id:guid}")]
    public Task<IActionResult> Edit(Guid id, AdminUserInput input, CancellationToken ct)
    {
        input.Id = id;
        return Save(input, false, ct);
    }

    private async Task<IActionResult> Save(AdminUserInput input, bool create, CancellationToken ct)
    {
        if (input.Id == Guid.Empty)
            ModelState.AddModelError(nameof(input.Id), "Indica el UUID de la cuenta en Supabase Auth.");
        if (!ModelState.IsValid)
            return View("Edit", input);

        try
        {
            if (await supabase.SaveAsync(input.Id, input.Nombre, input.Activo, create, ct))
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError(string.Empty,
                "No se pudo guardar. Debe quedar al menos un administrador activo.");
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(string.Empty, "Supabase rechazó el cambio. Verifica el UUID y que el perfil todavía no exista si estás creando uno.");
        }

        return View("Edit", input);
    }
}

public sealed class AdminUserInput
{
    public Guid Id { get; set; }

    [Required, StringLength(120)]
    public string Nombre { get; set; } = "";

    public bool Activo { get; set; } = true;
}
