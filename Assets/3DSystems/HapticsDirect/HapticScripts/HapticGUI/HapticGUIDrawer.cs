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
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace HapticGUI
{
    [CustomPropertyDrawer(typeof(StackableFieldAttribute), true)]
    public class HapticGUIDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var drawrer = (StackableFieldAttribute)attribute;
            drawrer.Setup(property, fieldInfo);

            bool includeChildren = true, visible = true;
            foreach (var attr in drawrer.decorators.AsEnumerable().Reverse())
            {
                attr.SetSerializedProperty(property);
                attr.SetFieldInfo(fieldInfo);
                attr.visible = visible;
                if (!attr.BeforeGetHeight(ref property, ref label, ref includeChildren))
                    visible = false;
            }

            float height = 0;
            if (visible)
                height = drawrer.GetPropertyHeight(property, label, includeChildren);

            foreach (var attr in drawrer.decorators)
            {
                height = attr.GetHeight(property, label, height);
                attr.AfterGetHeight(property, label, includeChildren);
            }

            if (height == 0)
                height = -EditorGUIUtility.standardVerticalSpacing;
            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (position.height <= 0) return;
#if !NET_4_6
//            UnityEditor.PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Standalone, ApiCompatibilityLevel.NET_4_6);
#endif
            var drawrer = (StackableFieldAttribute)attribute;
            drawrer.Setup(property, fieldInfo);

            bool includeChildren = true, visible = true;
            foreach (var attr in drawrer.decorators.AsEnumerable().Reverse())
            {
                attr.SetSerializedProperty(property);
                attr.SetFieldInfo(fieldInfo);
                attr.visible = visible;
                visible = attr.BeforeGUI(ref position, ref property, ref label, ref includeChildren, visible);
            }

            if (visible)
                drawrer.OnGUI(position, property, label, includeChildren);

            foreach (var attr in drawrer.decorators)
                attr.AfterGUI(position, property, label);
        }

#if UNITY_2018_1_OR_NEWER && UNITY_EDITOR
        public override bool CanCacheInspectorGUI(SerializedProperty property)
        {
            if (attribute is INoCacheInspectorGUI) return false;
            var drawrer = (StackableFieldAttribute)attribute;
            drawrer.Setup(property, fieldInfo);
            return !drawrer.decorators.OfType<INoCacheInspectorGUI>().Any();
        }
#endif
    }
}
#endif