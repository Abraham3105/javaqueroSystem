using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Javaquero.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Javaquero.Web.Controllers;

[Authorize(Roles = "ADMIN")]
[AutoValidateAntiforgeryToken]
public sealed class AdminController(SupabaseAdmin supabase) : Controller
{
    public IActionResult Index() => View();

    [AllowAnonymous]
    public IActionResult Login() => View(new AdminLogin());

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Login(AdminLogin input, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(input);

        try
        {
            var id = await supabase.SignInAsync(input.Email, input.Password, cancellationToken);
            if (id is Guid userId && await supabase.IsActiveAdminAsync(userId, cancellationToken))
            {
                var identity = new ClaimsIdentity(
                    [new Claim(ClaimTypes.NameIdentifier, userId.ToString()), new Claim(ClaimTypes.Role, "ADMIN")],
                    CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(new ClaimsPrincipal(identity));
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, "No se pudo iniciar sesión con esa cuenta.");
        }
        catch (Exception exception) when (exception is HttpRequestException or InvalidOperationException)
        {
            ModelState.AddModelError(string.Empty, "El servicio de inicio de sesión no está disponible.");
        }

        return View(input);
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }
}

public sealed class AdminLogin
{
    [Required, EmailAddress, Display(Name = "Correo")]
    public string Email { get; set; } = "";

    [Required, DataType(DataType.Password), Display(Name = "Contraseña")]
    public string Password { get; set; } = "";
}
