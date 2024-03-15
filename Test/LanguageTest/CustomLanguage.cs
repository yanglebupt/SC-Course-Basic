using System;
using UnityEngine;

namespace YLBasic
{
  [Serializable]
  public class LanguageSentence
  {
    public string ch;
    public string en;
  }

  [CreateAssetMenu(fileName = "CustomLanguage", menuName = "LanguageDataset", order = 0)]

  public class CustomLanguage : LanguageDataset<LanguageSentence> { }

}