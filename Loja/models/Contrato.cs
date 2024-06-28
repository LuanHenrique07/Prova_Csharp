using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Loja.models;

public class Contrato
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int ClienteId { get; set; }

        [Required]
        public int ServicoId { get; set; }

        [Required]
        public decimal PrecoCobrado { get; set; }

        [Required]
        public DateTime DataContratacao { get; set; }

        [ForeignKey("ServicoId")]
        public Servico Servico { get; set; }
    }