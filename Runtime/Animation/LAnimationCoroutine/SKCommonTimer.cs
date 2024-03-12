namespace YLBasic
{
  // https://github.com/Skyrim07/SKCell/blob/main/Assets/SKCell/Common/SKCommonTimer.cs

  /// <summary>
  /// 需要挂载到空物体上，管理了全部的动画协程状态
  /// </summary>

  public sealed class SKCommonTimer : SKMonoSingleton<SKCommonTimer>
  {
    protected override void Awake()
    {
      base.Awake();
    }
  }
}