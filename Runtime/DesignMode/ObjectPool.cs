using UnityEngine;
using System;
using System.Collections.Generic;

namespace YLBasic
{
  [Serializable]
  public struct ObjectPoolOptions
  {
    public int initPoolSize;
    public int maxPoolSize;
    public GameObject prefab;

    public ObjectPoolOptions(int _initPoolSize, int _maxPoolSize, GameObject _prefab) : this()
    {
      initPoolSize = _initPoolSize;
      maxPoolSize = _maxPoolSize;
      prefab = _prefab;
    }

#nullable enable
    public Action<GameObject>? OnGet;
    public Action<GameObject>? OnRelease;
    public Action<GameObject>? OnDestroy;
  }

  public class ObjectPool : MonoBehaviour
  {
    private List<GameObject> pool = new List<GameObject>();
    public int CurrentCount => pool.Count;
    private int maxPoolSize = 20;
    private int initPoolSize = 3;
    private GameObject objPrefab;
    private ObjectPoolOptions _options;

    public ObjectPool(ObjectPoolOptions options)
    {
      _options = options;
      maxPoolSize = options.maxPoolSize;
      initPoolSize = options.initPoolSize;
      objPrefab = options.prefab;
      WarmUp();
    }

    public void WarmUp()
    {
      for (int i = 0; i < initPoolSize; i++)
      {
        GameObject obj = createFromPrefab();
        pool.Add(obj);
      }
    }

    public GameObject createFromPrefab(bool active = false)
    {
      GameObject obj = Instantiate(objPrefab);
      obj.SetActive(active);
      return obj;
    }

    public GameObject UsedByIndex(int index)
    {
      GameObject gameObject = pool[index];
      gameObject.SetActive(true);
      return gameObject;
    }

    public bool IsUsed(GameObject gameObject)
    {
      return gameObject.activeInHierarchy;
    }

    public GameObject GetFromPool()
    {
      int count = pool.Count;
      int selectedIndex = -1;
      for (int i = 0; i < count; i++)
      {
        if (!IsUsed(pool[i]))
        {
          selectedIndex = i;
          break;
        }
      }
      // 池子里面有还未使用得返回即可，否则创建新得
      GameObject obj = selectedIndex > -1 ? UsedByIndex(selectedIndex) : createFromPrefab(true);
      if (selectedIndex == -1 && CurrentCount < maxPoolSize) // 如果还有剩余空间，将新创建得加入
        pool.Add(obj);

      _options.OnGet?.Invoke(obj);
      return obj;
    }

    public void ReleaseToPool(GameObject obj)
    {
      obj.SetActive(false);
      _options.OnRelease?.Invoke(obj);
      if (!pool.Contains(obj))  // 不包含，说明是新创建，并且超过了空间，直接销毁即可
      {
        _options.OnDestroy?.Invoke(obj);
        Destroy(obj);
      }
    }
  }
}

