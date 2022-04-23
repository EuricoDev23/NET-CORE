using System;

namespace CORE.MVC.Models
{
    [Source(Constants.Database)]
    public class AutoNumberLog : Entity
    {
        [Pk(PK.Database)]
        public int IDAutoNumberLog { get; set; }
        public int IDAutoNumber { get; set; }
        public DateTime DataRegisto { get; set; }
        public int Number { get; set; }
        public Type Mode { get; set; }
        [Fk("IDAutoNumber")]
        public AutoNumber AutoNumber { get; set; }

        public enum Type
        {
            Pending = 1,
            Allocated = 2,
            Generated = 3
        }
        protected override void TriggerBeforeInsert()
        {
            DataRegisto = DateTime.Now;
        }
    }
}
