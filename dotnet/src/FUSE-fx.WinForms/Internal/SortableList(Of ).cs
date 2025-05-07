using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace System.Data.Fuse.WinForms.Internal {

  internal class SortableList<T> : IList<T> {

    private IList<T> _BaseList;

    public SortableList(IList<T> baseList) {
      _BaseList = baseList;
    }

    public Func<IEnumerable<T>, IEnumerable<T>> SortingDelegate { get; set; }

    public void Add(T item) {
      _BaseList.Add(item);
    }

    public void Clear() {
      _BaseList.Clear();
    }

    public bool Contains(T item) {
      return _BaseList.Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex) {
      _BaseList.CopyTo(array, arrayIndex);
    }

    public int Count {
      get {
        return _BaseList.Count;
      }
    }

    public bool IsReadOnly {
      get {
        return _BaseList.IsReadOnly;
      }
    }

    public bool Remove(T item) {
      return _BaseList.Remove(item);
    }

    public IEnumerator<T> GetEnumerator() {
      if (this.SortingDelegate == null) {
        return _BaseList.GetEnumerator();
      }
      else {
        return this.SortingDelegate.Invoke(_BaseList).GetEnumerator();
      }
    }

    public IEnumerator<T> GetEnumeratorUntyped() {
      return this.GetEnumerator();
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => this.GetEnumeratorUntyped();

    public int IndexOf(T item) {
      int index = 0;
      foreach (var itm in this) {
        if (Equals(itm, item)) {
          return index;
        }
        index += 1;
      }
      return -1;
    }

    public void Insert(int index, T item) {
      if (this.SortingDelegate == null) {
        _BaseList.Insert(index, item);
      }
      else {
        _BaseList.Add(item);
      }
    }

    public T this[int index] {
      get {
        return this.Skip(index).First();
      }
      set {
        var addressedItem = this[index];
        int indexInBase = _BaseList.IndexOf(addressedItem);
        _BaseList[indexInBase] = value;
      }
    }

    public void RemoveAt(int index) {
      this.Remove(this[index]);
    }

    IEnumerator IEnumerable.GetEnumerator() {
      return this.GetEnumerator();
    }
  }

}