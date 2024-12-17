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
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HapticGUI
{
    public class InGroupAttribute : HapticGUIAttribute
    {
#if UNITY_EDITOR
        private string[] m_Names;
        private static HashSet<string> s_GroupSet = new HashSet<string>();
        private static Stack<HashSet<string>> s_GroupStack = new Stack<HashSet<string>>();
#endif
        public InGroupAttribute(string name)
        {
#if UNITY_EDITOR
            m_Names = name.Split(',');
#endif
        }
#if UNITY_EDITOR
        public static void Add(string name)
        {
            s_GroupSet.Add(name);
        }

        public static void Remove(string name)
        {
            s_GroupSet.Remove(name);
        }

        public static void Push()
        {
            s_GroupStack.Push(s_GroupSet);
            s_GroupSet = new HashSet<string>();
        }

        public static void Pop()
        {
            s_GroupSet = s_GroupStack.Pop();
        }

        public override bool BeforeGetHeight(ref SerializedProperty property, ref GUIContent label, ref bool includeChildren)
        {
            if (!IsVisible()) return true;
            return s_GroupSet.Overlaps(m_Names);
        }

        public override float GetHeight(SerializedProperty property, GUIContent label, float height)
        {
            if (!IsVisible()) return height;
            return s_GroupSet.Overlaps(m_Names) ? height : 0;
        }

        public override bool BeforeGUI(ref Rect position, ref SerializedProperty property, ref GUIContent label, ref bool includeChildren, bool visible)
        {
            if (!IsVisible()) return visible;
            if (!visible) return false;
            return s_GroupSet.Overlaps(m_Names);
        }
#endif
    }
}