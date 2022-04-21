using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqToDB.Mapping;

namespace TESTE
{
   public class Categoria:CORE.MVC.Entity
    {
        [PrimaryKey, Identity]
        public int IDCategoria { get; set; }

        [Column, NotNull]
        public string Nome { get; set; }

    }
}
