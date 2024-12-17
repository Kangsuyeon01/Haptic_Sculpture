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
using UnityEngine;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HapticGUI
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public abstract class HapticGUIAttribute : Attribute
    {
        public bool visible = true;
        public int order = 0;
        public string byPass = string.Empty;
#if UNITY_EDITOR
        protected SerializedProperty m_SerializedProperty = null;
        protected FieldInfo m_FieldInfo = null;

        private DynamicValue<bool> m_ByPass = null;

        public void SetSerializedProperty(SerializedProperty property)
        {
            m_SerializedProperty = property.Copy();
        }

        public void SetFieldInfo(FieldInfo fieldInfo)
        {
            m_FieldInfo = fieldInfo;
        }

        public bool IsVisible()
        {
            if (m_ByPass == null)
                m_ByPass = new DynamicValue<bool>(byPass, m_SerializedProperty);
            m_ByPass.Update(m_SerializedProperty);
            if (!string.IsNullOrEmpty(byPass))
                if (m_ByPass.GetValue()) return false;
            return visible;
        }

        public virtual bool BeforeGetHeight(ref SerializedProperty property, ref GUIContent label, ref bool includeChildren) { return true; }
        public virtual void AfterGetHeight(SerializedProperty property, GUIContent label, bool includeChildren) { }

        public virtual float GetHeight(SerializedProperty property, GUIContent label, float height)
        {
            return height;
        }

        public virtual bool BeforeGUI(ref Rect position, ref SerializedProperty property, ref GUIContent label, ref bool includeChildren, bool visible) { return true; }
        public virtual void AfterGUI(Rect position, SerializedProperty property, GUIContent label) { }
#endif
    }
}