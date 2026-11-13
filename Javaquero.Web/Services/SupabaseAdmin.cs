using Javaquero.Web.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace Javaquero.Web.Services;

public sealed class SupabaseAdmin(HttpClient client, IConfiguration configuration)
{
    public async Task<Guid?> SignInAsync(string email, string password, CancellationToken cancellationToken)
    {
        using var request = CreateRequest(HttpMethod.Post, "auth/v1/token?grant_type=password", "PublishableKey");
        request.Content = JsonContent.Create(new { email, password });
        using var response = await client.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return null;

        using var json = await response.Content.ReadFromJsonAsync<JsonDocument>(cancellationToken);
        return json!.RootElement.GetProperty("user").GetProperty("id").GetGuid();
    }

    public async Task<bool> IsActiveAdminAsync(Guid id, CancellationToken cancellationToken)
    {
        using var request = CreateRequest(HttpMethod.Get,
            $"rest/v1/JV_USUARIO_TB?select=id,JV_ROL_TB!inner(codigo)&id=eq.{id:D}&activo=eq.true&JV_ROL_TB.codigo=eq.ADMIN",
            "SecretKey");
        using var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        using var json = await response.Content.ReadFromJsonAsync<JsonDocument>(cancellationToken);
        return json!.RootElement.GetArrayLength() == 1;
    }

    public async Task<AdminProfile[]> ListAsync(CancellationToken ct)
    {
        using var request = CreateRequest(HttpMethod.Get,
            "rest/v1/JV_USUARIO_TB?select=id,nombre,activo,JV_ROL_TB!inner(codigo)&JV_ROL_TB.codigo=eq.ADMIN&order=nombre",
            "SecretKey");
        using var response = await client.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AdminProfile[]>(ct))!;
    }

    public async Task<bool> SaveAsync(Guid id, string name, bool active, bool create, CancellationToken ct)
    {
        using var request = CreateRequest(HttpMethod.Post, "rest/v1/rpc/jv_guardar_usuario", "SecretKey");
        request.Content = JsonContent.Create(new { p_id = id, p_nombre = name, p_activo = active, p_nuevo = create });
        using var response = await client.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<bool>(ct);
    }

    private const string ProductsPath = "rest/v1/JV_PRODUCTO_TB";

    public async Task<Product[]> ListProductsAsync(long? id, CancellationToken ct, bool activeOnly = false)
    {
        var query = id is long productId ? $"id=eq.{productId}" : "order=nombre,id";
        if (activeOnly)
            query += "&activo=eq.true";
        using var request = CreateRequest(HttpMethod.Get, $"{ProductsPath}?{query}", "SecretKey");
        using var response = await client.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<Product[]>(ct))!;
    }

    public async Task<bool> WriteProductAsync(HttpMethod method, long? id, Product? product, CancellationToken ct, bool writeImage = true)
    {
        var path = id is long productId ? $"{ProductsPath}?id=eq.{productId}" : ProductsPath;
        using var request = CreateRequest(method, path, "SecretKey");
        request.Headers.Add("Prefer", "return=representation");
        if (product is not null)
        {
            var payload = JsonSerializer.SerializeToNode(new
            {
                slug = product.Slug.Trim(), nombre = product.Nombre.Trim(), origen = product.Origen.Trim(),
                proceso = product.Proceso.Trim(), puntaje_sca = product.PuntajeSca, notas = product.Notas,
                presentacion = product.Presentacion.Trim(), precio_usd = product.PrecioUsd,
                precio_crc = product.PrecioCrc, imagen = product.Imagen.Trim(), activo = product.Activo
            })!.AsObject();
            if (!writeImage) payload.Remove("imagen");
            request.Content = JsonContent.Create(payload);
        }
        using var response = await client.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();
        using var json = await response.Content.ReadFromJsonAsync<JsonDocument>(ct);
        return json!.RootElement.GetArrayLength() == 1;
    }

    public async Task<T> ReadDocumentsAsync<T>(string path, CancellationToken ct)
    {
        using var request = CreateRequest(HttpMethod.Get, $"rest/v1/{path}", "SecretKey");
        using var response = await client.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<T>(ct))!;
    }

    public async Task SavePresentationPriceAsync(PresentationPrice price, CancellationToken ct)
    {
        using var request = CreateRequest(HttpMethod.Post,
            "rest/v1/JV_PRODUCTO_PRECIO_TB?on_conflict=producto_id,gramos", "SecretKey");
        request.Headers.Add("Prefer", "resolution=merge-duplicates");
        request.Content = JsonContent.Create(price);
        using var response = await client.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task<long> CreateQuoteAsync(Guid requestId, string customer, object lines, CancellationToken ct)
    {
        using var request = CreateRequest(HttpMethod.Post, "rest/v1/rpc/jv_crear_cotizacion", "SecretKey");
        request.Content = JsonContent.Create(new { p_solicitud = requestId, p_cliente = customer, p_lineas = lines });
        using var response = await client.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<long>(ct);
    }

    public async Task<string> UploadProductImageAsync(IFormFile file, CancellationToken ct)
    {
        if (file.Length is < 12 or > 5 * 1024 * 1024)
            throw new InvalidDataException("Selecciona una imagen de hasta 5 MB.");

        await using var stream = file.OpenReadStream();
        var header = new byte[12];
        await stream.ReadExactlyAsync(header, ct);
        stream.Position = 0;

        var (extension, mediaType) = header switch
        {
            [255, 216, 255, ..] => ("jpg", "image/jpeg"),
            [137, 80, 78, 71, 13, 10, 26, 10, ..] => ("png", "image/png"),
            [82, 73, 70, 70, _, _, _, _, 87, 69, 66, 80] => ("webp", "image/webp"),
            _ => throw new InvalidDataException("Selecciona una imagen JPG, PNG o WebP.")
        };

        var path = $"cafes-imagenes/{Guid.NewGuid():N}.{extension}";
        using var request = CreateRequest(HttpMethod.Post, $"storage/v1/object/{path}", "SecretKey");
        using var form = new MultipartFormDataContent();
        var content = new StreamContent(stream);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(mediaType);
        form.Add(content, "file", $"cafe.{extension}");
        request.Content = form;
        using var response = await client.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
            throw new InvalidDataException("No se pudo subir la foto. Verifica la configuración de Storage.");

        return $"{configuration["Supabase:Url"]!.TrimEnd('/')}/storage/v1/object/public/{path}";
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string path, string keyName)
    {
        var url = configuration["Supabase:Url"];
        var key = configuration[$"Supabase:{keyName}"];
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps ||
            string.IsNullOrWhiteSpace(key))
            throw new InvalidOperationException("Falta la configuración de Supabase.");

        var request = new HttpRequestMessage(method, $"{url!.TrimEnd('/')}/{path}");
        request.Headers.Add("apikey", key);
        return request;
    }
}

public sealed record AdminProfile(Guid Id, string Nombre, bool Activo);
