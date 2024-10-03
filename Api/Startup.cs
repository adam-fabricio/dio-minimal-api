using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using dio_minimal_api.Dominio.DTOs;
using dio_minimal_api.Dominio.ModelViews;
using dio_minimal_api.Dominio.Servicos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MinimalApi.Dominio.Entidades;
using MinimalApi.Dominio.Interfaces;
using MinimalApi.DTOs;
using MinimalApi.Infraestrutura.Db;

public class Startup
{
    public string key = "";
    public IConfiguration Configuration { get; set; } = default!;
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
        key = Configuration.GetSection("Jwt").ToString() ?? "";
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddAuthentication(option =>
        {
            option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(option =>
        {
            option.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                ValidateIssuer = false,
                ValidateAudience = false,
            };
        });

        services.AddAuthorization();

        services.AddScoped<IAdministradorServico, AdministradorServico>();
        services.AddScoped<IVeiculoServico, VeiculoServico>();

        services.AddDbContext<DbContexto>(options =>
        {
            options.UseMySql(
                Configuration.GetConnectionString("mysql"),
                ServerVersion.AutoDetect(Configuration.GetConnectionString("mysql"))
            );
        });

        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Insira o token JWT aqui"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] {}
                }
            });
        });

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(
                builder =>
                {
                    builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseCors();

        app.UseEndpoints(endpoints =>
        {
            #region Home
            
            endpoints.MapGet("/", () => Results.Json(new Home())).WithTags("Home").AllowAnonymous();

            #endregion

            #region Administradores

            string GerarTokenJwt(Administrador administrador)
            {
                var securitykey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(key));
                var credentials = new SigningCredentials(
                    securitykey, SecurityAlgorithms.HmacSha256);
                
                var claims = new List<Claim>()
                {
                    new Claim("Email", administrador.Email),
                    new Claim("Perfil", administrador.Perfil),
                    new Claim(ClaimTypes.Role, administrador.Perfil)
                };

                var token = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.Now.AddDays(1),
                    signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
            }

            endpoints.MapPost("/administradores/login", (
                [FromBody] LoginDTO loginDTO,
                IAdministradorServico administradorServico) => 
            {
                var usuario = administradorServico.Login(loginDTO);

                if (usuario != null)
                    return Results.Ok(new UsuarioLogado
                    {
                        Email = usuario.Email,
                        Perfil = usuario.Perfil,
                        Token = GerarTokenJwt(usuario)
                    });
                else
                    return Results.Unauthorized();
            }).AllowAnonymous().WithTags("Administradores");

            endpoints.MapPost("administradores", (
                [FromBody] AdministradorDTO usuario, 
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
                
                return Results.Created($"/administradores/{novoUsuario.Id}",
                    usuarioView);

            }).WithTags("Administradores")
            .RequireAuthorization(new AuthorizeAttribute{ Roles = "Adm"});

            endpoints.MapGet("/administradores", (
                [FromQuery] int? pagina, 
                IAdministradorServico administradorServico) =>
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
            ).WithTags("Administradores")
            .RequireAuthorization(new AuthorizeAttribute{ Roles = "Adm"});

            endpoints.MapGet("/administraodores/{id}",(
                [FromRoute] int id, 
                IAdministradorServico administradorServico) =>
                {
                    var usuario = administradorServico.BuscaPorId(id);
                    if (usuario == null)
                        return Results.NotFound();

                    return Results.Ok(usuario);

                }).WithTags("Administradores")
                .RequireAuthorization(new AuthorizeAttribute{ Roles = "Adm"});


            endpoints.MapDelete("/administradores/{id}", (
                [FromRoute] int id, 
                IAdministradorServico administradorServico) =>
                {
                    var usuario = administradorServico.BuscaPorId(id);
                    if (usuario == null)
                        return Results.NotFound();

                    administradorServico.Deletar(usuario);
                    
                    return Results.Ok(usuario);
                }
            ).WithTags("Administradores")
            .RequireAuthorization(new AuthorizeAttribute{ Roles = "Adm"});

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

            endpoints.MapPost("/veiculos", (
                [FromBody] VeiculoDTO veiculoDTO, 
                IVeiculoServico veiculoServico) => 
                {
                    var veiculo = new Veiculo
                    {
                        Nome = veiculoDTO.Nome,
                        Marca = veiculoDTO.Marca,
                        Ano = veiculoDTO.Ano
                    };
                    ErrosDeValidacao validacao = validaDTO(veiculoDTO);

                    if (validacao.Mensagens.Count > 0)
                        return Results.BadRequest(validacao);
                    
                    veiculoServico.Incluir(veiculo);

                    return Results.Created($"/veiculo/{veiculo.Id}", veiculo);

                }).WithTags("Veiculos")
                .RequireAuthorization(new AuthorizeAttribute{ Roles = "Adm,Editor"});

            endpoints.MapGet("/veiculo", ([FromQuery] int? pagina,
                IVeiculoServico veiculoServico) => 
                {
                    return Results.Ok(veiculoServico.Todos(pagina ?? 1));
                }).WithTags("Veiculos")
                .RequireAuthorization(new AuthorizeAttribute{ Roles = "Adm,Editor"});

            endpoints.MapGet("/veiculos/{id}",([FromRoute] int id, 
                IVeiculoServico veiculoServico) => 
                {
                    var veiculo = veiculoServico.BuscaPorId(id);
                    if (veiculo == null) return Results.NotFound();
                    return Results.Ok(veiculo);
                }).WithTags("Veiculos")
                .RequireAuthorization(new AuthorizeAttribute{ Roles = "Adm,Editor"});

            endpoints.MapPut("/veiculos/{id}",([FromRoute] int id, 
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
                }).WithTags("Veiculos")
                .RequireAuthorization(new AuthorizeAttribute{ Roles = "Adm"});

            endpoints.MapDelete("/veiculos/{id}",([FromRoute] int id, IVeiculoServico veiculoServico) => {
                var veiculo = veiculoServico.BuscaPorId(id);
                if (veiculo == null) return Results.NotFound();
                veiculoServico.Apagar(veiculo);    
                return Results.NoContent();
            }).WithTags("Veiculos")
            .RequireAuthorization(new AuthorizeAttribute{ Roles = "Adm,Editor"});

            #endregion
        
        });

        
    }
}