using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Fulu.Core.Extensions
{
    public static class LinqExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static Expression Property(this Expression expression, string propertyName)
        {
            return Expression.Property(expression, propertyName);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Expression AndAlso(this Expression left, Expression right)
        {
            return Expression.AndAlso(left, right);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="methodName"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public static Expression Call(this Expression instance, string methodName, params Expression[] arguments)
        {
            return Expression.Call(instance, instance.Type.GetMethod(methodName), arguments);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Expression GreaterThan(this Expression left, Expression right)
        {
            return Expression.GreaterThan(left, right);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="body"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static Expression<T> ToLambda<T>(this Expression body, params ParameterExpression[] parameters)
        {
            return Expression.Lambda<T>(body, parameters);
        }
        /// <summary>
        /// 单个AND有效，多个AND有效；单个OR无效，多个OR无效；混应时写在AND后的OR有效 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Expression<Func<T, bool>> True<T>() { return param => true; }
        /// <summary>
        /// 单个AND无效，多个AND无效；单个OR有效，多个OR有效；混应时写在OR后面的AND有效 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Expression<Func<T, bool>> False<T>() { return param => false; }
        /// <summary>
        /// 组合And
        /// </summary>
        /// <returns></returns>
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
        {
            return first.Compose(second, Expression.AndAlso);
        }
        /// <summary>
        /// 组合Or
        /// </summary>
        /// <returns></returns>
        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
        {
            return first.Compose(second, Expression.OrElse);
        }

        /// <summary>
        /// Combines the first expression with the second using the specified merge function.
        /// </summary>
        static Expression<T> Compose<T>(this Expression<T> first, Expression<T> second, Func<Expression, Expression, Expression> merge)
        {
            var map = first.Parameters
                .Select((f, i) => new { f, s = second.Parameters[i] })
                .ToDictionary(p => p.s, p => p.f);
            var secondBody = ParameterRebinder.ReplaceParameters(map, second.Body);
            return Expression.Lambda<T>(merge(first.Body, secondBody), first.Parameters);
        }

        /// <summary>
        /// 通过定义查询条件List类简化方法
        /// </summary>
        /// <typeparam name="T">EF实体</typeparam>
        /// <param name="queryList">查询List条件参数</param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> GetAndLambdaExpression<T>(List<SearchModel> queryList)
        {
            Type type = typeof(T);
            Expression expression = Expression.Constant(true);
            ParameterExpression parameter = Expression.Parameter(type, "p");//在表达式树中使用ParameterExpression或者ParameterExpression表达式表示变量类型

            foreach (SearchModel item in queryList)
            {
                Expression left = Expression.Property(parameter, typeof(T).GetProperty(item.Key));
                Expression right = Expression.Constant(item.Text);
                Expression filter;
                switch (item.Condition)
                {
                    case "Equal"://em查询条件类型.等于:
                        filter = Expression.Equal(left, right);
                        break;
                    case "NotEqual":// em查询条件类型.不等于:
                        filter = Expression.NotEqual(left, right);
                        break;
                    case "More":// em查询条件类型.大于:
                        filter = Expression.GreaterThan(left, right);
                        break;
                    case "MoreEqual"://查询条件类型.大于等于:
                        filter = Expression.GreaterThanOrEqual(left, right);
                        break;
                    case "Less":// em查询条件类型.小于:
                        filter = Expression.LessThan(left, right);
                        break;
                    case "LessEqual":// em查询条件类型.小于等于:
                        filter = Expression.LessThanOrEqual(left, right);
                        break;
                    case "Like"://em查询条件类型.like:
                        filter = Expression.Call(left, typeof(string).GetMethod("Contains"), right);
                        break;
                    case "NotLike":// em查询条件类型.notlike:
                        filter = Expression.Not(Expression.Call(left, typeof(string).GetMethod("Contains"), right));
                        break;
                    default:
                        filter = Expression.Equal(left, right);
                        break;
                }
                expression = Expression.And(expression, filter);
            }
            return Expression.Lambda<Func<T, bool>>(expression, parameter);

            //原始判断方法
            //switch (methons[i])
            //{
            //    case "=":
            //        temp = Expression.Equal(Expression.Call(Expression.Property(expression_param, TType.GetProperty(keys[i])),
            //            TType.GetMethod("ToString")),
            //         Expression.Constant(values[i]));
            //        expression_return = Expression.And(expression_return, temp);
            //        break;
            //    case "%":
            //        temp = Expression.Call(Expression.Property(expression_param, TType.GetProperty(keys[i])),
            //            typeof(string).GetMethod("Contains"),
            //            Expression.Constant(values[i], typeof(string)));
            //        expression_return = Expression.And(expression_return, temp);
            //        break;
            //    case ">": //大于方法
            //        temp = Expression.Call(Expression.Property(expression_param, TType.GetProperty(keys[i])),
            //            typeof(double).GetType().GetMethod("GreaterThan"), Expression.Constant(values[i]));
            //        expression_return = Expression.And(expression_return, temp);
            //        break;
            //    case "<": //小于方法
            //        temp = Expression.Call(Expression.Property(expression_param, TType.GetProperty(keys[i])),
            //            typeof(double).GetType().GetMethod("LessThan"), Expression.Constant(values[i]));
            //        expression_return = Expression.And(expression_return, temp);
            //        break;
            //    case ">=":
            //        temp = Expression.Call(Expression.Property(expression_param, TType.GetProperty(keys[i])),
            //            typeof(double).GetType().GetMethod("GreaterThanOrEqual"), Expression.Constant(values[i]));
            //        expression_return = Expression.And(expression_return, temp);
            //        break;
            //    case "<=":
            //        temp = Expression.Call(Expression.Property(expression_param, TType.GetProperty(keys[i])),
            //            TType.GetProperty(keys[i]).GetType().GetMethod("LessThanOrEqual"), Expression.Constant(values[i]));
            //        expression_return = Expression.And(expression_return, temp);
            //        break;
            //    case "in": 
            //        string[] strarr = values[i].ToString().Split(',');
            //        Expression or_return = Expression.Constant(false);
            //        for (int k = 0; k < strarr.Length; k++)
            //        {
            //            temp = Expression.Equal(Expression.Call(Expression.Property(expression_param, TType.GetProperty(keys[i])),
            //                TType.GetMethod("ToString")),
            //             Expression.Constant(strarr[k]));
            //            or_return = Expression.Or(or_return, temp);
            //        }

            //        expression_return = Expression.And(expression_return, or_return);
            //        break;


        }

        /// <summary>
        /// ParameterRebinder
        /// </summary>
        private class ParameterRebinder : ExpressionVisitor
        {
            /// <summary>
            /// The ParameterExpression map
            /// </summary>
            readonly Dictionary<ParameterExpression, ParameterExpression> map;
            /// <summary>
            /// Initializes a new instance of the <see cref="ParameterRebinder"/> class.
            /// </summary>
            /// <param name="map">The map.</param>
            ParameterRebinder(Dictionary<ParameterExpression, ParameterExpression> map)
            {
                this.map = map ?? new Dictionary<ParameterExpression, ParameterExpression>();
            }
            /// <summary>
            /// Replaces the parameters.
            /// </summary>
            /// <param name="map">The map.</param>
            /// <param name="exp">The exp.</param>
            /// <returns>Expression</returns>
            public static Expression ReplaceParameters(Dictionary<ParameterExpression, ParameterExpression> map, Expression exp)
            {
                return new ParameterRebinder(map).Visit(exp);
            }
            /// <summary>
            /// Visits the parameter.
            /// </summary>
            /// <param name="p">The p.</param>
            /// <returns>Expression</returns>
            protected override Expression VisitParameter(ParameterExpression p)
            {
                ParameterExpression replacement;

                if (map.TryGetValue(p, out replacement))
                {
                    p = replacement;
                }
                return base.VisitParameter(p);
            }
        }

    }

    /// <summary>
    /// 定义查询条件
    /// </summary>
    public class SearchModel
    {
        private string key;
        private string condition;
        private string text;
        /// <summary>
        /// 查询Key，对应数据库字段名
        /// </summary>
        public string Key
        {
            get
            {
                return key;
            }

            set
            {
                key = value;
            }
        }
        /// <summary>
        /// 查询条件
        /// </summary>
        public string Condition
        {
            get
            {
                return condition;
            }

            set
            {
                condition = value;
            }
        }
        /// <summary>
        /// 查询的值
        /// </summary>
        public string Text
        {
            get
            {
                return text;
            }

            set
            {
                text = value;
            }
        }
    }
}