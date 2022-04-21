using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CORE.MVC.Reflection
{
    internal static class View
    {
        #region Assembly Finds 
        public static List<Type> AssemblyFindModels()
        {
            List<Type> list = new List<Type>();
            
           list = ReflectionExtension.AssemblyGetTypes(typeof(MVC.View));

            return list.Distinct().ToList();
        }
        #endregion

        #region View
        public static Dictionary<Type, DatabaseModel.View> LoadViews(List<Type> Types)
        {

            Dictionary<Type, DatabaseModel.View> tables = new Dictionary<Type, DatabaseModel.View>(Types.Count);

            foreach (var type_model in Types)
            {
                var model = Activator.CreateInstance(type_model).GetType();

                var tb = model.GetSourceAttribute();

                var item = new DatabaseModel.View
                {
                    FullName = tb.FullName,
                    Name = tb.ShortName
                };
                foreach (var col in model.Properties().LoadColumns())
                {
                    item.Columns.Add(col.Name, col.Property.PropertyType);
                }

                string fileName = model.GetFileName(model.Name);
                string fileSql = model.FileReadLine(fileName);
                item.File = new DatabaseModel.File {
                    Name = fileName,
                    Sql = fileSql
                };

                tables.Add(type_model, item);
            }
            return tables;
        }

        #endregion

    }
}
