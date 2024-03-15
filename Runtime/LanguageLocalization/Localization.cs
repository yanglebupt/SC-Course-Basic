using System;
using UnityEngine;

namespace YLBasic
{
  // 请获取单例 `Instance`
  public class Localization
  {
    private string _lang;
    public string lang { get => _lang; set { _lang = value; onChange?.Invoke(value); } }

    private Action<string> onChange;

    public static void Subscribe(Action<string> watcher)
    {
      Singleton<Localization>.Instance.onChange += watcher;
    }

    public static void UnSubscribe(Action<string> watcher)
    {
      Singleton<Localization>.Instance.onChange -= watcher;
    }

    public static void SetLang(string lang)
    {
      if (lang == GetLang()) return;
      Singleton<Localization>.Instance.lang = lang;
    }

    public static string GetLang()
    {
      return Singleton<Localization>.Instance.lang;
    }

  }
}

