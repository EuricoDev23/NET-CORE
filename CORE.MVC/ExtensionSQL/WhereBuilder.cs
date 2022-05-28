using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace CORE.MVC.ExtensionSQL
{
    public class WhereBuilder
    {
        #region Classes
        internal class FromSQL
        {
            public string Alias { get; set; }
            public Type Type { get; set; }
            public DatabaseModel.Table Table { get; set; }
            public StringBuilder From { get; set; } = new StringBuilder();
            public StringBuilder Where { get; set; } = new StringBuilder();
        }
        public class SQL {
            public string Where { get; private set; } = string.Empty;
            public List<LinqToDB.Data.DataParameter> Parameters { get; private set; } = new List<LinqToDB.Data.DataParameter>();
            public SQL(string Where)
            {
                this.Where = Where;
            }
            public SQL(string Where,ICollection<LinqToDB.Data.DataParameter> parameters)
            {
                this.Where = Where;
                if(parameters.Count > 0)
                this.Parameters.AddRange(parameters);
            }
        }

        #endregion
        private Dictionary<string, FromSQL> Builder = new Dictionary<string, FromSQL>();
        private Dictionary<string, object> Params = new Dictionary<string, object>();
        private readonly IDictionary<ExpressionType, string> nodeTypeMappings = new Dictionary<ExpressionType, string>
        {
            {ExpressionType.Add, "+"},
            {ExpressionType.And, "AND"},
            {ExpressionType.AndAlso, "AND"},
            {ExpressionType.Divide, "/"},
            {ExpressionType.Equal, "="},
            {ExpressionType.ExclusiveOr, "^"},
            {ExpressionType.GreaterThan, ">"},
            {ExpressionType.GreaterThanOrEqual, ">="},
            {ExpressionType.LessThan, "<"},
            {ExpressionType.LessThanOrEqual, "<="},
            {ExpressionType.Modulo, "%"},
            {ExpressionType.Multiply, "*"},
            {ExpressionType.Negate, "-"},
            {ExpressionType.Not, "NOT"},
            {ExpressionType.NotEqual, "<>"},
            {ExpressionType.Or, "OR"},
            {ExpressionType.OrElse, "OR"},
            {ExpressionType.Subtract, "-"}
        };
        public SQL ToSql<T>(Expression<Func<T, bool>> expression)
        {
            var i = 1;
            var result = Recurse<T>(ref i, expression.Body, isUnary: true);
            StringBuilder sql = new StringBuilder();
            foreach (var item in Builder)
            {
                if (item.Value.From.Length > 1)
                {
                    sql.AppendLine(item.Value.From.ToString());
                    sql.Replace("{where}",item.Value.Where.ToString());
                }
                else
                {
                    sql.AppendLine(item.Value.Where.ToString());
                }
                sql.AppendLine();
            }
            var p = new List<LinqToDB.Data.DataParameter>(Params.Count);

            foreach (var item in Params)
            {
                p.Add(new LinqToDB.Data.DataParameter(item.Key, item.Value));
            }
            return new SQL(sql.ToString(),p);
        }

        private WherePart Recurse<T>(ref int i, Expression expression, bool isUnary = false,
            string prefix = null, string postfix = null, bool left = true)
        {
          sql = MemberExpressionBuilder<T>(expression);

            switch (expression)
            {
                case UnaryExpression unary: return UnaryExpressionExtract<T>(ref i, unary);
                case BinaryExpression binary: return BinaryExpressionExtract<T>(ref i, binary);
                case ConstantExpression constant: return ConstantExpressionExtract(ref i, constant, isUnary, prefix, postfix, left);
                case MemberExpression member: return MemberExpressionExtract<T>(ref i, member, isUnary, prefix, postfix, left);
                case MethodCallExpression method: return MethodCallExpressionExtract<T>(ref i, method);
                case InvocationExpression invocation: return InvocationExpressionExtract<T>(ref i, invocation, left);
                default: throw new Exception("Unsupported expression: " + expression.GetType().Name);
            }
        }

        private WherePart InvocationExpressionExtract<T>(ref int i, InvocationExpression expression, bool left)
        {
           var s= Recurse<T>(ref i, ((Expression<Func<T, bool>>) expression.Expression).Body, left: left);
            //sql.Where.Append(AddSQL(s.Sql));
            return s;
        }

        private WherePart MethodCallExpressionExtract<T>(ref int i, MethodCallExpression expression)
        {
            sql = MemberExpressionBuilder<T>(expression.Object);

            // LIKE queries:
            if (expression.Method == typeof(string).GetMethod("Contains", new[] {typeof(string)}))
            {
                sql.Where.Append("(");
                var w1 = Recurse<T>(ref i, expression.Object);
                sql.Where.Append(AddSQL("LIKE"));
                var w2 = Recurse<T>(ref i, expression.Arguments[0], prefix: "%", postfix: "%");
                sql.Where.Append(")");
                var s = WherePart.Concat(w1, "LIKE",
                    w2);
                return s;
            }

            if (expression.Method == typeof(string).GetMethod("StartsWith", new[] {typeof(string)}))
            {
                sql.Where.Append("(");

                var w1 = Recurse<T>(ref i, expression.Object);
                sql.Where.Append(AddSQL("LIKE"));
                var w2 = Recurse<T>(ref i, expression.Arguments[0], postfix: "%");
                sql.Where.Append(")");

                var s = WherePart.Concat(w1, "LIKE",
                    w2);
                return s;
            }

            if (expression.Method == typeof(string).GetMethod("EndsWith", new[] {typeof(string)}))
            {
                sql.Where.Append("(");

                var w1 = Recurse<T>(ref i, expression.Object);
                sql.Where.Append(AddSQL("LIKE"));
                var w2 = Recurse<T>(ref i, expression.Arguments[0], prefix: "%");
                sql.Where.Append(")");
                var s = WherePart.Concat(w1, "LIKE",
                    w2); 
                return s;
            }

            if (expression.Method == typeof(string).GetMethod("Equals", new[] {typeof(string)}))
            {
                sql.Where.Append("(");

                var w1 = Recurse<T>(ref i, expression.Object);
                sql.Where.Append(AddSQL("="));
                var w2 = Recurse<T>(ref i, expression.Arguments[0], left: false);
                sql.Where.Append(")");
                var s = WherePart.Concat(w1, "=",
                    w2);
                return s;
            }

            // IN queries:
            if (expression.Method.Name == "Contains")
            {
                Expression collection;
                Expression property;
                if (expression.Method.IsDefined(typeof(ExtensionAttribute)) && expression.Arguments.Count == 2)
                {
                    collection = expression.Arguments[0];
                    property = expression.Arguments[1];
                }
                else if (!expression.Method.IsDefined(typeof(ExtensionAttribute)) && expression.Arguments.Count == 1)
                {
                    collection = expression.Object;
                    property = expression.Arguments[0];
                }
                else
                {
                    throw new Exception("Unsupported method call: " + expression.Method.Name);
                }

                var values = (IEnumerable) GetValue(collection);
                var w1 = Recurse<T>(ref i, property);
                sql.Where.Append(AddSQL("IN"));
                var w2 = WherePart.IsCollection(ref i, values);
                var s= WherePart.Concat(w1, "IN", w2);
                //sql.Where.Append(AddSQL(s.Sql));
                return s;
            }
            
            if(expression.Type == typeof(DateTime) || expression.Method.ReturnType == typeof(DateTime))
            {
                var value = GetValue(expression);
                var w2 = WherePart.IsParameter(i++, value);
                var p = AddParam(value);
                sql.Where.Append(("CONVERT(DATETIME,"+p+")"));
                return w2;
            }
                throw new Exception("Unsupported method call: " + expression.Method.Name);
        }
        FromSQL sql = null;
        private string AddSQL(string sql)
        {
            return sql + " ";
        }
        private string AddParam(object val)
        {
            KeyValuePair<string, object> item = Params.FirstOrDefault(a => a.Value == val);
            if(string.IsNullOrWhiteSpace(item.Key))
            {
              item = new KeyValuePair<string, object>($"@p{Params.Count}",val);
                Params.Add(item.Key, item.Value);
            }
            return item.Key;
        }
        private WherePart MemberExpressionExtract<T>(ref int i, MemberExpression expression, bool isUnary,
            string prefix, string postfix, bool left)
        {
            if (isUnary && expression.Type == typeof(bool))
            {
                var w1 = Recurse<T>(ref i, expression);
                sql.Where.Append(AddSQL("="));
                sql.Where.Append(AddSQL("1"));
                var s= WherePart.Concat(w1, "=", WherePart.IsSql("1"));
                return s;
            }

            if (expression.Member is PropertyInfo property)
            {
                sql = MemberExpressionBuilder<T>(expression);
                //if (left==false)

                if (property.PropertyType == typeof(DateTime)|| property.PropertyType == typeof(DateTime?))
                {
                    var tb = DatabaseModel.Instance.GetTable(expression.Expression.Type);
                    var colName = tb.Columns.FirstOrDefault(a=>a.Property.Name == property.Name).Name;
                    var tableName = tb.Name;
                    //sql.Where.Append(AddSQL($"CONVERT(VARCHAR,{tableName}.[{colName}],112)"));
                    sql.Where.Append(AddSQL($"{tableName}.[{colName}]"));
                    return WherePart.IsSql($"{tableName}.[{colName}]");
                }
                else
                {
                    var colName = GetColumModel(property);
                    var tableName = GetTableModel(property.DeclaringType.IsAbstract
                        ? ((ParameterExpression)expression.Expression).Type
                        : property.DeclaringType);
                    sql.Where.Append(AddSQL($"{tableName}.[{colName}]"));
                    return WherePart.IsSql($"{tableName}.[{colName}]"); //MemberExpressionSubQuery<T>(expression, $"{tableName}.[{colName}]");
                }

            }

            if (expression.Member is FieldInfo /*|| left == false*/)
            {
                var value = GetValue(expression);
                if (value is string textValue)
                {
                    value = prefix + textValue + postfix;
                }
                var p = AddParam(value);
                sql.Where.Append(AddSQL(p));
                return WherePart.IsParameter(i++, value);
            }
            

            throw new Exception($"Expression does not refer to a property or field: {expression}");
        }
        private MemberExpression GetMember(Expression ex)
        {
            if (ex is BinaryExpression x)
            {   
                if (x.Left is MemberExpression xl)
                {
                    return xl;
                }
                var m = GetMember(x.Left);
                if (m != null)
                    return m;
                if (x.Right is MemberExpression xr)
                {
                    return xr;
                }
                var mr = GetMember(x.Right);
                if (mr != null)
                    return mr;
            }
            return null;
        }
        private FromSQL MemberExpressionBuilder<T>(Expression expression)
        {
            MemberExpression m=GetMember(expression);
            if(m==null && expression is MemberExpression ma)
            {
                m = ma;
            }
            if (m == null)
            {
                return sql;
            }
            PropertyInfo property = (PropertyInfo)m.Member;
            var ex = m.ToString().Replace("."+property.Name,"");
            FromSQL from = Builder.FirstOrDefault(i => i.Key == ex).Value;
            if(from == null)
            {
                from = new FromSQL
                {
                    Alias = "t" + Builder.Count,
                    Type = property.ReflectedType,
                    Table = DatabaseModel.Instance.GetTable(property.ReflectedType)
                };
                if (ex.IndexOf(".") > -1)
                {
                    var typeFK = ((MemberExpression)m.Expression).Member.ReflectedType;
                    var tbFK = DatabaseModel.Instance.GetTable(typeFK);
                    from.From.Append("EXISTS (SELECT 1 FROM ");
                    from.From.Append($"{from.Table.Name} where {from.Table.Name}.{from.Table.PrimaryKey.Name} = {tbFK.Name}.{tbFK.GetColumnFK(from.Type.Name).Name} {" AND {where}"})");
                }
                Builder.Add(ex,from);                 
            }

            return from;
        }

        public string GetColumModel(PropertyInfo pi)
        {   
            return pi.GetColumModel().Name;
        }
        public string GetTableModel(Type pi)
        {
            return DatabaseModel.Instance.GetTable(pi).Name;
        }
        private WherePart ConstantExpressionExtract(ref int i, ConstantExpression expression, bool isUnary,
            string prefix, string postfix, bool left)
        {
            var value = expression.Value;

            switch (value)
            {
                case null:
                    var sx= WherePart.IsSql("NULL");
                    sql.Where.Append(AddSQL(sx.Sql));
                    return sx;
                case int _:
                    var ss = WherePart.IsSql(value.ToString());
                    sql.Where.Append(AddSQL(ss.Sql));
                    return ss;
                case string text:
                    value = prefix + text + postfix;
                    //sql.Where.Append((string)value);
                    break;
            }
            if (value is DateTime)
            {
                var sb = WherePart.IsParameter(i++, value);
                var p = AddParam(value);
                //sql.Where.Append(AddSQL(p));
                sql.Where.Append(AddSQL("CONVERT(datetime,"+p+")"));
                return sb;
            }
            if (!(value is bool) || isUnary)
            {
                var sb = WherePart.IsParameter(i++, value); 
                var p = AddParam(value);
                sql.Where.Append(AddSQL(p));
                return sb;
            }
            if (value is bool)
            {
                var result = ((bool)value) ? "1" : "0";
                //if (left)
                //    result = result.Equals("1") ? "1=1" : "0=0";
                var s = WherePart.IsSql(result);
                sql.Where.Append(AddSQL(result));
                return s;
            }
            return WherePart.IsSql("");
        }

        private WherePart BinaryExpressionExtract<T>(ref int i, BinaryExpression expression)
        {
            sql.Where.Append("(");
            var s= WherePart.Concat(Recurse<T>(ref i, expression.Left), NodeTypeToString(expression.NodeType),
                Recurse<T>(ref i, expression.Right, left: false));
            sql.Where.Append(")");
            return s;
        }
       
        private WherePart UnaryExpressionExtract<T>(ref int i, UnaryExpression expression)
        {
            //sql.Where.Append("(");
            var sinal = NodeTypeToString(expression.NodeType);
            var w1 = Recurse<T>(ref i, expression.Operand, true);
            var s = WherePart.Concat(sinal, w1);
            //sql.Where.Append(")");
            //sql.Where.Append(AddSQL(s.Sql));
            return s;
        }

        private object GetValue(Expression member)
        {
            var objectMember = Expression.Convert(member, typeof(object));
            var getterLambda = Expression.Lambda<Func<object>>(objectMember);
            var getter = getterLambda.Compile();
            return getter();
        }
        
        private string NodeTypeToString(ExpressionType nodeType)
        {   var s= nodeTypeMappings.TryGetValue(nodeType, out var value)
                ? value
                : string.Empty;
            sql.Where.Append($" {s} ");
            return s;
        }

        //public static List<T> AsList<T>(this IEnumerable<T> source) =>
        //    (source == null || source is List<T>) ? (List<T>) source : source.ToList();
    }
}