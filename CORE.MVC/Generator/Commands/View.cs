using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.MVC.Generator.Commands
{
   internal class View
    {
        public class Helper
        {
            public const string Create = "CREATE VIEW {0} AS {1}";
        }
        public static string Create(KeyValuePair<Type, DatabaseModel.View> view)
        {
            StringBuilder sb = new StringBuilder(string.Format(Helper.Create, view.Value.Name,view.Value.File.Sql));

            return sb.ToString();
        }
        public static string Create(ICollection<KeyValuePair<Type, DatabaseModel.View>> view)
        {

            StringBuilder sb = new StringBuilder();
            foreach (var item in view)
            {
                sb.AppendLine(Create(item)+"\n");
            }

            return sb.ToString();
        }
    }
}
