using MinimalApi.Dominio.Entidades;

namespace Test.Domain.Entidades;

[TestClass]
public class VeiculoTest
{
    [TestMethod]
    public void TestarGetSetPropriedades()
    {
        // Organizar
        var veiculo = new Veiculo();

        // Agir
        veiculo.Id = 1;
        veiculo.Marca = "Ford";
        veiculo.Ano = 1986;
        veiculo.Nome = "Alfa";


        //Testar
        
        Assert.AreEqual(1, veiculo.Id);
        Assert.AreEqual("Ford", veiculo.Marca);
        Assert.AreEqual(1986, veiculo.Ano);
        Assert.AreEqual("Alfa", veiculo.Nome);
    }
}
