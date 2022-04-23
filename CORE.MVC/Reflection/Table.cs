using LinqToDB.Mapping;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace CORE.MVC.Reflection
{
    internal static class Table
    {
        #region Assembly Finds 
        public static List<Type> AssemblyFindModels()
        {
            List<Type> list = new List<Type>();

            list = ReflectionExtension.AssemblyGetTypes(typeof(Entity));
            //list.AddRange(new Type[] {
            //     typeof(Models.AutoLog),
            //     typeof(Models.AutoAction),
            //     typeof(Models.Components),
            //});

            return list.Distinct().ToList();
        }
        #endregion

        #region Load Database Model

        public static List<DatabaseModel.Column> LoadColumns(this List<PropertyInfo> props)
        {
            List<DatabaseModel.Column> columns = new List<DatabaseModel.Column>();

            foreach (var item in props)
            {
                if ((item.GetCustomAttribute<ColumnAttribute>() != null || item.GetCustomAttribute<PrimaryKeyAttribute>() != null) && item.GetCustomAttribute<NotMappedAttribute>() == null)
                {
                    var col = item.GetColumnAttribute();
                    var pk = item.GetPkAttribute();
                    var AutoIncrement = item.GetIdentityAttribute();

                    var c = new DatabaseModel.Column
                    {
                        Name = string.IsNullOrWhiteSpace(col.Name) ? item.Name : col.Name,
                        Display = item.GetCustomAttribute<DisplayAttribute>()?.Name ?? item.Name,
                        Length = col.Length,
                        Precision = col.Precision,
                        Scale = col.Scale,
                        Type = col.DataType == LinqToDB.DataType.Undefined ? item.PropertyType.DbType() : col.DataType,
                        Unique = item.GetCustomAttribute<UniqueAttribute>() != null,
                        IsNull = pk != null || item.GetCustomAttribute<NotNullAttribute>() != null ? false : true,// item.PropertyType.IsNull(),
                        Property = item
                    };
                    if (pk != null)
                    {
                        c.PrimaryKey = true;
                    }
                    if (AutoIncrement != null)
                    {
                        c.AutoIncrement = AutoIncrement.AutoIncrement;
                    }
                    columns.Add(c);
                }
            }
            return columns;
        }

        public static Dictionary<Type, DatabaseModel.Table> LoadTables(List<Type> Types)
        {
            Dictionary<Type, DatabaseModel.Table> tables = new Dictionary<Type, DatabaseModel.Table>(Types.Count);

            foreach (var type_model in Types)
            {
                var model = Activator.CreateInstance(type_model).GetType();

                var tb = model.GetSourceAttribute();

                var item = new DatabaseModel.Table
                {
                    Database = tb.Database,
                    SimpleName = tb.ShortName,
                    ShortName = tb.Name,
                    Schema = tb.Schema,
                    Name = tb.FullName,
                    TempName = tb.TempFullName,
                    Columns = model.Properties().LoadColumns()
                };
                //Listar objectos FK
                foreach (var x in model.PropertiesClass())
                {
                    var fk = new DatabaseModel.Table.FK();
                    fk.Fields = x.GetFkAttribute();
                    fk.TypeModel = x.GetTypeCollection();
                    fk.IsNotSave = x.AttributeValue<NotSaveAttribute>() != null;
                    //fk.Fields.IsChield = (item.PrimaryKey.AutoIncrement.HasValue && item.PrimaryKey.Name == fk.Fields.ForeignKey);
                    //fk.Fields.IsChield = (item.PrimaryKey.i && item.PrimaryKey.Name == fk.Fields.ForeignKey);
                    if (item.PrimaryKey.PrimaryKey && (item.PrimaryKey.Name == fk.Fields.ForeignKey))
                    {
                        fk.Fields.IsChield = true;
                    }
                    item.Fks.Add(x.Name, fk);
                }
                tables.Add(model, item);
            }
            return tables;
        }

        #endregion


    }
}
