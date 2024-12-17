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
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HapticGUI
{
    public class ValidateValueAttribute : ValidateAttribute
    {
        public bool inverted = false;
#if UNITY_EDITOR
        private string m_Condition;
        private DynamicValue<bool> m_DynamicCondition = null;
#endif
        public ValidateValueAttribute(string message, string condition)
        {
#if UNITY_EDITOR
            m_Message = message;
            m_Condition = condition;
#endif
        }
#if UNITY_EDITOR
        protected DynamicValue<bool> GetCondition()
        {
            if (m_DynamicCondition == null)
                m_DynamicCondition = new DynamicValue<bool>(m_Condition, m_SerializedProperty);
            m_DynamicCondition.Update(m_SerializedProperty);
            return m_DynamicCondition;
        }

        public override bool ValidateProperty(SerializedProperty property)
        {
            m_SerializedProperty = property;
            var check = inverted ? false : true;
            if (!property.hasMultipleDifferentValues)
                return GetCondition().GetValue() == check;
            return GetCondition().GetValues().All(value => value == check);
        }

        protected override string OnProcessMessage(string message)
        {
            message = message.Replace("%1", m_SerializedProperty.displayName);
            return message;
        }
#endif
    }
}