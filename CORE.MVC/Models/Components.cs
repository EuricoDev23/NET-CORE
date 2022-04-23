using LinqToDB.Mapping;
using System;

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
