// Program.cs
using CadastroMateriais.Data;
using Microsoft.EntityFrameworkCore;

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<DataContext>(options =>
    options.UseMySql(connectionString,
    new MySqlServerVersion(new Version(8,0,27))));

var builder = WebApplication.CreateBuilder(args);

// Configurar o banco de dados MySQL
builder.Services.AddDbContext<DataContext>(options =>
    options.UseMySql("Server=localhost;Database=CadastroMateriais;User=root;Password=720910;", 
    new MySqlServerVersion(new Version(8, 0, 27))));

// Adicionar CORS e serviços ao contêiner
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<DataContext>();

    if (await context.TestConnection())
    {
        Console.WriteLine("Conexão com o banco de dados bem-sucedida!");
    }
    else
    {
        Console.WriteLine("Falha na conexão com o banco de dados.");
    }
}

// Configurar o middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();
app.UseCors("AllowAll");

// Mapear controllers
app.MapControllers();

app.Run();
