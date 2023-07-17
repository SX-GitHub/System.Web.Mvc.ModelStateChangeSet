using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace System.Web.Mvc.ModelStateChangeSet
{
    public static partial class ModelStateChangeSet
    {
        private static readonly Dictionary<Type, object> _accessorCache = new Dictionary<Type, object>();

        public static Dictionary<string, Func<T, object>> GetPropertyAccessors<T>(this Type type)
        {
            if (!_accessorCache.ContainsKey(type))
                _accessorCache.Add(type, GetPropertyAccessors(type, "", new Dictionary<string, Func<T, object>>()));
            return (Dictionary<string, Func<T, object>>)_accessorCache[type];
        }

        private static Dictionary<string, Func<T, object>> GetPropertyAccessors<T>(Type currentType, string path, Dictionary<string, Func<T, object>> map)
        {
            foreach (var property in currentType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (IsSimpleType(property.PropertyType))
                {
                    var parameter = Expression.Parameter(typeof(T));
                    var member = GetMemberExpression(parameter, $"{path}{property.Name}");
                    var lambda = Expression.Lambda<Func<T, object>>(member, parameter);
                    map.Add($"{path}{property.Name}", lambda.Compile());
                    continue;
                }

                GetPropertyAccessors(property.PropertyType, $"{path}{property.Name}.", map);
            }
            return map;
        }

        private static Expression GetMemberExpression(Expression accessor, string propertyPath)
        {
            (var first, var nested) = propertyPath.Split('.').FirstAndRemainder();
            accessor = Expression.PropertyOrField(accessor, first);

            foreach (var layer in nested)
            {
                var property = Expression.PropertyOrField(accessor, layer);
                accessor = Expression.Condition(Expression.Equal(accessor, Expression.Constant(null)),
                    Expression.Convert(Expression.Constant(null), property.Type), property);
            }

            return Expression.Convert(accessor, typeof(object));
        }

        private static bool IsSimpleType(Type type) => type.IsPrimitive || type.IsEnum || type.Namespace == "System";

        private static (T, IEnumerable<T>) FirstAndRemainder<T>(this T[] values) => (values.First(), values.Skip(1));

    }
}
