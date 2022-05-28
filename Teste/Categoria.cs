using CORE.MVC;
using LinqToDB.Mapping;
using System.Collections.Generic;

namespace TESTE
{
    public class Categoria : CORE.MVC.Entity
    {
        [PrimaryKey, Identity]
        public int IDCategoria { get; set; }

        [Column, NotNull]
        public string Nome { get; set; }
        /*[Association(ThisKey = nameof(IDCategoria), OtherKey = nameof(Produto.IDCategoria))]
        [NotSave]
        public List<Produto> ProdutoList { get; set; }*/
    }
}
