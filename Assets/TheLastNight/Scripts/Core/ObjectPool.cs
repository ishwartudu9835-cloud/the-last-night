// Minimal generic component pool — avoids Instantiate/Destroy garbage on mobile.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheLastNight
{
    public class ComponentPool<T> where T : Component
    {
        readonly Stack<T> free = new Stack<T>();
        readonly Func<T> create;
        public int CreatedCount { get; private set; }

        public ComponentPool(Func<T> create, int prewarm = 0)
        {
            this.create = create;
            for (int i = 0; i < prewarm; i++) Release(Make());
        }

        T Make() { CreatedCount++; return create(); }

        public T Get()
        {
            T item = free.Count > 0 ? free.Pop() : Make();
            item.gameObject.SetActive(true);
            return item;
        }

        public void Release(T item)
        {
            if (item == null) return;
            item.gameObject.SetActive(false);
            free.Push(item);
        }
    }
}
