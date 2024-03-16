using System;
using System.Collections.Generic;
using YLBasic;

[Serializable]
public class LanguageSentence_2
{
  public string name;
  public string ch;
  public string en;
}

[ExcelAsset]
public class lang : LanguageDataset<LanguageSentence_2>
{
  public List<LanguageSentence_2> Sheet2;
  public override string GetText(int id, string lang)
  {
    return GetText(id, lang, Sheet2);
  }
}
