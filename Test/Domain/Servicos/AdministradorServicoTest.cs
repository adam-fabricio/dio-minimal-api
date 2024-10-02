using MinimalApi.Infraestrutura.Db;
using Microsoft.Extensions.Configuration;
using MinimalApi.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using dio_minimal_api.Dominio.Servicos;

namespace Test.Domain.Servicos;

[TestClass]
public class AdministradorServicoTest
{
    
    private DbContexto CriarContextoTeste(){
        // var builder = new ConfigurationBuilder()
        //     .SetBasePath(Directory.GetCurrentDirectory())
        //     .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true) // Configuração base
        //     .AddEnvironmentVariables();
        // var configuration = builder.Build();

        return new DbContexto(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                {"ConnectionStrings:mysql", "Server=127.17.0.2;Port=3306;Database=minimal_api_test;Uid=root;Pwd=root;" },
                { "Jwt", "test-jwt-token" }
            }).Build());
    }
        [TestMethod]
        public void TestSalvarAdministrador()
        {
            // Organizar
            var context = CriarContextoTeste();
            context.Database.ExecuteSqlRaw("TRUNCATE TABLE Administradores");

            var adm = new Administrador();
            adm.Email = "teste@teste.com";
            adm.Senha = "teste";
            adm.Perfil = "Adm";

            var administradorServico = new AdministradorServico(context);

            // acao
            administradorServico.Incluir(adm);

            // teste

            Assert.AreEqual(1, administradorServico.Todos(1).Count());

        }
}
