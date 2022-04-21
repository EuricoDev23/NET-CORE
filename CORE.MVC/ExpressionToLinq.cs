using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace CORE.MVC
{
    internal static class ExpressionToLinq
    {
        internal static string Convert(Expression expr, out string[] entities)
        {
            entities = null;
            try
            {
                ConvertVisitor visitor = new ConvertVisitor();
                expr = visitor.Visit(expr);
                entities = visitor.GetEntities();
                PrintVisitor visitor2 = new PrintVisitor();
                visitor2.Visit(expr);
                return visitor2.GetText();
            }
            catch
            {
                return expr.ToString();
            }
        }

        private static bool IsAnonymousType(Type t)
        {
            object[] customAttributes = t.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false);
            return ((customAttributes != null) && (customAttributes.Length > 0));
        }

        private sealed class ConvertVisitor : CORE.MVC.ExpressionVisitor
        {
            private HashSet<string> entities = new HashSet<string>();

            internal ConvertVisitor()
            {
            }

            private static ExpressionToLinq.QueryExpression ContinueQuery(ExpressionToLinq.QueryExpression query, string identifier, Type type)
            {
                if ((query.Last.Type != ExpressionToLinq.QueryClauseType.Select) && (query.Last.Type != ExpressionToLinq.QueryClauseType.GroupBy))
                {
                    query.Append(new ExpressionToLinq.QuerySelectClause(Expression.Parameter(query.Type, query.Last.Identifier)));
                }
                ExpressionToLinq.QueryContinuationClause clause = new ExpressionToLinq.QueryContinuationClause(query, identifier);
                query = new ExpressionToLinq.QueryExpression(type);
                query.Append(clause);
                return query;
            }

            private ExpressionToLinq.QueryExpression CreateQueryExpression(Expression source, ParameterExpression identifier, MethodCallExpression m)
            {
                ExpressionToLinq.QueryExpression query = null;
                if (source.NodeType == (ExpressionType.ModuloAssign | ExpressionType.NewArrayInit))
                {
                    query = (ExpressionToLinq.QueryExpression)source;
                    MethodCallExpression call = m.Arguments[0] as MethodCallExpression;
                    if (((query.Last.Type == ExpressionToLinq.QueryClauseType.Select) && (call != null)) && (identifier.Name.StartsWith("<>") && ExpressionToLinq.IsAnonymousType(identifier.Type)))
                    {
                        string str;
                        Expression expression3;
                        if ((IsLetSelect(call, out str, out expression3) || IsSelectMany(call)) || IsJoin(call))
                        {
                            query.RemoveLast();
                            if (expression3 != null)
                            {
                                expression3 = this.Visit(expression3);
                                query.Append(new ExpressionToLinq.QueryLetClause(expression3, str));
                            }
                        }
                        return query;
                    }
                    if (query.Last.Identifier != identifier.Name)
                    {
                        query = ContinueQuery(query, identifier.Name, m.Type);
                    }
                    return query;
                }
                query = new ExpressionToLinq.QueryExpression(m.Type);
                query.Append(new ExpressionToLinq.QueryFromClause(source, identifier.Name));
                return query;
            }

            internal string[] GetEntities()
            {
                return this.entities.ToArray<string>();
            }

            private static LambdaExpression GetLambda(Expression expr)
            {
                while (expr.NodeType == ExpressionType.Quote)
                {
                    expr = ((UnaryExpression)expr).Operand;
                }
                return (expr as LambdaExpression);
            }

            private static LambdaExpression GetSimpleLambda(Expression expr)
            {
                LambdaExpression lambda = GetLambda(expr);
                if ((lambda != null) && (lambda.Parameters.Count == 1))
                {
                    return lambda;
                }
                return null;
            }

            private static bool IsGeneratedLambda(LambdaExpression lambda)
            {
                if (((lambda != null) && (lambda.Parameters.Count == 2)) && (lambda.Body.NodeType == ExpressionType.New))
                {
                    NewExpression body = (NewExpression)lambda.Body;
                    if (((body.Members != null) && (body.Members.Count == 2)) && (body.Arguments.Count == 2))
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            Expression expression2 = body.Arguments[i];
                            string name = lambda.Parameters[i].Name;
                            string str2 = body.Members[i].Name;
                            if (((expression2.NodeType != ExpressionType.Parameter) || (((ParameterExpression)expression2).Name != name)) || ((str2 != name) && (str2 != ("get_" + name))))
                            {
                                return false;
                            }
                        }
                        return true;
                    }
                }
                return false;
            }

            private static bool IsJoin(MethodCallExpression call)
            {
                return ((((call.Method.Name == "Join") || (call.Method.Name == "GroupJoin")) && (call.Arguments.Count == 5)) && IsGeneratedLambda(GetLambda(call.Arguments[4])));
            }

            private static bool IsLetSelect(MethodCallExpression call, out string identifier, out Expression expr)
            {
                identifier = null;
                expr = null;
                if (((call != null) && (call.Method.Name == "Select")) && (call.Arguments.Count == 2))
                {
                    LambdaExpression simpleLambda = GetSimpleLambda(call.Arguments[1]);
                    if (((simpleLambda != null) && (simpleLambda.Body.NodeType == ExpressionType.New)) && ExpressionToLinq.IsAnonymousType(simpleLambda.Body.Type))
                    {
                        NewExpression body = (NewExpression)simpleLambda.Body;
                        if (((body.Arguments.Count == 2) && (body.Members != null)) && (body.Members.Count == 2))
                        {
                            ParameterExpression expression3 = body.Arguments[0] as ParameterExpression;
                            identifier = body.Members[1].Name;
                            expr = body.Arguments[1];
                            if ((expression3 == null) || (expression3.Name != simpleLambda.Parameters[0].Name))
                            {
                                return false;
                            }
                            if (body.Members[0].Name != expression3.Name)
                            {
                                return (body.Members[0].Name == ("get_" + expression3.Name));
                            }
                            return true;
                        }
                    }
                }
                return false;
            }

            private static bool IsSelectMany(MethodCallExpression call)
            {
                return (((call.Method.Name == "SelectMany") && (call.Arguments.Count == 3)) && IsGeneratedLambda(GetLambda(call.Arguments[2])));
            }

            internal override Expression Visit(Expression exp)
            {
                if (exp == null)
                {
                    return null;
                }
                Expression inner = base.Visit(exp);
                if ((inner != null) && (inner.Type != exp.Type))
                {
                    return new ExpressionToLinq.TypeFixExpression(inner, exp.Type);
                }
                return inner;
            }

            internal override Expression VisitConstant(ConstantExpression c)
            {
                if (c.Type.IsGenericType && typeof(IQueryable).IsAssignableFrom(c.Type))
                {
                    Type type = c.Type.GetGenericArguments()[0];
                    this.entities.Add(type.Name);
                }
                return base.VisitConstant(c);
            }

            internal override Expression VisitMemberAccess(MemberExpression m)
            {
                if (m.Expression != null)
                {
                    Expression expression = this.Visit(m.Expression);
                    if (expression.NodeType == ExpressionType.Parameter)
                    {
                        if (ExpressionToLinq.IsAnonymousType(expression.Type) && ((ParameterExpression)expression).Name.StartsWith("<>"))
                        {
                            return Expression.Parameter(m.Type, m.Member.Name);
                        }
                    }
                    else if (((expression.NodeType == ExpressionType.Constant) && ExpressionToLinq.IsAnonymousType(expression.Type)) && expression.Type.Name.StartsWith("<>"))
                    {
                        return Expression.Parameter(m.Type, m.Member.Name);
                    }
                }
                return base.VisitMemberAccess(m);
            }

            internal override Expression VisitMethodCall(MethodCallExpression m)
            {
                if (m.Type.IsGenericType && ((m.Type.GetGenericTypeDefinition() == typeof(IQueryable<>)) || (m.Type.GetGenericTypeDefinition() == typeof(IOrderedQueryable<>))))
                {
                    switch (m.Method.Name)
                    {
                        case "Select":
                            return this.VisitQueryableSelect(m);

                        case "SelectMany":
                            return this.VisitQueryableSelectMany(m);

                        case "Where":
                            return this.VisitQueryableWhere(m);

                        case "GroupJoin":
                            return this.VisitQueryableJoin(m, true);

                        case "Join":
                            return this.VisitQueryableJoin(m, false);

                        case "GroupBy":
                            return this.VisitQueryableGroupBy(m);

                        case "OrderByDescending":
                        case "OrderBy":
                        case "ThenBy":
                        case "ThenByDescending":
                            return this.VisitQueryableOrderBy(m);
                    }
                }
                return base.VisitMethodCall(m);
            }

            private Expression VisitQueryableGroupBy(MethodCallExpression m)
            {
                LambdaExpression simpleLambda = GetSimpleLambda(m.Arguments[1]);
                if ((simpleLambda != null) && ((m.Arguments.Count == 2) || (m.Arguments.Count == 3)))
                {
                    ParameterExpression identifier = simpleLambda.Parameters[0];
                    Expression source = this.Visit(m.Arguments[0]);
                    Expression key = this.Visit(simpleLambda.Body);
                    Expression expression5 = null;
                    if (m.Arguments.Count == 3)
                    {
                        simpleLambda = GetSimpleLambda(m.Arguments[2]);
                        if ((simpleLambda != null) && (simpleLambda.Parameters[0].Name == identifier.Name))
                        {
                            expression5 = this.Visit(simpleLambda.Body);
                        }
                    }
                    else
                    {
                        expression5 = simpleLambda.Parameters[0];
                    }
                    if (expression5 != null)
                    {
                        ExpressionToLinq.QueryExpression expression6 = this.CreateQueryExpression(source, identifier, m);
                        expression6.Append(new ExpressionToLinq.QueryGroupByClause(expression5, key));
                        return expression6;
                    }
                }
                return base.VisitMethodCall(m);
            }

            private Expression VisitQueryableJoin(MethodCallExpression m, bool isGroupJoin)
            {
                if (m.Arguments.Count == 5)
                {
                    LambdaExpression simpleLambda = GetSimpleLambda(m.Arguments[2]);
                    if (simpleLambda != null)
                    {
                        LambdaExpression expression2 = GetSimpleLambda(m.Arguments[3]);
                        if (expression2 != null)
                        {
                            LambdaExpression lambda = GetLambda(m.Arguments[4]);
                            if ((lambda != null) && (lambda.Parameters.Count == 2))
                            {
                                string name = simpleLambda.Parameters[0].Name;
                                string identifier = expression2.Parameters[0].Name;
                                if ((lambda.Parameters[0].Name == name) && ((lambda.Parameters[1].Name == identifier) || isGroupJoin))
                                {
                                    Expression source = this.Visit(m.Arguments[0]);
                                    Expression expression5 = this.Visit(m.Arguments[1]);
                                    Expression outerKey = this.Visit(simpleLambda.Body);
                                    Expression innerKey = this.Visit(expression2.Body);
                                    Expression selection = this.Visit(lambda.Body);
                                    ExpressionToLinq.QueryExpression expression9 = this.CreateQueryExpression(source, simpleLambda.Parameters[0], m);
                                    expression9.Append(new ExpressionToLinq.QueryJoinClause(expression5, identifier, outerKey, innerKey, isGroupJoin ? lambda.Parameters[1].Name : ""));
                                    expression9.Append(new ExpressionToLinq.QuerySelectClause(selection));
                                    return expression9;
                                }
                            }
                        }
                    }
                }
                return base.VisitMethodCall(m);
            }

            private Expression VisitQueryableOrderBy(MethodCallExpression m)
            {
                LambdaExpression simpleLambda = GetSimpleLambda(m.Arguments[1]);
                if ((simpleLambda != null) && (m.Arguments.Count == 2))
                {
                    bool descending = m.Method.Name.EndsWith("Descending");
                    ParameterExpression identifier = simpleLambda.Parameters[0];
                    Expression source = this.Visit(m.Arguments[0]);
                    Expression expr = this.Visit(simpleLambda.Body);
                    if (m.Method.Name.StartsWith("OrderBy"))
                    {
                        ExpressionToLinq.QueryExpression expression5 = this.CreateQueryExpression(source, identifier, m);
                        expression5.Append(new ExpressionToLinq.QueryOrderByClause(identifier.Name).Append(new ExpressionToLinq.QueryOrdering(expr, descending)));
                        return expression5;
                    }
                    if (source.NodeType == (ExpressionType.ModuloAssign | ExpressionType.NewArrayInit))
                    {
                        ExpressionToLinq.QueryExpression expression6 = (ExpressionToLinq.QueryExpression)source;
                        if ((expression6.Last.Type == ExpressionToLinq.QueryClauseType.OrderBy) && (expression6.Last.Identifier == identifier.Name))
                        {
                            ((ExpressionToLinq.QueryOrderByClause)expression6.Last).Append(new ExpressionToLinq.QueryOrdering(expr, descending));
                            return expression6;
                        }
                    }
                }
                return base.VisitMethodCall(m);
            }

            private Expression VisitQueryableSelect(MethodCallExpression m)
            {
                LambdaExpression simpleLambda = GetSimpleLambda(m.Arguments[1]);
                if ((simpleLambda != null) && (m.Arguments.Count == 2))
                {
                    ParameterExpression identifier = simpleLambda.Parameters[0];
                    Expression source = this.Visit(m.Arguments[0]);
                    Expression selection = this.Visit(simpleLambda.Body);
                    ExpressionToLinq.QueryExpression expression5 = this.CreateQueryExpression(source, identifier, m);
                    expression5.Append(new ExpressionToLinq.QuerySelectClause(selection));
                    return expression5;
                }
                return base.VisitMethodCall(m);
            }

            private Expression VisitQueryableSelectMany(MethodCallExpression m)
            {
                LambdaExpression simpleLambda = GetSimpleLambda(m.Arguments[1]);
                if ((simpleLambda != null) && (m.Arguments.Count == 3))
                {
                    ParameterExpression identifier = simpleLambda.Parameters[0];
                    Expression source = this.Visit(m.Arguments[0]);
                    Expression expression4 = this.Visit(simpleLambda.Body);
                    simpleLambda = GetLambda(m.Arguments[2]);
                    if (((simpleLambda != null) && (simpleLambda.Parameters.Count == 2)) && (simpleLambda.Parameters[0].Name == identifier.Name))
                    {
                        Expression selection = this.Visit(simpleLambda.Body);
                        ExpressionToLinq.QueryExpression expression6 = this.CreateQueryExpression(source, identifier, m);
                        expression6.Append(new ExpressionToLinq.QueryFromClause(expression4, simpleLambda.Parameters[1].Name));
                        expression6.Append(new ExpressionToLinq.QuerySelectClause(selection));
                        return expression6;
                    }
                }
                return base.VisitMethodCall(m);
            }

            private Expression VisitQueryableWhere(MethodCallExpression m)
            {
                LambdaExpression simpleLambda = GetSimpleLambda(m.Arguments[1]);
                if ((simpleLambda != null) && (m.Arguments.Count == 2))
                {
                    ParameterExpression identifier = simpleLambda.Parameters[0];
                    Expression source = this.Visit(m.Arguments[0]);
                    Expression condition = this.Visit(simpleLambda.Body);
                    ExpressionToLinq.QueryExpression expression5 = this.CreateQueryExpression(source, identifier, m);
                    expression5.Append(new ExpressionToLinq.QueryWhereClause(condition, identifier.Name));
                    return expression5;
                }
                return base.VisitMethodCall(m);
            }
        }

        private static class ExpressionTypeEx
        {
            internal const ExpressionType QueryExpression = (ExpressionType.ModuloAssign | ExpressionType.NewArrayInit);
            internal const ExpressionType _TypeFix = ((ExpressionType)0x3e8);
        }

        private sealed class PrintVisitor : CORE.MVC.ExpressionVisitor
        {
            private StringBuilder _out = new StringBuilder();
            private int indent;

            internal PrintVisitor()
            {
            }

            private static string FormatConstant(ConstantExpression c)
            {
                object obj2 = c.Value;
                if (obj2 == null)
                {
                    return "null";
                }
                Type type = Nullable.GetUnderlyingType(c.Type) ?? c.Type;
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Empty:
                        return "null";

                    case TypeCode.DBNull:
                        return "DBNull.Value";

                    case TypeCode.Boolean:
                        return obj2.ToString();

                    case TypeCode.Char:
                        if (((char)obj2) != '\0')
                        {
                            return ("'" + obj2.ToString() + "'");
                        }
                        return @"'\0'";

                    case TypeCode.SByte:
                        {
                            sbyte num6 = (sbyte)obj2;
                            return num6.ToString(CultureInfo.InvariantCulture);
                        }
                    case TypeCode.Byte:
                        return obj2.ToString();

                    case TypeCode.Int16:
                        {
                            short num3 = (short)obj2;
                            return num3.ToString(CultureInfo.InvariantCulture);
                        }
                    case TypeCode.UInt16:
                        {
                            ushort num8 = (ushort)obj2;
                            return num8.ToString(CultureInfo.InvariantCulture);
                        }
                    case TypeCode.Int32:
                        {
                            int num4 = (int)obj2;
                            return num4.ToString(CultureInfo.InvariantCulture);
                        }
                    case TypeCode.UInt32:
                        {
                            uint num9 = (uint)obj2;
                            return num9.ToString(CultureInfo.InvariantCulture);
                        }
                    case TypeCode.Int64:
                        {
                            long num5 = (long)obj2;
                            return num5.ToString(CultureInfo.InvariantCulture);
                        }
                    case TypeCode.UInt64:
                        {
                            ulong num10 = (ulong)obj2;
                            return num10.ToString(CultureInfo.InvariantCulture);
                        }
                    case TypeCode.Single:
                        {
                            float num7 = (float)obj2;
                            return (num7.ToString(CultureInfo.InvariantCulture) + "f");
                        }
                    case TypeCode.Double:
                        {
                            double num2 = (double)obj2;
                            return (num2.ToString(CultureInfo.InvariantCulture) + "d");
                        }
                    case TypeCode.Decimal:
                        {
                            decimal num = (decimal)obj2;
                            return (num.ToString(CultureInfo.InvariantCulture) + "m");
                        }
                    case TypeCode.DateTime:
                        {
                            DateTime time = (DateTime)obj2;
                            return ("DateTime.Parse(\"" + time.ToString() + "\")");
                        }
                    case TypeCode.String:
                        return ("\"" + obj2.ToString().Replace("\"", "\\\"") + "\"");
                }
                if (type == typeof(Guid))
                {
                    return ("Guid.Parse(\"" + obj2.ToString() + "\")");
                }
                if (type == typeof(TimeSpan))
                {
                    return ("TimeSpan.Parse(\"" + obj2.ToString() + "\")");
                }
                return obj2.ToString();
            }

            private static string GetBinaryOperator(ExpressionType t)
            {
                switch (t)
                {
                    case ExpressionType.Add:
                        return "+";

                    case ExpressionType.AddChecked:
                        return "+";

                    case ExpressionType.And:
                        return "AND";

                    case ExpressionType.AndAlso:
                        return "AND";

                    //case ExpressionType.Coalesce:
                    //    return "??";

                    case ExpressionType.Divide:
                        return "/";

                    case ExpressionType.Equal:
                        return "==";

                    //case ExpressionType.ExclusiveOr:
                    //    return "^";

                    case ExpressionType.GreaterThan:
                        return ">";

                    case ExpressionType.GreaterThanOrEqual:
                        return ">=";

                    //case ExpressionType.LeftShift:
                    //    return "<<";

                    case ExpressionType.LessThan:
                        return "<";

                    case ExpressionType.LessThanOrEqual:
                        return "<=";

                    case ExpressionType.Modulo:
                        return "%";

                    case ExpressionType.Multiply:
                        return "*";

                    case ExpressionType.MultiplyChecked:
                        return "*";

                    case ExpressionType.NotEqual:
                        return "<>";

                    case ExpressionType.Or:
                        return "OR";

                    case ExpressionType.OrElse:
                        return "OR";

                    //case ExpressionType.Power:
                    //    return "^";

                    case ExpressionType.RightShift:
                        return ">>";

                    case ExpressionType.Subtract:
                        return "-";

                    case ExpressionType.SubtractChecked:
                        return "-";
                }
                throw new NotSupportedException(t.ToString());
            }

            internal string GetText()
            {
                return this._out.ToString();
            }

            private static string GetTypeName(Type t)
            {
                switch (Type.GetTypeCode(t))
                {
                    case TypeCode.DBNull:
                    case TypeCode.DateTime:
                        return t.Name;

                    case TypeCode.Boolean:
                        return "bool";

                    case TypeCode.Char:
                        return "char";

                    case TypeCode.SByte:
                        return "sbyte";

                    case TypeCode.Byte:
                        return "byte";

                    case TypeCode.Int16:
                        return "short";

                    case TypeCode.UInt16:
                        return "ushort";

                    case TypeCode.Int32:
                        return "int";

                    case TypeCode.UInt32:
                        return "uint";

                    case TypeCode.Int64:
                        return "long";

                    case TypeCode.UInt64:
                        return "ulong";

                    case TypeCode.Single:
                        return "float";

                    case TypeCode.Double:
                        return "double";

                    case TypeCode.Decimal:
                        return "decimal";

                    case TypeCode.String:
                        return "string";
                }
                if (!t.IsGenericType)
                {
                    return t.Name;
                }
                StringBuilder builder = new StringBuilder();
                builder.Append(t.Name.Substring(0, t.Name.Length - 2));
                builder.Append("<");
                bool flag = true;
                foreach (Type type in t.GetGenericArguments())
                {
                    if (flag)
                    {
                        flag = false;
                    }
                    else
                    {
                        builder.Append(",");
                    }
                    builder.Append(GetTypeName(type));
                }
                builder.Append(">");
                return builder.ToString();
            }

            private void Indent()
            {
                this.indent++;
            }

            private static bool OperandRequiresIndent(ExpressionType t)
            {
                return (t == (ExpressionType.ModuloAssign | ExpressionType.NewArrayInit));
            }

            private static bool OperandRequiresParens(ExpressionType t)
            {
                return ((((t != ExpressionType.Constant) && (t != ExpressionType.Call)) && (t != ExpressionType.MemberAccess)) && (t != ExpressionType.Parameter));
            }

            private ExpressionToLinq.PrintVisitor Out(string value)
            {
                this._out.Append(value);
                return this;
            }

            private ExpressionToLinq.PrintVisitor OutLine(string value = "")
            {
                if (this._out.Length > 0)
                {
                    this._out.AppendLine(value);
                }
                if (this.indent > 0)
                {
                    this._out.Append('\t', this.indent);
                }
                return this;
            }

            private void Unindent()
            {
                this.indent--;
            }

            internal override Expression Visit(Expression exp)
            {
                if (exp != null)
                {
                    ExpressionType nodeType = exp.NodeType;
                    if (nodeType == (ExpressionType.ModuloAssign | ExpressionType.NewArrayInit))
                    {
                        return this.VisitQueryExpression((ExpressionToLinq.QueryExpression)exp);
                    }
                    if (nodeType == ((ExpressionType)0x3e8))
                    {
                        return this.Visit(((ExpressionToLinq.TypeFixExpression)exp).Inner);
                    }
                }
                return base.Visit(exp);
            }

            internal override Expression VisitBinary(BinaryExpression b)
            {
                if (b.NodeType == ExpressionType.ArrayIndex)
                {
                    this.VisitFormatted(b.Left);
                    this.Out("[");
                    this.Visit(b.Right);
                    this.Out("]");
                    return b;
                }
                this.VisitFormatted(b.Left);
                this.Out(" ");
                this.Out(GetBinaryOperator(b.NodeType));
                this.Out(" ");
                this.VisitFormatted(b.Right);
                return b;
            }

            internal override Expression VisitConditional(ConditionalExpression c)
            {
                this.VisitFormatted(c.Test);
                this.Out(" ? ");
                this.VisitFormatted(c.IfTrue);
                this.Out(" : ");
                this.VisitFormatted(c.IfFalse);
                return c;
            }

            internal override Expression VisitConstant(ConstantExpression c)
            {
                if (c.Value != null)
                {
                    if (c.Type.IsGenericType && typeof(IQueryable).IsAssignableFrom(c.Type))
                    {
                        Type type = c.Type.GetGenericArguments()[0];
                        this.Out(type.Name);
                        return c;
                    }
                    this.Out(FormatConstant(c));
                    return c;
                }
                this.Out("NULL");
                return c;
            }

            private void VisitExpressions(IEnumerable list, string separator = ", ")
            {
                bool flag = true;
                foreach (Expression expression in list)
                {
                    if (flag)
                    {
                        flag = false;
                    }
                    else
                    {
                        this.Out(separator);
                    }
                    this.Visit(expression);
                }
            }

            private void VisitExpressions(IEnumerable list, string open, string close, string separator = ", ")
            {
                this.Out(open);
                this.VisitExpressions(list, separator);
                this.Out(close);
            }

            private void VisitFormatted(Expression expr)
            {
                bool flag = OperandRequiresParens(expr.NodeType);
                bool flag2 = OperandRequiresIndent(expr.NodeType);
                if (flag)
                {
                    this.Out("(");
                }
                if (flag2)
                {
                    this.Indent();
                }
                this.Visit(expr);
                if (flag2)
                {
                    this.Unindent();
                }
                if (flag)
                {
                    this.Out(")");
                }
            }

            internal override Expression VisitLambda(LambdaExpression lambda)
            {
                if (lambda.Parameters.Count == 1)
                {
                    this.Visit(lambda.Parameters[0]);
                }
                else
                {
                    this.VisitExpressions(lambda.Parameters, "(", ")", ", ");
                }
                //this.Out(" => ");
                this.Visit(lambda.Body);
                return lambda;
            }

            internal override Expression VisitListInit(ListInitExpression init)
            {
                this.Visit(init.NewExpression);
                this.Indent();
                this.OutLine(" {");
                for (int i = 0; i < init.Initializers.Count; i++)
                {
                    if (i > 0)
                    {
                        this.OutLine(",");
                    }
                    this.Visit(init.Initializers[i].Arguments[0]);
                }
                this.Unindent();
                this.OutLine("");
                this.Out("}");
                return init;
            }

            internal override Expression VisitMemberAccess(MemberExpression m)
            {
                if (m.Expression != null)
                {
                    this.VisitFormatted(m.Expression);
                }
                else
                {
                    this.Out(GetTypeName(m.Member.DeclaringType));
                }
                this.Out(".");
                this.Out(m.Member.Name);
                return m;
            }

            internal override MemberAssignment VisitMemberAssignment(MemberAssignment assignment)
            {
                this.Out(assignment.Member.Name);
                this.Out(" = ");
                this.Visit(assignment.Expression);
                return assignment;
            }

            internal override Expression VisitMemberInit(MemberInitExpression init)
            {
                this.Visit(init.NewExpression);
                this.Indent();
                this.OutLine(" {");
                for (int i = 0; i < init.Bindings.Count; i++)
                {
                    if (i > 0)
                    {
                        this.OutLine(",");
                    }
                    this.VisitBinding(init.Bindings[i]);
                }
                this.Unindent();
                this.OutLine("");
                this.Out("}");
                return init;
            }

            internal override Expression VisitMethodCall(MethodCallExpression m)
            {
                int num = 0;
                Expression expr = m.Object;
                if (Attribute.GetCustomAttribute(m.Method, typeof(ExtensionAttribute)) != null)
                {
                    num = 1;
                    expr = m.Arguments[0];
                }
                if (expr != null)
                {
                    this.VisitFormatted(expr);
                }
                else
                {
                    this.Out(GetTypeName(m.Method.DeclaringType));
                }
                this.Out(".");
                this.Out(m.Method.Name);
                this.Out("(");
                int num2 = num;
                int count = m.Arguments.Count;
                while (num2 < count)
                {
                    if (num2 > num)
                    {
                        this.Out(", ");
                    }
                    this.Visit(m.Arguments[num2]);
                    num2++;
                }
                this.Out(")");
                return m;
            }

            internal override NewExpression VisitNew(NewExpression nex)
            {
                bool flag = ExpressionToLinq.IsAnonymousType(nex.Type);
                this.Out("new ");
                if (!flag)
                {
                    this.Out(GetTypeName(nex.Type));
                }
                if (flag)
                {
                    this.Indent();
                    this.OutLine("{");
                    for (int i = 0; i < nex.Arguments.Count; i++)
                    {
                        if (i > 0)
                        {
                            this.OutLine(",");
                        }
                        if (nex.Members != null)
                        {
                            this.Out(nex.Members[i].Name).Out(" = ");
                        }
                        this.VisitFormatted(nex.Arguments[i]);
                    }
                    this.Unindent();
                    this.OutLine("");
                    this.Out("}");
                    return nex;
                }
                this.VisitExpressions(nex.Arguments, "(", ")", ", ");
                return nex;
            }

            internal override Expression VisitNewArray(NewArrayExpression na)
            {
                if (na.NodeType == ExpressionType.NewArrayInit)
                {
                    this.Out("new [] ");
                    this.VisitExpressions(na.Expressions, "{", "}", ", ");
                    return na;
                }
                if (na.NodeType == ExpressionType.NewArrayBounds)
                {
                    this.Out("new ");
                    this.Out(GetTypeName(na.Type.GetElementType()));
                    this.VisitExpressions(na.Expressions, "[", "]", ", ");
                }
                return na;
            }

            internal override Expression VisitParameter(ParameterExpression p)
            {
                this.Out(p.Name);
                return p;
            }

            private Expression VisitQueryExpression(ExpressionToLinq.QueryExpression queryExpression)
            {
                foreach (ExpressionToLinq.QueryClause clause in queryExpression)
                {
                    this.OutLine("");
                    switch (clause.Type)
                    {
                        case ExpressionToLinq.QueryClauseType.From:
                            {
                                ExpressionToLinq.QueryFromClause clause3 = (ExpressionToLinq.QueryFromClause)clause;
                                this.Out("from ");
                                this.Out(clause3.Identifier);
                                this.Out(" in ");
                                this.VisitFormatted(clause3.Source);
                                break;
                            }
                        case ExpressionToLinq.QueryClauseType.Where:
                            {
                                ExpressionToLinq.QueryWhereClause clause5 = (ExpressionToLinq.QueryWhereClause)clause;
                                this.Out("where ");
                                this.Visit(clause5.Condition);
                                break;
                            }
                        case ExpressionToLinq.QueryClauseType.GroupBy:
                            {
                                ExpressionToLinq.QueryGroupByClause clause6 = (ExpressionToLinq.QueryGroupByClause)clause;
                                this.Out("group ");
                                this.Visit(clause6.Source);
                                this.Out(" by ");
                                this.Visit(clause6.Key);
                                break;
                            }
                        case ExpressionToLinq.QueryClauseType.Join:
                            {
                                ExpressionToLinq.QueryJoinClause clause7 = (ExpressionToLinq.QueryJoinClause)clause;
                                this.Out("join ");
                                this.Out(clause7.Identifier);
                                this.Out(" in ");
                                this.VisitFormatted(clause7.Source);
                                this.Out(" on ");
                                this.VisitFormatted(clause7.OuterKey);
                                this.Out(" equals ");
                                this.VisitFormatted(clause7.InnerKey);
                                if (!string.IsNullOrEmpty(clause7.IntoIdentifier))
                                {
                                    this.Out(" into ").Out(clause7.IntoIdentifier);
                                }
                                break;
                            }
                        case ExpressionToLinq.QueryClauseType.OrderBy:
                            {
                                ExpressionToLinq.QueryOrderByClause clause8 = (ExpressionToLinq.QueryOrderByClause)clause;
                                this.Out("orderby ");
                                bool flag = true;
                                foreach (ExpressionToLinq.QueryOrdering ordering in clause8)
                                {
                                    if (flag)
                                    {
                                        flag = false;
                                    }
                                    else
                                    {
                                        this.Out(", ");
                                    }
                                    this.Visit(ordering.Expr);
                                    if (ordering.Descending)
                                    {
                                        this.Out(" descending");
                                    }
                                }
                                break;
                            }
                        case ExpressionToLinq.QueryClauseType.Let:
                            {
                                ExpressionToLinq.QueryLetClause clause4 = (ExpressionToLinq.QueryLetClause)clause;
                                this.Out("let ");
                                this.Out(clause4.Identifier);
                                this.Out(" = ");
                                this.Visit(clause4.Expr);
                                break;
                            }
                        case ExpressionToLinq.QueryClauseType.Select:
                            {
                                ExpressionToLinq.QuerySelectClause clause9 = (ExpressionToLinq.QuerySelectClause)clause;
                                this.Out("select ");
                                this.Visit(clause9.Selection);
                                break;
                            }
                        case ExpressionToLinq.QueryClauseType.Continuation:
                            {
                                ExpressionToLinq.QueryContinuationClause clause2 = (ExpressionToLinq.QueryContinuationClause)clause;
                                this.Visit(clause2.Query);
                                this.Out(" into ");
                                this.Out(clause2.Identifier);
                                break;
                            }
                    }
                }
                if ((queryExpression.Last.Type != ExpressionToLinq.QueryClauseType.Select) && (queryExpression.Last.Type != ExpressionToLinq.QueryClauseType.GroupBy))
                {
                    this.OutLine("");
                    this.Out("select ");
                    this.Out(queryExpression.Last.Identifier);
                }
                return queryExpression;
            }

            internal override Expression VisitUnary(UnaryExpression u)
            {
                ExpressionType nodeType = u.NodeType;
                switch (nodeType)
                {
                    case ExpressionType.Negate:
                        this.Out("-");
                        break;

                    case ExpressionType.NegateChecked:
                        this.Out("checked(-");
                        break;

                    case ExpressionType.Not:
                        this.Out("!");
                        break;

                    case ExpressionType.ConvertChecked:
                        this.Out("checked(").Out("(").Out(GetTypeName(u.Type)).Out(")");
                        break;

                    case ExpressionType.Convert:
                        this.Out("(").Out(GetTypeName(u.Type)).Out(")");
                        break;
                }
                this.Visit(u.Operand);
                switch (nodeType)
                {
                    case ExpressionType.ArrayLength:
                        this.Out(".Length");
                        return u;

                    case ExpressionType.ConvertChecked:
                    case ExpressionType.NegateChecked:
                        this.Out(")");
                        return u;

                    case ExpressionType.TypeAs:
                        this.Out(" as ").Out(GetTypeName(u.Type));
                        break;
                }
                return u;
            }
        }

        private abstract class QueryClause
        {
            internal readonly ExpressionToLinq.QueryClauseType Type;
            internal readonly string Identifier;
            internal ExpressionToLinq.QueryClause Next;
            internal ExpressionToLinq.QueryClause Prev;

            internal QueryClause(ExpressionToLinq.QueryClauseType type, string identifier)
            {
                this.Type = type;
                this.Identifier = identifier;
            }
        }

        private enum QueryClauseType
        {
            From,
            Where,
            GroupBy,
            Join,
            OrderBy,
            Let,
            Select,
            Continuation
        }

        private sealed class QueryContinuationClause : ExpressionToLinq.QueryClause
        {
            internal readonly ExpressionToLinq.QueryExpression Query;

            internal QueryContinuationClause(ExpressionToLinq.QueryExpression query, string identifier) : base(ExpressionToLinq.QueryClauseType.Continuation, identifier)
            {
                this.Query = query;
            }
        }

        private sealed class QueryExpression : Expression, IEnumerable<ExpressionToLinq.QueryClause>, IEnumerable
        {
            private int count;
            private ExpressionToLinq.QueryClause first;
            private ExpressionToLinq.QueryClause last;

            internal QueryExpression(Type type) : base(ExpressionType.ModuloAssign | ExpressionType.NewArrayInit, type)
            {
            }

            internal ExpressionToLinq.QueryExpression Append(ExpressionToLinq.QueryClause clause)
            {
                this.count++;
                if (this.first == null)
                {
                    this.first = clause;
                }
                else if (this.last == null)
                {
                    this.first.Next = this.last = clause;
                    clause.Prev = this.first;
                }
                else
                {
                    clause.Prev = this.last;
                    this.last.Next = this.last = clause;
                }
                return this;
            }

            public IEnumerator<ExpressionToLinq.QueryClause> GetEnumerator()
            {
                ExpressionToLinq.QueryClause first = this.first;
                while (true)
                {
                    if (first == null)
                    {
                        yield break;
                    }
                    yield return first;
                    first = first.Next;
                }
            }

            internal ExpressionToLinq.QueryExpression RemoveLast()
            {
                if (this.first != null)
                {
                    this.count--;
                    if (this.last == null)
                    {
                        this.first = null;
                    }
                    else
                    {
                        ExpressionToLinq.QueryClause prev = this.last.Prev;
                        prev.Next = this.last.Prev = (ExpressionToLinq.QueryClause)(this.last = null);
                        if (prev != this.first)
                        {
                            this.last = prev;
                        }
                    }
                }
                return this;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            internal ExpressionToLinq.QueryClause First
            {
                get
                {
                    return this.first;
                }
            }

            internal ExpressionToLinq.QueryClause Last
            {
                get
                {
                    return this.last;
                }
            }

        }

        private sealed class QueryFromClause : ExpressionToLinq.QueryClause
        {
            internal readonly Expression Source;

            internal QueryFromClause(Expression source, string identifier) : base(ExpressionToLinq.QueryClauseType.From, identifier)
            {
                this.Source = source;
            }
        }

        private sealed class QueryGroupByClause : ExpressionToLinq.QueryClause
        {
            internal readonly Expression Source;
            internal readonly Expression Key;

            internal QueryGroupByClause(Expression source, Expression key) : base(ExpressionToLinq.QueryClauseType.GroupBy, string.Empty)
            {
                this.Source = source;
                this.Key = key;
            }
        }

        private sealed class QueryJoinClause : ExpressionToLinq.QueryClause
        {
            internal readonly Expression Source;
            internal readonly Expression OuterKey;
            internal readonly Expression InnerKey;
            internal readonly string IntoIdentifier;

            internal QueryJoinClause(Expression source, string identifier, Expression outerKey, Expression innerKey, string intoIdentifier = "") : base(ExpressionToLinq.QueryClauseType.Join, identifier)
            {
                this.Source = source;
                this.OuterKey = outerKey;
                this.InnerKey = innerKey;
                this.IntoIdentifier = intoIdentifier;
            }
        }

        private sealed class QueryLetClause : ExpressionToLinq.QueryClause
        {
            internal readonly Expression Expr;

            internal QueryLetClause(Expression expr, string identifier) : base(ExpressionToLinq.QueryClauseType.Let, identifier)
            {
                this.Expr = expr;
            }
        }

        private sealed class QueryOrderByClause : ExpressionToLinq.QueryClause, IEnumerable<ExpressionToLinq.QueryOrdering>, IEnumerable
        {
            private ExpressionToLinq.QueryOrdering first;
            private ExpressionToLinq.QueryOrdering last;

            internal QueryOrderByClause(string identifier) : base(ExpressionToLinq.QueryClauseType.OrderBy, identifier)
            {
            }

            internal ExpressionToLinq.QueryOrderByClause Append(ExpressionToLinq.QueryOrdering ordering)
            {
                if (this.first == null)
                {
                    this.first = this.last = ordering;
                }
                else
                {
                    this.last.Next = this.last = ordering;
                }
                return this;
            }

            public IEnumerator<ExpressionToLinq.QueryOrdering> GetEnumerator()
            {
                ExpressionToLinq.QueryOrdering first = this.first;
                while (true)
                {
                    if (first == null)
                    {
                        yield break;
                    }
                    yield return first;
                    first = first.Next;
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

        }

        private sealed class QueryOrdering
        {
            internal readonly Expression Expr;
            internal readonly bool Descending;
            internal ExpressionToLinq.QueryOrdering Next;

            internal QueryOrdering(Expression expr, bool descending = false)
            {
                this.Expr = expr;
                this.Descending = descending;
            }
        }

        private sealed class QuerySelectClause : ExpressionToLinq.QueryClause
        {
            internal readonly Expression Selection;

            internal QuerySelectClause(Expression selection) : base(ExpressionToLinq.QueryClauseType.Select, string.Empty)
            {
                this.Selection = selection;
            }
        }

        private sealed class QueryWhereClause : ExpressionToLinq.QueryClause
        {
            internal readonly Expression Condition;

            internal QueryWhereClause(Expression condition, string identifier) : base(ExpressionToLinq.QueryClauseType.Where, identifier)
            {
                this.Condition = condition;
            }
        }

        private sealed class TypeFixExpression : Expression
        {
            private Expression inner;

            internal TypeFixExpression(Expression inner, Type type) : base((ExpressionType)0x3e8, type)
            {
                this.inner = inner;
            }

            internal Expression Inner
            {
                get
                {
                    return this.inner;
                }
            }
        }
    }

}
