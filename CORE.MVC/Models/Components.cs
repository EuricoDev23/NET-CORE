using LinqToDB.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.MVC.Models
{
    [Source(Constants.Database)]
     class Components : Entity
    {
        [Pk(PK.Database)]
        public int IDComponent { get; set; }
        [Column]
        public string DatabaseModel { get; set; }
        
    }
}
