using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;

namespace Assets.Scripts.Common
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class DisplayAttribute : Attribute
    {
        private const BindingFlags BindingAttr = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default;

        public static Dictionary<MemberInfo, System.Func<string>> GetDisplays(object o)
        {
            var functions = new Dictionary<MemberInfo, System.Func<string>>();
            var properties = o.GetType().GetProperties(BindingAttr);
            foreach (var property in properties)
            {
                var attr = property.GetCustomAttribute<DisplayAttribute>();
                if (attr is null)
                    continue;
                functions[property] = () => property.GetValue(o).ToString();
            }
            var fields = o.GetType().GetFields(BindingAttr);
            foreach (var field in fields)
            {
                var attr = field.GetCustomAttribute<DisplayAttribute>();
                if (attr is null)
                    continue;
                functions[field] = () => field.GetValue(o).ToString();
            }
            return functions;
        }
    }
}
