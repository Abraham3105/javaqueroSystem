using System.Security.Claims;
using Javaquero.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Javaquero.Web.Services;

public sealed class AdminCookieEvents(SupabaseAdmin supabase) : CookieAuthenticationEvents
{
    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        try
        {
            if (Guid.TryParse(context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier), out var id) &&
                await supabase.IsActiveAdminAsync(id, context.HttpContext.RequestAborted))
                return;
        }
        catch (Exception exception) when (exception is HttpRequestException or InvalidOperationException)
        {
            context.RejectPrincipal();
            return;
        }

        context.RejectPrincipal();
        await context.HttpContext.SignOutAsync(context.Scheme.Name);
    }
}
