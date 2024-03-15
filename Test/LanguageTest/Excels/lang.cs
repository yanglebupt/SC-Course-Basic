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
  public List<LanguageSentence_2> Sheet1;

  public override string GetText(int id, string lang)
  {
    LanguageSentence_2 sentence = Sheet1[id];
    var res = Reflect.get(sentence, lang);
    return res is ReflectNotFoundError ? (res as ReflectNotFoundError).Message : res as string;
  }
}
