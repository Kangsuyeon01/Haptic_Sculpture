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



using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HapticGUI
{
    public abstract class ValidateAttribute : HapticGUIAttribute, IValidateProperty
    {
        MessageType IValidateProperty.messageType { get { return messageType; } }

        public bool indented = true;
        public MessageType messageType = MessageType.Error;
        public bool below = true;
        public float height = -1;
#if UNITY_EDITOR
        protected string m_Message;

        private DynamicValue<string> m_DynamicMessage = null;
        private Dictionary<string, Data> m_Data = new Dictionary<string, Data>();

        private class Data
        {
            public float height;
            public float boxHeight;
            public GUIContent content = null;
        }
#endif

#if UNITY_EDITOR
        protected virtual string OnProcessMessage(string message) { return message; }

        public abstract bool ValidateProperty(SerializedProperty property);

        public virtual string GetMessage(SerializedProperty property)
        {
            if (m_DynamicMessage == null)
                m_DynamicMessage = new DynamicValue<string>(m_Message, property);
            m_DynamicMessage.Update(property);
            return m_DynamicMessage.GetValue();
        }

        public string GetFinalMessage()
        {
            return OnProcessMessage(GetMessage(m_SerializedProperty));
        }

        public override float GetHeight(SerializedProperty property, GUIContent label, float height)
        {
            if (!IsVisible()) return height;
            if (ValidateProperty(property)) return height;

            var data = m_Data.Get(property.propertyPath);
            data.height = height;
            if (data.content == null)
            {
                var method = typeof(EditorGUIUtility).GetMethod("GetHelpIcon", BindingFlags.Static | BindingFlags.NonPublic);
                var icon = (Texture2D)method.Invoke(null, new object[] { (UnityEditor.MessageType)messageType });
                data.content = new GUIContent(GetFinalMessage(), icon);
            }
            data.content.text = GetFinalMessage();
            data.boxHeight = EditorStyles.helpBox.CalcSize(data.content).y;
            if (this.height >= 0)
                data.boxHeight = this.height;

            return height + data.boxHeight + 2;
        }

        public override bool BeforeGUI(ref Rect position, ref SerializedProperty property, ref GUIContent label, ref bool includeChildren, bool visible)
        {
            if (!IsVisible()) return visible;
            if (!visible) return false;

            var data = m_Data.Get(property.propertyPath);
            if (!ValidateProperty(property))
            {
                var indent = indented ? EditorGUI.IndentedRect(position) : new Rect(position);
                position.height = data.height;
                indent.height = data.boxHeight;
                if (!below)
                    position.y += data.boxHeight + 2;
                else
                    indent.y += data.height + 2;
                GUI.Label(indent, data.content, EditorStyles.helpBox);

                data.boxHeight = EditorStyles.helpBox.CalcHeight(data.content, indent.width);
            }

            return true;
        }
#endif
    }
}