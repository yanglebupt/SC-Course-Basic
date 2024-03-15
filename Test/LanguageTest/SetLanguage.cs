using UnityEngine;

namespace YLBasic
{
  public class SetLanguage : MonoBehaviour
  {
    public string lang = "en";
    void Update()
    {
      Localization.SetLang(lang);
      gameObject.name = lang;
    }
  }
}

