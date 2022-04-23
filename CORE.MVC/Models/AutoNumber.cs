using System;
using System.Collections.Generic;
using System.Linq;

namespace CORE.MVC.Models
{
    [Source(Constants.Database)]
    public class AutoNumber : Entity
    {
        [Pk(PK.Database)]
        public int IDAutoNumber { get; set; }
        public string TypeOf { get; set; }
        public DateTime DataRegisto { get; set; }
        public DateTime? DataValidade { get; set; }
        public int Start { get; set; } = 1;
        public string Chave { get; set; }
        public int Next { get { return Numbers.Count + 1; } }

        public int Generated { get { return Numbers.Count(i => i.Mode == AutoNumberLog.Type.Generated); } }
        public int Pending { get { return Numbers.Count(i => i.Mode == AutoNumberLog.Type.Pending); } }
        public int Allocated { get { return Numbers.Count(i => i.Mode == AutoNumberLog.Type.Allocated); } }
        public Type Mode { get; set; }
        [Fk("IDAutoNumber", "IDAutoNumber")]
        public List<AutoNumberLog> Numbers { get; set; } = new List<AutoNumberLog>();

        public enum Type
        {
            Year = 1,
            Normal = 2
        }
        protected override void TriggerBeforeInsert()
        {
            DataRegisto = DateTime.Now;
        }
        //public static Result Register(System.Type Tipo, Type Mode)
        //{
        //    AutoNumber auto = new AutoNumber();
        //    auto.TypeOf = Tipo.FullName;
        //    auto.DataRegisto = DateTime.Now;
        //    auto.Mode = Mode;
        //    if (Mode == Type.Year) {
        //        auto.DataValidade = DateTime.Parse($"{DateTime.Now.Year}-12-31");
        //    }
        //    auto.Action = Config.Action.Record;

        //    return auto.Save();
        //}

        public static AutoNumberLog Generate(System.Type Tipo, Type Mode = Type.Year)
        {
            var dm = new DataMapper();
            AutoNumber auto_number = null;
            //AutoNumber auto_number = DataMapper<AutoNumber>.Find.First(i => i.TypeOf == Tipo.FullName && (
            //    (Mode == Type.Year && i.Mode == Mode && (i.DataValidade.HasValue == false || i.DataValidade.GetValueOrDefault().Ticks > DateTime.Now.Ticks) )||
            //    (i.Mode==Mode)
            //));
            bool new_auto_number = false;
            if (auto_number == null || auto_number.IDAutoNumber == 0)
            {
                auto_number = new AutoNumber();
                auto_number.TypeOf = Tipo.FullName;
                auto_number.DataRegisto = DateTime.Now;
                auto_number.Mode = Mode;
                if (Mode == Type.Year)
                {
                    auto_number.DataValidade = DateTime.Parse($"{DateTime.Now.Year}-12-31");
                    auto_number.Chave = $"{DateTime.Now.Year}";
                }
                //auto_number.Save();
                new_auto_number = true;
            }
            //AutoNumberLog
            AutoNumberLog auto = auto_number.Numbers.FirstOrDefault(i => i.Mode == AutoNumberLog.Type.Pending);
            if (auto != null && auto.IDAutoNumberLog > 0)
            {
                auto.Mode = AutoNumberLog.Type.Allocated;
                dm.Save(auto);
            }
            else
            {
                auto = new AutoNumberLog();
                auto.IDAutoNumber = auto_number.IDAutoNumber;
                auto.Mode = AutoNumberLog.Type.Allocated;
                auto.Number = new_auto_number ? auto_number.Start : auto_number.Next;
                auto_number.Numbers.Add(auto);

                dm.Save(auto);
                auto.AutoNumber = auto_number;
            }
            return auto;
        }
    }
}
