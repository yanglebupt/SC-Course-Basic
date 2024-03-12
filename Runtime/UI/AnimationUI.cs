using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEditor;

namespace YLBasic
{
  public class AnimationUI : MonoBehaviour
  {
    public bool useTransition = true;
    public float transitionTime = 0.1f;

    private AnimationOptions commonOptions = new AnimationOptions();

    protected virtual void Start()
    {
      commonOptions.duration = transitionTime;
      commonOptions.fill = AnimationFill.FORWARDS;
    }

    private List<LAnimation> anis = new List<LAnimation>();
    protected virtual void FixedUpdate()
    {
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

    public void TransitionColor<T>(T t, Color src, Color tar) where T : MaskableGraphic
    {
      if (EditorApplication.isPlaying && useTransition)
        TransitionColor((Color c) => t.color = c, () => { }, src, tar);
      else
        t.color = tar;
    }

    public void TransitionColorTo<T>(T t, Color tar) where T : MaskableGraphic
    {
      if (EditorApplication.isPlaying && useTransition)
        TransitionColor((Color c) => t.color = c, () => { }, t.color, tar);
      else
        t.color = tar;
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
  }
}
