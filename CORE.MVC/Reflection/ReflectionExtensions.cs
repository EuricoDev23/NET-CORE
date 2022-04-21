using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Linq.Expressions;
using System.Data;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using static System.Net.Mime.MediaTypeNames;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Drawing;
using LinqToDB.Mapping;
using static CORE.MVC.DatabaseModel;

namespace CORE.MVC
{
    public static class ReflectionExtension
    {
        public static List<Type> AssemblyGetTypes(Type filter){
            List<Type> list = new List<Type>();
            string codeBase = Assembly.GetExecutingAssembly().Location;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            var dlls = new DirectoryInfo(Path.GetDirectoryName(path)).GetFiles("*.dll");
            foreach (var dll in dlls)
            {
                var assembly = Assembly.LoadFile(dll.FullName);
               var tmp_list = assembly.ExportedTypes.Distinct().Where(x => filter.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract && (x.FullName != filter.FullName)).ToList();
               if(tmp_list!=null && tmp_list.Count > 0){
                    list.AddRange(tmp_list);
               }
            }
            return list.Distinct().ToList();
        }
        internal static T AttributeValue<T>(this Type reflec)
        {
            try
            {
                return (T)reflec.GetCustomAttributes(typeof(T), false).FirstOrDefault();
            }
            catch (Exception)
            {
                return default(T);
            }
        }
        internal static string GetColumn(this PropertyInfo prop)
        {
           return DatabaseModel.Instance.Tables[prop.ReflectedType].GetNameByPropertyMame(prop.Name);
            
        }
        internal static T AttributeValue<T>(this PropertyInfo prop)
        {
            try
            {
                return (T)prop.GetCustomAttributes(typeof(T), false).FirstOrDefault();
            }
            catch (Exception)
            {
                return default(T);
            }
        }

        internal static DatabaseModel.Column GetColumModel(this PropertyInfo prop)
        {
            return DatabaseModel.Instance.Tables[prop.ReflectedType].Columns.FirstOrDefault(i=>i.Property.Name==prop.Name);
        }

        internal static Type GetTypeCollection(this Type type)
        {
            try
            {
                var prop = Activator.CreateInstance(type).GetType();
                return prop.GetMethod("GetEnumerator").ReturnType.GetProperty("Current").PropertyType;

            }
            catch (Exception)
            {
                return type;
            }
        }
        internal static Type GetTypeCollection(this PropertyInfo prop)
        {
            try
            {
                //var x = prop.PropertyType.GetMethod("GetEnumerator").ReturnType.GetProperty("Current");
                return prop.PropertyType.GetMethod("GetEnumerator").ReturnType.GetProperty("Current").PropertyType;
                
            }
            catch (Exception)
            {
                return prop.PropertyType;
            }
        }
        public static void SetValue(object obj, string name, object val)
        {
            obj.SetValueProperty(name, val);
        }
        public static object GetValue(object obj, string name)
        {
            return obj.GetValueProperty(name);
        }
        internal static void SetValueProperty(this object obj,string name,object val)
        {            
            try
            {
                PropertyInfo prop = obj.GetType().GetProperty(name,BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public);

                if (Nullable.GetUnderlyingType(prop.PropertyType) != null || val.GetType() == prop.PropertyType) {
                    prop.SetValue(obj, val, null);
                }
                else
                {
                    prop.SetValue(obj, Convert.ChangeType(val,prop.PropertyType),null);
                }
            }
            catch (Exception ex)
            {
                //Set Value for Enum type
                try
                {
                    PropertyInfo prop = obj.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    prop.SetValue(obj, Enum.ToObject(prop.PropertyType, val), null);
                    return;
                }
                catch
                {
                    //   throw;
                }
            }

        }
        internal static object GetValueProperty(this object obj, string name)
        {
           return obj.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).GetValue(obj);
        }
        
        internal static Type GetParentObject(this object obj)
        {
           return obj.GetType().ReflectedType;
        }
        public static List<PropertyInfo> Properties<T>(this Expression<Func<T, object>> selector)
        {
            try
            {
                var reflec = selector.Body.Type;
                return reflec.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(i =>!i.PropertyType.IsGenericTypeDefinition && !i.PropertyType.FullName.Contains("Collections") && i.PropertyType.FullName.Contains("System") && !i.PropertyType.IsConstructedGenericType).ToList();
            }
            catch (Exception)
            {
                return new List<PropertyInfo>(0);
            }
        }
        public static List<PropertyInfo> Properties(this Type reflec)
        {
            try
            {
                //var list = reflec.GetProperties(BindingFlags.Instance|BindingFlags.Public);
                return reflec.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(i =>(!i.PropertyType.IsGenericTypeDefinition && !i.PropertyType.FullName.Contains("Collections") && i.PropertyType.FullName.Contains("System"))||(i.PropertyType.IsEnum)).ToList();
            }
            catch (Exception)
            {
                return new List<PropertyInfo>(0);
            }
        }
        public static List<PropertyInfo> PropertiesClass(this Type reflec)
        {
            try
            {
                return reflec.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(i =>i.AttributeValue<LinqToDB.Mapping.AssociationAttribute>()!=null && i.AttributeValue<NotMappedAttribute>()==null).ToList();
            }
            catch (Exception)
            {
                return new List<PropertyInfo>(0);
            }
        }
        public static List<PropertyInfo> PropertiesGenerics(this Type reflec)
        {
            try
            {
                //return reflec.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(i => i.AttributeValue<FkAttribute>() != null && i.AttributeValue<NotMappedAttribute>() == null).ToList();

                return reflec.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(i => (i.AttributeValue<LinqToDB.Mapping.AssociationAttribute>() != null && i.AttributeValue<NotMappedAttribute>() == null) || (i.PropertyType.BaseType==typeof(Entity)) || (i.PropertyType.IsClass && i.PropertyType.IsGenericType)).ToList();
            }
            catch (Exception)
            {
                return new List<PropertyInfo>(0);
            }
        }
        public static List<object[]> PropertiesGenerics<T>(this Expression<Func<T, object>> selector)
        {
            try
            {
                var fks = selector.Body.Type.PropertiesGenerics();
                var list = new List<object[]>();

                foreach (var item in fks)
                {
                    list.Add(new object[]{
                    item.Name,
                    typeof(T).GetProperty(item.Name).PropertyType,
                    item.PropertyType.Properties().Select(i=>i.Name).ToArray()
                    });
                }

                return list;
            }
            catch (Exception)
            {
                return default(List<object[]>);
            }
        }

        public static bool IsCollections(this PropertyInfo prop)
        {
            bool rs = false;
            try
            {
                dynamic m = Activator.CreateInstance(prop.PropertyType) as dynamic;
                if (m.Count >= 0)
                {
                    rs = true;
                }
            }
            catch { }
            return rs;
        }
        public static bool IsCollections(this Type type)
        {
            bool rs = false;
            try
            {
                dynamic m = Activator.CreateInstance(type) as dynamic;
                if (m.Count >= 0)
                {
                    rs = true;
                }
            }
            catch { }
            return rs;
        }
        public static bool IsNumericType(this Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }
        public static bool IsNull(this Type reflec){
            return reflec.Name.ToLower().Contains("string") || reflec.Name.ToLower().Contains("byte") || reflec.FullName.Contains("Nullable");
        }
        public static LinqToDB.DataType DbType(this Type reflec)
        {
            if (reflec.FullName.Contains("Nullable")){
                reflec = Nullable.GetUnderlyingType(reflec) ?? reflec; //reflec.GenericTypeArguments[0];
            }
            try
            {   
                if (reflec.GetEnumNames().Length > 0)
                {
                    return LinqToDB.DataType.Int32;
                }
            }
            catch (Exception)
            {
                
            }
            if(reflec == typeof(Int32))
            {
                return LinqToDB.DataType.Int32;
            }
            else if (reflec == typeof(Int64)|| reflec == typeof(long))
            {
                return LinqToDB.DataType.Int64;
            }
            else if (reflec == typeof(decimal) || reflec == typeof(Decimal) || reflec == typeof(double) || reflec == typeof(Double))
            {
                return LinqToDB.DataType.Decimal;
            }
            else if (reflec == typeof(float))
            {
                return LinqToDB.DataType.DecFloat;
            }
            else if (reflec == typeof(string) || reflec == typeof(String))
            {
                return LinqToDB.DataType.VarChar;
            }
            else if (reflec == typeof(char) || reflec == typeof(Char) || reflec == typeof(char[]) || reflec == typeof(Char[]))
            {
                return LinqToDB.DataType.Char;
            }
            else if (reflec == typeof(DateTime))
            {
                return LinqToDB.DataType.DateTime;
            }
            else if (reflec == typeof(byte[]) || reflec == typeof(byte?[]))
            {
                return LinqToDB.DataType.VarBinary;
            }
            else if (reflec == typeof(bool))
            {
                return LinqToDB.DataType.Boolean;
            }
            return LinqToDB.DataType.Variant;
        }
        public static string DbSqlName(this LinqToDB.DataType reflec)
        {
            if (reflec == LinqToDB.DataType.Variant)
            {
                return "sql_variant";
            }
            if (reflec == LinqToDB.DataType.Int64 || reflec == LinqToDB.DataType.Int128 || reflec == LinqToDB.DataType.Long )
            {
                return "BIGINT";
            }
            if (reflec.ToString().ToLower().Contains("int"))
            {
                return "int";
            }
            if (reflec == LinqToDB.DataType.Boolean)
            {
                return "bit";
            }
            if (reflec == LinqToDB.DataType.Decimal)
            {
                return "decimal(18,2)";
            }
            if (reflec == LinqToDB.DataType.VarChar)
            {
                return "varchar(50)";
            }
            return reflec.ToString().ToUpper();
        }
        public static bool Equal(this Type reflec, object obj1, object obj2)
        {
            var pros = reflec.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                foreach (var item in pros)
                {
                    if(obj1.GetValueProperty(item.Name)!=(obj2.GetValueProperty(item.Name))){
                        return false;
                    }
                }
                return true;            
        }

        #region DataTable
        public static DataTable DataTable<T>(this IEnumerable<T> list)
        {            
            return ReflectionExtension.DataTable<T>(list,typeof(T));
        }
        public static DataTable DataTable<T>(IEnumerable<T> list,Type type)
        {
            DataTable table = CreateTable(type);
            Type entityType = typeof(T);
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(type);
            foreach (T item in list)
            {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                {
                    try
                    {
                        row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                    }
                    catch (Exception)
                    {
                    }
                }
                table.Rows.Add(row);
            }
            return table;
        }
        internal static DataTable CreateTable(Type type)
        {
            Type entityType = type; //typeof(T);
            DataTable table = new DataTable(entityType.FullName);
            
            var properties = entityType.GetProperties(BindingFlags.Public|BindingFlags.Instance|BindingFlags.GetProperty).Where(a=>a.AttributeValue<DisplayAttribute>()!=null);
            //PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(entityType);
            //foreach (PropertyDescriptor prop in properties)
            if (properties != null)
            {
                foreach (var prop in properties.OrderBy(a=>a.AttributeValue<DisplayAttribute>().Order).ToList())
                {
                    if (prop.Name != "Status")
                    {
                        var display = prop.AttributeValue<DisplayAttribute>();
                        var pk = prop.AttributeValue<PkAttribute>();
                        var col = table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
                        if(string.IsNullOrWhiteSpace(display.Name)==false){
                            col.Caption = display.Name;
                        }
                        if(pk!=null){
                            table.PrimaryKey = new DataColumn[] { col };
                        }
                        
                        //var col = table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
                        //var display = prop.AttributeValue<DisplayAttribute>();
                        //if (display != null)
                        //{ col.Caption = display.Name; }
                    }
                }
            }else{
                foreach (var prop in entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty))
                {
                    if (prop.Name != "Status")
                    {
                        var col = table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
                    }
                }
            }
            return table;
        }
        #endregion

        #region Custom Attributes

        internal static SourceAttribute GetSourceAttribute(this Type reflec)
        {
            var t = reflec.GetTableAttribute();
            var  tb = new SourceAttribute(t.Database,t.Name,string.IsNullOrWhiteSpace(t.Schema) ? "dbo":t.Schema);
            
            return tb;
        }

        internal static TableAttribute GetTableAttribute(this Type reflec)
        {
            TableAttribute tb = reflec.AttributeValue<TableAttribute>();
            if(tb == null){
                tb = new TableAttribute { Database=DatabaseConsts.DefaultBD,Name= reflec.Name };
            }
            if (string.IsNullOrWhiteSpace(tb.Database))
            {
                tb.Database = DatabaseConsts.DefaultBD;
            }
            if (string.IsNullOrWhiteSpace(tb.Name)){ tb.Name = reflec.Name; }

            return  tb;
        }
        internal static ColumnAttribute GetColumnAttribute(this PropertyInfo prop)
        {
            ColumnAttribute Column = prop.AttributeValue<ColumnAttribute>();
            if(Column==null){
                var db_type = prop.PropertyType.DbType();
                Column = new ColumnAttribute { Name = prop.Name };
            }
            return Column;
        }
        internal static PrimaryKeyAttribute GetPkAttribute(this PropertyInfo prop)
        {
            return prop.AttributeValue<PrimaryKeyAttribute>();
        }
        internal static IdentityModeAttribute GetIdentityAttribute(this PropertyInfo prop)
        {
            return prop.AttributeValue<IdentityModeAttribute>() ?? (prop.AttributeValue<IdentityAttribute>()!=null ? new IdentityModeAttribute():null);
        }
        internal static FkAttribute GetFkAttribute(this PropertyInfo prop)
        {
            //return prop.AttributeValue<FkAttribute>() ?? new FkAttribute("");
            var ass = prop.AttributeValue<LinqToDB.Mapping.AssociationAttribute>();
            if (ass != null)
            {
                return new FkAttribute()
                {
                    ForeignKey = ass.ThisKey,
                    ParentKey = ass.OtherKey
                };
            }
            return  new FkAttribute("");

        }

        internal static NotMappedAttribute GetNotMappedAttribute(this PropertyInfo prop)
        {
            return prop.AttributeValue<NotMappedAttribute>();
        }
        internal static void CallMedthod(this object model,string name)
        {
            model.GetType().GetMethod(name, BindingFlags.CreateInstance | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public |BindingFlags.DeclaredOnly)?.Invoke(model, null);           
        }
        #endregion

        #region Enums
        public static int Value(this Entity.State status){
            return (int)status;
        }
        #endregion

        #region DatabaseModel
       
        public static string GetTableShema(this Entity model)
        {
            var tb = DatabaseModel.Instance.Tables.FirstOrDefault(i => i.Key.FullName == model.GetType().FullName);
            return tb.Value.Name;
        }
        internal static List<KeyValuePair<string, DatabaseModel.Table.FK>> ForeignKeyParents(this DatabaseModel.Table table)
        {
            var list = new List<KeyValuePair<string, DatabaseModel.Table.FK>>();
            foreach (var item in table.Fks.Where(i=>i.Value.IsNotSave==false).ToList())
            {
                var tbFK = DatabaseModel.Instance.Tables[item.Value.TypeModel];
                if (tbFK.PrimaryKey.PrimaryKey && (string.IsNullOrWhiteSpace(item.Value.Fields.ParentKey) || item.Value.Fields.ParentKey == tbFK.PrimaryKey.Name))
                {
                    var tmp = item.Value.Clone<DatabaseModel.Table.FK>();
                    tmp.Fields.IsChield = false;
                    tmp.TypeModel = item.Value.TypeModel;
                    list.Add(new KeyValuePair<string, DatabaseModel.Table.FK>(item.Key, tmp));
                }
            }
            return list;
           //return table.Fks.Where(i =>i.Value.Fields.IsChield==false).ToList();
        }
        internal static List<KeyValuePair<string, DatabaseModel.Table.FK>> ForeignKeyChields(this DatabaseModel.Table table)
        {
            var list = new List<KeyValuePair<string, DatabaseModel.Table.FK>>();
            foreach (var item in table.Fks.Where(i => i.Value.IsNotSave == false).ToList())
            {
                var tbFK = DatabaseModel.Instance.Tables[item.Value.TypeModel];
                if (string.IsNullOrWhiteSpace(item.Value.Fields.ParentKey) == false && ((table.PrimaryKey.Name == item.Value.Fields.ParentKey)|| tbFK.PrimaryKey.Name != item.Value.Fields.ParentKey))
                {
                    var tmp = item.Value.Clone<DatabaseModel.Table.FK>();
                    tmp.Fields.IsChield = true;
                    tmp.TypeModel = item.Value.TypeModel;

                    list.Add(new KeyValuePair<string, DatabaseModel.Table.FK>(item.Key, tmp));
                }
            }
            return list;
            //return table.Fks.Where(i => i.Value.Fields.IsChield).ToList();
        }
        #endregion

        #region QueryTranslator
         
        //internal static QueryTranslator.ItemMember PrimaryAlias(this Dictionary<string, QueryTranslator.ItemMember> item)
        //{
        //    return item.FirstOrDefault(i =>i.Key.Split('.').Length==1).Value;
        //}
        #endregion

        #region Assembly Data

        public static string GetFileName(this Type type,string file)
        {
            Assembly assembly = Assembly.GetAssembly(type);
            try
            {
               return assembly.GetManifestResourceNames().FirstOrDefault(i=>i.Contains($".{file}."));
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }
        public static string FileReadLine(this Type type, string file)
        {
            Assembly assembly = Assembly.GetAssembly(type);
            using (StreamReader reader = new StreamReader(assembly.GetManifestResourceStream(file)))
            {
                return reader.ReadToEnd();
            }
        }
        //public static Bitmap GetBitmap(this Type type, string file)
        //{
        //    Assembly assembly = Assembly.GetAssembly(type);
        //    using (Stream reader = assembly.GetManifestResourceStream(file))
        //    {
        //        return new Bitmap(reader);
        //    }
        //}
        public static bool SetFileDiretory(this Type type, string file,string path)
        {
            Assembly assembly = Assembly.GetAssembly(type);
            using (MemoryStream reader = new MemoryStream())
            {
                reader.WriteTo(assembly.GetManifestResourceStream(file));
                System.IO.File.WriteAllBytes(path,reader.ToArray());
                return true;
            }
        }
        #endregion

        #region Clone

        internal static T Clone<T>(this object model){
            object tmp = Activator.CreateInstance(model.GetType());
            foreach (var item in model.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic|BindingFlags.Static))
            {
                tmp.SetValueProperty(item.Name, model.GetValueProperty(item.Name));
            }
            return (T)tmp;
        }
        /// <summary>
        /// Actualizar objecto
        /// </summary>
        /// <param name="model">Destino</param>
        /// <param name="source">Origem</param>
        internal static void Set(this object model,object source)
        {           
                foreach (var item in source.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
                {   
                    if(item.CanWrite)
                    model.SetValueProperty(item.Name, source.GetValueProperty(item.Name));
                }
        }

        #endregion

        #region Data Mapper
        /// <summary>
        /// Valida, caso tenha erro dispara uma Exception
        /// </summary>
        /// <param name="rs"></param>
        /// <returns>Resultado em bool</returns>
        public static bool Validate(this Result rs)
        {
            if(rs.Success==false){
                throw rs.Exception == null ? new Exception(rs.Message) : rs.Exception;
            }
            return rs.Success;
        }

        #endregion
    }

}
