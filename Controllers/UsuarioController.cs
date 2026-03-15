using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using BCrypt.Net;
using CadastroMateriais.Models;

[ApiController]
[Route("[controller]")]
public class UsuarioController : ControllerBase
{
    private readonly string connectionString = "server=localhost;database=CadastroMateriais;user=root;password=720910";

    [HttpPost("registrar")]
    public IActionResult Registrar([FromBody] Usuario usuario)
    {
        try
        {
            string hash = BCrypt.Net.BCrypt.HashPassword(usuario.SenhaHash);

            using var connection = new MySqlConnection(connectionString);
            connection.Open();

            string sql = "INSERT INTO usuarios (nome, email, senha_hash) VALUES (@Nome, @Email, @SenhaHash)";
            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Nome", usuario.Nome);
            command.Parameters.AddWithValue("@Email", usuario.Email);
            command.Parameters.AddWithValue("@SenhaHash", hash);

            command.ExecuteNonQuery();

            return Ok("Usuário cadastrado com sucesso.");
        }
        catch (Exception ex)
        {
            return BadRequest("Erro ao cadastrar: " + ex.Message);
        }
    }


    [HttpPost("login")]
    public IActionResult Login([FromBody] Usuario login)
    {
        using var connection = new MySqlConnection(connectionString);
        connection.Open();

        string sql = "SELECT id, senha_hash FROM usuarios WHERE email = @Email";
        using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Email", login.Email);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {   
            int idUsuario = reader.GetInt32("id");
            string senhaHash = reader["senha_hash"].ToString()!;
            bool senhaValida = BCrypt.Net.BCrypt.Verify(login.SenhaHash, senhaHash);

            if (senhaValida)
                return Ok(new { message = "Login realizado com sucesso.", idUsuario = idUsuario });
        }

        return Unauthorized("Email ou senha inválidos.");
    }
    [HttpPost("esqueci-senha")]
    public IActionResult EsqueciSenha([FromBody] EsqueciSenhaRequest request)
    {
        string email = request.Email;

        if (string.IsNullOrEmpty(email))
            return BadRequest("Email inválido.");

        using var connection = new MySqlConnection(connectionString);
        connection.Open();

        string sqlCheck = "SELECT id FROM usuarios WHERE email = @Email";
        using var checkCommand = new MySqlCommand(sqlCheck, connection);
        checkCommand.Parameters.AddWithValue("@Email", email);
        var idUsuario = checkCommand.ExecuteScalar();

        if (idUsuario == null)
        {
            return Ok("Se o e-mail existir, um link foi enviado.");
        }

        var token = Guid.NewGuid().ToString();
        var expiry = DateTime.UtcNow.AddHours(1);

        string sqlUpdate = "UPDATE usuarios SET reset_token = @Token, reset_token_expiry = @Expiry WHERE id = @Id";
        using var updateCommand = new MySqlCommand(sqlUpdate, connection);
        updateCommand.Parameters.AddWithValue("@Token", token);
        updateCommand.Parameters.AddWithValue("@Expiry", expiry);
        updateCommand.Parameters.AddWithValue("@Id", idUsuario);
        updateCommand.ExecuteNonQuery();

        var resetLink = $"http://127.0.0.1:5500/Frontend/reset-senha.html?token={token}";
        Console.WriteLine($"[DEBUG] Link de reset: {resetLink}");

        return Ok("Se o e-mail existir, um link foi enviado.");
    }


    [HttpPost("resetar-senha")]
    public IActionResult ResetarSenha([FromBody] ResetSenhaRequest request)
    {
        var token = request.Token;
        var novaSenha = request.NovaSenha;

        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(novaSenha))
            return BadRequest("Token e nova senha são obrigatórios.");
        // ... restante da lógica

        using var connection = new MySqlConnection(connectionString);
        connection.Open();

        // Confere se há usuário com o token válido
        string sqlCheck = "SELECT id FROM usuarios WHERE reset_token = @Token AND reset_token_expiry > @Now";
        using var checkCommand = new MySqlCommand(sqlCheck, connection);
        checkCommand.Parameters.AddWithValue("@Token", token);
        checkCommand.Parameters.AddWithValue("@Now", DateTime.UtcNow);

        var idUsuario = checkCommand.ExecuteScalar();

        if (idUsuario == null)
        {
            return BadRequest("Token inválido ou expirado.");
        }

        // Gera o hash da nova senha
        string hash = BCrypt.Net.BCrypt.HashPassword(novaSenha);

        // Atualiza senha e limpa o token
        string sqlUpdate = "UPDATE usuarios SET senha_hash = @Senha, reset_token = NULL, reset_token_expiry = NULL WHERE id = @Id";
        using var updateCommand = new MySqlCommand(sqlUpdate, connection);
        updateCommand.Parameters.AddWithValue("@Senha", hash);
        updateCommand.Parameters.AddWithValue("@Id", idUsuario);
        updateCommand.ExecuteNonQuery();

        return Ok("Senha atualizada com sucesso!");
    }



}

