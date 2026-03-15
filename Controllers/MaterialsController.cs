using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CadastroMateriais.Data;
using CadastroMateriais.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace CadastroMateriais.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MaterialController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly string connectionString = "Server=localhost;Database=CadastroMateriais;User=root;Password=720910;";



        public MaterialController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Material>>> GetMaterials()
        {
            return await _context.Materiais.ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<Material>> CreateMaterial([FromBody] Material material)
        {
            if (material == null)
            {
                return BadRequest();
            }

            // Validações básicas
            if (string.IsNullOrWhiteSpace(material.Nome) ||
                string.IsNullOrWhiteSpace(material.Descricao) ||
                material.Quantidade <= 0)
            {
                return BadRequest("Dados inválidos.");
            }

            // 🔍 Verifica se já existe um material com o mesmo nome
            var materialExistente = await _context.Materiais
                .FirstOrDefaultAsync(m => m.Nome == material.Nome);

            if (materialExistente != null)
            {
                return Conflict($"Já existe um material cadastrado com o nome '{material.Nome}'.");
            }

            // ✅ Salva o novo material
            await _context.Materiais.AddAsync(material);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Material cadastrado com sucesso.",
                id = material.Id // agora front-end recebe o ID
            });
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMaterial(int id)
        {
            var material = await _context.Materiais.FindAsync(id);
            if (material == null)
            {
                return NotFound();
            }

            _context.Materiais.Remove(material);
            await _context.SaveChangesAsync();

            return NoContent(); // Retorna 204 No Content em caso de sucesso
        }

        [HttpPut("{id}/remover")]
        public async Task<IActionResult> RemoverQuantidade(int id, [FromBody] RemoverRequest req)
        {
            var material = await _context.Materiais.FindAsync(id);
            if (material == null)
            {
                return NotFound("Material não encontrado.");
            }

            if (req.Quantidade <= 0)
            {
                return BadRequest("A quantidade precisa ser maior que zero.");
            }

            if (req.Quantidade > material.Quantidade)
            {
                return BadRequest("Quantidade informada é maior que a disponível.");
            }

            material.Quantidade -= req.Quantidade;

            await _context.SaveChangesAsync();

            return Ok(material);
        }

        [HttpPut("{id}/adicionar")]
        public async Task<IActionResult> AdicionarQuantidade(int id, [FromBody] RemoverRequest req)
        {
            var material = await _context.Materiais.FindAsync(id);
            if (material == null)
            {
                return NotFound("Material não encontrado.");
            }

            if (req.Quantidade <= 0)
            {
                return BadRequest("A quantidade precisa ser maior que zero.");
            }

            material.Quantidade += req.Quantidade;
            await _context.SaveChangesAsync();

            return Ok(material);
        }
        [HttpPost("registrar-movimentacao")]
        public IActionResult RegistrarMovimentacao([FromBody] MovimentacaoRequest request)
        {
            // Aqui forçamos o usuário para teste
            //int usuarioId = 1; // teste com um usuário válido que já exista no banco

            using var connection = new MySqlConnection(connectionString);
            connection.Open();

            var sql = @"
        INSERT INTO movimentacoes (material_id, quantidade, tipo, usuario_id)
        VALUES (@MaterialId, @Quantidade, @Tipo, @UsuarioId)";
            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@MaterialId", request.MaterialId);
            command.Parameters.AddWithValue("@Quantidade", request.Quantidade);
            command.Parameters.AddWithValue("@Tipo", request.Tipo);
            command.Parameters.AddWithValue("@UsuarioId", request.UsuarioId);

            var rows = command.ExecuteNonQuery();

            return Ok($"Movimentação registrada com sucesso. Linhas afetadas: {rows}");
        }




    }


}


