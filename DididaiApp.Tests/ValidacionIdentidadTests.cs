using DididaiApp.Core.Models;

namespace DididaiApp.Tests;

/// <summary>
/// Pruebas de la validación de identidad: algoritmo de letra de control (DNI/NIE),
/// dependencia del tipo de documento declarado y formato E.164 del teléfono. Es la
/// lógica de negocio más sensible del CRUD de socios (errores silenciosos y caros),
/// y son funciones puras: se testean sin BD ni HTTP.
/// </summary>
public class ValidacionIdentidadTests
{
    // ---- DNI español: letra de control (módulo 23) ----

    [Theory]
    [InlineData("12345678Z")] // número 12345678 % 23 = 14 -> 'Z'
    [InlineData("00000000T")] // 0 -> 'T'
    [InlineData("12345678z")] // minúscula: se normaliza a mayúscula
    public void DniEspanol_LetraCorrecta_EsValido(string dni)
        => Assert.True(ValidacionIdentidad.DocumentoValido(dni, TipoDocumento.DniEspanol));

    [Theory]
    [InlineData("12345678A")] // letra incorrecta
    [InlineData("12345678")]  // sin letra
    [InlineData("1234567Z")]  // 7 dígitos
    [InlineData("123456789Z")]// 9 dígitos
    [InlineData("ABCDEFGHZ")] // no numérico
    public void DniEspanol_LetraOFormatoIncorrecto_NoEsValido(string dni)
        => Assert.False(ValidacionIdentidad.DocumentoValido(dni, TipoDocumento.DniEspanol));

    // ---- NIE: prefijo X/Y/Z sustituido por 0/1/2 y luego como el DNI ----

    [Theory]
    [InlineData("X1234567L")] // 01234567 % 23 = 15 -> 'L'
    [InlineData("Y1234567X")]
    [InlineData("Z1234567R")]
    public void Nie_LetraCorrecta_EsValido(string nie)
        => Assert.True(ValidacionIdentidad.DocumentoValido(nie, TipoDocumento.Nie));

    [Theory]
    [InlineData("X1234567Z")] // letra incorrecta
    [InlineData("A1234567L")] // prefijo no válido
    [InlineData("X123456L")]  // 6 dígitos
    public void Nie_Incorrecto_NoEsValido(string nie)
        => Assert.False(ValidacionIdentidad.DocumentoValido(nie, TipoDocumento.Nie));

    // ---- Pasaporte / Otro: laxo (solo presencia) ----

    [Theory]
    [InlineData(TipoDocumento.Pasaporte, "AB-123.456")]
    [InlineData(TipoDocumento.Otro, "cualquier-cosa")]
    [InlineData(TipoDocumento.Pasaporte, "12345678A")] // no se le exige letra correcta
    public void PasaporteYOtro_ConValor_SonValidos(TipoDocumento tipo, string doc)
        => Assert.True(ValidacionIdentidad.DocumentoValido(doc, tipo));

    [Theory]
    [InlineData(TipoDocumento.DniEspanol)]
    [InlineData(TipoDocumento.Nie)]
    [InlineData(TipoDocumento.Pasaporte)]
    [InlineData(TipoDocumento.Otro)]
    public void Documento_Vacio_NoEsValido_ParaCualquierTipo(TipoDocumento tipo)
    {
        Assert.False(ValidacionIdentidad.DocumentoValido("", tipo));
        Assert.False(ValidacionIdentidad.DocumentoValido("   ", tipo));
        Assert.False(ValidacionIdentidad.DocumentoValido(null, tipo));
    }

    [Fact]
    public void Documento_ValidacionNoDependeDelPais_SinoDelTipo()
    {
        // El mismo DNI válido es válido como DniEspanol e inválido si se declara NIE.
        Assert.True(ValidacionIdentidad.DocumentoValido("12345678Z", TipoDocumento.DniEspanol));
        Assert.False(ValidacionIdentidad.DocumentoValido("12345678Z", TipoDocumento.Nie));
    }

    // ---- Teléfono E.164 ----

    [Theory]
    [InlineData("+34600111222")]
    [InlineData("+441234567890")]
    [InlineData("+34 600 111 222")] // separadores: se normalizan
    [InlineData("+34-600-111-222")]
    [InlineData("+1 (555) 123-4567")]
    public void Telefono_E164Valido_EsValido(string tel)
        => Assert.True(ValidacionIdentidad.TelefonoValido(tel));

    [Theory]
    [InlineData("600111222")]     // sin prefijo internacional
    [InlineData("+0123456789")]   // empieza por 0 tras el +
    [InlineData("+341")]          // demasiado corto
    [InlineData("+34600111222333444")] // demasiado largo (>15 dígitos)
    [InlineData("")]
    [InlineData("abc")]
    public void Telefono_Invalido_NoEsValido(string tel)
        => Assert.False(ValidacionIdentidad.TelefonoValido(tel));

    [Theory]
    [InlineData("+34 600 111 222", "+34600111222")]
    [InlineData(" +34-600.111(222) ", "+34600111222")]
    public void NormalizarTelefono_QuitaSeparadores(string entrada, string esperado)
        => Assert.Equal(esperado, ValidacionIdentidad.NormalizarTelefono(entrada));
}
