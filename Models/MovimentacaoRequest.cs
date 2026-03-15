public class MovimentacaoRequest
{
    public int MaterialId { get; set; }
    public int Quantidade { get; set; }
    public string Tipo { get; set; } // "entrada" ou "saida"
    public int UsuarioId { get; set; } // <-- novo campo
}

