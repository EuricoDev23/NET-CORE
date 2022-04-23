using LinqToDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using static CORE.MVC.DatabaseModel;

namespace CORE.MVC
{
    internal static class DatabaseConsts
    {

        internal static string DefaultBD = @"core";
        internal static string Master = @"master";
        internal static JsonConfig JsonConfig = null;
        public static Table GetTable(this Dictionary<Type, Table> values, Type type)
        {
            var Table = values.FirstOrDefault(i => i.Key.FullName == type.FullName).Value;
            return Table;
        }
    }
    internal sealed class DatabaseModel
    {
        internal static DataMapper DataMapper = null;
        internal static object syncRoot = new Object();
        private static volatile DataMapper.StateMode state = DataMapper.StateMode.None;

        internal static volatile DatabaseModel DatabaseBody_ = null;
        //internal static volatile bool Generate;

        [JsonIgnore]
        public static DataMapper.StateMode State
        {
            get
            {
                return state;
            }
        }

        [JsonIgnore]
        public static DatabaseModel Instance
        {
            get
            {
                if (DatabaseBody_ == null)
                {
                    lock (syncRoot)
                    {
                        if (DatabaseBody_ == null)
                        {
                            CORE.MVC.Log.Write("A conectar-se ao servidor");

                            DatabaseBody_ = Reflection.Database.LoadDatabaseModel();
                            CORE.MVC.Log.Write("A extrair componentes do banco de dados");
                            state = DataMapper.StateMode.Running;
                            
                        }
                    }
                }
                return DatabaseBody_;
            }
        }

        private readonly List<Function> Functions_ = new List<Function>();
        private List<Procedure> Procedures_ = new List<Procedure>();

        public Dictionary<Type, JsonConnetion> Mapper { get; set; } = new Dictionary<Type, JsonConnetion>(0);
        public Dictionary<Type, Table> Tables { get; set; } = new Dictionary<Type, Table>(0);

        public Dictionary<Type, View> Views { get; set; } = new Dictionary<Type, View>(0);

        public List<Function> Functions { get { return Functions_; } }
        public List<Procedure> Procedures { get { return Procedures_; } }

        public Base TableOrView(Type type)
        {
            try
            {
                Base b = null;
                if (type.BaseType.Name == typeof(Entity).Name)
                {
                    b = new Base();
                    var tb = Instance.Tables[type];
                    b.Name = tb.Name;
                    b.Cols.AddRange(tb.Columns.Select(a => a.Name));
                }
                else
                {
                    b = new Base();
                    var tb = Instance.Views[type];
                    b.Name = tb.Name;
                    b.Cols.AddRange(tb.Columns.Select(a => a.Key));
                }
                return b;
            }
            catch (Exception)
            {
                return null;
            }
        }

        #region Classes

        #region structures

        public class Table
        {
            public string Database { get; set; }
            public string Name { get; set; }
            public string SimpleName { get; set; }
            public string ShortName { get; set; }
            public string Schema { get; set; }

            public string TempName { get; set; }
            public List<Column> Columns { get; set; } = new List<Column>(0);
            public Dictionary<string, FK> Fks { get; set; } = new Dictionary<string, FK>(0);

            public string GetNameByPropertyMame(string name)
            {
                return Columns.FirstOrDefault(i => i.Property.Name == name).Name;
            }
            public Column[] GetPrimaryKeys()
            {
                return this.Columns.Where(i => i.PrimaryKey).ToArray();
            }
            [JsonIgnore]
            public Column PrimaryKey
            {
                get { try { return this.GetPrimaryKeys()[0]; } catch { return null; } }
            }
            public class FK
            {
                public FkAttribute Fields { get; set; }
                public Type TypeModel { get; set; }
                public bool IsNotSave { get; set; }
            }
            public Column GetColumnFK(string tb_name)
            {
                try
                {
                    var fk = Fks[tb_name];
                    return Instance.Tables[fk.TypeModel].Columns.FirstOrDefault(i => (fk.Fields.IsChield && i.Property.Name == fk.Fields.ParentKey) || (fk.Fields.IsChield == false && i.Property.Name == fk.Fields.ForeignKey));
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
            public Table GetTableFK(string tb_name)
            {
                try
                {
                    var fk = Fks[tb_name];
                    return Instance.Tables[fk.TypeModel];
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }

        public class View
        {
            public string Name { get; set; }
            public string FullName { get; set; }
            public File File { get; set; } = new File();
            public Dictionary<string, Type> Columns { get; set; } = new Dictionary<string, Type>(0);
        }
        public class Function
        {
        }
        public class Procedure
        {
        }
        #endregion

        #region Fields

        public class Field
        {
            public bool Unique { get; set; }
            public string Name { get; set; }
            public string Display { get; set; }
            public DataType Type { get; set; } = LinqToDB.DataType.NVarChar;
            public int Length { get; set; }
            public int Precision { get; set; }
            public int Scale { get; set; }
            public bool IsNull { get; set; }
            public int Order { get; set; }
            public string DBDataType
            {
                get
                {
                    var t = Type.DbSqlName();
                    if (Length > 0 && t.Contains("("))
                    {
                        return $"{t.Split("(")[0]}({Length})";
                    }
                    else if (Precision > 0 && Scale > 0 && t.Contains("("))
                    {
                        return $"{t.Split("(")[0]}({Precision},{Scale})";
                    }
                    return t;
                }
            }

        }
        public class Column : Field
        {
            #region Primary Key
            public bool PrimaryKey { get; set; }
            public Entity.PK? AutoIncrement { get; set; }

            #endregion
            [JsonIgnore]
            public System.Reflection.PropertyInfo Property { get; set; }
        }
        public class File
        {
            public string Name { get; set; }
            public string Sql { get; set; }
        }
        public class Base
        {
            public string Name { get; set; }
            public List<string> Cols { get; set; }
        }
        #endregion

        #endregion
        public static void SetStatus(DataMapper.StateMode State)
        {
            lock (syncRoot)
            {
                state = State;
            }
        }
        public Dictionary<string, IEnumerable<string>> Databases
        {
            get
            {
                Dictionary<string, IEnumerable<string>> list = new Dictionary<string, IEnumerable<string>>();

                foreach (var item in Tables.OrderByDescending(i => i.Value.Fks.Count() == 0).GroupBy(i => (i.Key.GetSourceAttribute().Database)).Select(i => i.Key))
                {
                    list.Add(item, Tables.Where(i => i.Key.GetSourceAttribute().Database == item && i.Key.GetSourceAttribute().Schema.Contains("dbo") == false).GroupBy(i => i.Key.GetSourceAttribute().Schema).Select(i => i.Key));
                }
                return list;
            }
        }

    }

}
