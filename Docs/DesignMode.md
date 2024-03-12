# ObjectPool

对象池，不需要频繁的销毁和创建对象

## Constructor

接受 `ObjectPoolOptions` 类型的参数，类型如下

```csharp

public struct ObjectPoolOptions{
  public int initPoolSize; /* 初始大小 */
  public int maxPoolSize; /* 最大大小 */
  public GameObject prefab; /* 存放的预制体 */

  public ObjectPoolOptions(int _initPoolSize, int _maxPoolSize, GameObject _prefab) : this()
  {
    initPoolSize = _initPoolSize;
    maxPoolSize = _maxPoolSize;
    prefab = _prefab;
  }

  public Action<GameObject>? OnGet; /* 从池子中获取对象的回调 */
  public Action<GameObject>? OnRelease; /* 释放回池子中的回调 */
  public Action<GameObject>? OnDestroy; /* 销毁对象的回调 */
}
```

## Methods

#### GetFromPool() 

```csharp
GameObject GetFromPool()
```

当需要使用对象时

- 直接从池子中获取空闲的对象，如果不存在则新建对象
- 若新建，根据最大容量决定是否将新建对象放入池中
- 显示并标记为非空闲，然后返回

#### ReleaseToPool()

```csharp
void ReleaseToPool(GameObject obj)
```

不需要使用对象时

- 取消显示，标记为空闲
- 如果对象不存在池中，直接销毁

# Singleton

通过泛型来创建单例对象，然后使用静态 `Instance` 属性获取，使用如下

```csharp
Demo demo = Singleton<Demo>.Instance;
```

