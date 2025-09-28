using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : Component
{
    private readonly T prefab;
    private readonly Transform root;
    private readonly Stack<T> stack = new();

    public ObjectPool(T prefab, Transform root = null)
    {
        this.prefab = prefab;
        this.root = root;
    }

    public T Get()
    {
        T x = stack.Count > 0 ? stack.Pop() : Object.Instantiate(prefab, root);
        x.gameObject.SetActive(true);
        return x;
    }

    public void Release(T x)
    {
        x.gameObject.SetActive(false);
        stack.Push(x);
    }
}
