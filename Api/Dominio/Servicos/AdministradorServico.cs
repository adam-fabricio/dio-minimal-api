using MinimalApi.Dominio.Entidades;
using MinimalApi.Dominio.Interfaces;
using MinimalApi.DTOs;
using MinimalApi.Infraestrutura.Db;

namespace dio_minimal_api.Dominio.Servicos
{
    public class AdministradorServico : IAdministradorServico
    {
        private readonly DbContexto _contexto;
        public AdministradorServico(DbContexto contexto)
        {
            _contexto = contexto;
        }

        public Administrador? BuscaPorId(int id)
        {
            var usuario = _contexto.Administradores.Where(x => x.Id == id).FirstOrDefault();
            return usuario;
        }

        public void Deletar(Administrador administrador)
        {
            _contexto.Administradores.Remove(administrador);
            _contexto.SaveChanges();
        }

        public void Incluir(Administrador administrador)
        {
            _contexto.Administradores.Add(administrador);
            _contexto.SaveChanges();
        }

        public Administrador? Login(LoginDTO loginDTO)
        {
            return _contexto.Administradores.Where(x => 
            x.Email == loginDTO.Email && x.Senha == loginDTO.Senha).FirstOrDefault();
        }

        public List<Administrador> Todos(int? pagina)
        {
            int itemsPorPaginas = 10;
            var veiculos = _contexto.Administradores.AsQueryable();

            veiculos = veiculos.Skip(((pagina ?? 1) - 1) * itemsPorPaginas).Take(itemsPorPaginas);

            return veiculos.ToList();
        }
    }
}