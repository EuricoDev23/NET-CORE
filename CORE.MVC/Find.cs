using LinqToDB.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CORE.MVC
{
    public class Data<T>
    {
        private DataMapper mapper = null;
        private int? top;
        public Data(DataMapper mapper)
        {
            this.mapper = mapper;
        }
       
        private Dictionary<string, PropertyInfo> selectField = new Dictionary<string, PropertyInfo>();
        private string getTop()
        {
            return top.GetValueOrDefault() > 0 ? $"top({top.GetValueOrDefault()})" : "";
        }
        public List<T> All()
        {
            var tb = typeof(T).TableModel();
            var list = ExecuteReader(typeof(T), $"SELECT {getTop()} * FROM {tb.ShortName}");
            return list.Cast<T>().ToList();
        }
        public List<T> All(Expression<Func<T, bool>> where)
        {
            return null;
        }
        public List<T> All(string where, object param = null)
        {
            return null;
        }
        public T First()
        {
            return default(T);
        }
        public T First(Expression<Func<T, bool>> where)
        {
            return default(T);
        }
        public T First(string where, object param = null)
        {
            return default(T);
        }
        public bool Exists(Expression<Func<T, bool>> where)
        {
            return true;
        }
        public Data<T> Select(Expression<Func<T, object>> select)
        {
            var exp = select.Body.Type.Properties();

            return this;
        }
        public Data<T> Top(int limit)
        {
            this.top = limit;
            return this;
        }
        private List<object> ExecuteReader(Type type, string sql, params DataParameter[] parameters)
        {
            List<dynamic> rs = new List<dynamic>(parameters.Length > 0 ? mapper.Data.Query<dynamic>(sql: sql, parameters) : mapper.Data.Query<dynamic>(sql: sql));
            List<object> list = new List<object>(rs.Count);

            for (int i = 0; i < rs?.Count(); i++)
            {
                object model = Model(type, (IDictionary<String, Object>)rs.ElementAt(i), null);
                list.Insert(i, model);
            }

            return list;
        }
        private Task<List<dynamic>> ExecuteReaderAsync(Type type, string where, params DataParameter[] parameters)
        {
            //dynamic model = Activator.CreateInstance(type);
            var rs = parameters.Length > 0 ? mapper.Data.QueryToListAsync<dynamic>(sql: where, parameters) : mapper.Data.QueryToListAsync<dynamic>(sql: where);
            return rs;
        }
        List<SearchKey> Conflito = new List<SearchKey>();

        private object Model(Type type, IDictionary<String, Object> row, Type? select)
        {
            object model = type.CreateInstanteClass();

            if (row != null)
            {
                var tb = type.TableModel();
                //Carregar campos default's
                foreach (var col in tb.Columns)
                {
                    try
                    {
                        object val = row[col.Name];
                        //Type t = Nullable.GetUnderlyingType(item.Property.PropertyType) ?? item.Property.PropertyType;
                        if (val != null && !val.Equals(DBNull.Value) && col.Property.CanWrite)
                        {
                            model.SetValueProperty(col.Property.Name, val);
                        }
                    }
                    catch (Exception ex)
                    {
                        //throw;
                    }
                }
                //Carregar Row
                model.SetValueProperty("RowNumber", model.GetValueProperty(tb.PrimaryKey.Name));

                bool FK = true;

                if (FK && tb.Fks.Count > 0)
                {
                    if (!Conflito.Any(m => m.Type.GetHashCode() == model.GetHashCode()))
                    {
                        Conflito.Add(new SearchKey
                        {
                            Model = model,
                            Type = type,
                            Value = model.GetValueProperty(tb.PrimaryKey.Name)
                        });
                    }
                    foreach (var item in tb.Fks)
                    {
                        object FieldVal = model.GetValueProperty(item.Value.Fields.ForeignKey);

                        if (FieldVal != null && FieldVal != (object)"")
                        {
                            var property = model.GetType().GetProperty(item.Key);

                            //SearchKey ModelMemory = Conflito.FirstOrDefault(m => m.Value.Equals(FieldVal) && m.Type.Name == item.Value.TypeModel.Name);
                            SearchKey ModelMemory = Conflito.FirstOrDefault(m => m.Value.Equals(FieldVal) && m.Type.FullName == property.PropertyType.FullName);

                            var PropertyType = select.PropertiesGenerics().FirstOrDefault(m => m.Name == item.Key)?.PropertyType;
                            string[] cc = GetCampos(PropertyType);

                            if (ModelMemory != null)
                            {
                                model.SetValueProperty(item.Key, ModelMemory.Model);
                            }
                            else
                            {
                                if (select == null || (select != null && PropertyType != null))
                                {
                                    var tbFK = item.Value.TypeModel.TableModel();
                                    List<DataParameter> parms = new List<DataParameter>();
                                    parms.Add(new DataParameter("val", FieldVal));
                                    List<string> wl = new List<string>();
                                    wl.Add(" 1=1 ");
                                    if (item.Value.Fields.DefaultValues != null)
                                    {
                                        foreach (var itemParam in item.Value.Fields.DefaultValues)
                                        {
                                            parms.Add(new DataParameter(itemParam.Key, itemParam.Value));
                                            wl.Add(string.Format(" {0} = @{1} ", itemParam.Key, itemParam.Key));
                                        }
                                    }
                                    string w = string.Join(" AND ", wl);
                                    List<object> dtFK = new List<object>();
                                    if (!string.IsNullOrWhiteSpace(item.Value.Fields.ParentKey))
                                    {
                                        dtFK = ExecuteReader(item.Value.TypeModel, $"SELECT * FROM {item.Value.TypeModel.TableModel().ShortName} WHERE {tbFK.GetNameByPropertyMame(item.Value.Fields.ParentKey)} = @val and " + w, parms.ToArray());
                                    }
                                    else
                                    {
                                        dtFK = ExecuteReader(item.Value.TypeModel, $"SELECT * FROM {item.Value.TypeModel.TableModel().ShortName} WHERE {tbFK.PrimaryKey.Name} = @val and " + w, parms.ToArray());
                                    }

                                    if (dtFK != null && dtFK.Count > 0)
                                    {
                                        if (property.IsCollections())
                                        {
                                            MethodInfo method = this.GetType().GetMethod("CloneListAs", BindingFlags.NonPublic | BindingFlags.Instance);
                                            if (method != null)
                                            {
                                                MethodInfo genericMethod = method.MakeGenericMethod(Type.GetType(item.Value.TypeModel.AssemblyQualifiedName));

                                                var newList = genericMethod.Invoke(this, new[] { dtFK });
                                                model.SetValueProperty(item.Key, newList);
                                            }
                                        }
                                        else
                                        {
                                            //model.SetValueProperty(item.Key, item.Value.TypeModel.CreateInstanteClass());
                                            model.SetValueProperty(item.Key, dtFK[0]);
                                        }
                                    }
                                    else
                                    {
                                        model.SetValueProperty(item.Key, Activator.CreateInstance(property.PropertyType));
                                    }
                                }
                                else
                                {
                                    model.SetValueProperty(item.Key, Activator.CreateInstance(property.PropertyType));
                                }
                            }
                        }
                    }
                }
                model.CallMedthod("TriggerSearch");
                ((Entity)model).PreserveState();

            }
            return model;
        }
        private List<T> CloneListAs<T>(IList<object> source)
        {
            return source.Cast<T>().ToList();
        }
        private string[] GetCampos(Type type)
        {
            if (type != null)
            {
                var prop = type.IsCollections() ? type.GetTypeCollection().Properties() : type.Properties();
                if (prop != null)
                {
                    List<string> list = new List<string>(prop.Select(i => i.Name).ToArray());
                    if (!list.Any(i => i == "Status"))
                    {
                        list.Add("Status");
                    }

                    return list.ToArray();
                }
            }
            return null;
        }
        internal class SearchKey
        {
            public object Value { get; set; }
            public object Model { get; set; }
            public Type Type { get; set; }
        }
    }

    public static class Search
    {
        public static Data<T> Find <T>()
        {
          return new Data<T>(typeof(T).GetDataMapper());
        }
    }
}
