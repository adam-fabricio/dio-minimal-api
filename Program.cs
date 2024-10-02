using MinimalApi.Infraestrutura.Db;
using MinimalApi.DTOs;
using Microsoft.EntityFrameworkCore;
using MinimalApi.Dominio.Interfaces;
using dio_minimal_api.Dominio.Servicos;
using Microsoft.AspNetCore.Mvc;
using dio_minimal_api.Dominio.ModelViews;
using MinimalApi.Dominio.Entidades;
using dio_minimal_api.Dominio.DTOs;
using dio_minimal_api.Dominio.Enums;



#region  Builder

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IAdministradorServico, AdministradorServico>();
builder.Services.AddScoped<IVeiculoServico, VeiculoServico>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<DbContexto>(options => {
    options.UseMySql(
        builder.Configuration.GetConnectionString("mysql"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("mysql"))
    );
});

var app = builder.Build();
#endregion

#region Home
app.MapGet("/", () => Results.Json(new Home())).WithTags("Home");
#endregion

#region Administradores

app.MapPost("/administradores/login", 
            (
                [FromBody] LoginDTO loginDTO,
                IAdministradorServico administradorServico
            ) => 
{
    if (administradorServico.Login(loginDTO) != null)
        return Results.Ok("Login com sucesso");
    else
        return Results.Unauthorized();
}).WithTags("Administradores");

app.MapPost("administradores", ([FromBody] AdministradorDTO usuario, 
                                IAdministradorServico administradorServico) =>
{
    ErrosDeValidacao validacao = new()
    {
        Mensagens = []
    };


    if (string.IsNullOrEmpty(usuario.Email))
        validacao.Mensagens.Add("O Email não pode ser vazio");
    if (string.IsNullOrEmpty(usuario.Senha))
        validacao.Mensagens.Add("A senha não pode ser vazia");
    if (usuario.Perfil == null)
        validacao.Mensagens.Add("O Perfil não pode ser nulo");
    
    if (validacao.Mensagens.Count > 0)
        return Results.BadRequest(validacao);
        

    var novoUsuario = new Administrador
    {
        Email = usuario.Email ?? string.Empty,
        Senha = usuario.Senha ?? string.Empty,
        Perfil = usuario.Perfil.ToString() ?? string.Empty
    };

    administradorServico.Incluir(novoUsuario);

    var usuarioView = new AdministradorModelView
    {
        Id = novoUsuario.Id,
        Email = novoUsuario.Email,
        Perfil = novoUsuario.Perfil
    };
    
    return Results.Created($"/administradores/{novoUsuario.Id}", usuarioView);

}).WithTags("Administradores");

app.MapGet("/administradores", (
    [FromQuery] int? pagina, IAdministradorServico administradorServico) =>
    {
        var administradores = administradorServico.Todos(pagina ?? 1);
        var adms = new List<AdministradorModelView>();
        
        foreach(var adm in administradores)
            adms.Add(new AdministradorModelView
            {
                Id = adm.Id,
                Email = adm.Email,
                Perfil = adm.Perfil
            });
        
        return Results.Ok(adms);
    }
).WithTags("Administradores");

app.MapGet("/administraodores/{id}",(
    [FromRoute] int id, IAdministradorServico administradorServico) => 
    {
        var usuario = administradorServico.BuscaPorId(id);
        if (usuario == null)
            return Results.NotFound();

        return Results.Ok(usuario);

    }).WithTags("Administradores");


app.MapDelete("/administradores/{id}", (
    [FromRoute] int id, IAdministradorServico administradorServico) =>
    {
        var usuario = administradorServico.BuscaPorId(id);
        if (usuario == null)
            return Results.NotFound();

        administradorServico.Deletar(usuario);
        
        return Results.Ok(usuario);
    }
).WithTags("Administradores");


#endregion



#region Veiculos

ErrosDeValidacao validaDTO(VeiculoDTO veiculo)
{
    var validacao = new ErrosDeValidacao
    {
        Mensagens = []
    };

    if (string.IsNullOrEmpty(veiculo.Nome))
        validacao.Mensagens.Add("O nome não pode ser vazio");
    if (string.IsNullOrEmpty(veiculo.Marca))
        validacao.Mensagens.Add("A marca não pode ser vazia");
    if (veiculo.Ano < 1885)
        validacao.Mensagens.Add("O Ano está anterior da invenção do carro.");
    if (veiculo.Ano > DateTime.Now.Year)
        validacao.Mensagens.Add("O Ano está superior ao atual.");
    return validacao;
}

app.MapPost("/veiculos", ([FromBody] VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) => {
    var veiculo = new Veiculo{
        Nome = veiculoDTO.Nome,
        Marca = veiculoDTO.Marca,
        Ano = veiculoDTO.Ano
    };
    ErrosDeValidacao validacao = validaDTO(veiculoDTO);
    if (validacao.Mensagens.Count > 0)
        return Results.BadRequest(validacao);
    
    veiculoServico.Incluir(veiculo);

    return Results.Created($"/veiculo/{veiculo.Id}", veiculo);
}).WithTags("Veiculos");

app.MapGet("/veiculo", ([FromQuery] int? pagina, IVeiculoServico veiculoServico) => {
    return Results.Ok(veiculoServico.Todos(pagina ?? 1));
}).WithTags("Veiculos");

app.MapGet("/veiculos/{id}",([FromRoute] int id, IVeiculoServico veiculoServico) => {
    var veiculo = veiculoServico.BuscaPorId(id);
    if (veiculo == null) return Results.NotFound();
    return Results.Ok(veiculo);
}).WithTags("Veiculos");

app.MapPut("/veiculos/{id}",([FromRoute] int id, 
            VeiculoDTO veiculoDTO, 
            IVeiculoServico veiculoServico) => {
    var veiculo = veiculoServico.BuscaPorId(id);
    if (veiculo == null) 
        return Results.NotFound();
    
    ErrosDeValidacao validacao = validaDTO(veiculoDTO);
    if (validacao.Mensagens.Count > 0)
        return Results.BadRequest(validacao);
    
    veiculo.Nome = veiculoDTO.Nome;
    veiculo.Marca = veiculoDTO.Marca;
    veiculo.Ano = veiculoDTO.Ano;
    veiculoServico.Atualizar(veiculo);
    return Results.Ok(veiculo);
}).WithTags("Veiculos");

app.MapDelete("/veiculos/{id}",([FromRoute] int id, IVeiculoServico veiculoServico) => {
    var veiculo = veiculoServico.BuscaPorId(id);
    if (veiculo == null) return Results.NotFound();
    veiculoServico.Apagar(veiculo);    
    return Results.NoContent();
}).WithTags("Veiculos");

#endregion

#region App
app.UseSwagger();
app.UseSwaggerUI();

app.Run();
#endregion
