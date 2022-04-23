using System;

namespace CORE.MVC.Models
{
    [Source(Constants.Database)]
    class AutoLog : Entity
    {
        [Pk(PK.Application)]
        public long ID { get; set; }
        public string Descricao { get; set; }
        public DateTime DataExec { get; set; }

        //public static Result ApplyAction(Type Tipo)
        //{
        //    AutoAction auto = new AutoAction();
        //    auto.Descricao = Tipo.ToString();
        //    auto.DataExec = DateTime.Now;
        //    auto.Tipo = Tipo;
        //    auto.Action = Config.Action.Record;

        //    return auto.Save();
        //}
    }
}
