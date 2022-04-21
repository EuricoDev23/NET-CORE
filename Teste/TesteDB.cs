using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TESTE
{
   public class TESTEDB:CORE.MVC.DataMapper
    {
        protected override void Initialize(string ConnectionName = "TESTE")
        {
            base.Initialize(ConnectionName);
        }
    }
}
