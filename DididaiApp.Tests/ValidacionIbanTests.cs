using DididaiApp.Core.Models;

namespace DididaiApp.Tests;

/// <summary>
/// Pruebas del validador de IBAN (mod-97, ISO 13616), internacional (no atado a
/// España). Escritas ANTES de la implementación (TDD): definen el comportamiento
/// esperado. El IBAN se usará en la cuota domiciliada de Colaboraciones.
/// </summary>
public class ValidacionIbanTests
{
    [Theory]
    [InlineData("ES9121000418450200051332")]     // España (ejemplo estándar)
    [InlineData("GB82WEST12345698765432")]        // Reino Unido
    [InlineData("DE89370400440532013000")]        // Alemania
    [InlineData("FR1420041010050500013M02606")]   // Francia (incluye letra en la parte nacional)
    [InlineData("NL91ABNA0417164300")]            // Países Bajos
    public void Iban_Valido_DevuelveTrue(string iban)
        => Assert.True(ValidacionIban.EsValido(iban));

    [Theory]
    [InlineData("ES9121000418450200051332", "ES91 2100 0418 4502 0005 1332")] // con espacios
    [InlineData("ES9121000418450200051332", "es9121000418450200051332")]       // minúsculas
    public void Iban_ConEspaciosOMinusculas_SeNormalizaYEsValido(string _, string entrada)
        => Assert.True(ValidacionIban.EsValido(entrada));

    [Theory]
    [InlineData("ES9021000418450200051332")] // dígitos de control mal (mod-97 falla)
    [InlineData("GB00WEST12345698765432")]   // control 00 inválido
    public void Iban_DigitosDeControlIncorrectos_DevuelveFalse(string iban)
        => Assert.False(ValidacionIban.EsValido(iban));

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("ES")]                          // demasiado corto
    [InlineData("1234567890")]                  // no empieza por 2 letras de país
    [InlineData("ZZ9121000418450200051332")]    // país inexistente (longitud no registrada)
    [InlineData("ES91210004184502000513321234567890")] // demasiado largo (>34)
    public void Iban_FormatoInvalido_DevuelveFalse(string? iban)
        => Assert.False(ValidacionIban.EsValido(iban));

    [Theory]
    [InlineData("ES91 2100 0418 4502 0005 1332", "ES9121000418450200051332")]
    [InlineData(" es9121000418450200051332 ", "ES9121000418450200051332")]
    public void Normalizar_QuitaEspaciosYPoneMayusculas(string entrada, string esperado)
        => Assert.Equal(esperado, ValidacionIban.Normalizar(entrada));
}
