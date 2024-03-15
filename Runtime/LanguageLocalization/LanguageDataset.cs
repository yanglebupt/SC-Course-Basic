using System.Collections.Generic;
using UnityEngine;

namespace YLBasic
{
  public class LanguageDataset<T> : ScriptableObject
  {
    public List<T> languageSentences;

    public string GetText(int id)
    {
      return GetText(id, Localization.GetLang());
    }

    public string GetText(string id)
    {
      return GetText(id, Localization.GetLang());
    }

    public string GetText(string id, string lang)
    {
      return GetText(int.Parse(id), lang);
    }

    public virtual string GetText(int id, string lang)
    {
      T sentence = languageSentences[id];
      var res = Reflect.get(sentence, lang);
      return res is ReflectNotFoundError ? (res as ReflectNotFoundError).Message : res as string;
    }
  }

  public abstract class LanguageConfig<T> : ScriptableObject
  {
    public List<T> languageConfig;

    public abstract string ConfigName
    {
      get;
    }

    public Dictionary<string, string> GetConifg(string lang)
    {
      Dictionary<string, string> config = new Dictionary<string, string>();
      foreach (T item in languageConfig)
      {
        config.Add(Reflect.get(item, ConfigName) as string, Reflect.get(item, lang) as string);
      }
      return config;
    }
  }
}