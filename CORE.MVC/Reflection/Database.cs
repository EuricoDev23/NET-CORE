using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CORE.MVC.Reflection
{
   internal static class Database
    {
        public static string[] GetStringByDatabase()
        {
            List<Type> list = new List<Type>();
            string[] con =null;

            list = ReflectionExtension.AssemblyGetTypes(typeof(DataMapper));
            
            if(con == null){
                try
                {
                    var mtd_con = list.FirstOrDefault().GetMethod("Initialize", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var p = mtd_con.GetParameters();
                    con = new string[] { p[0].RawDefaultValue?.ToString(), p[1].RawDefaultValue?.ToString() };
                }
                catch (Exception)
                {
                    con = new string[] { "CORE", "CORE_FRAMEWORK" };
                }
            }

            return con;
        }
        public static JsonConfig GetStringByDatabases()
        {            
                try
                {
                   return System.Text.Json.JsonSerializer.Deserialize<JsonConfig>(File.ReadAllText("core_mvc.json"));

                }
                catch (Exception)
                {
                return null;
                   }
            
        }

        public static DataMapper[] GetInitializes()
        {
            List<Type> list = new List<Type>();

            //var t1 = Assembly.GetEntryAssembly().GetTypes().Distinct().Where(x => typeof(DataMapper).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract && (x.FullName != typeof(DataMapper).FullName)).ToList();
            //if (t1.Count > 0)
            //{
            //    list.AddRange(t1);
            //}

            //foreach (var item in Assembly.GetEntryAssembly().GetReferencedAssemblies())
            //{
            //    var types = Assembly.Load(item).ExportedTypes.Distinct().Where(x => typeof(DataMapper).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract).ToList();
            //    if (types != null && types.Count() > 0)
            //    {
            //        foreach (var t in types)
            //        {
            //            if (list.Exists(j => j.FullName == t.FullName) == false)
            //            {
            //                list.Add(t);
            //            }
            //        }
            //    }
            //}
            list = ReflectionExtension.AssemblyGetTypes(typeof(DataMapper));

            var op = new List<DataMapper>();
            foreach (var item in list)
            {
                op.Add((DataMapper)Activator.CreateInstance(item));
            }
            return op.ToArray();
        }
        public static DatabaseModel LoadDatabaseModel()
        {
            var Types = Table.AssemblyFindModels();
            var Views = View.AssemblyFindModels();

            DatabaseModel database = new DatabaseModel();

            database.Tables = Table.LoadTables(Types);
            database.Views = View.LoadViews(Views);

            return database;
        }

    }
}
