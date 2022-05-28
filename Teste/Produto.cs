using LinqToDB.Mapping;
using System.Collections.Generic;

namespace TESTE
{
    [Table]
    public class Produto : CORE.MVC.Entity
    {
        [PrimaryKey, Identity]
        public int ProductID { get; set; }

        [Column(Length = 100), NotNull]
        public string Nome { get; set; }
        [Column]
        public int? IDCategoria { get; set; }

        [Column, NotNull]
        public bool IsStock { get; set; }

        [Association(ThisKey = nameof(IDCategoria), OtherKey = nameof(TESTE.Categoria.IDCategoria))]
        public Categoria Categoria { get; set; }
    }
}
