using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CORE.MVC.Entity;

namespace CORE.MVC
{
   [AttributeUsage(AttributeTargets.Class)]
   public class SourceAttribute : Attribute
    {
        internal string Database { get; set; }
        internal string Name { get; set; }
        internal string Schema { get; set; }
        internal string File { get; set; }
        internal string FullName
        {
            get
            {
                return string.Concat(Database, ".", Schema, ".", Name);
            }
        }
        internal string TempFullName
        {
            get
            {
                return string.Concat(Constants.Database, ".dbo.", FullName.Replace(".", "_"));
            }
        }
        internal string ShortName
        {
            get
            {
                return string.Concat(Schema, ".", Name);
            }
        }
        internal string TempShortName
        {
            get
            {
                return string.Concat("dbo.", FullName.Replace(".", "_"));
            }
        }
        public SourceAttribute(string Database)
        {
            this.Database = Database;
            this.Schema = "dbo";
        }
        public SourceAttribute(string Database, string Name)
        {
            this.Database = Database;
            this.Name = Name;
            this.Schema = "dbo";
        }
        
        public SourceAttribute(string Database, string Name, string Schema="dbo")
        {
            this.Database = Database;
            this.Name = Name;
            this.Schema = Schema;
        }
        //public SourceAttribute(string Database, string Name, string Schema,string File)
        //{
        //    this.Database = Database;
        //    this.Name = Name;
        //    this.Schema = Schema;
        //    this.File = File;
        //}
    }

    #region Keys
    [AttributeUsage(AttributeTargets.Property)]
    public class PkAttribute : Attribute
    {
        internal Entity.PK? AutoIncrement;

        /// <summary>
        /// Primary Key
        /// </summary>
        public PkAttribute()
        {
        }
        /// <summary>
        /// Primary Key
        /// </summary>
        /// <param name="AutoIncrement"> Auto-incremento</param>
        public PkAttribute(Entity.PK AutoIncrement)
        {
            this.AutoIncrement = AutoIncrement;
        }
        

    }
    public class IdentityModeAttribute : Attribute
    {
        internal Entity.PK AutoIncrement=PK.Database;

        /// <summary>
        /// Primary Key
        /// </summary>
        public IdentityModeAttribute()
        {
        }
        /// <summary>
        /// Primary Key
        /// </summary>
        /// <param name="AutoIncrement"> Auto-incremento</param>
        public IdentityModeAttribute(Entity.PK Mode)
        {
            this.AutoIncrement = Mode;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class FkAttribute : Attribute
    {
        internal string ForeignKey;
        internal string ParentKey;
        internal bool IsChield;
        internal Entity.OnDelete DeleteMode=OnDelete.Default;
        internal Dictionary<string, object> DefaultValues=null;
        internal FkAttribute() { }
        public static string DefaultValue(string Campo, object Value)
        {
            Dictionary<string, object> values = new Dictionary<string, object>();
            values.Add(Campo, Value);
            return System.Text.Json.JsonSerializer.Serialize(values,values.GetType());
        }
        public FkAttribute(string ForeignKey, Entity.OnDelete OnDelete=OnDelete.Default)
        {
            DeleteMode = OnDelete;
            this.ForeignKey = ForeignKey;
        }
        public FkAttribute(string ForeignKey, Entity.OnDelete OnDelete, string DefaultValues)
        {
            DeleteMode = OnDelete;
            this.ForeignKey = ForeignKey;
            try
            {
                if (DefaultValues != null)
                {
                    this.DefaultValues = JsonSerializer.Deserialize<Dictionary<string, object>>(DefaultValues);
                }
            }
            catch (Exception)
            {
                this.DefaultValues = null;
            }
        }
        public FkAttribute(string ForeignKey, string ParentKey, Entity.OnDelete OnDelete = Entity.OnDelete.Default)
        {
            DeleteMode = OnDelete;
            this.ForeignKey = ForeignKey;
            this.ParentKey = ParentKey;
        }
        public FkAttribute(string ForeignKey, string ParentKey, Entity.OnDelete OnDelete, string DefaultValues)
        {
            DeleteMode = OnDelete;
            this.ForeignKey = ForeignKey;
            this.ParentKey = ParentKey;
            try
            {
                if (DefaultValues != null)
                {
                    this.DefaultValues = JsonSerializer.Deserialize<Dictionary<string, object>>(DefaultValues);
                }
            }
            catch (Exception)
            {
                this.DefaultValues = null;
            }
        }
        public static object Values(object m){
            return m;
        }
    }
    internal static class FkAttributeExtension{

        public static string Value(this Entity.OnDelete on)
        {
            if (on == Entity.OnDelete.Cascade)
            { return " ON DELETE CASCADE"; }
            else if (on == Entity.OnDelete.SetNull)
            { return " ON DELETE SET NULL"; }
            return "";
        }
    }
    #endregion

    #region Colunas
    //[AttributeUsage(AttributeTargets.Property)]

    //public sealed class ColumnAttribute : Attribute
    //{
    //    internal string Name;
    //    internal System.Data.SqlDbType DataType=System.Data.SqlDbType.VarChar;
    //    internal string Capacity;
    //    internal object DefaultValue = null;
    //    public ColumnAttribute(string Name, System.Data.SqlDbType DataType, string Capacity,object DefaultValue)
    //    {
    //        this.Name = Name;
    //        this.DataType = DataType;
    //        this.Capacity = Capacity;
    //        this.DefaultValue = DefaultValue;
    //    }
    //    /// <summary>
    //    /// Informe o nome da coluna
    //    /// </summary>
    //    /// <param name="Name">Nome da coluna</param>
    //    public ColumnAttribute(string Name)
    //    {
    //        this.Name = Name;
    //    }
    //    /// <summary>
    //    /// Informe o nome e tipo de dado
    //    /// </summary>
    //    /// <param name="Name">Nome do campo</param>
    //    /// <param name="DataType">Tipo de dado</param>
    //    public ColumnAttribute(string Name, System.Data.SqlDbType DataType)
    //    {
    //        this.Name = Name;
    //        this.DataType = DataType;
    //    }
    //    public ColumnAttribute(string Name, string Capacity)
    //    {
    //        this.Name = Name;
    //        this.Capacity = Capacity;
    //    }
    //    public ColumnAttribute(string Name, object DefaultValue)
    //    {
    //        this.Name = Name;
    //        this.DefaultValue = DefaultValue;
    //    }
    //    public ColumnAttribute(string Name, System.Data.SqlDbType DataType, string Capacity)
    //    {
    //        this.Name = Name;
    //        this.DataType = DataType;
    //        this.Capacity = Capacity;
    //    }

    //}
    /// <summary>
    /// Não incluir na base de dados
    /// </summary>
    public sealed class NotMappedAttribute : Attribute {
    }
    public sealed class NotSaveAttribute : Attribute
    {
    }
    #endregion

    #region Constraint
    /// <summary>
    /// Campo de valor Unico
    /// </summary>
    public sealed class UniqueAttribute : Attribute
    {
    }
    public sealed class ExistAttribute : Attribute
    {
    }
    #endregion
}
