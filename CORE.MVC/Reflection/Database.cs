using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CORE.MVC.Reflection
{
    internal static class Database
    {
        public static List<Type> GetDatabasesMapper()
        {
            List<Type> list = new List<Type>();

            list = ReflectionExtension.AssemblyGetTypes(typeof(DataMapper));

            return list;
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

        public static List<DataMapper> GetInitializes()
        {
            List<Type> list = new List<Type>();
            list = ReflectionExtension.AssemblyGetTypes(typeof(DataMapper));

            var op = new List<DataMapper>();
            foreach (var item in list)
            {
                op.Add((DataMapper)Activator.CreateInstance(item));
            }
            return op;
        }

        public static DatabaseModel LoadDatabaseModel()
        {
            var db_list = GetDatabasesMapper();
            DatabaseModel database = new DatabaseModel();
            var db_con_list = GetStringByDatabases();
            if (db_list == null)
            {
                throw new Exception("Arquivo 'core_mvc.json' não encontrado!");
            }
            for (int i = 0; i < db_list.Count; i++)
            {
                var db_type = db_list[i];
                var mtd_con = db_type.GetMethod("Initialize", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var p = mtd_con.GetParameters();
                var name = p[0].RawDefaultValue?.ToString();
                var conf = db_con_list.Connetions.FirstOrDefault(i => i.Key == name).Value;
                if (conf == null)
                {
                    throw new Exception($"'{db_type.FullName}' chave de conexão não encontrado!");
                }
                database.Mapper.Add(db_type, conf);
            }
            DatabaseModel.DatabaseBody_ = database;
            var Types = Table.AssemblyFindModels();
            var Views = View.AssemblyFindModels();

            database.Tables = Table.LoadTables(Types);
            database.Views = View.LoadViews(Views);

            return database;
        }

    }
}
