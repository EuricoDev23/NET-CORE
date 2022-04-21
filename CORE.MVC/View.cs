using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.MVC
{
   public class View
    {
        //public int Status { get; set; } = Entity.State.Active.Value();
        [NotMapped]
        [Display(Order = 0,Name ="No.")]
        public int RowNumber { get; set; }
        public View(){ }
        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
