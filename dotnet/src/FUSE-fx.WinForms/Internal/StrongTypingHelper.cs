using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;


namespace System.Data.Fuse.WinForms.Internal {


  internal static class StrongTypingExtensions {

    [EditorBrowsable(EditorBrowsableState.Always)]
    public static IQueryable<TObj> Where<TObj>(this IEnumerable<TObj> extendee, string propertyName, object value) {
      var objectType = typeof(TObj);
      var source = extendee.AsQueryable();
      var parameter = Expression.Parameter(objectType, "p");
      Expression whereExp = null;
      var valueType = objectType.GetProperty(propertyName).PropertyType;
      object typedValue = null;

      if (value is string && !(valueType == typeof(string))) {
        valueType.TryParse((string)value, ref typedValue);
      }

      Expression left = Expression.Property(parameter, propertyName);
      var right = Expression.Constant(typedValue);
      whereExp = Expression.Equal(left, right);

      var delegateType = typeof(Func<,>).MakeGenericType(objectType, typeof(bool));
      var predicate = Expression.Lambda(delegateType, whereExp, new ParameterExpression[] { parameter });


      var whereMethod = typeof(Queryable).GetMethods().Last(m => m.Name.Equals("Where"));
      var typedWhereMethod = whereMethod.MakeGenericMethod(objectType);

      var result = typedWhereMethod.Invoke(source, new object[] { source, predicate });

      return (IQueryable<TObj>)result;
    }

    [EditorBrowsable(EditorBrowsableState.Always)]
    public static IQueryable<TObj> Where<TObj>(this IEnumerable<TObj> extendee, Dictionary<string, object> keyPropertyValues) {
      var objectType = typeof(TObj);
      var source = extendee.AsQueryable();
      var parameter = Expression.Parameter(objectType, "p");
      Expression whereExp = null;

      foreach (string propertyName in keyPropertyValues.Keys) {

        Expression left = Expression.Property(parameter, propertyName);
        var value = keyPropertyValues[propertyName];
        var right = Expression.Constant(value);

        if (whereExp == null) {
          whereExp = Expression.Equal(left, right);
        }
        else {
          whereExp = Expression.And(whereExp, Expression.Equal(left, right));
        }

      }

      var delegateType = typeof(Func<,>).MakeGenericType(objectType, typeof(bool));
      var predicate = Expression.Lambda(delegateType, whereExp, new ParameterExpression[] { parameter });


      var whereMethod = typeof(Queryable).GetMethods().Last(m => m.Name.Equals("Where"));
      var typedWhereMethod = whereMethod.MakeGenericMethod(objectType);

      var result = typedWhereMethod.Invoke(source, new object[] { source, predicate });

      return (IQueryable<TObj>)result;
    }

  }

  internal class StrongTypingHelper {

    private StrongTypingHelper() {
    }





    public TObj Select<TObj>(IEnumerable<TObj> extendee, Dictionary<string, object> keyPropertyValues) {

      var objectType = typeof(TObj);
      var source = extendee.AsQueryable();
      var parameter = Expression.Parameter(objectType, "p");
      Expression @where = null;

      foreach (string propertyName in keyPropertyValues.Keys) {

        Expression left = Expression.Property(parameter, propertyName);
        var value = keyPropertyValues[propertyName];
        var right = Expression.Constant(value);

        if (where == null) {
          where = Expression.Equal(left, right);
        }
        else {
          where = Expression.And(where, Expression.Equal(left, right));
        }

      }

      var delegateType = typeof(Func<,>).MakeGenericType(objectType, typeof(bool));
      var predicate = Expression.Lambda(delegateType, where, new ParameterExpression[] { parameter });
      var singleMethod = typeof(Queryable).GetMethods().Last(m => m.Name.Equals("SingleOrDefault"));
      var genericSingle = singleMethod.MakeGenericMethod(objectType);
      var o = genericSingle.Invoke(source, new object[] { source, predicate });

      return (TObj)o;
    }

    public static Expression<Func<TObj, bool>> GetPropertyEqualExpression<TObj, TProperty>(Expression<Func<TObj, TProperty>> propertyPicker, TProperty compareValue) {

      string propertyName = GetPropertyAsString(propertyPicker);
      var objectType = typeof(TObj);
      var parameter = Expression.Parameter(objectType, "p");

      Expression expressionBody = Expression.Equal(Expression.Property(parameter, propertyName), Expression.Constant(compareValue));


      var delegateType = typeof(Func<,>).MakeGenericType(objectType, typeof(bool));

      var predicate = Expression.Lambda(delegateType, expressionBody, new ParameterExpression[] { parameter });

      return (Expression<Func<TObj, bool>>)predicate;
    }

    public static string GetPropertyAsString<TObj>(Expression<Func<TObj, object>> expression) {
      return GetPropertyAsString<TObj, object>(expression);
    }

    public static string GetPropertyAsString<TObj, TProperty>(Expression<Func<TObj, TProperty>> expression) {
      return GetProperty(expression).Name;
    }

    public static PropertyInfo GetProperty<T>(Expression<Func<T>> expression) {
      MemberExpression member = expression.Body as MemberExpression;

      if (member != null) {
        return member.Member as PropertyInfo;
      }

      if (expression.Body.NodeType == ExpressionType.Convert) {

        member = ((UnaryExpression)expression.Body).Operand as MemberExpression;

        if (member != null) {
          return member.Member as PropertyInfo;
        }

      }

      throw new ArgumentException("Expression stellt keinen Zugriff auf einen Member dar", expression.ToString());
    }

    public static PropertyInfo GetProperty<T, TR>(Expression<Func<T, TR>> expression) {
      MemberExpression member = expression.Body as MemberExpression;

      if (member != null) {
        return (PropertyInfo)member.Member;
      }

      switch (expression.Body.NodeType) {
        case ExpressionType.Convert:
        case ExpressionType.ConvertChecked: {
            member = ((UnaryExpression)expression.Body).Operand as MemberExpression;
            if (member != null) {
              return (PropertyInfo)member.Member;
            }

            break;
          }
      }

      string nodeTypeName = Enum.GetName(typeof(ExpressionType), expression.Body.NodeType);

      throw new ArgumentException(string.Format("Die Expression '{0}' stellt keinen Zugriff auf einen Member dar oder deren Typ ({1}) wird nicht unterstützt.{2}" + "Erlaubt sind 'MemberExpressions' oder Expressions vom Typ: 'Convert'.", expression.ToString(), nodeTypeName, Environment.NewLine), expression.ToString());

    }

  }
}