//using Microsoft.SqlServer.Management.Common;
//using Microsoft.SqlServer.Management.Smo;
using CORE.MVC.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using LinqToDB.Data;

namespace CORE.MVC.Generator.Commands
{
   internal class Database
    {
        public class Helper {

            public const string UseMaster = "USE master;";
            public const string Create = "CREATE DATABASE {0};\nGO\nUSE {0};\nGO\n";
            public const string Drop = "DROP DATABASE {0};\nGO\n";
            public const string CreateSchema = "CREATE SCHEMA {0};\nGO\n";
            public const string Go = "GO";
        }
        static List<KeyValuePair<Type, DatabaseModel.Table>> tables = new List<KeyValuePair<Type, DatabaseModel.Table>>();
        //static List<KeyValuePair<Type, DatabaseModel.Table>> tablesFK = new List<KeyValuePair<Type, DatabaseModel.Table>>();
        private static List<KeyValuePair<Type, DatabaseModel.Table>> TableOrderCreate(IEnumerable<KeyValuePair<Type, DatabaseModel.Table>> tabs)
        {            
            var temp = new List<KeyValuePair<Type, DatabaseModel.Table>>(tabs.OrderBy(a=>a.Value.Fks.Count==0));
               
            var tables = new List<KeyValuePair<Type, DatabaseModel.Table>>();
            //var tablesFK = new List<KeyValuePair<Type, DatabaseModel.Table>>();
            int i = 0;
            while (temp.Count > 0)
            {
                try
                {
                    var tb = temp[i];
                    if (tb.Value.Fks.Count == 0)
                    {
                        tables.Add(tb);
                        temp.Remove(tb);
                    }else{
                        bool success = true;
                        foreach (var item in tb.Value.Fks)
                        {                        
                            if (item.Value.Fields.IsChield == false && item.Value.TypeModel.FullName!=tb.Key.FullName && tables.Any(a=>a.Key.FullName == item.Value.TypeModel.FullName)==false)
                            {
                                success = false;
                                break;
                            }
                        }
                        if(success){
                            tables.Add(tb);
                            temp.Remove(tb);
                        }
                        else{
                            i++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    i = 0;
                }
            }

            return tables;
        }
        private static List<KeyValuePair<Type, DatabaseModel.View>> ViewOrder(IEnumerable<KeyValuePair<Type, DatabaseModel.View>> tabs)
        {
            var temp = new List<KeyValuePair<Type, DatabaseModel.View>>(tabs);

            var tables = new List<KeyValuePair<Type, DatabaseModel.View>>();
            //var tablesFK = new List<KeyValuePair<Type, DatabaseModel.Table>>();
            int i = 0;
            while (temp.Count > 0)
            {
                try
                {
                    var tb = temp[i];
                    {
                        bool success = true;
                        
                        foreach (var item in tabs.Where(a=>a.Key.FullName!=tb.Key.FullName).ToList())
                        {
                            if(tb.Value.File.Sql.Contains(item.Value.Name) && tables.Any(k=>k.Key.FullName==item.Key.FullName)==false){
                                success = false;
                                break;
                            }
                        }
                        if (success)
                        {
                            tables.Add(tb);
                            temp.Remove(tb);
                        }                        
                        i++;
                    }
                }
                catch (Exception ex)
                {
                    i = 0;
                }
            }

            return tables;
        }

        public static bool IsChange(DatabaseModel databaseModel, DatabaseModel appModel)
        {
            try
            {
               return (new DictionaryComparer<Type, DatabaseModel.Table>().Equals(appModel.Tables, databaseModel.Tables))==false;
                //return appModel.Tables.(databaseModel.Tables);
            }
            catch (Exception ex){
                return false;
            }
        }
        private static string ReplaceGo(string cmd){
            return cmd.Replace("GO", "").Replace("go","");
        }
        public static bool Create(DataMapper entity)
        {
            var db = DatabaseModel.Instance;

            //entity.Data.StartTransaction();

            try
            {   //if (!File.Exists("database.sql"))
                //{
                //    File.WriteAllText("database.sql", sql);
                //}
                foreach (var item in db.Databases)
                {
                    bool create = true;

                    if (entity.ExistsDatabase(item.Key))
                    {
                        create = false;
                        ////Carregar versão atual - DatabaseModel
                        //string component = entity.Find.First<Models.Components>().DatabaseModel;
                        //if (component != null)
                        //{
                        //    var dbJson = JsonConvert.DeserializeObject<DatabaseModel>(component);

                        //    //if (IsChange(dbJson, db))
                        //    //{
                        //    //    DropDataBase(entity);
                        //    //    generate = true;
                        //    //}
                        //}
                    }
                    if (create)
                    {
                        //Use master
                        CORE.MVC.Log.Write("A criar banco de dados '" + item.Key + "'");
                       entity.Data.Connection.ChangeDatabase(DatabaseConsts.Master);

                        entity.Data.Execute(ReplaceGo(Helper.UseMaster));
                        //generate database
                        entity.Data.Execute(string.Format("CREATE DATABASE {0};", item.Key));
                        entity.Data.Execute(string.Format("USE {0};", item.Key));
                        //generate schema
                        foreach (var shema in item.Value.Distinct().ToArray())
                        {
                            entity.Data.Execute(string.Format("CREATE SCHEMA {0}; ", shema));

                        }

                        //GENERATED TABLES
                        foreach (var tb in TableOrderCreate(db.Tables.Where(i => i.Value.Name.Contains(item.Key))))
                        {
                            Generator.Commands.Table.Create(entity,tb);
                            CORE.MVC.Log.Write("'" + item.Key + "': a criar tabela "+tb.Value.Name);

                        }

                        //GENERATED VIEWS
                        entity.Data.Execute(string.Format("USE {0};", item.Key));

                        foreach (var view in ViewOrder(db.Views.Where(i => i.Value.FullName.Contains(item.Key)).ToList()))
                        {
                            entity.Data.Execute(Generator.Commands.View.Create(view));
                        }

                        //GENERATED TABLES-TEMP
                        //if (item.Key.Contains(Constants.Database) == false)
                        //{
                        //    entity.Data.Execute(string.Format("USE {0};", Constants.Database));

                        //    foreach (var tb in TableOrderCreate(db.Tables.Where(i => i.Value.Name.Contains(item.Key))))
                        //    {
                        //        entity.Data.Execute(Generator.Commands.Table.Create(tb, true));
                        //    }
                        //}
                        //entity.Data.Commit();
                        entity.Data.Execute(ReplaceGo(Helper.UseMaster));

                        Debug.WriteLine("base de dados criada com êxito");
                        //entity.Save(AutoAction.Instance(item.Key, AutoAction.Type.CreateDatabase)).Validate();

                    }
                }
                
                return true;
            }
            catch (Exception ex)
            {
                DropDataBase(entity);
                return false;
            }
        }

        public static void DropDataBase(DataMapper entity)
        {
            entity.Data.Execute(ReplaceGo(Helper.UseMaster));
            try
            {   
                foreach (var item in DatabaseModel.Instance.Databases.Keys.ToList())
                {
                    try
                    {
                        //if (entity.Find.Exists<AutoAction>(i => i.DbName == item && i.Tipo == AutoAction.Type.WriteData)==false)
                        {
                            entity.Data.Execute(ReplaceGo(string.Format(Helper.Drop, item)));
                        }
                    }
                    catch (Exception ex)
                    {
                        entity.Data.Execute(ReplaceGo(string.Format(Helper.Drop, item)));
                    }
                }
            }
            catch 
            {

            }
        }

        public static void DropDataBases(DataMapper entity)
        {
            entity.Data.Execute(ReplaceGo(Helper.UseMaster));
            try
            {
                foreach (var item in DatabaseModel.Instance.Databases.Keys.ToList())
                {
                    try
                    {
                       // if (entity.Find.Exists<AutoAction>(i => i.DbName == item && i.Tipo == AutoAction.Type.WriteData) == false)
                        {
                            entity.Data.Execute(ReplaceGo(string.Format(Helper.Drop, item)));
                        }
                    }
                    catch (Exception ex)
                    {
                        entity.Data.Execute(ReplaceGo(string.Format(Helper.Drop, item)));
                    }
                }
            }
            catch
            {

            }
        }
        internal static bool CreateDatabaseInServer(DataMapper entity)
        {
            Console.WriteLine(":::A criar base de dados:::");
            var rs = Create(entity);
            //if(rs){
            //    Console.WriteLine("base de dados criada com êxito");
            //    AutoAction.ApplyAction(AutoAction.Type.CreateDatabase);
            //}
            return rs;
        }

        public class DictionaryComparer<TKey, TValue> :
    IEqualityComparer<Dictionary<TKey, TValue>>
        {
            private IEqualityComparer<TValue> valueComparer;
            public DictionaryComparer(IEqualityComparer<TValue> valueComparer = null)
            {
                this.valueComparer = valueComparer ?? EqualityComparer<TValue>.Default;
            }
            public bool Equals(Dictionary<TKey, TValue> x, Dictionary<TKey, TValue> y)
            {
                if (x.Count != y.Count)
                    return false;
                if (x.Keys.Except(y.Keys).Any())
                    return false;
                if (y.Keys.Except(x.Keys).Any())
                    return false;
                foreach (var pair in x)
                    if (!valueComparer.Equals(pair.Value, y[pair.Key]))
                        return false;
                return true;
            }

            public int GetHashCode(Dictionary<TKey, TValue> obj)
            {
                throw new NotImplementedException();
            }
        }
    }

}
