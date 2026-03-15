namespace CadastroMateriais.Models
{
    public class Usuario
    {
        public int Id { get; set; }         // opcional, usado pelo DB
        public string Nome { get; set; }    = string.Empty;
        public string Email { get; set; }   = string.Empty;
        public string SenhaHash { get; set; } = string.Empty;
    }
}

