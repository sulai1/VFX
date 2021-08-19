using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Common
{
    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class ActionAttribute : Attribute
    {
        private readonly Func<object[]> parameters;

        public ActionAttribute(Func<object[]> parameters)
        {
            this.parameters = parameters;
        }
        public ActionAttribute(object[] parameters) : this(() => parameters) { }
        public ActionAttribute() : this(() => new object[0]) { }


        public static Dictionary<MethodInfo, System.Action> GetActions(object o)
        {
            var actions = new Dictionary<MethodInfo, System.Action>();
            var methods = o.GetType().GetMethods();
            foreach (var method in methods)
            {
                var attr = method.GetCustomAttribute<ActionAttribute>();
                if (attr != null)
                {
                    var parameters = attr.parameters();
                    if (method.GetParameters().Length == parameters.Length)
                        actions[method] = () => method.Invoke(o, parameters);
                }
            }
            return actions;
        }
    }
}
