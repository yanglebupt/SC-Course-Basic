using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using System.Collections.Generic;

namespace YLBasic
{
  // 本地化组件
  public class LocalizationComponent : MonoBehaviour
  {
    private Text[] texts;
    private TMP_Text[] t_texts;

    private string[] id_texts;
    private string[] id_t_texts;
    private Regex regex = new Regex("{(.*?)}");

    [SerializeField]
    private ScriptableObject languageDataset;
    [SerializeField]
    private ScriptableObject languageConfig;
    [SerializeField]
    private bool onlySelf = false;  // 是否只收集自己的
    [SerializeField]
    private bool subscribe = true;  // 是否订阅全局的语言类型

    [SerializeField]
    [HideInInspector]
    private string lang = "en"; // 外部调整的

    private void Start()
    {
      if (subscribe)
        Localization.Subscribe(OnChange);
      UpdateTextComponents();
    }

    public void SetOnlySelf(bool onlySelf)
    {
      if (this.onlySelf == onlySelf) return;
      UpdateTextComponents(false);
    }

    public void SetSubscribe(bool subscribe)
    {
      if (this.subscribe == subscribe) return;
      if (subscribe) Localization.Subscribe(OnChange);
      else Localization.UnSubscribe(OnChange);
      this.subscribe = subscribe;
    }

    public void SetLang(string lang)
    {
      if (this.lang == lang || subscribe) return;
      OnChange(subscribe ? "" : lang);
    }

#if UNITY_EDITOR
    public void DrawEditorPreview(SerializedObject serializedObject)
    {
      serializedObject.Update();
      bool subscribe = serializedObject.FindProperty("subscribe").boolValue;
      SerializedProperty lang = serializedObject.FindProperty("lang");
      if (!subscribe)
      {
        EditorGUILayout.PropertyField(lang);
      }
      serializedObject.ApplyModifiedProperties();
    }
#endif

    public void ForEachTextComponents(Action<MaskableGraphic, int> action)
    {
      int c = texts.Length, t_c = t_texts.Length;
      int count = Mathf.Max(c, t_c);
      for (int i = 0; i < count; i++)
      {
        if (i < c) action(texts[i], i);
        if (i < t_c) action(t_texts[i], i);
      }
    }

    public bool FilterComponent(GameObject obj)
    {
      LocalizationComponent localizationCom = obj.GetComponent<LocalizationComponent>();
      return localizationCom == null;
    }

    // 如果子组件发生动态变换需要更新
    public void UpdateTextComponents(bool force = true)
    {
      texts = GetComponents<Text>();
      t_texts = GetComponents<TMP_Text>();
      if (!onlySelf)
      {
        texts = texts.Concat(
          GetComponentsInChildren<Text>().Where(component => FilterComponent(component.gameObject)).ToArray()
        ).ToArray();

        t_texts = t_texts.Concat(
          GetComponentsInChildren<TMP_Text>().Where(component => FilterComponent(component.gameObject)).ToArray()
        ).ToArray();
      }

      id_texts = new string[texts.Length];
      id_t_texts = new string[t_texts.Length];
      ForEachTextComponents((text, i) => StoreTextTemplate(text, i));
      if (force)
        OnChange(subscribe ? "" : lang);
    }

    public void StoreTextTemplate(MaskableGraphic item, int i)
    {
      if (item is Text)
      {
        id_texts[i] = (item as Text).text;
      }
      else if (item is TMP_Text)
      {
        id_t_texts[i] = (item as TMP_Text).text;
      }
    }

    public string FetchTextTemplate(MaskableGraphic item, int i)
    {
      return item is TMP_Text ? id_t_texts[i] : id_texts[i];
    }

    public void OnChange(string lang)
    {
      ForEachTextComponents((text, i) => UpdateText(text, i, lang));
    }

    public void OnChange()
    {
      ForEachTextComponents((text, i) => UpdateText(text, i));
    }

    public void UpdateText(MaskableGraphic item, int i, string lang = "")
    {
      string template = FetchTextTemplate(item, i);
      MatchEvaluator evaluator = new MatchEvaluator((Match match) => RelpaceLang(match, lang));
      string lang_content = regex.Replace(template, evaluator, -1);
      Reflect.set(item, "text", lang_content);
      Dictionary<string, string> config = Reflect.apply(languageConfig, "GetConifg", new object[] { lang }) as Dictionary<string, string>;
      // 设置语言配置
      foreach (KeyValuePair<string, string> pair in config)
      {
        if (pair.Key == "Font")
        {
          Reflect.set(item, "font", AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(pair.Value));
        }
      }
    }

    public string RelpaceLang(Match match, string lang = "")
    {
      string id = match.Groups[1].Value;
      string text = Reflect.apply(languageDataset, "GetText", lang == "" ? new object[] { id } : new object[] { id, lang }) as string;
      return text;
    }
  }
}
