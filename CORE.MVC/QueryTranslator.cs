using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CORE.MVC
{
    internal class QueryTranslator : ExpressionVisitor
    {
        //Find find = null;
        //private List<MemberClass> MemberList = new List<MemberClass>();

        //public Result Query { get; private set; } = null;
        //public QueryTranslator(Find find = null) {

        //    this.find = find;
        //    var me = GetMemberClass(find.Principal, true);
        //}
        //private StringBuilder sb;

        //private string _orderBy = string.Empty;
        //private int? _skip = null;
        //private int? _take = null;
        //private string _whereClause = string.Empty;

        //public int? Skip
        //{
        //    get
        //    {
        //        return _skip;
        //    }
        //}

        //public int? Take
        //{
        //    get
        //    {
        //        return _take;
        //    }
        //}

        //public string OrderBy
        //{
        //    get
        //    {
        //        return _orderBy;
        //    }
        //}

        //public string WhereClause
        //{
        //    get
        //    {
        //        return _whereClause;
        //    }
        //}
        //private void MemberClassJoins(MemberExpression m)
        //{
        //    var cont = m.ToString().Split('.');
        //    MemberExpression ex = null;
        //    MemberExpression memberEx = null;
        //    DatabaseModel.Table tbFK1 = null;
        //    try
        //    {               
        //        //ex = (m.Expression as MemberExpression).Expression as MemberExpression;
        //        memberEx = (m.Expression as MemberExpression);
        //        //string nolock = find.mapper.Connection.UseTransaction ? "(nolock)" : "";
        //        string nolock = "";

        //        var mbPK = GetMemberClass(find.Principal);                
        //        tbFK1 = mbPK.Table.GetTableFK(cont[1]);
                
        //        var PK_ColFK = mbPK.Table.GetColumnFK(cont[1]);
        //        if(mbPK.Joins.ToString().Contains(tbFK1.Name)){
        //            return;
        //        }
        //        string alias = cont.Length==2 ? GetMemberClass(memberEx.Type).Alias : tbFK1.Name;
        //        string joinQ = PK_ColFK.IsNull ? "LEFT JOIN" : "INNER JOIN";

        //        if (alias.Contains(".") == false)
        //        {
        //            mbPK.Joins.Append($" {joinQ} {tbFK1.Name} {alias} {nolock} ON ");
        //        }
        //        else
        //        {
        //            mbPK.Joins.Append($" {joinQ} {tbFK1.Name} {nolock} ON ");
        //        }
        //        mbPK.Joins.Append($"{alias}.{tbFK1.PrimaryKey.Name} = ");
        //        mbPK.Joins.Append($"{mbPK.Alias}.{PK_ColFK.Name}");

        //        for (int i = 2; i < cont.Length-1; i++)
        //        {
        //            var tbFK2 = tbFK1.GetTableFK(cont[i]);
        //            var PK_ColFK2 = tbFK1.GetColumnFK(cont[i]);

        //            if (mbPK.Joins.ToString().Contains(tbFK2.Name))
        //            {
        //                return;
        //            }
        //            alias = GetMemberClass(memberEx.Type).Alias;

        //            joinQ = PK_ColFK2.IsNull ? "LEFT JOIN" : "INNER JOIN";

        //            mbPK.Joins.Append($" {joinQ} {tbFK2.Name} {alias} {nolock} ON ");
        //            mbPK.Joins.Append($"{alias}.{tbFK2.PrimaryKey.Name} = ");
        //            mbPK.Joins.Append($"{tbFK1.Name}.{PK_ColFK2.Name}");
        //        }
                
        //    }
        //    catch (Exception ex1)
        //    {

        //    }
           
        //}

        //private MemberClass GetMemberClass(Type type, bool default_ = false)
        //{
        //    try
        //    {
        //        MemberClass member = MemberList.FirstOrDefault(i => i.TypeMember.FullName == type.FullName && i.TypeMember.MetadataToken == type.MetadataToken);

        //        if (member == null)
        //        {
        //            MemberClassType flag = default_ == true ? MemberClassType.Default : MemberClassType.Table;
        //            if ((type.Name == typeof(TypeOfParam).Name))
        //            {
        //                flag = MemberClassType.Constant;
        //            }
        //            member = new MemberClass
        //            {
        //                Flag = flag,
        //                TypeMember = type,
        //                //Table = flag == MemberClassType.Constant ? null : DatabaseModel.Instance.Tables[type],
        //                Alias = $"T{MemberList.Count}"
        //            };
        //            MemberList.Add(member);
        //        }
        //        return member;
        //    }
        //    catch (Exception b)
        //    {
        //        return null;
        //    }
        //}
        //private ItemMember AddParameter(Type tb_type, string name, object value, bool Constant, Type type_value)
        //{
        //    var member = GetMemberClass(tb_type);
        //    Type type = null;

        //    ItemMember item = null;
        //    if (Constant) {
        //        type = value.GetType();
        //        item = member.ItensMember.FirstOrDefault(i => i.Name == name && i.Value.GetType().GetHashCode() == type.GetHashCode() && object.Equals(i.Value, value) && i.Constant == true);

        //        if (item == null)
        //        {
        //            item = new ItemMember
        //            {
        //                Constant = true,
        //                Name = name,
        //                Member = member,
        //                Value = value,
        //                Parameter = $"@{name}{member.ItensMember.Count}"
        //            };
        //            member.ItensMember.Add(item);
        //        }
        //    }
        //    else {
        //        type = type_value;
        //        item = member.ItensMember.FirstOrDefault(i => i.Column.Property.PropertyType.FullName == type.FullName && i.Name == name && i.Constant == false);

        //        if (item == null)
        //        {
        //            item = new ItemMember
        //            {
        //                Column = member.Table ?.Columns.FirstOrDefault(a => a.Property.Name == name),
        //                Constant = false,
        //                Name = name,
        //                Member = member,
        //                Value = value,
        //                Parameter = $"@{name}{member.ItensMember.Count}"
        //            };
        //            member.ItensMember.Add(item);
        //        }
        //    }
        //    return item;
        //}

        //public string Translate(Expression expression)
        //{
        //    this.sb = new StringBuilder();
        //    this.Visit(expression);
        //    _whereClause = this.sb.ToString();
        //    var lis = MemberList;
        //    var mb = MemberList.FirstOrDefault(i => i.Flag == MemberClassType.Default);//GetMemberClass(find.Principal, true);
        //    var parameters = new List<Microsoft.Data.SqlClient.SqlParameter>();

        //    var conts = MemberList.FirstOrDefault(i => i.Flag == MemberClassType.Constant);
        //    if (conts != null)
        //    {
        //        foreach (var item in conts.ItensMember.Distinct())
        //        {
        //            parameters.Add(new Microsoft.Data.SqlClient.SqlParameter(item.Parameter, item.Value));
        //        }
        //    }
        //    //string nolock = find.mapper.Connection.UseTransaction ? "(nolock)" : "";
        //    string nolock = "";
        //    Query = mb.Table==null ? new Result { 
        //    Member = mb,
        //    Where=_whereClause,
        //    Parameters= parameters.ToArray()

        //    } : new Result {
        //        Member = mb,
        //        Where = _whereClause,
        //        SelectFrom = $"SELECT row_number() over(order by {mb.Alias}.{mb.Table.PrimaryKey.Name}) as RowNumber,{mb.Alias}.* FROM {mb.Table.Name} {mb.Alias} {nolock}" + mb.Joins.ToString(),
        //        Parameters = parameters.ToArray()
        //    };
        //    return _whereClause;
        //}
        //Type GetExpressionType(Expression ex)
        //{
        //    try
        //    {
        //        return (ex as MemberExpression).Member.DeclaringType;
        //    }
        //    catch (Exception)
        //    {
        //        return typeof(TypeOfParam);
        //    }
        //}
        //private static Expression StripQuotes(Expression e)
        //{
        //    while (e.NodeType == ExpressionType.Quote)
        //    {
        //        e = ((UnaryExpression)e).Operand;
        //    }
        //    return e;
        //}

        //protected override Expression VisitMethodCall(MethodCallExpression m)
        //{
        //    if (m.Method.DeclaringType == typeof(Queryable) && m.Method.Name == "Where")
        //    {
        //        this.Visit(m.Arguments[0]);
        //        LambdaExpression lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
        //        this.Visit(lambda.Body);
        //        return m;
        //    }
        //    else if (m.Method.Name == "Take")
        //    {
        //        if (this.ParseTakeExpression(m))
        //        {
        //            Expression nextExpression = m.Arguments[0];
        //            return this.Visit(nextExpression);
        //        }
        //    }
        //    else if (m.Method.Name == "Skip")
        //    {
        //        if (this.ParseSkipExpression(m))
        //        {
        //            Expression nextExpression = m.Arguments[0];
        //            return this.Visit(nextExpression);
        //        }
        //    }
        //    else if (m.Method.Name == "OrderBy")
        //    {
        //        if (this.ParseOrderByExpression(m, "ASC"))
        //        {
        //            Expression nextExpression = m.Arguments[0];
        //            return this.Visit(nextExpression);
        //        }
        //    }
        //    else if (m.Method.Name == "OrderByDescending")
        //    {
        //        if (this.ParseOrderByExpression(m, "DESC"))
        //        {
        //            Expression nextExpression = m.Arguments[0];
        //            return this.Visit(nextExpression);
        //        }
        //    }
        //    else if (m.Method.Name == "Contains")
        //    {
        //        if (this.ParseContainsExpression(m))
        //        {
        //            Expression nextExpression = m.Arguments[0];
        //            return m;// this.Visit(nextExpression);
        //        }
        //    }
        //    else if (m.Method.Name == "Like")
        //    {
        //        if (this.ParseContainsExpression(m))
        //        {
        //            Expression nextExpression = m.Arguments[0];
        //            return m;// this.Visit(nextExpression);
        //        }
        //    }
        //    else if (m.Method.Name == "NotIn")
        //    {
        //        if (this.ParseNotInExpression(m))
        //        {
        //            Expression nextExpression = m.Arguments[0];
        //            return m;// this.Visit(nextExpression);
        //        }
        //    }
        //    else if (m.Method.Name == "In")
        //    {
        //        if (this.ParseInExpression(m))
        //        {
        //            Expression nextExpression = m.Arguments[0];
        //            return m;// this.Visit(nextExpression);
        //        }
        //    }
        //    else if (m.Method.ReturnType != null)
        //    {
        //        var ex = ConstantExpression.Constant(getValue(m));
        //        this.Visit(ex);
        //        return m;
        //    }
        //    throw new NotSupportedException(string.Format("The method '{0}' is not supported", m.Method.Name));
        //}

        //private object getValue(Expression m)
        //{
        //    var member = Expression.Convert(m, typeof(object));
        //    var lambdal = Expression.Lambda<Func<object>>(member);
        //    return lambdal.Compile().DynamicInvoke();
        //}

        //protected override Expression VisitUnary(UnaryExpression u)
        //{
        //    switch (u.NodeType)
        //    {
        //        case ExpressionType.Not:
        //            if (_ExpressionRight.ToString().Contains("!"))
        //            { sb.Append(" NOT "); }
        //            this.Visit(u.Operand);
        //            break;
        //        case ExpressionType.Convert:
        //            this.Visit(u.Operand);
        //            break;
        //        default:
        //            throw new NotSupportedException(string.Format("The unary operator '{0}' is not supported", u.NodeType));
        //    }
        //    return u;
        //}


        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="b"></param>
        ///// <returns></returns>
        //Expression _Expression;
        //Expression _ExpressionLeft;
        //Expression _ExpressionRight;
        //protected override Expression VisitBinary(BinaryExpression b)
        //{
        //    sb.Append("(");

        //    _Expression = b;
        //    _ExpressionLeft = b.Left;
        //    _ExpressionRight = b.Right;
            
        //    this.Visit(b.Left);

        //    _Expression = b;
        //    _ExpressionLeft = b.Left;
        //    _ExpressionRight = b.Right;
        //    switch (b.NodeType)
        //    {
        //        case ExpressionType.And:
        //            sb.Append(" AND ");
        //            break;

        //        case ExpressionType.AndAlso:
        //            sb.Append(" AND ");
        //            break;

        //        case ExpressionType.Or:
        //            sb.Append(" OR ");
        //            break;

        //        case ExpressionType.OrElse:
        //            sb.Append(" OR ");
        //            break;

        //        case ExpressionType.Equal:
        //            if (IsNullConstant(b.Right))
        //            {
        //                //sb.Append(" IS NULL");
        //                sb.Append(" IS ");
        //            }
        //            else if (_Expression.ToString().Contains("HasValue"))
        //            {
        //                sb.Append(" ");
        //            }
        //            else{
        //                sb.Append(" = ");
        //            }
        //            break;

        //        case ExpressionType.NotEqual:
        //            if (IsNullConstant(b.Right))
        //            {
        //                sb.Append(" IS NOT ");
        //                //sb.Append(" IS NOT NULL");
        //            }
        //            else
        //            {
        //                sb.Append(" <> ");
        //            }
        //            break;

        //        case ExpressionType.LessThan:
        //            sb.Append(" < ");
        //            break;

        //        case ExpressionType.LessThanOrEqual:
        //            sb.Append(" <= ");
        //            break;

        //        case ExpressionType.GreaterThan:
        //            sb.Append(" > ");
        //            break;

        //        case ExpressionType.GreaterThanOrEqual:
        //            sb.Append(" >= ");
        //            break;

        //        case ExpressionType.Multiply:
        //            sb.Append(" * ");
        //            break;

        //        case ExpressionType.Subtract:
        //            sb.Append(" - ");
        //            break;

        //        case ExpressionType.Divide:
        //            sb.Append(" / ");
        //            break;

        //        case ExpressionType.Add:
        //            sb.Append(" + ");
        //            break;

        //        default:
        //            break;
        //            throw new NotSupportedException(string.Format("The binary operator '{0}' is not supported", b.NodeType));

        //    }

        //    this.Visit(b.Right);

        //    _Expression = b;
        //    _ExpressionLeft = b.Left;
        //    _ExpressionRight = b.Right;

        //    sb.Append(")");

        //    return b;
        //}

        //protected override Expression VisitConstant(ConstantExpression c)
        //{
        //    IQueryable q = c.Value as IQueryable;

        //    var exp_ = _Expression ?.ToString();
        //    var expL_ = _ExpressionLeft ?.ToString();
        //    var expR_ = _ExpressionRight?.ToString();

        //    setLike();
        //    setIn();
        //    setNotIn();

        //    if (q == null && c.Value == null)
        //    {
        //        sb.Append("NULL");
        //    }
        //    else if (q == null)
        //    {
        //        string val = c.Value?.ToString() ?? "";

        //        if (likeFlag && val.Contains("%") == false)
        //        {
        //            sb.Append("'%");
        //            sb.Append(c.Value);
        //            sb.Append("%'");
        //        }
        //        else if (likeFlag)
        //        {
        //            sb.Append("'");
        //            sb.Append(c.Value);
        //            sb.Append("'");
        //        }
        //        else
        //        {
        //            switch (Type.GetTypeCode(c.Value.GetType()))
        //            {
        //                case TypeCode.Boolean:
        //                    if (exp_.Contains("HasValue"))
        //                    {
        //                        sb.Append(((bool)c.Value) ? "IS NOT NULL" : "IS NULL");
        //                    }
        //                    else
        //                    {
        //                        sb.Append(((bool)c.Value) ? 1 : 0);
        //                    }
        //                    break;

        //                case TypeCode.String:
        //                    sb.Append("'");
        //                    sb.Append(c.Value);
        //                    sb.Append("'");
        //                    break;

        //                case TypeCode.DateTime:
        //                    //sb.Append("'");
        //                    //sb.Append(c.Value);
        //                    //sb.Append("'");
        //                    if (InFlag || NotInFlag)
        //                    {
        //                        sb.Append("'");
        //                        sb.Append(((DateTime)c.Value).ToString("yyyy-MM-dd HH:mm:ss.fff"));
        //                        sb.Append("'");
        //                    }
        //                    else
        //                    {
        //                        sb.Append("'");
        //                        sb.Append(((DateTime)c.Value).ToString("yyyy-MM-dd HH:mm:ss.fff"));
        //                        sb.Append("'");
        //                    }
        //                    break;

        //                case TypeCode.Object:
        //                    throw new NotSupportedException(string.Format("The constant for '{0}' is not supported", c.Value));

        //                default:
        //                    sb.Append(c.Value);
        //                    break;
        //            }
        //        }
        //    }

        //    return c;
        //}

        //private void setLike()
        //{
        //    if (likeFlag && _Expression != null && _Expression.ToString().Contains("Like"))
        //    {
        //        if (likeCont > 0)
        //        {
        //            sb.Append(" OR ");
        //            likeFlag = false;
        //            this.Visit(_ExpressionLeft);
        //            likeFlag = true;
        //        }
        //        sb.Append(" LIKE ");
        //        likeCont++;
        //    }
        //}
        //private void setIn()
        //{
        //    if (InFlag && _Expression != null && _Expression.ToString().Contains("In"))
        //    {
        //        if (InCont > 0)
        //        {
        //            sb.Append(",");
        //        }
        //        InCont++;
        //    }
        //}
        //private void setNotIn()
        //{
        //    if (NotInFlag && _Expression != null && _Expression.ToString().Contains("NotIn"))
        //    {
        //        if (NotInCont > 0)
        //        {
        //            sb.Append(",");
        //        }
        //        NotInCont++;
        //    }
        //}

        //Expression GetExpression(string key)
        //{
        //    if (_ExpressionLeft.ToString().Contains(key))
        //        return _ExpressionLeft;
        //    if (_ExpressionRight.ToString().Contains(key))
        //        return _ExpressionRight;

        //    return _Expression;
        //}
        //protected override Expression VisitMember(MemberExpression m)
        //{
        //    var exp_ = _Expression ?.ToString();
        //    var expL_ = _ExpressionLeft ?.ToString();
        //    var expR_ = _ExpressionRight ?.ToString();

        //    setLike();
        //    LikeConcatPerc(true);
        //    setIn();
        //    setNotIn();
        //    //variaveis externas
        //    if (m.Expression != null && m.Expression.NodeType == ExpressionType.Constant)
        //    {
        //        var Member = getMemberValue(m);
        //        var itemParam = AddParameter(typeof(TypeOfParam), Member.Key, Member.Value, true, m.Type);
        //        if (likeFlag)
        //        {
        //            sb.Append($"Convert(varchar,{itemParam.Parameter})");
        //        }
        //        else if ((InFlag || NotInFlag) && m.Type.FullName.Contains("DateTime"))
        //        {
        //            sb.Append($"{itemParam.Parameter}");
        //        }
        //        else if(m.Type.FullName.Contains("DateTime"))
        //        {
        //            sb.Append("Convert(varchar,");
        //            sb.Append(itemParam.Parameter);
        //            sb.Append(",121)");
        //        }
        //        else {
        //            sb.Append($"{itemParam.Parameter}");
        //        }
        //        LikeConcatPerc(false);
        //        return m;
        //    }
        //    //campos
        //    if (m.Expression != null && m.Expression.NodeType == ExpressionType.Parameter)
        //    {
        //        var item = AddParameter(m.Expression.Type, m.Member.Name, null, false, m.Type);
                
        //        if (exp_ != null && exp_.Contains("Like") && likeFlag==false && m.Type.FullName.Contains("DateTime"))
        //        {
        //            sb.Append("Convert(varchar,");
        //            sb.Append(item.Member.Alias);
        //            sb.Append(".");
        //            sb.Append(item.Column == null ? item.Name :item.Column.Name) ;
        //            sb.Append(",121)");
        //        }
        //        else if ((InFlag || NotInFlag || (exp_ != null && exp_.Contains("In"))) && m.Type.FullName.Contains("DateTime"))
        //        {
        //            //sb.Append("Convert(varchar,");
        //            //sb.Append(item.Member.Alias);
        //            //sb.Append(".");
        //            //sb.Append(item.Column.Name);
        //            //sb.Append(")");
        //            sb.Append(item.Member.Alias);
        //            sb.Append(".");
        //            sb.Append(item.Column == null ? item.Name : item.Column.Name);

        //            //sb.Append(item.Column.Name);
        //        }
        //        else if (m.Type.FullName.Contains("DateTime"))
        //        {
        //            sb.Append("Convert(varchar,");
        //            sb.Append(item.Member.Alias);
        //            sb.Append(".");
        //            //sb.Append(item.Column.Name);
        //            sb.Append(item.Column == null ? item.Name : item.Column.Name);
        //            sb.Append(",121)");
        //        }
        //        else
        //        {
        //            sb.Append(item.Member.Alias);
        //            sb.Append(".");
        //            //sb.Append(item.Column.Name);
        //            sb.Append(item.Column == null ? item.Name : item.Column.Name);
        //        }
        //        LikeConcatPerc(false);
        //        return m;
        //    }
        //    //Quando for classe
        //    if (m.Expression != null && m.Expression.NodeType == ExpressionType.MemberAccess)
        //    {
        //        //Localizar objectos joins
        //        MemberClassJoins(m);
        //        //Nullable
        //        if(m.Member.Name.Contains("HasValue"))
        //        {
        //            var ex = (m.Expression as MemberExpression).Member;
                    
        //            var item = AddParameter(ex.ReflectedType, ex.Name, null, false, m.Expression.Type.GenericTypeArguments[0]);

        //            sb.Append(item.Member.Alias);
        //            sb.Append(".");
        //            sb.Append(item.Column.Name);
        //            var exp1 = expR_;

        //            if(exp1.Contains(item.Column.Name) && exp1.Contains("=")==false)
        //            {
        //                sb.Append(exp1.Contains("Not") ? " IS NULL ":" IS NOT NULL ");
        //            }
        //            LikeConcatPerc(false);
        //        }
        //        else{

        //            var item = AddParameter(m.Member.DeclaringType, m.Member.Name, null, false, m.Type);
        //            if (exp_ != null && exp_.Contains("Like") && likeFlag == false && m.Type.FullName.Contains("DateTime"))
        //            {
        //                sb.Append("Convert(varchar,");
        //                sb.Append(item.Member.Alias);
        //                sb.Append(".");
        //                sb.Append(item.Column.Name);
        //                sb.Append(",121)");
        //            }
        //            else if ((InFlag || NotInFlag) && m.Type.FullName.Contains("DateTime"))
        //            {
        //                sb.Append("Convert(varchar,");
        //                sb.Append(item.Member.Alias);
        //                sb.Append(".");
        //                sb.Append(item.Column.Name);
        //                sb.Append(")");
        //            }
        //            else if (m.Type.FullName.Contains("DateTime"))
        //            {
        //                sb.Append("Convert(varchar,");
        //                sb.Append(item.Member.Alias);
        //                sb.Append(".");
        //                sb.Append(item.Column.Name);
        //                sb.Append(",121)");
        //            }
        //            else
        //            {
        //                sb.Append(item.Member.Alias);
        //                sb.Append(".");
        //                sb.Append(item.Column.Name);
        //            }

        //            LikeConcatPerc(false);
        //        }
        //        return m;
        //    }
        //    throw new NotSupportedException(string.Format("The member '{0}' is not supported", m.Member.Name));
        //}

        //private void LikeConcatPerc(bool add = true)
        //{
        //    if (likeFlag && add)
        //    {
        //        sb.Append("'%'+");
        //    }
        //    else if (likeFlag && add == false)
        //    {
        //        sb.Append("+'%'");
        //    }
        //}

        //protected bool IsNullConstant(Expression exp)
        //{
        //    return (exp.NodeType == ExpressionType.Constant && ((ConstantExpression)exp).Value == null);
        //}
        //protected bool IsNullNotValue(Expression exp)
        //{
        //    return (exp.NodeType == ExpressionType.Constant && ((ConstantExpression)exp).Value == null);
        //}
        //private bool ParseOrderByExpression(MethodCallExpression expression, string order)
        //{
        //    UnaryExpression unary = (UnaryExpression)expression.Arguments[1];
        //    LambdaExpression lambdaExpression = (LambdaExpression)unary.Operand;

        //    //lambdaExpression = (LambdaExpression)Evaluator.PartialEval(lambdaExpression);

        //    MemberExpression body = lambdaExpression.Body as MemberExpression;
        //    if (body != null)
        //    {
        //        if (string.IsNullOrEmpty(_orderBy))
        //        {
        //            _orderBy = string.Format("{0} {1}", body.Member.Name, order);
        //        }
        //        else
        //        {
        //            _orderBy = string.Format("{0}, {1} {2}", _orderBy, body.Member.Name, order);
        //        }

        //        return true;
        //    }

        //    return false;
        //}

        //private bool ParseTakeExpression(MethodCallExpression expression)
        //{
        //    ConstantExpression sizeExpression = (ConstantExpression)expression.Arguments[1];

        //    int size;
        //    if (int.TryParse(sizeExpression.Value.ToString(), out size))
        //    {
        //        _take = size;
        //        return true;
        //    }

        //    return false;
        //}

        //private bool ParseSkipExpression(MethodCallExpression expression)
        //{
        //    ConstantExpression sizeExpression = (ConstantExpression)expression.Arguments[1];

        //    int size;
        //    if (int.TryParse(sizeExpression.Value.ToString(), out size))
        //    {
        //        _skip = size;
        //        return true;
        //    }

        //    return false;
        //}
        //bool likeFlag = false;
        //bool InFlag = false;
        //bool NotInFlag = false;
        //int likeCont = 0;
        //private bool ParseContainsExpression(MethodCallExpression expression)
        //{
        //    //UnaryExpression unary = (UnaryExpression)expression.Arguments[1];
        //    //LambdaExpression lambdaExpression = (LambdaExpression)unary.Operand;

        //    //lambdaExpression = (LambdaExpression)Evaluator.PartialEval(lambdaExpression);
            
        //    Expression body = (expression.Object as MemberExpression ?? expression.Arguments[0]);
        //    if (body != null)
        //    {
        //        _Expression = expression;
        //        _ExpressionLeft = body;
        //        sb.Append("(");
        //        this.Visit(body);
        //        likeCont = 0;
        //        //sb.Append(" LIKE ");
        //        likeFlag = true;
        //        Expression exp = expression.Arguments.Count == 1 ? expression.Arguments[0] : expression.Arguments[1];
        //        //var val = getValue(exp);
        //        this.Visit(exp);
        //        sb.Append(")");
        //        likeFlag = false;
        //        return true;
        //    }

        //    return false;
        //}
        //int InCont = 0;
        //private bool ParseInExpression(MethodCallExpression expression)
        //{
        //    Expression body = (expression.Object as MemberExpression ?? expression.Arguments[0]);
        //    if (body != null)
        //    {
        //        _Expression = expression;
        //        _ExpressionLeft = body;
        //        sb.Append("(");
        //        this.Visit(body);
        //        InFlag = true;
        //        sb.Append(" IN(");
        //        InCont = 0;
        //        Expression exp = expression.Arguments.Count == 1 ? expression.Arguments[0] : expression.Arguments[1];
        //        //var val = getValue(exp);
        //        this.Visit(exp);
        //        sb.Append("))");
        //        InFlag = false;
        //        return true;
        //    }

        //    return false;
        //}
        //int NotInCont = 0;
        //private bool ParseNotInExpression(MethodCallExpression expression)
        //{
        //    Expression body = (expression.Object as MemberExpression ?? expression.Arguments[0]);
        //    if (body != null)
        //    {
        //        _Expression = expression;
        //        _ExpressionLeft = body;
        //        sb.Append("(");
        //        this.Visit(body);
        //        NotInFlag = true;
        //        sb.Append(" NOT IN(");
        //        NotInCont = 0;
        //        Expression exp = expression.Arguments.Count == 1 ? expression.Arguments[0] : expression.Arguments[1];
        //        //var val = getValue(exp);
        //        this.Visit(exp);
        //        sb.Append("))");
        //        NotInFlag = false;
        //        return true;
        //    }

        //    return false;
        //}

        //public KeyValuePair<string, object> getMemberValue(MemberExpression m) {
        //    var member = Expression.Convert(m, typeof(object));
        //    var lambda = Expression.Lambda<Func<object>>(member);
        //    var getter = lambda.Compile();

        //    return new KeyValuePair<string, object>(m.Member.Name, getter());
        //}


        //#region Helper
        //internal class MemberClass {
        //    public MemberClassType Flag { get; set; } = MemberClassType.Constant;
        //    public string Alias { get; set; }
        //    public Type TypeMember { get; set; }
        //    public DatabaseModel.Table Table
        //    {
        //        get
        //        {
        //            try
        //            {
        //                return DatabaseModel.Instance.Tables[TypeMember];
        //            }
        //            catch (Exception)
        //            {
        //                return null;
        //            } } }
        //    public List<ItemMember> ItensMember { get; set; } = new List<ItemMember>();
        //    public StringBuilder Where { get; set; } = new StringBuilder();
        //    public StringBuilder Joins { get; set; } = new StringBuilder();

        //}
        //internal enum MemberClassType{
        //Constant=0,
        //Default=1,
        //Table=2,
        //SubTable=3,
        //View=4
        //}
        //internal class ItemMember{
        //    //public Type Type { get; set; }
        //    public MemberClass Member { get; set; }
        //    public DatabaseModel.Column Column { get; set; }
        //    public string Parameter { get; set; }
        //    public string Name { get; set; }
        //    public object Value { get; set; }
        //    public bool Constant { get; set; }
        //}

        //#endregion
        //public class TypeOfParam{

        //}
        //public class Result{
        //    public string SelectFrom { get;internal set; }
        //    public string Where { get;internal set; }
        //    public MemberClass Member { get;internal set; }
        //    public SqlParameter[] Parameters { get; internal set; }

        //}
    }
}
