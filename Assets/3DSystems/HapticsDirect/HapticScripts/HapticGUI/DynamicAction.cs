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



#if UNITY_EDITOR
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace HapticGUI
{
    public class DynamicAction
    {
        private SerializedProperty m_SerializedProperty;

        private Type m_MethodDeclaring = null;
        private Type m_ParameterType = null;
        private Action m_Static = null;
        private Action<object> m_Instance = null;
        private Action<object> m_StaticInput = null;
        private Action<object, object> m_InstanceInput = null;

        private List<object> m_ObjectList = new List<object>();
        private List<FieldInfo> m_FieldInfoList = new List<FieldInfo>();
        private List<Func<object, object>> m_ObjectGetterList = new List<Func<object, object>>();

        public DynamicAction(string input, SerializedProperty property)
        {
            Update(property);
            ProcessInput(input);
        }

        public void Update(SerializedProperty property)
        {
            m_SerializedProperty = property;
        }

        public void DoAction()
        {
            foreach (var obj in m_SerializedProperty.serializedObject.targetObjects)
                DoAction(obj);
        }

        private void DoAction(UnityEngine.Object obj)
        {
            object self = null, declaring = null;
            if (m_SerializedProperty.GetObjectsFromGetter(m_ObjectGetterList, m_ObjectList))
            {
                declaring = m_ObjectList.FirstOrDefault(o => o != null && o.GetType().Equals(m_MethodDeclaring));
                self = m_ObjectList[m_ObjectList.Count - 1];
            }

            if (m_Static != null)
                m_Static.Invoke();
            if (declaring != null && m_Instance != null)
                m_Instance.Invoke(declaring);

            if (m_ParameterType != null && m_ParameterType.Equals(typeof(SerializedProperty)))
                self = m_SerializedProperty;
            if (self != null && m_StaticInput != null)
                m_StaticInput.Invoke(self);
            if (self != null && declaring != null && m_InstanceInput != null)
                m_InstanceInput.Invoke(declaring, self);
        }

        private int GetArrayIndex()
        {
            int index = -1;
            if (m_SerializedProperty.propertyPath.EndsWith("]"))
            {
                var path = m_SerializedProperty.propertyPath.TrimEnd(']');
                path = path.Substring(path.LastIndexOf("data[") + 5);
                int.TryParse(path, out index);
            }
            return index;
        }

        private bool CheckType(Type parameter, Type type)
        {
            return parameter.Equals(type);
        }

        private void ProcessInput(string input)
        {
            if (input == null) return;

            input = input.TrimStart('#');
            var flag = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

            bool ok1 = m_SerializedProperty.GetFieldInfos(m_FieldInfoList);
            bool ok2 = m_SerializedProperty.GetObjectGetters(m_ObjectGetterList);
            if (!ok1 || !ok2)
                return;

            var selfType = m_FieldInfoList[m_FieldInfoList.Count - 1].FieldType;
            if (selfType.IsArrayOrList())
                selfType = selfType.GetArrayOrListElementType();

            var type = m_SerializedProperty.serializedObject.targetObject.GetType();
            var method = type.GetMethod(input, flag);
            if (method != null)
            {
                var parameters = method.GetParameters();
                if (parameters.Length == 0)
                {
                    m_Static = method.MakeStaticFunc<Action>();
                    m_MethodDeclaring = method.DeclaringType;
                    return;
                }
                var type2 = parameters[0].ParameterType;
                if (parameters.Length == 1 && (type2.Equals(selfType) || type2.Equals(typeof(SerializedProperty))))
                {
                    m_StaticInput = method.MakeStaticFuncGenericInput<Action<object>>();
                    m_MethodDeclaring = method.DeclaringType;
                    m_ParameterType = type2;
                    return;
                }
            }
            foreach (var field2 in m_FieldInfoList.AsEnumerable().Reverse())
            {
                var type2 = field2.FieldType;
                if (type2.IsArrayOrList())
                    type2 = type2.GetArrayOrListElementType();
                method = type2.GetMethod(input, flag);
                if (method != null)
                {
                    var parameters = method.GetParameters();
                    if (parameters.Length == 0)
                    {
                        m_Static = method.MakeStaticFunc<Action>();
                        m_MethodDeclaring = method.DeclaringType;
                        return;
                    }
                    type2 = parameters[0].ParameterType;
                    if (parameters.Length == 1 && (type2.Equals(selfType) || type2.Equals(typeof(SerializedProperty))))
                    {
                        m_StaticInput = method.MakeStaticFuncGenericInput<Action<object>>();
                        m_MethodDeclaring = method.DeclaringType;
                        m_ParameterType = type2;
                        return;
                    }
                }
            }

            flag = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            method = type.GetMethod(input, flag);
            if (method != null)
            {
                var parameters = method.GetParameters();
                if (parameters.Length == 0)
                {
                    m_Instance = method.MakeFuncGenericThis<Action<object>>();
                    m_MethodDeclaring = method.DeclaringType;
                    return;
                }
                var type2 = parameters[0].ParameterType;
                if (parameters.Length == 1 && (type2.Equals(selfType) || type2.Equals(typeof(SerializedProperty))))
                {
                    m_InstanceInput = method.MakeFuncGenericInput<Action<object, object>>();
                    m_MethodDeclaring = method.DeclaringType;
                    m_ParameterType = type2;
                    return;
                }
            }
            foreach (var field2 in m_FieldInfoList.AsEnumerable().Reverse())
            {
                var type2 = field2.FieldType;
                if (type2.IsArrayOrList())
                    type2 = type2.GetArrayOrListElementType();
                method = type2.GetMethod(input, flag);
                if (method != null)
                {
                    var parameters = method.GetParameters();
                    if (parameters.Length == 0)
                    {
                        m_Instance = method.MakeFuncGenericThis<Action<object>>();
                        m_MethodDeclaring = method.DeclaringType;
                        return;
                    }
                    type2 = parameters[0].ParameterType;
                    if (parameters.Length == 1 && (type2.Equals(selfType) || type2.Equals(typeof(SerializedProperty))))
                    {
                        m_InstanceInput = method.MakeFuncGenericInput<Action<object, object>>();
                        m_MethodDeclaring = method.DeclaringType;
                        m_ParameterType = type2;
                        return;
                    }
                }
            }
        }
    }
}
#endif
