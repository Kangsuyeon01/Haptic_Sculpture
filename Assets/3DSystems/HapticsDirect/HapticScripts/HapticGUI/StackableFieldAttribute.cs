//This file or parts of this code are base on the project
//
// https://github.com/j1930021/Stackable-Decorator
//
//MIT License
//
//Copyright (c) 2018 Kinwailo
//
//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:
//
//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.



using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HapticGUI
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class StackableFieldAttribute : PropertyAttribute
    {
#if UNITY_EDITOR
        public SerializedProperty m_SerializedProperty { get; private set; }
        public FieldInfo m_FieldInfo { get; private set; }

        public ReadOnlyCollection<HapticGUIAttribute> decorators { get { return m_Decorators.AsReadOnly(); } }
        private List<HapticGUIAttribute> m_Decorators = null;

        private static List<FieldInfo> s_FieldInfos = new List<FieldInfo>();
        private static Func<Rect, SerializedProperty, GUIContent, bool> s_DefaultPropertyFieldFunc = null;
        private Dictionary<string, List<float>> m_PropertyHeights = new Dictionary<string, List<float>>();

        public void Setup(SerializedProperty property, FieldInfo fieldInfo)
        {
            m_SerializedProperty = property;
            m_FieldInfo = fieldInfo;
            if (m_Decorators == null)
                m_Decorators = fieldInfo.GetCustomAttributes(typeof(HapticGUIAttribute), false)
                    .Cast<HapticGUIAttribute>().OrderBy(s => s.order).ToList();
        }

        protected bool HasVisibleChildFields(SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Vector2:
                case SerializedPropertyType.Vector3:
                case SerializedPropertyType.Rect:
                case SerializedPropertyType.Bounds:
                    return false;
            }
            return property.hasVisibleChildren;
        }

        protected bool DefaultPropertyField(Rect position, SerializedProperty property, GUIContent label)
        {
            if (s_DefaultPropertyFieldFunc == null)
            {
                var method = typeof(EditorGUI).GetMethod("DefaultPropertyField", BindingFlags.NonPublic | BindingFlags.Static);
                s_DefaultPropertyFieldFunc = method.MakeFunc<Func<Rect, SerializedProperty, GUIContent, bool>>();
            }
            if (s_DefaultPropertyFieldFunc == null) return false;
            return s_DefaultPropertyFieldFunc(position, property, label);
        }

        public virtual float GetPropertyHeight(SerializedProperty property, GUIContent label, bool includeChildren)
        {
            float result = EditorGUI.GetPropertyHeight(property.propertyType, label);
            if (!includeChildren)
                return result;
            else
            {
                bool enter = property.isExpanded && HasVisibleChildFields(property);
                if (!enter) return result;

                var propertyHeights = m_PropertyHeights.Get(property.propertyPath);
                propertyHeights.Clear();

                var prop = property.Copy();
                var depth = prop.depth;
                while (prop.NextVisible(enter) && prop.depth > depth)
                {
                    enter = false;
                    prop.GetFieldInfos(s_FieldInfos);
                    if (s_FieldInfos[s_FieldInfos.Count - 1].GetCustomAttributes(typeof(InGroupAttribute), false).Any())
                        continue;
                    var h = EditorGUI.GetPropertyHeight(prop, null, true);
                    propertyHeights.Add(h);
                    result += h + 2f;
                }
            }
            return result;
        }

        public virtual void OnGUI(Rect position, SerializedProperty property, GUIContent label, bool includeChildren)
        {
            if (!includeChildren || !HasVisibleChildFields(property))
                DefaultPropertyField(position, property, label);
            else
            {
                var iconSize = EditorGUIUtility.GetIconSize();
                bool enabled = GUI.enabled;
                int indentLevel = EditorGUI.indentLevel;

                var propertyHeights = m_PropertyHeights.Get(property.propertyPath);
                int index = 0;

                var prop = property.Copy();
                var depth = prop.depth;
                EditorGUI.indentLevel = prop.depth + (indentLevel - depth);
                position.height = EditorGUI.GetPropertyHeight(property.propertyType, label);
                bool enter = DefaultPropertyField(position, prop, label) && HasVisibleChildFields(prop);
                if (!enter) return;

                position.y += position.height + 2f;
                while (prop.NextVisible(enter) && prop.depth > depth)
                {
                    enter = false;
                    prop.GetFieldInfos(s_FieldInfos);
                    if (s_FieldInfos[s_FieldInfos.Count - 1].GetCustomAttributes(typeof(InGroupAttribute), false).Any())
                        continue;

                    EditorGUI.indentLevel = prop.depth + (indentLevel - depth);
                    if (propertyHeights.Any())
                        position.height = propertyHeights[index];
                    else
                        position.height = EditorGUI.GetPropertyHeight(prop, null, false);
                    index++;

                    EditorGUI.BeginChangeCheck();
                    enter = EditorGUI.PropertyField(position, prop, null, false) && HasVisibleChildFields(prop);
                    if (EditorGUI.EndChangeCheck()) break;
                    position.y += position.height + 2f;
                }
                GUI.enabled = enabled;
                EditorGUIUtility.SetIconSize(iconSize);
                EditorGUI.indentLevel = indentLevel;
            }
        }
#endif
    }
}