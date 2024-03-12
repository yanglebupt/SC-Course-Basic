using UnityEngine;
using UnityEditor;

namespace YLBasic
{
  [CustomEditor(typeof(LTextAnimator))]
  public class LTextAnimatorEditor : Editor
  {
    bool expanded = true;
    public override void OnInspectorGUI()
    {
      base.OnInspectorGUI();
      if (expanded)
      {
        GUI.contentColor = new Color(0.9f, 0.8f, 0.7f);
        if (GUILayout.Button("Hide efffct command"))
        {
          expanded = false;
        }
        GUI.contentColor = Color.white;
        GUILayout.Label("To use command: <effectName k=v k=v1,v2>your text</effectName>");
        GUILayout.Label("1. Wave: <wave d=time h=height>");
        GUILayout.Label("2. Banner: <banner d=time c=r,g,b,a>");
        GUILayout.Label("4. Twinkle: <twinkle d=time a=alpha>");
      }
      else
      {
        GUI.contentColor = new Color(0.9f, 0.8f, 0.7f);
        if (GUILayout.Button("View effect commands"))
        {
          expanded = true;
        }
        GUI.contentColor = Color.white;
      }
    }
  };
}


