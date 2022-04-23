using System.ComponentModel.DataAnnotations;

namespace CORE.MVC
{
    public class View
    {
        //public int Status { get; set; } = Entity.State.Active.Value();
        [NotMapped]
        [Display(Order = 0, Name = "No.")]
        public int RowNumber { get; set; }
        public View() { }
        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
