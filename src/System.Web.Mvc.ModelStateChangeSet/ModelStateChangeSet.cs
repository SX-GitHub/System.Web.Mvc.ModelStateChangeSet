using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

namespace System.Web.Mvc.ModelStateChangeSet
{
    public static partial class ModelStateChangeSet
    {
        private static readonly Dictionary<Type, string[]> _keyCache = new Dictionary<Type, string[]>();

        public static Dictionary<string, T> Subset<T>(this Dictionary<string, T> values, string prefix) where T : class
            => values.Where(item => item.Key.StartsWith(prefix))
            .ToDictionary(item => item.Key.Substring(prefix.Length), item => item.Value);

        public static Dictionary<string, object> GetValuesToSave<T>(this List<ModelChange<T>> changes) where T : class
            => changes.ToDictionary(change => change.Property, change => change.NewValue);

        public static Dictionary<string, object> IncludeKeys<T>(this Dictionary<string, object> values, T source) where T : class
        {
            var type = typeof(T);
            if (!_keyCache.ContainsKey(type))
                _keyCache.Add(type, FindKeys(typeof(T)).ToArray());
            return values.Include(source, _keyCache[type]);
        }

        public static IEnumerable<string> FindKeys(Type type, string path = "")
        {
            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (IsSimpleType(property.PropertyType))
                {
                    if (property.GetCustomAttribute<KeyAttribute>() != null || property.GetCustomAttribute<ForeignKeyAttribute>() != null)
                        yield return property.PropertyType.Name;
                    continue;
                }
                FindKeys(property.PropertyType, $"{path}{property.Name}.");
            }
        }

        public static Dictionary<string, object> Include<T>(this Dictionary<string, object> values, T source, params string[] properties)
        {
            var accessors = typeof(T).GetPropertyAccessors<T>();
            foreach (var property in properties.Where(p => accessors.ContainsKey(p)))
            {
                if (values.ContainsKey(property))
                {
                    values[property] = accessors[property]?.Invoke(source);
                    continue;
                }
                values.Add(property, accessors[property]?.Invoke(source));
            }

            return values;
        }

        public static List<ModelChange<T>> GetChangeSet<T>(this ModelStateDictionary modelState, T model, T original)
            => GetChanges<T>(modelState, model, original).ToList();

        public static IEnumerable<ModelChange<T>> GetChanges<T>(ModelStateDictionary modelState, T model, T original)
        {
            var accessors = typeof(T).GetPropertyAccessors<T>();
            foreach ((var key, var accessor) in modelState.Keys.Where(name => accessors.ContainsKey(name)).Select(key => (key, accessors[key])))
            {
                var modelValue = accessor?.Invoke(model);
                var originalValue = accessor?.Invoke(original);
                if (!Equals(modelValue, originalValue))
                    yield return new ModelChange<T> { Property = key, OldValue = originalValue, NewValue = modelValue };
            }
        }
    }
}
