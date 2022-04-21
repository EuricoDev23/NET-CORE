using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqToDB.Mapping;

namespace TESTE
{
   [Table]
   public class Produto:CORE.MVC.Entity
    {
        [PrimaryKey, Identity]
        public int ProductID { get; set; }

        [Column(Length = 100), NotNull]
        public string Nome { get; set; }
        [Column]
        public int? IDCategoria { get; set; }

        [Association(ThisKey = nameof(IDCategoria), OtherKey = nameof(TESTE.Categoria.IDCategoria))]
        public Categoria Categoria { get; set; }

    }
}
