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
using UnityEngine.Profiling;
using System;
using System.Linq;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HapticGUI
{
    public class LabelAttribute : StyledDecoratorAttribute
    {
#if UNITY_EDITOR
        protected override string m_defaultStyle { get { return "label"; } }

        private float m_LabelWidth = -2;
        private float m_SavedLabelWidth;
#endif
        public LabelAttribute()
        {
#if UNITY_EDITOR
#endif
        }

        public LabelAttribute(float width)
        {
#if UNITY_EDITOR
            m_LabelWidth = width;
#endif
        }
#if UNITY_EDITOR
        private void ProcessLabel(ref GUIContent label)
        {
            var content = m_Content;
            if (title == null)
                content.text = label.text;
            if (icon == null)
                content.image = label.image;
            if (tooltip == null)
                content.tooltip = label.tooltip;
            label = content;
        }

        public override bool BeforeGetHeight(ref SerializedProperty property, ref GUIContent label, ref bool includeChildren)
        {
            if (!IsVisible()) return true;
            ProcessLabel(ref label);
            return true;
        }

        public override bool BeforeGUI(ref Rect position, ref SerializedProperty property, ref GUIContent label, ref bool includeChildren, bool visible)
        {
            m_SavedLabelWidth = EditorGUIUtility.labelWidth;
            if (!IsVisible()) return visible;

            ProcessLabel(ref label);

            if (m_LabelWidth == 0)
                label = GUIContent.none;
            else if (m_LabelWidth == -2)
                EditorGUIUtility.labelWidth = 0;
            else if (m_LabelWidth == -1)
                EditorGUIUtility.labelWidth = m_Style.CalcSize(label).x + 2 + (position.indent().x - position.x);
            else if (m_LabelWidth > 0 && m_LabelWidth <= 1)
                EditorGUIUtility.labelWidth = m_LabelWidth * position.width;
            else
                EditorGUIUtility.labelWidth = m_LabelWidth;
            return visible;
        }

        public override void AfterGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUIUtility.labelWidth = m_SavedLabelWidth;
        }
#endif
    }

    // NC

    public abstract class ConditionalAttribute : HapticGUIAttribute
    {
        public bool inverted = false;
#if UNITY_EDITOR
        private bool? m_Static = null;
        private string m_Condition;
        private DynamicValue<bool> m_DynamicCondition = null;
#endif
        public ConditionalAttribute(bool condition)
        {
#if UNITY_EDITOR
            m_Static = condition;
#endif
        }
        public ConditionalAttribute(string condition)
        {
#if UNITY_EDITOR
            m_Condition = condition;
#endif
        }
#if UNITY_EDITOR
        protected DynamicValue<bool> GetCondition()
        {
            if (m_DynamicCondition == null)
            {
                if (m_Static == null)
                    m_DynamicCondition = new DynamicValue<bool>(m_Condition, m_SerializedProperty);
                else
                    m_DynamicCondition = new DynamicValue<bool>(m_Static.Value);
            }
            m_DynamicCondition.Update(m_SerializedProperty);
            return m_DynamicCondition;
        }

        protected bool MatchAll()
        {
            var check = inverted ? false : true;
            if (!m_SerializedProperty.hasMultipleDifferentValues)
                return GetCondition().GetValue() == check;
            return GetCondition().GetValues().All(value => value == check);
        }

        protected bool MatchAny()
        {
            var check = inverted ? false : true;
            if (!m_SerializedProperty.hasMultipleDifferentValues)
                return GetCondition().GetValue() == check;
            return GetCondition().GetValues().Any(value => value == check);
        }
#endif
    }

    //NC




    public class EnableIfAttribute : ConditionalAttribute
    {
        public bool enable = true;
        public bool disable = true;
        public bool all = true;
#if UNITY_EDITOR
        private bool m_SavedEnabled;
#endif
        public EnableIfAttribute(bool condition) : base(condition)
        {
#if UNITY_EDITOR
#endif
        }

        public EnableIfAttribute(string condition) : base(condition)
        {
#if UNITY_EDITOR
#endif
        }
#if UNITY_EDITOR
        public override float GetHeight(SerializedProperty property, GUIContent label, float height)
        {
            return height;
        }

        public override bool BeforeGUI(ref Rect position, ref SerializedProperty property, ref GUIContent label, ref bool includeChildren, bool visible)
        {
            if (!IsVisible()) return visible;
            if (!visible) return false;
            m_SavedEnabled = GUI.enabled;
            var condition = all ? MatchAll() : MatchAny();
            if (condition && enable)
                GUI.enabled = true;
            if (!condition && disable)
                GUI.enabled = false;
            return visible;
        }

        public override void AfterGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.enabled = m_SavedEnabled;
        }
#endif
    }

    //---

    public class GroupAttribute : HapticGUIAttribute
    {
        public bool indented = false;
        public bool indentChildren = false;
        public float spacing = 2;
        public bool fixedCell = true;
#if UNITY_EDITOR
        private string m_Name;
        private bool m_Children = false;
        private string[] m_Properties;
        private int m_PropertyCount;
        private float[] m_Heights;
        private int m_SavedIndentLevel;

        private Dictionary<string, Data> m_Data = new Dictionary<string, Data>();

        private class Data
        {
            public List<SerializedProperty> properties = new List<SerializedProperty>();
            public List<float> propertyHeights = new List<float>();
            public List<float> rowHeights = new List<float>();
        }
#endif
        public GroupAttribute(string name, bool children, string properties, params float[] heights)
        {
#if UNITY_EDITOR
            m_Name = name;
            m_Children = children;
            m_Properties = properties.Split(',');
            m_PropertyCount = 0;
            m_Heights = heights;
#endif
        }

        public GroupAttribute(string name, int properties, params float[] heights)
        {
#if UNITY_EDITOR
            m_Name = name;
            m_Children = false;
            m_Properties = new string[] { };
            m_PropertyCount = properties;
            m_Heights = heights;
#endif
        }
#if UNITY_EDITOR
        private void GetProperties(List<SerializedProperty> list)
        {
            if (m_PropertyCount > 0)
                m_SerializedProperty.GetProperties(m_PropertyCount, list);
            else if (m_Children)
                m_SerializedProperty.GetChildrenProperties(m_Properties, list);
            else
                m_SerializedProperty.GetProperties(m_Properties, list);
        }

        public override float GetHeight(SerializedProperty property, GUIContent label, float height)
        {
            if (!IsVisible()) return height;

            var data = m_Data.Get(property.propertyPath);
            GetProperties(data.properties);
            data.propertyHeights.Clear();
            data.rowHeights.Clear();

            var heights = m_Heights.AsEnumerable();
            int diff = data.properties.Count + 1 - m_Heights.Length;
            if (diff > 0) heights = heights.Concat(Enumerable.Repeat(-1f, diff));
            heights = heights.Take(data.properties.Count + 1);

            int index = -1;
            float result = 0;
            InGroupAttribute.Add(m_Name);
            foreach (var h in heights)
            {

                if (index < 0)
                    height = h >= 0 ? h : height;
                else
                {
                    var propertyHeight = data.properties[index] == null ? 0 : EditorGUI.GetPropertyHeight(data.properties[index], null, true);
                    data.propertyHeights.Add(propertyHeight);

                    if (h >= 0)
                        height = h;
                    else
                        height = propertyHeight;
                }

                if (height < 0) height = 0;
                data.rowHeights.Add(height);
                if (result > 0 && height > 0) result += spacing;
                result += height;
                index++;
            }
            InGroupAttribute.Remove(m_Name);
            return result;
        }

        public override bool BeforeGUI(ref Rect position, ref SerializedProperty property, ref GUIContent label, ref bool includeChildren, bool visible)
        {
            m_SavedIndentLevel = EditorGUI.indentLevel;
            if (!IsVisible()) return visible;
            if (!visible) return false;
            if (Event.current.type == EventType.Layout) return visible;

            var text = label.text;
            var image = label.image;
            var tooltip = label.tooltip;

            var rect = position.indent(indented);

            var data = m_Data.Get(property.propertyPath);
            GetProperties(data.properties);

            int index = -1;
            if (indentChildren)
                EditorGUI.indentLevel++;
            InGroupAttribute.Add(m_Name);
            foreach (var cell in rect.VerticalDistribute(spacing, data.rowHeights))
            {
                if (index < 0)
                    position = cell;
                else if (data.properties[index] != null && cell.height > 0)
                    EditorGUI.PropertyField(cell.Height(data.propertyHeights[index]), data.properties[index], null, true);
                index++;
            }
            InGroupAttribute.Remove(m_Name);

            label.text = text;
            label.image = image;
            label.tooltip = tooltip;
            return position.height > 0;
        }

        public override void AfterGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.indentLevel = m_SavedIndentLevel;
        }
#endif
    }

    //--

    public class FoldoutAttribute : Styled2DecoratorAttribute
    {
        public bool indented = true;
        public bool hierarchyMode = true;
        public bool indentChildren = true;
#if UNITY_EDITOR
        protected override string m_defaultStyle { get { return "Foldout"; } }
        protected override string m_defaultStyle2 { get { return "label"; } }

        private int m_SavedIndentLevel;
#endif
        public FoldoutAttribute()
        {
#if UNITY_EDITOR
#endif
        }
#if UNITY_EDITOR
        public override float GetHeight(SerializedProperty property, GUIContent label, float height)
        {
            if (!IsVisible()) return height;
            height += EditorGUIUtility.singleLineHeight + 2;
            if (!property.isExpanded)
                height = EditorGUIUtility.singleLineHeight;
            return height;
        }

        public override bool BeforeGUI(ref Rect position, ref SerializedProperty property, ref GUIContent label, ref bool includeChildren, bool visible)
        {
            m_SavedIndentLevel = EditorGUI.indentLevel;
            if (!IsVisible()) return visible;
            if (!visible) return false;
            if (Event.current.type == EventType.Layout) return visible;

            var hierarchyMode = EditorGUIUtility.hierarchyMode;
            EditorGUIUtility.hierarchyMode = this.hierarchyMode;

            var h = EditorGUIUtility.singleLineHeight;
            var indent = position.indent(indented);
            GUI.Label(indent.Height(h), GUIContent.none, m_Style2);

            property.isExpanded = EditorGUI.Foldout(position.Height(h), property.isExpanded, m_Content, true, m_Style);

            position.yMin += h + 2;

            EditorGUIUtility.hierarchyMode = hierarchyMode;
            if (indentChildren && property.isExpanded)
                EditorGUI.indentLevel++;

            return property.isExpanded;
        }

        public override void AfterGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.indentLevel = m_SavedIndentLevel;
        }
#endif
    }

    //--

    public class ShowIfAttribute : ConditionalAttribute
    {
        public bool enable = true;
        public bool disable = true;
        public bool all = true;
#if UNITY_EDITOR
#endif
        public ShowIfAttribute(bool condition) : base(condition)
        {
#if UNITY_EDITOR
#endif
        }

        public ShowIfAttribute(string condition) : base(condition)
        {
#if UNITY_EDITOR
#endif
        }
#if UNITY_EDITOR
        public override bool BeforeGetHeight(ref SerializedProperty property, ref GUIContent label, ref bool includeChildren)
        {
            if (!IsVisible()) return true;
            var condition = all ? MatchAll() : MatchAny();
            if (!condition && disable)
                return false;
            return true;
        }

        public override float GetHeight(SerializedProperty property, GUIContent label, float height)
        {
            if (!IsVisible()) return height;
            var condition = all ? MatchAll() : MatchAny();
            if (!condition && disable)
                return 0;
            return height;
        }

        public override bool BeforeGUI(ref Rect position, ref SerializedProperty property, ref GUIContent label, ref bool includeChildren, bool visible)
        {
            if (!IsVisible()) return visible;
            if (!visible) return false;
            var condition = all ? MatchAll() : MatchAny();
            if (!condition && disable)
                visible = false;
            return visible;
        }
#endif
    }

    //--

    public class LabelOnlyAttribute : StackableFieldAttribute
    {
#if UNITY_EDITOR
        private static GUIStyle s_Style = null;
#endif
        public LabelOnlyAttribute()
        {
#if UNITY_EDITOR
#endif
        }
#if UNITY_EDITOR
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label, bool includeChildren)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label, bool includeChildren)
        {
            if (s_Style == null)
            {
                s_Style = new GUIStyle("label");
                s_Style.padding = new RectOffset();
                s_Style.alignment = TextAnchor.MiddleLeft;
            }
            GUI.Label(position, label, s_Style);
        }
#endif
    }

    //--

    public class SliderAttribute : StackableFieldAttribute
    {
        public bool showField = true;
#if UNITY_EDITOR
        private float m_Min;
        private float m_Max;
#endif
        public SliderAttribute(float min, float max)
        {
#if UNITY_EDITOR
            m_Min = min;
            m_Max = max;
#endif
        }
#if UNITY_EDITOR
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label, bool includeChildren)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label, bool includeChildren)
        {
            if (property.propertyType != SerializedPropertyType.Float && property.propertyType != SerializedPropertyType.Integer)
            {
                EditorGUI.LabelField(position, label.text, "Use with float or int.");
                return;
            }

            if (showField)
            {
                if (property.propertyType == SerializedPropertyType.Float)
                    EditorGUI.Slider(position, property, m_Min, m_Max, label);
                if (property.propertyType == SerializedPropertyType.Integer)
                    EditorGUI.IntSlider(position, property, (int)m_Min, (int)m_Max, label);
                return;
            }

            label = EditorGUI.BeginProperty(position, label, property);
            if (property.propertyType == SerializedPropertyType.Float)
            {
                var value = GUI.HorizontalSlider(position, property.floatValue, m_Min, m_Max);
                if (value != property.floatValue)
                    property.floatValue = value;
            }
            if (property.propertyType == SerializedPropertyType.Integer)
            {
                var value = Mathf.RoundToInt(GUI.HorizontalSlider(position, property.intValue, m_Min, m_Max));
                if (value != property.intValue)
                    property.intValue = value;
            }
            EditorGUI.EndProperty();
        }
#endif
    }

    //--

    public class EnumMaskButtonAttribute : StackableFieldAttribute
    {
        public string styles = null;
        public bool all = true;
        public string exclude = string.Empty;
        public int column = -1;
        public int hOffset = 0;
        public int vOffset = 0;
#if UNITY_EDITOR
        private ButtonMask m_ButtonMask = null;
#endif
        public EnumMaskButtonAttribute()
        {
#if UNITY_EDITOR
#endif
        }
#if UNITY_EDITOR
        private ButtonMask GetButtonMask()
        {
            if (m_ButtonMask == null)
            {
                var type = m_FieldInfo.FieldType;
                type = type.IsArrayOrList() ? type.GetArrayOrListElementType() : type;

                var names = Enum.GetNames(type).Except(exclude.Split(',')).ToList();
                var values = names.Select(n => Convert.ToInt64(Enum.Parse(type, n))).ToList();
                int index;
                while ((index = values.IndexOf(0)) >= 0)
                {
                    names.RemoveAt(index);
                    values.RemoveAt(index);
                }
                m_ButtonMask = new ButtonMask(names.ToArray(), values.ToArray(), all, styles == null ? EditorStyles.miniButton.name : styles);
            }
            m_ButtonMask.hOffset = hOffset;
            m_ButtonMask.vOffset = vOffset;
            return m_ButtonMask;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label, bool includeChildren)
        {
            var height = base.GetPropertyHeight(property, label, includeChildren);
            if (property.propertyType != SerializedPropertyType.Enum) return height;
            return Mathf.Max(height, GetButtonMask().GetButtonSize(column).y);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label, bool includeChildren)
        {
            if (property.propertyType != SerializedPropertyType.Enum)
            {
                EditorGUI.LabelField(position, label.text, "Use with Enum.");
                return;
            }

            if (column == -1) column = GetButtonMask().GetCount();

            label = EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, label);
            position.height = GetButtonMask().GetButtonSize(column).y;
            int longValue = (int)GetButtonMask().Draw(position, property.longValue, column);
            if (longValue != property.longValue)
                property.longValue = longValue;
            EditorGUI.EndProperty();
        }
#endif
    }

    //--

    public class ColorAttribute : HapticGUIAttribute
    {
#if UNITY_EDITOR
        private Color m_Color;
        private Color m_GUIColor;
#endif
        public ColorAttribute(float r, float g, float b, float a)
        {
#if UNITY_EDITOR
            m_Color = new Color(r, g, b, a);
#endif
        }
#if UNITY_EDITOR
        public override bool BeforeGUI(ref Rect position, ref SerializedProperty property, ref GUIContent label, ref bool includeChildren, bool visible)
        {
            m_GUIColor = GUI.color;
            if (!IsVisible()) return visible;
            GUI.color = m_Color;
            return visible;
        }

        public override void AfterGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.color = m_GUIColor;
        }
#endif
    }

    //--

    public class ColorFieldAttribute : StackableFieldAttribute
    {
        public bool showEyedropper = true;
        public bool showAlpha = true;
        public bool hdr = false;
        public float minBrightness = 0f;
        public float maxBrightness = 8f;
        public float minExposureValue = 0.125f;
        public float maxExposureValue = 3f;
#if UNITY_EDITOR
        //private ColorPickerHDRConfig m_HDRConfig = null;
#endif
        public ColorFieldAttribute()
        {
#if UNITY_EDITOR
#endif
        }
#if UNITY_EDITOR
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label, bool includeChildren)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label, bool includeChildren)
        {
            if (property.propertyType != SerializedPropertyType.Color)
            {
                EditorGUI.LabelField(position, label.text, "Use with Color.");
                return;
            }

            EditorGUI.BeginChangeCheck();
            Color colorValue;
#if UNITY_2018_1_OR_NEWER
            colorValue = EditorGUI.ColorField(position, label, property.colorValue, showEyedropper, showAlpha, hdr);
#else
            if (m_HDRConfig == null)
                m_HDRConfig = new ColorPickerHDRConfig(minBrightness, maxBrightness, minExposureValue, maxExposureValue);
            colorValue = EditorGUI.ColorField(position, label, property.colorValue, showEyedropper, showAlpha, hdr, m_HDRConfig);
#endif
            if (EditorGUI.EndChangeCheck())
                property.colorValue = colorValue;
        }
#endif
    }

    //--

    public class RangeSliderAttribute : StackableFieldAttribute
    {
        public bool integer = false;
        public bool showInLabel = false;
#if UNITY_EDITOR
        private float m_Min;
        private float m_Max;
#endif
        public RangeSliderAttribute(float min, float max)
        {
#if UNITY_EDITOR
            m_Min = min;
            m_Max = max;
#endif
        }
#if UNITY_EDITOR
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label, bool includeChildren)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label, bool includeChildren)
        {
            Vector2 value;
#if UNITY_2017_2_OR_NEWER
            if (property.propertyType == SerializedPropertyType.Vector2Int)
            {
                integer = true;
                value = property.vector2IntValue;
            }
            else if (property.propertyType == SerializedPropertyType.Vector2)
#else
            if (property.propertyType == SerializedPropertyType.Vector2)
#endif
            {
                value = property.vector2Value;
            }
            else
            {
                EditorGUI.LabelField(position, label.text, "Use with Vector2 or Vector2Int.");
                return;
            }

            if (showInLabel)
            {
                label.text += " " + value.ToString(integer ? "0" : "0.00");
            }
            label = EditorGUI.BeginProperty(position, label, property);
            EditorGUI.MinMaxSlider(position, label, ref value.x, ref value.y, m_Min, m_Max);
            if (integer)
            {
                value.x = Mathf.RoundToInt(value.x);
                value.y = Mathf.RoundToInt(value.y);
            }
#if UNITY_2017_2_OR_NEWER
            if (property.propertyType == SerializedPropertyType.Vector2Int)
                if (Vector2Int.RoundToInt(value) != property.vector2IntValue)
                    property.vector2IntValue = Vector2Int.RoundToInt(value);
#endif
            if (property.propertyType == SerializedPropertyType.Vector2)
            {
                if (value != property.vector2Value)
                    property.vector2Value = value;
            }
            EditorGUI.EndProperty();
        }
#endif
    }
}