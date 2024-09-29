using MinimalApi.Dominio.Entidades;
using MinimalApi.DTOs;

namespace MinimalApi.Dominio.Interfaces;

public interface IVeiculoServico {
    List<Veiculo> Todos (int pagina, string? nome = null, string? marca = null);
    
    Veiculo BuscaPorId(int id);

    Veiculo Incluir(Veiculo veiculo);

    Veiculo Atualizar(Veiculo veiculo);

    void Apagar(Veiculo veiculo);
}