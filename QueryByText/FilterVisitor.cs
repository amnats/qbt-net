using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;

namespace QueryByText
{
    // This is not an example of a visitor pattern
    // This is a disgrace 
    public class FilterVisitor
    {
        public FilterVisitor(Expression parameterExpr)
        {
            _parameterExpr = parameterExpr;
        }

        private Expression _parameterExpr;

        public Expression Visit(FilterExpression exp)
        {
            if (exp == null)
                return null;

            switch (exp.NodeType)
            {
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.LessThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LessThanOrEqual:
                    return VisitBinary(exp);

                case ExpressionType.MemberAccess:
                    return VisitField(exp);

                case ExpressionType.Constant:
                    return VisitConstant(exp);

                default:
                    throw new NotSupportedException($"Not supported NodeType {exp.NodeType} with value {exp.Value}");
            }
        }

        private Expression VisitBinary(FilterExpression exp)
        {
            var leftExpr = Visit(exp.LeftExpr);
            var rightExpr = Visit(exp.RightExpr);

            ConvertByPriority(ref leftExpr, ref rightExpr);

            return Expression.MakeBinary(exp.NodeType, leftExpr, rightExpr);
        }

        private void ConvertByPriority(ref Expression left, ref Expression right)
        {
            var leftTypeCode = Type.GetTypeCode(left.Type);
            var rightTypeCode = Type.GetTypeCode(right.Type);

            if (leftTypeCode == rightTypeCode)
                return;

            if (leftTypeCode > rightTypeCode)
                right = Expression.Convert(right, left.Type);
            else
                left = Expression.Convert(left, right.Type);
        }

        private Expression VisitConstant(FilterExpression exp)
        {
            // TODO: add support for other types
            return Expression.Constant(exp.Value, exp.ValueType);
        }

        private Expression VisitField(FilterExpression exp)
        {
            string propName = Helper.FirstToUppercase((string)exp.Value);
            return Expression.MakeMemberAccess(_parameterExpr, _parameterExpr.Type.GetProperty(propName));
        }
    }
}
