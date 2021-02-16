using System;
using System.Collections;
using UnityEngine;


namespace BubbleSoccer
{
    public class Heap<T> where T : IHeapItem<T>
    {
        //helpers
        private T[] items;
        private int currentItemCount;
        private int parentIndex;
        private int ItemAIndex;
        private int childIndexLeft;
        private int childIndexRight;
        private int swapIndex;
        private T parentItem;
        private T firstItem;

        public Heap(int maxHeapSize)
        {
            items = new T[maxHeapSize];
        }

        public void Add(T item)
        {
            item.HeapIndex = currentItemCount;
            items[currentItemCount] = item;
            SortUp(item);
            currentItemCount++;
        }

        public T RemoveFirstItem()
        {
            firstItem = items[0];
            currentItemCount--;
            items[0] = items[currentItemCount];
            items[0].HeapIndex = 0;
            SortDown(items[0]);
            return firstItem;
        }

        public void UpdateItem(T item)
        {
            SortUp(item);
        }

        public int Count
        {
            get
            {
                return currentItemCount;
            }
        }

        public bool Contains(T item)
        {
            return Equals(items[item.HeapIndex], item);
        }

        void SortDown(T item)
        {
            while (true)
            {
                childIndexLeft = item.HeapIndex * 2 + 1;
                childIndexRight = item.HeapIndex * 2 + 2;
                swapIndex = 0;

                if (childIndexLeft < currentItemCount)
                {
                    swapIndex = childIndexLeft;
                    if (childIndexRight < currentItemCount)
                    {
                        if (items[childIndexLeft].CompareTo(items[childIndexRight]) < 0)
                        {
                            swapIndex = childIndexRight;
                        }
                    }

                    if (item.CompareTo(items[swapIndex]) < 0)
                    {
                        Swap(item, items[swapIndex]);
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    return;
                }
            }
        }
        void SortUp(T item)
        {
            parentIndex = (item.HeapIndex - 1) / 2;
            while (true)
            {
                parentItem = items[parentIndex];
                if (item.CompareTo(parentItem) > 0)
                {
                    Swap(item, parentItem);
                }
                else
                {
                    break;
                }

                parentIndex = (item.HeapIndex - 1) / 2;
            }
        }

        void Swap(T itemA, T itemB)
        {
            items[itemA.HeapIndex] = itemB;
            items[itemB.HeapIndex] = itemA;

            ItemAIndex = itemA.HeapIndex;
            itemA.HeapIndex = itemB.HeapIndex;
            itemB.HeapIndex = ItemAIndex;
        }
    }

    public interface IHeapItem<T> : IComparable<T>
    {
        int HeapIndex
        {
            get;
            set;
        }
    }
}
