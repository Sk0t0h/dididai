using DididaiApp.Core.Models;

namespace DididaiApp.Tests;

/// <summary>
/// Pruebas del catálogo de prefijos telefónicos: España primera/por defecto y
/// resolución de prefijo por país (usado para preseleccionar en el formulario).
/// </summary>
public class PrefijosTelefonicosTests
{
    [Fact]
    public void Catalogo_NoEstaVacio_YEspanaEsLaPrimera()
    {
        Assert.NotEmpty(PrefijosTelefonicos.Todos);
        Assert.Equal("ES", PrefijosTelefonicos.Todos[0].PaisCodigo);
        Assert.Equal("+34", PrefijosTelefonicos.Todos[0].Codigo);
        Assert.Equal("+34", PrefijosTelefonicos.CodigoPorDefecto);
    }

    [Theory]
    [InlineData("ES", "+34")]
    [InlineData("GB", "+44")]
    [InlineData("FR", "+33")]
    [InlineData("gb", "+44")] // se normaliza
    public void CodigoDePais_DevuelveElPrefijo(string pais, string prefijoEsperado)
        => Assert.Equal(prefijoEsperado, PrefijosTelefonicos.CodigoDePais(pais));

    [Theory]
    [InlineData("ZZ")]
    [InlineData("")]
    [InlineData(null)]
    public void CodigoDePais_PaisNoListado_DevuelveVacio(string? pais)
        => Assert.Equal(string.Empty, PrefijosTelefonicos.CodigoDePais(pais));
}
