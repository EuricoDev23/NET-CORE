using LinqToDB.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CORE.MVC.Generator.Commands
{
    internal class Table
    {
        public class Helper
        {

            public const string Create = "CREATE TABLE {0} ";
            public const string AddColumn = "{0} {1} {2}";

            public const string Null = "NULL";
            public const string Not_Null = "NOT NULL";

            public const string PrimaryKey = "PRIMARY KEY ({0})";
            //public const string PrimaryKey = "ALTER TABLE {0} ADD CONSTRAINT PK_{0} PRIMARY KEY ({1})";
            public const string Constraint = "CONSTRAINT {0} FOREIGN KEY ({1}) REFERENCES {2} ({3})";
            public const string Constraint_v2 = "ALTER TABLE {0} ADD CONSTRAINT {1} FOREIGN KEY ({2}) REFERENCES {3} ({4})";
            public const string ConstraintUnique = "CONSTRAINT UNIQUE_{0} UNIQUE({1})";
        }
        private static string AutoImcrement(DataMapper mapper)
        {
            Dictionary<string, string> db = new Dictionary<string, string>();
            db.Add("sqlserver", "IDENTITY(100,1)");
            db.Add("mysql", "AUTO_INCREMENT");
            var sx = db.FirstOrDefault(a => mapper.Config.Provider.ToLower().Contains(a.Key)).Value;
            return sx;
        }
        private string RemoveSchema(string tb)
        {
            var c = tb.Split(".");
            return c.Length == 1 ? tb:c[c.Length-1];
        }
        public static void Create(DataMapper mapper, KeyValuePair<Type, DatabaseModel.Table> table, bool temp_table = false)
        {
            var tbSource = table.Key.GetSourceAttribute();
            StringBuilder sb = new StringBuilder(string.Format(Helper.Create, temp_table ? tbSource.TempShortName : tbSource.Name));

            var cols = new List<string>();
            //percorrer campos
            foreach (var item in table.Value.Columns)
            {
                cols.Add(string.Format(Helper.AddColumn,
                item.Name,
                string.Concat(item.DBDataType, temp_table == false && item.AutoIncrement == Entity.PK.Database ? $" {AutoImcrement(mapper)}" : ""),
                item.IsNull ? Helper.Null : Helper.Not_Null));
            }
            ////percorrer Pks
            var pks = table.Value.GetPrimaryKeys();
            if (pks != null && pks.Length > 0)
            {
                cols.Add(string.Format(Helper.PrimaryKey, string.Join(",", table.Value.GetPrimaryKeys().Select(i => i.Name))));
            }
            if (temp_table == false)
            {
                //Constraints
                foreach (var item in table.Value.Columns.Where(i => i.Unique).ToList())
                {
                    cols.Add(string.Format(Helper.ConstraintUnique, $"{tbSource.Name}_{item.Name}", item.Name));
                }
            }
            sb.AppendFormat("(\n{0}\n);", string.Join(",\n", cols));
            sb.AppendLine("\n");
            File.AppendAllText("tmp.sql", sb.ToString());
            mapper.Data.Execute(sb.ToString());
            //sb.Clear();
            //percorrer Fks
            if (temp_table == false)
            {
                //percorrer Pks
                //var pks = table.Value.GetPrimaryKeys();
                //if (pks != null && pks.Length > 0)
                //{
                //    var sqlpk = string.Format(Helper.PrimaryKey,
                //        tbSource.ShortName,
                //        string.Join(",", table.Value.GetPrimaryKeys().Select(i => i.Name))
                //        );                    
                //    mapper.Data.Execute(sqlpk);
                //}

                string sql = "";
                foreach (var item in table.Value.Fks)
                {
                    var fk = DatabaseModel.Instance.Tables[item.Value.TypeModel];
                    var fk_source = item.Value.TypeModel.GetSourceAttribute();

                    if (item.Value.Fields.IsChield == false && fk_source.Database.ToUpper() == tbSource.Database.ToUpper())
                    {
                        sql = string.Format(Helper.Constraint_v2,
                        tbSource.Name,
                        string.Concat("FK_", tbSource.Name, "_", item.Key),
                        item.Value.Fields.ForeignKey,
                        fk_source.Name,
                        string.IsNullOrWhiteSpace(item.Value.Fields.ParentKey) ? fk.PrimaryKey.Name : item.Value.Fields.ParentKey
                        ) + item.Value.Fields.DeleteMode.Value();
                        sb.Append(sql);

                        //File.WriteAllText("tmp.sql", sb.ToString());
                        mapper.Data.Execute(sql);
                    }
                }
            }

            //return sb.ToString();
        }
        public static void Create(DataMapper mapper, ICollection<KeyValuePair<Type, DatabaseModel.Table>> tables, bool temp_table = false)
        {

            //StringBuilder sb = new StringBuilder();
            foreach (var item in tables)
            {
                Create(mapper, item, temp_table);
            }

        }
    }

}
