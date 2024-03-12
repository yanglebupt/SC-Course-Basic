using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text.RegularExpressions;
using System;

namespace YLBasic
{
  /// <summary>
  /// 记录不同 inline effects 的 mark 标记字符串和正则匹配模板
  /// </summary>
  public static class InLineEffectMarkTools
  {
    public static readonly Dictionary<InlineEffectMarkType, string> dic = new Dictionary<InlineEffectMarkType, string>()
  {
    {InlineEffectMarkType.None, "none"},
    {InlineEffectMarkType.Banner , "banner"},
    {InlineEffectMarkType.Twinkle ,"twinkle"},
    {InlineEffectMarkType.Dangle , "dangle"},
    {InlineEffectMarkType.Excl , "excl"},
    {InlineEffectMarkType.Wave , "wave"},
  };
    public static readonly InlineEffectMarkType[] li = (InlineEffectMarkType[])Enum.GetValues(typeof(InlineEffectMarkType));
    public static string GetMarkString(InlineEffectMarkType type)
    {
      return dic[type];
    }
    public static Regex GetMarkRegex(InlineEffectMarkType type)
    {
      string mark = dic[type];
      return new Regex($"<{mark}.*?>(.*?)</{mark}.*?>");
    }

    public static Dictionary<string, string> ParseMarkParams(InlineEffectMarkType type, string matchString)
    {
      string mark = dic[type];
      Regex regex = new Regex($"^<{mark}\\s+(.*?)>");
      Match match = regex.Match(matchString);
      Dictionary<string, string> paramDict = new Dictionary<string, string>();
      if (regex.IsMatch(matchString))
      {
        string[] param = match.Groups[1].Value.Split(" ");
        foreach (string item in param)
        {
          string[] keyValuePair = item.Split("=");
          paramDict.Add(keyValuePair[0], keyValuePair[1]);
        }
      }
      return paramDict;
    }

    public static void Test()
    {
      string str = "The <banner>character</banner> with <banner>value</banner> was not found in the <twinkle>font asset</twinkle> or any potential fallbacks";
      Regex regex = GetMarkRegex(InlineEffectMarkType.Banner);
      MatchCollection res = regex.Matches(str);
      if (regex.IsMatch(str))
      {
        foreach (Match match in res)
        {
          Debug.Log(match.Groups[0].Value);
          Debug.Log(match.Groups[1].Value);
        }
      }
    }
  }

  public struct InlineMatchIndex : IComparable
  {
    public InlineEffectMarkType type;
    public int markStart;
    public int markEnd;
    public int contentStart;
    public int contentEnd;
    public string content;
    public string mark;
    public Dictionary<string, string> paramDict;

    public int CompareTo(object obj)
    {
      return markStart > ((InlineMatchIndex)obj).markStart ? 1 : -1;
    }
  }

  public enum InlineEffectMarkType
  {
    None,
    Banner,
    Twinkle,
    Wave,
    Dangle,
    Excl,
  }

  public enum InlineEffectHook
  {
    One,  // 一次单词依次执行
    Mark, // 等一个 mark 渲染完成执行
    All,  // 整句话完成
  }

  public enum TypeWriterType
  {
    NORMAL, // 一个个显示
    AlphaFade, // 透明度渐变显示
    Wave, // 波浪显示，一个一个掉下来
    Scale, // 缩放从大到正常
    Rotate,  // 旋转显示
    Stretch, // 拉伸显示
    Noise,
    // Shake // 上下左右抖动 
  }

  public enum DurationType
  {
    RunningTime,  // 打字机动画显示全部文字的时间
    PerSecond // 每秒多少个字符
  }


  /// <summary>
  /// 实现打字机文字动画（TypeWriter）和内联文字动画 (Inline Effect)
  /// </summary>
  public class LTextAnimator : MonoBehaviour
  {
    public TMP_Text tmp;
    #region Fields
    [Header("TypeWriter")]
    [Tooltip("是否使用打字机动画")]
    public bool useTypeWriter = true;
    [Tooltip("使用哪种时间类型")]
    public DurationType durationType = DurationType.RunningTime;
    [Tooltip("打字机动画显示全部文字的时间")]
    public float typeDuration = 10f;
    [Tooltip("每秒多少个字符")]
    public float wordPerSecond = 5f;
    [Tooltip("打字机动画类型")]
    public TypeWriterType typeWriterType = TypeWriterType.AlphaFade;
    [Tooltip("任意一种效果都保留 alphaFade 的效果")]
    public bool keepAlphaFade = true;
    [Tooltip("Wave 高度")]
    public float waveHeight = 30;
    [Tooltip("Scale 比例")]
    public float scaleRate = 2;
    [Tooltip("Rotate 角度")]
    public float rotateAngle = 45;
    [Tooltip("Stretch 幅度")]
    public float stretchAmplitude = 10;
    public float noiseAmplitude = 20f;

    [Tooltip("Shake 幅度")]
    public float shakeAmplitude = 0.5f;

    [Header("Inline Effects")]
    [Tooltip("是否使用内联文字动画")]
    public bool useInlineEffects;
    [Tooltip("Inline Effect 每个字符默认动画时间")]
    public float transitionTime = 0.5f;
    [Tooltip("是否将 mark 替换成空格")]
    public bool useInlineSpace;
    [Tooltip("什么时候触发 Inline Effects")]
    public InlineEffectHook inlineEffectWhen = InlineEffectHook.Mark;
    #endregion

    private TMP_TextInfo textInfo;
    private AnimationOptions commonAnimationOptions;
    private LAnimationCoroutineGroup LAnimationCoroutineGroup = new LAnimationCoroutineGroup();
    private bool skipTypeWrite = false;
#nullable enable
    private Coroutine? typeWriterCoroutine = null;
#nullable disable

    private void Start()
    {
      if (tmp == null)
      {
        tmp = GetComponent<TMP_Text>();
      }
      textInfo = tmp.textInfo;
      commonAnimationOptions = new AnimationOptions()
      {
        duration = transitionTime,
        delay = 0f,
        delayInIterations = false,
        direction = AnimationDirection.NORMAL,
        curve = LCurveType.Quadratic,
        fill = AnimationFill.FORWARDS,
        removeOnCompleted = true,
        iterations = 1,
        onCompleted = () => { },
        onWaitDelay = () => { },
        onCompletedParam = (LAnimationStateStruct state) => { },
        onWaitDelayParam = (LAnimationStateStruct state) => { },
        onFrameDict = new Dictionary<float, Action>(),
        onFrameDictParam = new Dictionary<float, Action<LAnimationStateStruct>>(),
      };
    }

    private void Update()
    {
      if (Input.GetKeyDown(KeyCode.Q))
      {
        skipTypeWrite = false;
        if (typeWriterCoroutine != null)
          StopCoroutine(typeWriterCoroutine);
        LAnimationCoroutineGroup.StopAndRemoveAllC();
        ApplyTypeWriter(typeWriterType, useInlineEffects);
      }
      else if (Input.GetKeyDown(KeyCode.O))
      {
        skipTypeWrite = true;
      }
    }

    public void ApplyTypeWriter(TypeWriterType type, bool useInlineEffects)
    {
      string str = tmp.text;
      for (int i = 0; i < textInfo.materialCount; i++)
      {
        textInfo.meshInfo[i].Clear();
      }
      tmp.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);  // 删除 mesh 中的全部顶点
      List<InlineMatchIndex> inlineEffects = new List<InlineMatchIndex>();
      int markCharacterCount = 0;
      if (useInlineEffects)
      {
        // 先进行 inline mark 的识别
        foreach (InlineEffectMarkType inlineType in InLineEffectMarkTools.li)
        {
          Regex regex = InLineEffectMarkTools.GetMarkRegex(inlineType);
          MatchCollection res = regex.Matches(str);
          if (regex.IsMatch(str))
          {
            foreach (Match match in res)
            {
              int markStart = match.Groups[0].Index;
              int contentStart = match.Groups[1].Index;
              string mark = match.Groups[0].Value;
              string content = match.Groups[1].Value;
              int markEnd = markStart + mark.Length - 1;
              int contentEnd = contentStart + content.Length - 1;
              markCharacterCount += mark.Length - content.Length;
              InlineMatchIndex inlineMatchIndex = new InlineMatchIndex()
              {
                markStart = markStart,
                contentStart = contentStart,
                markEnd = markEnd,
                contentEnd = contentEnd,
                type = inlineType,
                content = content,
                mark = mark,
                paramDict = InLineEffectMarkTools.ParseMarkParams(inlineType, mark)
              };
              inlineEffects.Add(inlineMatchIndex);
            }
          }

          // 需要排序，可以减少后面遍历数量
          inlineEffects.Sort((InlineMatchIndex a, InlineMatchIndex b) => a.CompareTo(b));
        }
      }
      // 计算字符显示间距时间需要等多少帧，先计算有效渲染字符数
      (bool IsWordCount, int count) = GetWordCountPerFrame(Time.fixedDeltaTime, textInfo.characterCount - markCharacterCount);
      typeWriterCoroutine = StartCoroutine(TypeWriter(IsWordCount, count, type, inlineEffects, useInlineSpace, inlineEffectWhen));
    }

    /// <summary>
    /// 计算一帧内显示多少个字符，如果小于 1，则代表多帧显示一个字符
    /// </summary>
    /// <param name="frameTime"></param>
    /// <param name="wordCounts"></param>
    /// <returns>(bool true 一帧显示多个字符，false 多帧显示一个字符，字符数或者帧数)</returns>
    public (bool, int) GetWordCountPerFrame(float frameTime, int wordCounts)
    {
      float wps = durationType == DurationType.PerSecond ? wordPerSecond : wordCounts * 1.0f / typeDuration;
      float wordInFrame = wps * frameTime;
      return (wordInFrame > 1, Mathf.CeilToInt(wordInFrame > 1 ? wordInFrame : 1.0f / wordInFrame));
    }

    public InlineMatchIndex? GetInlineMatchIndex(List<InlineMatchIndex> inlineEffects, int i, int start = 0)
    {
      for (int j = start; j < inlineEffects.Count; j++)
      {
        InlineMatchIndex inlineMatchIndex = inlineEffects[j];
        if (i == inlineMatchIndex.markStart)
        {
          return inlineMatchIndex;
        }
      }
      return null;
    }

    public IEnumerator TypeWriter(bool IsWordCount, int count, TypeWriterType type, List<InlineMatchIndex> inlineEffects, bool useInlineSpace, InlineEffectHook inlineEffectWhen)
    {
      InlineMatchIndex? curInlineEffect = null;
      bool isInLineEffect = false; // 当前遍历是否处于一个 inline effect 中
      bool startRenderInlineContent = false;  // InlineEffectHook.One， 暂时不支持
      bool isMarkDone = false; // InlineEffectHook.Mark
      Vector3 offset = Vector3.zero;
      int start = 0;
      int length = textInfo.characterCount;
      for (int i = 0; i < length; i++)
      {
        if (!isInLineEffect)
        {
          curInlineEffect = GetInlineMatchIndex(inlineEffects, i, start);
          if (curInlineEffect != null)
          {
            isInLineEffect = true;
            start++;
            continue;
          }
        }
        else
        {
          if (i >= curInlineEffect?.contentStart && i <= curInlineEffect?.contentEnd)
          {
            if (!startRenderInlineContent && !useInlineSpace)
            {
              // 首次进入计算 offset
              TMP_CharacterInfo markCharacter = textInfo.characterInfo[curInlineEffect?.markStart ?? 0];
              TMP_CharacterInfo contentCharacter = textInfo.characterInfo[curInlineEffect?.contentStart ?? 0];
              offset += GetVertexLeftCenter(markCharacter) - GetVertexLeftCenter(contentCharacter);
            }
            startRenderInlineContent = true;
          }
          else
          {
            if (i == curInlineEffect?.contentEnd + 1 && !useInlineSpace)
            {
              TMP_CharacterInfo markCharacter = textInfo.characterInfo[curInlineEffect?.markEnd ?? 0];
              TMP_CharacterInfo contentCharacter = textInfo.characterInfo[curInlineEffect?.contentEnd ?? 0];
              offset -= GetVertexRightCenter(markCharacter) - GetVertexRightCenter(contentCharacter);
            }
            startRenderInlineContent = false;
            isInLineEffect = i < curInlineEffect?.markEnd;
            isMarkDone = i == curInlineEffect?.markEnd;
            continue;
          }
        }

        TMP_CharacterInfo characterInfo = textInfo.characterInfo[i];
        bool isemoty = IsEmptySpace(characterInfo);
        if (IsEmptySpace(characterInfo)) continue;
        int materialIndex = characterInfo.materialReferenceIndex;
        int vertexIndex = characterInfo.vertexIndex;
        TMP_MeshInfo mesh = textInfo.meshInfo[materialIndex];
        TMP_Vertex[] vertices = new TMP_Vertex[4] { characterInfo.vertex_BL, characterInfo.vertex_TL, characterInfo.vertex_TR, characterInfo.vertex_BR };
        Vector3 centerPos = Vector3.zero;

        for (int k = 0; k < 4; k++)
        {
          Vector3 pos = vertices[k].position;
          pos.x = pos.x + offset.x;
          // 注意，这里不会更新 characterInfo 的顶点信息，struct 是值引用
          vertices[k].position = pos;
          mesh.vertices[k + vertexIndex] = pos;
          centerPos += 1.0f / 4 * pos;
        }

        Vector3 rotateOriPos = vertices[0].position;

        tmp.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);

        AnimationOptions opts = commonAnimationOptions;

        if (isMarkDone && inlineEffectWhen == InlineEffectHook.Mark)
        {
          // 嵌入 inline effects
          InlineMatchIndex preInlineEffect = inlineEffects[start - 1];
          opts.onCompleted = () =>
          {
            ApplyInlineEffect(preInlineEffect);
          };
        }
        else if (i == length - 1 && inlineEffectWhen == InlineEffectHook.All)
        {
          opts.onCompleted = () =>
          {
            foreach (InlineMatchIndex item in inlineEffects)
            {
              ApplyInlineEffect(item);
            }
          };
        }

        if (keepAlphaFade || type == TypeWriterType.AlphaFade)
        {
          LAnimationCoroutineGroup.MakeAnimationAndPlay((t) =>
          {
            for (int k = 0; k < 4; k++)
            {
              (Vector3 _, Color32 color) = TypeWriterTransformPosAndColor(vertices[k], Vector3.zero, TypeWriterType.AlphaFade, t);
              mesh.colors32[k + vertexIndex] = color;
            }
            tmp.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
          }, type == TypeWriterType.AlphaFade ? opts : commonAnimationOptions);
        }

        if (type != TypeWriterType.AlphaFade)
        {
          // 嵌入渐变动画
          LAnimationCoroutineGroup.MakeAnimationAndPlay((t) =>
          {
            for (int k = 0; k < 4; k++)
            {
              // 点的平移，旋转，缩放，可以去用变换矩阵进行
              Vector3 origin = type == TypeWriterType.Rotate ? rotateOriPos : centerPos;
              (Vector3 pos, Color32 color) = TypeWriterTransformPosAndColor(vertices[k], origin, type, t);
              mesh.vertices[k + vertexIndex] = pos;
            }
            tmp.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
          }, opts);
        }

        if (!skipTypeWrite)
        {
          if (!IsWordCount && i < length - 1)
            for (int j = 0; j < count; j++)  // count 帧渲染一个字符
            {
              yield return new WaitForFixedUpdate();
            }
          else if (IsWordCount && (i + 1) % count == 0)  // 一帧内渲染count个字符
          {
            yield return new WaitForFixedUpdate();
          }
        }

        isMarkDone = false;
      }

      // InlineEffectHook.All
      // if (inlineEffectWhen == InlineEffectHook.All)
      // {
      //   foreach (InlineMatchIndex item in inlineEffects)
      //   {
      //     ApplyInlineEffect(item);
      //   }
      // }
    }

    public void ApplyInlineEffect(InlineMatchIndex inlineMatchIndex)
    {
      int start = inlineMatchIndex.contentStart, end = inlineMatchIndex.contentEnd;
      InlineEffectMarkType type = inlineMatchIndex.type;
      Dictionary<string, string> paramDict = inlineMatchIndex.paramDict;
      List<string> aniList = new List<string>();
      for (int i = start; i <= end; i++)
      {
        TMP_CharacterInfo characterInfo = textInfo.characterInfo[i];
        if (IsEmptySpace(characterInfo)) continue;
        int materialIndex = characterInfo.materialReferenceIndex;
        int vertexIndex = characterInfo.vertexIndex;
        TMP_MeshInfo mesh = textInfo.meshInfo[materialIndex];
        TMP_Vertex[] vertices = new TMP_Vertex[4] { new TMP_Vertex() { position = mesh.vertices[0 + vertexIndex], color = mesh.colors32[0 + vertexIndex]},
                                                  new TMP_Vertex() { position = mesh.vertices[1 + vertexIndex], color = mesh.colors32[1 + vertexIndex]},
                                                  new TMP_Vertex() { position = mesh.vertices[2 + vertexIndex], color = mesh.colors32[2 + vertexIndex]},
                                                  new TMP_Vertex() { position = mesh.vertices[3 + vertexIndex], color = mesh.colors32[3 + vertexIndex]}
                                                  };
        Vector3 centerPos = Vector3.zero;
        Vector3 rotateOriPos = vertices[0].position;
        AnimationOptions opts = GetInlineEffectAnimationOptions(type, paramDict, i - start, end - start + 1, aniList);
        string aniId = LAnimationCoroutineGroup.MakeAnimation((t) =>
        {
          for (int k = 0; k < 4; k++)
          {
            (Vector3 pos, Color32 color) = InlineEffectTransformPosAndColor(vertices[k], Vector3.zero, paramDict, type, t);
            mesh.colors32[k + vertexIndex] = color;
            mesh.vertices[k + vertexIndex] = pos;
          }
          tmp.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
          tmp.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
        }, opts);
        aniList.Add(aniId);
      }
      if (type == InlineEffectMarkType.Twinkle) // 同步播放
      {
        foreach (string item in aniList)
        {
          LAnimationCoroutineGroup.RePlayC(item);
        }
      }
      else
      {
        LAnimationCoroutineGroup.RePlayC(aniList[0]); // 链式播放
      }
    }

    public AnimationOptions GetInlineEffectAnimationOptions(InlineEffectMarkType type, Dictionary<string, string> paramDict, int i, int total, List<string> aniList)
    {
      AnimationOptions options = commonAnimationOptions;
      options.onFrameDict = new Dictionary<float, Action>();
      options.onFrameDictParam = new Dictionary<float, Action<LAnimationStateStruct>>();
      if (type == InlineEffectMarkType.Banner)
      {
        options.iterations = 2;
        options.duration = float.Parse(paramDict["d"]) / 2.0f;
        options.direction = AnimationDirection.ALTERNATE;
        options.curve = LCurveType.QuadraticDouble;
        options.removeOnCompleted = false;
        // 第二次循环到 0.7 部分时
        options.onFrameDictParam.Add(0.7f, (LAnimationStateStruct state) =>
        {
          if (state.iter_count == 1)
            LAnimationCoroutineGroup.RePlayC(aniList[(i + 1) % total]);
        });
      }
      else if (type == InlineEffectMarkType.Twinkle)
      {
        options.iterations = -1;
        options.duration = float.Parse(paramDict["d"]);
        options.direction = AnimationDirection.ALTERNATE;
        options.curve = LCurveType.Linear;
      }
      else if (type == InlineEffectMarkType.Wave)
      {
        options.iterations = 2;
        options.duration = float.Parse(paramDict["d"]) / 2.0f;
        options.direction = AnimationDirection.ALTERNATE;
        options.curve = LCurveType.QuadraticDouble;
        options.removeOnCompleted = false;
        options.onFrameDictParam.Add(0.7f, (LAnimationStateStruct state) =>
        {
          if (state.iter_count == 1)
            LAnimationCoroutineGroup.RePlayC(aniList[(i + 1) % total]);
        });
      }
      return options;
    }

    // 可拓展的
    public (Vector3, Color32) InlineEffectTransformPosAndColor(TMP_Vertex vertex, Vector3 origin, Dictionary<string, string> paramDict, InlineEffectMarkType type, float t)
    {
      Vector3 pos = vertex.position;
      Color32 color = vertex.color;
      if (type == InlineEffectMarkType.Banner)
      {
        Color32 n_color = Color32.Lerp(color, String2Color(paramDict["c"]), t);
        return (pos, n_color);
      }
      else if (type == InlineEffectMarkType.Twinkle)
      {
        byte alpha = (byte)Mathf.Lerp(255, byte.Parse(paramDict["a"]), t);
        return (pos, new Color32(color.r, color.g, color.b, alpha));
      }
      else if (type == InlineEffectMarkType.Wave)
      {
        Vector3 n_pos = new Vector3(pos.x, pos.y + Mathf.Lerp(0, float.Parse(paramDict["h"]), t), pos.z);
        return (n_pos, color);
      }
      else
      {
        return (pos, color);
      }
    }


    // 可拓展的
    public (Vector3, Color32) TypeWriterTransformPosAndColor(TMP_Vertex vertex, Vector3 origin, TypeWriterType type, float t)
    {
      Vector3 pos = vertex.position;
      Color32 color = vertex.color;
      if (type == TypeWriterType.AlphaFade)
      {
        byte alpha = (byte)Mathf.Lerp(0, 255, t);
        return (pos, new Color32(color.r, color.g, color.b, alpha));
      }
      else if (type == TypeWriterType.Wave)
      {
        float offset = Mathf.Lerp(waveHeight, 0, t);
        return (new Vector3(pos.x, pos.y + offset, pos.z), color);
      }
      // 中心缩放
      else if (type == TypeWriterType.Scale)
      {
        float scale = Mathf.Lerp(scaleRate, 1, t);
        return (origin + scale * (pos - origin), color);
      }
      // 左下角旋转
      else if (type == TypeWriterType.Rotate)
      {
        float angle = Mathf.Lerp(rotateAngle, 0, t);
        return (rotateY(pos - origin, angle) + origin, color);
      }
      // 上下左右抖动
      else if (type == TypeWriterType.Stretch)
      {
        float offset = Mathf.Lerp(-1 * stretchAmplitude, 0, t);
        return (new Vector3(pos.x + offset, pos.y, pos.z), color);
      }
      else if (type == TypeWriterType.Noise)
      {
        float noiseX = noiseAmplitude * (t >= 1 ? 0 : (2 * Mathf.PerlinNoise1D(t) - 1));
        float noiseY = noiseAmplitude * (t >= 1 ? 0 : (2 * Mathf.PerlinNoise1D(t) - 1));
        return (new Vector3(pos.x + noiseX, pos.y + noiseY, pos.z), color);
      }
      else
      {
        return (pos, color);
      }
    }

    public Vector3 GetVertexCenter(TMP_CharacterInfo characterInfo)
    {
      return 1.0f / 4 * (characterInfo.vertex_BL.position + characterInfo.vertex_TL.position + characterInfo.vertex_TR.position + characterInfo.vertex_BR.position);
    }

    public Vector3 GetVertexLeftCenter(TMP_CharacterInfo characterInfo)
    {
      return 1.0f / 2 * (characterInfo.vertex_BL.position + characterInfo.vertex_TL.position);
    }

    public Vector3 GetVertexRightCenter(TMP_CharacterInfo characterInfo)
    {
      return 1.0f / 2 * (characterInfo.vertex_BR.position + characterInfo.vertex_TR.position);
    }

    public Color32 String2Color(string colorstr, byte alpha = 255)
    {
      string[] rgba = colorstr.Split(",");
      if (rgba.Length == 4)
      {
        return new Color32((byte)float.Parse(rgba[0]), (byte)float.Parse(rgba[1]), (byte)float.Parse(rgba[2]), (byte)float.Parse(rgba[3]));
      }
      else if (rgba.Length == 3)
      {
        return new Color32((byte)float.Parse(rgba[0]), (byte)float.Parse(rgba[1]), (byte)float.Parse(rgba[2]), alpha);
      }
      else
      {
        return new Color32(255, 255, 255, alpha);
      }
    }


    public Vector3 RandomVector2()
    {
      return new Vector2(Random(-1f, 1f), Random(-1f, 1f));
    }
    public float Random(float min, float max)
    {
      return UnityEngine.Random.Range(min, max);
    }
    public Vector2 rotateY(Vector2 pos, float angle)
    {
      float rad = Mathf.Deg2Rad * angle;
      float x = pos.x, y = pos.y;
      float n_x = x * Mathf.Cos(rad) - y * Mathf.Sin(rad);
      float n_y = x * Mathf.Sin(rad) + y * Mathf.Cos(rad);
      return new Vector2(n_x, n_y);
    }

    public Vector3 rotateY(Vector3 pos, float angle)
    {
      Vector2 rotate_p = rotateY(new Vector2(pos.x, pos.y), angle);
      return new Vector3(rotate_p.x, rotate_p.y, pos.z);
    }

    public Color32 Color2Color32(Color color)
    {
      return new Color32((byte)(color.r * 255.0f), (byte)(color.g * 255.0f), (byte)(color.b * 255.0f), (byte)(color.a * 255.0f));
    }

    public Color32 Color2Color32(Color color, byte a)
    {
      return new Color32((byte)(color.r * 255.0f), (byte)(color.g * 255.0f), (byte)(color.b * 255.0f), a);
    }

    public bool IsEmptySpace(TMP_CharacterInfo characterInfo)
    {
      return string.IsNullOrWhiteSpace("" + characterInfo.character) || (characterInfo.index > 0 && characterInfo.vertexIndex == 0);
    }
  }
}

