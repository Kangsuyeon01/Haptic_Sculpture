using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class HelpWindow : MonoBehaviour
    {
        public string m_Title;
        [TextArea(minLines: 10, maxLines: 50)]
        public string m_Description;

        public bool mShowingHelpWindow = true;
        private const float kPadding = 40f;
        GUIStyle style;

    private void OnGUI()
        {
        GUISkin oldSkin = GUI.skin;
        GUI.skin = Resources.Load<GUISkin>("Skin/CustomGUI");
        //GUI.skin = oldSkin;
        if (mShowingHelpWindow)
            {
                Vector2 size = GUI.skin.label.CalcSize(new GUIContent(m_Description));
                Vector2 halfSize = size * 0.5f;

                float maxWidth = Mathf.Min(Screen.width - kPadding, size.x);
                float left = Screen.width * 0.5f - maxWidth * 0.5f;
                float top = Screen.height * 0.4f - halfSize.y;

                Rect windowRect = new Rect(left, top, maxWidth, size.y);
                
                GUILayout.Window(400, windowRect, (id) => DrawWindow(id, maxWidth), m_Title);

            }
        }

        private void DrawWindow(int id, float maxWidth)
        {
        
            GUILayout.BeginVertical(GUI.skin.box);
             
            GUILayout.Label(m_Description);
            GUILayout.EndVertical();
            if (GUILayout.Button("Got it!"))
            {
                mShowingHelpWindow = false;
            }
        }
    }

