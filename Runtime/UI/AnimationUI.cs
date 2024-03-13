using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEditor;

namespace YLBasic
{
  public abstract class AnimationUI : CustomUI
  {
    [Header("过渡动画属性")]
    public bool useTransition = true;
    public AnimationOptions commonOptions = new AnimationOptions()
    {
      duration = 0.3f
    };
    private List<LAnimation> anis = new List<LAnimation>();

    protected virtual void FixedUpdate()
    {
      Debug.Log(anis.Count);
      for (int i = anis.Count - 1; i >= 0; i--)  // 一边遍历，一边删除
      {
        LAnimation item = anis[i];
        if (item.Completed)
        {
          anis.Remove(item);
          continue;
        }
        item.Update(Time.fixedDeltaTime);
      }
    }

    public void StopAllAnimation()
    {
      for (int i = 0; i < anis.Count; i++)
      {
        LAnimation item = anis[i];
        item.Stop();
        anis.Remove(item);
      }
    }

    public void TransitionColor<T>(T t, Color src, Color tar, Action onCompleted) where T : MaskableGraphic
    {
      if (EditorApplication.isPlaying && useTransition)
        TransitionColor((Color c) => t.color = c, onCompleted, src, tar);
      else
        t.color = tar;
    }

    public void TransitionColor<T>(T t, Color src, Color tar) where T : MaskableGraphic
    {
      TransitionColor(t, src, tar, () => { });
    }

    public void TransitionColorTo<T>(T t, Color tar, Action onCompleted) where T : MaskableGraphic
    {
      TransitionColor(t, t.color, tar, onCompleted);
    }

    public void TransitionColorTo<T>(T t, Color tar) where T : MaskableGraphic
    {
      TransitionColorTo(t, tar, () => { });
    }

    public void TransitionColor(Action<Color> onUpdate, Action onCompleted, Color src, Color tar)
    {
      AnimationOptions opt = commonOptions;
      opt.onCompleted = onCompleted;
      LAnimation ani = new LAnimation((v) =>
      {
        onUpdate(Color.Lerp(src, tar, v));
      }, opt);
      anis.Add(ani);
      ani.Play();
    }

    public void TransitionAlpha<T>(T t, float src, float tar, Action onCompleted) where T : MaskableGraphic
    {
      if (EditorApplication.isPlaying && useTransition)
        TransitionAlpha((float c) => t.color = GetColorByAlpha(t.color, c), onCompleted, src, tar);
      else
        t.color = GetColorByAlpha(t.color, tar);
    }

    public void TransitionAlpha<T>(T t, float src, float tar) where T : MaskableGraphic
    {
      TransitionAlpha(t, src, tar, () => { });
    }

    public void TransitionAlphaTo<T>(T t, float tar, Action onCompleted) where T : MaskableGraphic
    {
      TransitionAlpha(t, t.color.a, tar, onCompleted);
    }

    public void TransitionAlphaTo<T>(T t, float tar) where T : MaskableGraphic
    {
      TransitionAlphaTo(t, tar, () => { });
    }

    public void TransitionAlpha(Action<float> onUpdate, Action onCompleted, float src, float tar)
    {
      AnimationOptions opt = commonOptions;
      opt.onCompleted = onCompleted;
      LAnimation ani = new LAnimation((v) =>
      {
        onUpdate(Mathf.Lerp(src, tar, v));
      }, opt);
      anis.Add(ani);
      ani.Play();
    }

    public Color GetColorByAlpha(Color color, float alpha)
    {
      return new Color(color.r, color.g, color.b, alpha);
    }
  }
}
