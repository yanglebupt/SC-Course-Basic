using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace YLBasic
{
  public class QuerySelection : AnimationUI
  {
    public TMP_Text content; // 询问的文本
    public LButton yesBtn; // yes 按钮
    public LButton noBtn; // no 按钮
    private bool isTrue;
    public bool IsTrue { get => isTrue; } // 获取最终状态

    public UnityEvent OnYes; // 额外的回调，可以不用管
    public UnityEvent OnNo; // 额外的回调，可以不用管
    public UnityEvent<bool> OnSlected; // 额外的回调，可以不用管

    private CanvasGroup canvasGroup; // 用来控制是否显示的，后面会添加透明动画
    private string text_template;

    private void Awake()
    {
      // 订阅按钮点击，改变最终状态
      yesBtn.onPointClick.AddListener(() => { isTrue = true; Close(); OnSlected?.Invoke(isTrue); OnYes?.Invoke(); });
      noBtn.onPointClick.AddListener(() => { isTrue = false; Close(); OnSlected?.Invoke(isTrue); OnNo?.Invoke(); });

      // 默认状态，隐藏
      // 在gameobject挂载的代码中，不要在任何初始化周期中(包括Awake OnEnable Start)使用gameobject.SetActive(false); 不然无法用代码设置回true
      canvasGroup = GetComponent<CanvasGroup>();
      canvasGroup.alpha = 0;
    }

    public void Open()
    {
      if (gameObject.activeInHierarchy) return;
      gameObject.SetActive(true);
      TransitionAlphaTo(canvasGroup, 1);
    }

    public void Close()
    {
      if (!gameObject.activeInHierarchy) return;
      yesBtn.OnPointerExit(null);
      noBtn.OnPointerExit(null);
      TransitionAlphaTo(canvasGroup, 0, () =>
      {
        gameObject.SetActive(false);
      });
    }

    // 更新文本内容，参数是一个回调，输入字符模板，返回最终字符
    public void UpdateContentText(Func<string, string> func)
    {
      if (text_template == null)
        text_template = content.text;
      content.text = func(text_template);
    }
  }
}
