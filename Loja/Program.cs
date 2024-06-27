using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Loja.data;
using Loja.services;
using Microsoft.OpenApi.Models;
using Loja.models;

var builder = WebApplication.CreateBuilder(args);

// Configuração da conexão com o banco de dados
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<LojaDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 37))));

// Configuração da autenticação JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("senhasegura123"))
        };
    });

// Serviço de Usuário
builder.Services.AddScoped<UsuarioService>();

// Configuração do Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Loja API", Version = "v1" });
});

// Middleware para roteamento
var app = builder.Build();

// Middleware de rota
app.UseRouting();

// Middleware de autenticação
app.UseAuthentication();

// Middleware de autorização
app.UseAuthorization();

// Método para gerar o token JWT
string GenerateToken(string email)
{
    var tokenHandler = new JwtSecurityTokenHandler();
    var key = Encoding.ASCII.GetBytes("senhasegura123");
    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Email, email) }),
        Expires = DateTime.UtcNow.AddHours(1),
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
    };
    var token = tokenHandler.CreateToken(tokenDescriptor);
    return tokenHandler.WriteToken(token);
}

// Endpoint de Login
app.MapPost("/login", async (HttpContext context, UsuarioService usuarioService) =>
{
    // Receber o request
    var body = await context.Request.ReadFromJsonAsync<LoginModel>();

    // Verificar as credenciais do usuário
    var usuario = await usuarioService.Authenticate(body.Email, body.Senha);
    if (usuario == null)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsync("Credenciais inválidas");
        return;
    }

    // Gerar token JWT
    var token = GenerateToken(usuario.Email);

    await context.Response.WriteAsync(token);
});

// Endpoint para consultar todos os usuários
app.MapGet("/usuarios", async (UsuarioService usuarioService) =>
{
    var usuarios = await usuarioService.GetAllUsuariosAsync();
    return Results.Ok(usuarios);
});

// Endpoint para consultar um usuário pelo ID
app.MapGet("/usuarios/{id}", async (int id, UsuarioService usuarioService) =>
{
    var usuario = await usuarioService.GetUsuarioByIdAsync(id);
    if (usuario == null)
    {
        return Results.NotFound($"Usuário com ID {id} não encontrado.");
    }
    return Results.Ok(usuario);
});

// Endpoint para atualizar os dados de um usuário
app.MapPut("/usuarios/{id}", async (int id, Usuario usuario, UsuarioService usuarioService) =>
{
    if (id != usuario.Id)
    {
        return Results.BadRequest("ID do usuário não corresponde.");
    }
    await usuarioService.UpdateUsuarioAsync(usuario);
    return Results.Ok();
});

// Endpoint para excluir um usuário
app.MapDelete("/usuarios/{id}", async (int id, UsuarioService usuarioService) =>
{
    await usuarioService.DeleteUsuarioAsync(id);
    return Results.Ok();
});

// Rota para criar um novo serviço
app.MapPost("/servicos", async (Servico servico, LojaDbContext dbContext) =>
{
    dbContext.Servicos.Add(servico);
    await dbContext.SaveChangesAsync();
    return Results.Created($"/servicos/{servico.Id}", servico);
})
.RequireAuthorization(); // Exige autorização JWT

// Configuração do Swagger (apenas em ambiente de desenvolvimento)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Loja API v1");
    });
}

// Redirecionamento HTTPS
app.UseHttpsRedirection();

// Execução da aplicação
app.Run();
