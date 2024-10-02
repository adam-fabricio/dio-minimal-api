using MinimalApi.Dominio.Entidades;
using MinimalApi.DTOs;

namespace MinimalApi.Dominio.Interfaces;

public interface IAdministradorServico {
    Administrador? Login(LoginDTO loginDTO);
    List<Administrador> Todos (int? pagina);
    
    Administrador? BuscaPorId(int id);

    void Incluir(Administrador administrador);
    
    void Deletar(Administrador administrador);
}