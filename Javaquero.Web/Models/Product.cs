using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Javaquero.Web.Models;

public sealed class Product
{
    public long Id { get; set; }
    [Required, StringLength(120), RegularExpression("^[a-z0-9]+(-[a-z0-9]+)*$", ErrorMessage = "Usa minúsculas sin tildes, números y guiones. Ejemplo: cafe-tarrazu.")]
    public string Slug { get; set; } = "";
    [Required, StringLength(150)]
    public string Nombre { get; set; } = "";
    [Required, StringLength(150)]
    public string Origen { get; set; } = "";
    [Required, StringLength(100)]
    public string Proceso { get; set; } = "";
    [Range(0, 100), Display(Name = "Puntaje SCA")]
    [JsonPropertyName("puntaje_sca")]
    public decimal PuntajeSca { get; set; }
    public string[] Notas { get; set; } = [];
    [Required, StringLength(150), Display(Name = "Presentación")]
    public string Presentacion { get; set; } = "";
    [Range(0, 9999999999.99), Display(Name = "Precio USD")]
    [JsonPropertyName("precio_usd")]
    public decimal PrecioUsd { get; set; }
    [Range(0, 9999999999.99), Display(Name = "Precio CRC")]
    [JsonPropertyName("precio_crc")]
    public decimal PrecioCrc { get; set; }
    public string Imagen { get; set; } = "";
    public bool Activo { get; set; } = true;
}
