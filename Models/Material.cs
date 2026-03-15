using System.ComponentModel.DataAnnotations;

namespace CadastroMateriais.Models
{
    public class Material
    {
        [Key]
        public int Id { get; set; }
        public string? Nome { get; set; }
        public string? Descricao { get; set; }
        public int Quantidade { get; set; }
    }
}



