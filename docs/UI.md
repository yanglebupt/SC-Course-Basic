# UI

该模块提供了一种构造自己 UI 组件的规范

## CustomUI

构造自己 UI 组件的基类，自己的 UI 组件类必须继承自 `CustomUI`。使用方法是：在场景的 `Canvas` 下新建一个 `Empty GameObject` 占位，然后挂载自己 UI 组件，点击 `GenerateStructure` 按钮，生成组件结构即可

#### CustomUIEditor

为了能够实现上方的编辑器功能，请在 `Editor` 下面，声明一个类继承自 `CustomUIEditor<T>`，其中泛型 `T` 就是自己定义的 UI
组件类，例如下例中的 `LButton` 就是我自己定义的按钮类，这样挂载该组件时才会出现 `GenerateStructure` 按钮和相关功能

```csharp
[CustomEditor(typeof(LButton))]
public class CustomButtonEditor : CustomUIEditor<LButton> { };
```

#### abstract GenerateStructure()

UI 组件必须实现该方法，该方法决定了在面板上点击 `GenerateStructure` 按钮的反应

```csharp
abstract void GenerateStructure()
```

该方法还有一个重载方法用来从 Prefab 中创建结构，只需要提供 Prefab 的资源路径，需要注意 Prefab 上必须挂载所定义 UI 组件

```csharp
T GenerateStructure<T>(
  string prefabPath
) where T : CustomUI
```


因此我们推荐这样实现 `GenerateStructure()` 方法

```csharp
public class LButton : CustomUI {
  public override void GenerateStructure()
  {
    base.GenerateStructure<LButton>("Packages/com.ylbupt.sc-course-basic/Resources/UI/Prefabs/Button.prefab");
  }
}
```

#### abstract InitComponents()

初始化 UI 组件内部的各种子部件，这个方法将会在 `GenerateStructure()` 中调用。当然你也需要手动在 `Start` 中调用，这样后续才可以针对这些子部件进行具体操作

```csharp
public class LButton : CustomUI {
  [Tooltip("按钮背景组件")]
  public Image buttonImage;

  [Tooltip("按钮文字组件")]
  public TMP_Text buttonText;

  void Start()
  {
    InitComponents();
  }

  public override void InitComponents()
  {
    buttonImage = GetComponent<Image>();
    buttonText = GetComponentInChildren<TMP_Text>();
  }
}
```

#### abstract DrawEditorPreview()

面板更新的每帧执行，在这里你可以对面板属性的修改在子部件上做出响应，该方法还有一个重载

```csharp
abstract void DrawEditorPreview(
  SerializedObject serializedObject
)
```

可以通过 `serializedObject` 来控制面板显示内容，例如一些表单联动效果

## AnimationUI

带有一些动画辅助函数的 UI 组件基类，当你的 UI 组件需要一些过渡动画的时候，请继承 `AnimationUI` 而不是 `CustomUI`

#### TransitionColor()

```csharp
TransitionColor<T>(T t, Color src, Color tar) where T : MaskableGraphic
```

#### TransitionAlpha()

```csharp
TransitionAlpha<T>(T t, float src, float tar) where T : MaskableGraphic
```

# 内置组件

`LButton`、 `LSlider`、 `LToggle`