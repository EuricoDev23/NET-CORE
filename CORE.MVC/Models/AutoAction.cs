using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.MVC.Models
{
    [Source(Constants.Database)]
     class AutoAction:Entity
    {
        [Pk(PK.Database)]
        public int IDAutoAction { get; set; }
        public string DbName { get; set; }
        public string Descricao { get; set; }
        public DateTime? DataExec { get; set; }
        public Type Tipo { get; set; }
        public enum Type
        {
            CreateDatabase = 1,
            WriteData = 2,
            Backup = 3,
            Restore = 4,
        }
        public static Result ApplyAction(string Database, Type Tipo)
        {
            AutoAction auto = new AutoAction();
            auto.Descricao = Tipo.ToString();
            auto.DataExec = DateTime.Now;
            auto.DbName = Database;
            auto.Tipo = Tipo;
            return auto.Save();
        }
        public static AutoAction Instance(string Database, Type Tipo)
        {
            AutoAction auto = new AutoAction();
            auto.Descricao = Tipo.ToString();
            auto.DataExec = DateTime.Now;
            auto.DbName = Database;
            auto.Tipo = Tipo;
            return auto;
        }
        public static bool IsExecute(string Database,Type Tipo)
        {
            //return DataMapper<AutoAction>.Find.Exists(i =>i.DbName==Database && i.Tipo == Tipo && i.DataExec.HasValue);
            return true;
        }
    }
}
