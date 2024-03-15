using System;
using UnityEngine;

namespace YLBasic
{
  [Serializable]
  public class LanguageConfig_T
  {
    public string name;  // 1
    public string ch;
    public string en;
  }

  [CreateAssetMenu(fileName = "LanguageConfig", menuName = "LanguageConfig", order = 1)]

  public class CustomLanguageConfig : LanguageConfig<LanguageConfig_T>
  {
    public override string ConfigName { get => "name"; }  // 2 需要和 1 对应起来
  }
}