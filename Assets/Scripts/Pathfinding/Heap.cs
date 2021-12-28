using System;
using System.Collections.Generic;

//max binary heap
public class Heap<T> where T : IComparable<T>
{
    T[] items;
    Dictionary<T, int> indeces;
    int count = 0;

    public Heap(int size){
        items = new T[size];
        indeces = new Dictionary<T, int>(size);
    }

    public void Add(T item){
        indeces.Add(item, count);
        items[count++] = item;
        SortUp(item);
    }

    public void UpdateItem(T item){
        SortUp(item);
    }

    public T Pop(){
        T first = items[0];
        indeces.Remove(first);
        T newFirst = items[--count];
        items[0] = newFirst;
        indeces[newFirst] = 0;
        SortDown(newFirst);
        return first;
    }
    
    public int Count {
        get{
            return count;
        }
    }

    public bool Contains(T item){
        return indeces.ContainsKey(item);
    }

    void SortUp(T item){
        T parent = items[(indeces[item]-1)/2];
        if( item.CompareTo(parent) > 0){
            Swap(item, parent);
            SortUp(item);
        }
    }

    void SortDown(T item){
        int index = indeces[item];
        int idxLeft = index * 2 + 1;
        int idxRight = index * 2 + 2;
        if(idxLeft < count){
            T swapItem = items[idxLeft];
            if(swapItem.CompareTo(items[idxRight]) < 0){
                swapItem = items[idxRight];
            }
            if(swapItem.CompareTo(item) > 0){
                Swap(swapItem, item);
                SortDown(item);
            }
        }
    }

    void Swap(T item1, T item2){
        int idx1 = indeces[item1];
        int idx2 = indeces[item2];
        indeces[item1] = idx2;
        indeces[item2] = idx1;
        items[idx1] = item2;
        items[idx2] = item1;
    }
}
