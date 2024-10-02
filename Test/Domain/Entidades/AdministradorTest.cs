using MinimalApi.Dominio.Entidades;

namespace Test.Domain.Entidades;

[TestClass]
public class AdministradorTest
{
    [TestMethod]
    public void TestarGetSetPropriedades()
    {
        // Organizar
        var adm = new Administrador();

        // Agir
        adm.Id = 1;
        adm.Senha = "123";
        adm.Perfil = "Editor";
        adm.Email = "alfa@beta.com";


        //Testar
        
        Assert.AreEqual(1, adm.Id);
        Assert.AreEqual("alfa@beta.com", adm.Email);
        Assert.AreEqual("123", adm.Senha);
        Assert.AreEqual("Editor", adm.Perfil);
    }
}
