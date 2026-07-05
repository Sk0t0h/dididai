using DididaiApp.Core.Models;

namespace DididaiApp.Tests;

/// <summary>
/// Pruebas del catálogo de países: que España va primera/por defecto, que la
/// validación de código ISO acepta solo códigos reales, y que el nombre se resuelve.
/// </summary>
public class PaisesTests
{
    [Fact]
    public void Catalogo_NoEstaVacio_YEspanaEsElPrimero()
    {
        Assert.NotEmpty(Paises.Todos);
        Assert.Equal("ES", Paises.Todos[0].Codigo);
        Assert.Equal("ES", Paises.CodigoPorDefecto);
    }

    [Fact]
    public void Catalogo_NoTieneCodigosDuplicados()
    {
        var codigos = Paises.Todos.Select(p => p.Codigo).ToList();
        Assert.Equal(codigos.Count, codigos.Distinct().Count());
    }

    [Theory]
    [InlineData("ES")]
    [InlineData("GB")]
    [InlineData("es")] // se normaliza a mayúsculas
    [InlineData(" fr ")] // se recorta
    public void EsCodigoValido_CodigoReal_True(string codigo)
        => Assert.True(Paises.EsCodigoValido(codigo));

    [Theory]
    [InlineData("ZZ")]  // no es un país
    [InlineData("ESP")] // alpha-3, no alpha-2
    [InlineData("")]
    [InlineData(null)]
    public void EsCodigoValido_CodigoInvalido_False(string? codigo)
        => Assert.False(Paises.EsCodigoValido(codigo));

    [Fact]
    public void Nombre_DevuelveAlgoLegible_ParaEspana()
    {
        var nombre = Paises.Nombre("ES");
        Assert.False(string.IsNullOrWhiteSpace(nombre));
        Assert.NotEqual("ES", nombre); // se resolvió a un nombre, no al propio código
    }

    [Fact]
    public void Nombre_CodigoDesconocido_DevuelveElCodigo()
        => Assert.Equal("ZZ", Paises.Nombre("ZZ"));
}
