using System.Text.Json.Serialization;

namespace Javaquero.Web.Models;

public sealed record PresentationPrice(
    [property: JsonPropertyName("producto_id")] long ProductoId,
    int Gramos,
    [property: JsonPropertyName("precio_crc")] decimal PrecioCrc);

public sealed record QuoteLine(string Nombre, int Gramos, int Cantidad,
    [property: JsonPropertyName("precio_crc")] decimal PrecioCrc, decimal Subtotal);

public sealed record Quote(long Id, string Cliente,
    [property: JsonPropertyName("creado_en")] DateTimeOffset CreadoEn,
    decimal Total, QuoteLine[]? Lineas);

public sealed record DocumentData(Product[] Products, PresentationPrice[] Prices, Quote[] Quotes)
{
    public static readonly int[] Sizes = [250, 500, 1000];

    public static string Money(decimal amount) => "₡" + amount.ToString("N2", System.Globalization.CultureInfo.InvariantCulture);
    public static string Size(int grams) => grams == 1000 ? "1 kg" : $"{grams} g";
}
