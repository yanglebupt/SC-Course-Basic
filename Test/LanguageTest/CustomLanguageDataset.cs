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

  [CreateAssetMenu(fileName = "LanguageDataset", menuName = "LanguageDataset", order = 0)]

  public class CustomLanguageDataset : LanguageDataset<LanguageSentence> { }

}