using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Common
{
    [CustomEditor(typeof(MonoBehaviour), true, isFallback = true)]
    public class EditorBase : Editor
    {
        private Dictionary<MemberInfo, Func<string>> displays;
        private Dictionary<MethodInfo, System.Action> actions;

        private void OnEnable()
        {
            displays = DisplayAttribute.GetDisplays(target);
            actions = ActionAttribute.GetActions(target);
            Debug.Log("Enabled " + target.name + " : " + string.Join(",", actions));
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            Debug.Log("OnInspectorGUI " + target.name + " : " + string.Join(",", displays));


            foreach (var a in displays)
            {
                EditorGUILayout.LabelField(a.Key.Name,a.Value());
            }

            foreach (var a in actions)
            {
                if (GUILayout.Button(a.Key.Name))
                    a.Value();
            }

        }
    }
}
