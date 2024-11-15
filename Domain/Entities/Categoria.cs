using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Utilidades.Validador;
using Core.Utilidades.Atributos;

namespace Domain.Entities
{
    public class Categoria
    {
        [KeyAttribute, Required]
        public int PK_Categoria { get; set; }
        [Required]
        public string Nome { get; set; }
        public string Descricao { get; set; }

        [Editable(false)]
        public ValidaResultado Validacao { get; set; } = new ValidaResultado();
    }
}
