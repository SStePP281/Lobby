using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : Component
{
    private readonly Func<Transform, T> createFunc;
    private readonly Transform parent;
    private readonly List<T> pool;

    public ObjectPool(Func<Transform, T> createFunc, Transform parent)
    {
        pool = new List<T>();

        this.createFunc = createFunc;
        this.parent = parent;
    }

    public T GetElement()
    {
        T element;

        if (IsEmpty(out element))
        {
            CreateElement(out element);
        }

        element.gameObject.SetActive(true);
        return element;
        
    }

    public void Clear()
    {
        foreach (var element in pool)
        {
            if (element != null)
            {
                UnityEngine.Object.Destroy(element.gameObject);
            }
        }

        pool.Clear();
    }

    public void Hide()
    {
        foreach (T element in pool)
        {
            element.gameObject.SetActive(false);
        }
    }

    private void CreateElement(out T element)
    {
        T tempEl = createFunc(parent);
        pool.Add(tempEl);
        element = tempEl;
    }

    private bool IsEmpty(out T element)
    {
        foreach (T el in pool)
        {
            if (!el.gameObject.activeSelf)
            {
                element = el;
                return false;
            }
        }

        element = null;
        return true;
    }
}
