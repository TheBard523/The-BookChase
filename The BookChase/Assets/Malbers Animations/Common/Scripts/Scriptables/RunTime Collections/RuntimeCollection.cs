using System.Collections.Generic;
using UnityEngine;

namespace MalbersAnimations.Scriptables
{
    public abstract class RuntimeCollection<T> : ScriptableObject
    {
        protected List<T> items = new List<T>();

        /// <summary>Ammount of object on the list</summary>
        public int Count => items.Count;

        /// <summary> Clears the list of objects </summary>
        public void Clear() => items.Clear();

        /// <summary>Gets an object on the list by an index </summary>
        public T Item_Get(int index) => items[index];



        /// <summary>Gets the first object of the list</summary>
        public T Item_GetFirst() => items[0];

       


        /// <summary>Gets a rando first object of the list</summary>
        public T Item_GetRandom()
        {
            if (items != null && items.Count > 0)
            {
                var rnd = new System.Random();
                return items[rnd.Next(items.Count)];
            }
            return default;
        }

        public void Item_Add(T newItem)
        {
            if (!items.Contains(newItem))
                items.Add(newItem);
        }

        public void Item_Remove(T newItem)
        {
            if (items.Contains(newItem))
                items.Remove(newItem);
        }
    }
}