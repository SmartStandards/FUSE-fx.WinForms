using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;

namespace System.Data.Fuse.WinForms.Internal {

  internal abstract class GenericListEditingController<TItem> : IListEditingController<TItem>, IDisposable {

    #region  Constructor & Variables 

    private bool _NoDataSource = true;
    private ObservableCollection<TItem> _Cache = new ObservableCollection<TItem>();
    private TItem[] _CurrentItems = Array.Empty<TItem>();
    private bool _UpdatingSemaphore = false;

    public GenericListEditingController() {
      _Cache.CollectionChanged += this.Cache_CollectionChanged;
    }

    #endregion

    protected abstract IEnumerable<TItem> GetItems();
    protected abstract void OnItemAdded(TItem item);
    protected abstract void OnItemEdited(TItem item);
    protected abstract void OnItemRemoved(TItem item);

    #region  Events & Triggers 

    public event CurrentItemsChangedEventHandler CurrentItemsChanged;

    public event ItemContentChangedEventHandler<TItem> ItemContentChanged;

    public event ItemListChangedEventHandler<TItem> ItemListChanged;

    protected void OnCurrentItemsChanged() {
      if (this.CurrentItemsChanged != null) {
        CurrentItemsChanged?.Invoke(this, EventArgs.Empty);
      }
    }

    protected void OnItemListChanged(bool collectionDirectEdited) {
      if (this.ItemListChanged != null) {
        ItemListChanged?.Invoke(this, new ItemListChangedEventargs<TItem>(collectionDirectEdited));
      }
    }

    protected void OnItemContentChanged(TItem item) {
      if (this.ItemContentChanged != null) {
        ItemContentChanged?.Invoke(this, new ItemContentChangedEventargs<TItem>(item));
      }
    }

    #endregion

    protected virtual IEnumerable<TItem> ApplySorting(IEnumerable<TItem> unsorted) {
      return unsorted;
    }

    public virtual string GetItemIdentString(TItem item) {
      return item.ToString();
    }

    public virtual void InitializeItemPrototype(TItem prototype) {
    }

    public virtual bool IsItemDeleteable(TItem item) {
      return true;
    }

    #region  Consume 

    public IEnumerable<TItem> Items {
      get {
        lock (_Cache) {
          if (_NoDataSource) {
            return null;
          }
          else {
            return _Cache;
          }
        }
      }
    }

    public TItem[] CurrentItems {
      get {
        lock (_CurrentItems) {
          if (_NoDataSource) {
            return Array.Empty<TItem>();
          }
          else {
            return _CurrentItems;
          }
        }
      }
      set {
        lock (_CurrentItems) {
          if (value == null) {
            value = Array.Empty<TItem>();
          }

          if (!ReferenceEquals(_CurrentItems, value) && _NoDataSource == false) {
            _CurrentItems = value;
            this.OnCurrentItemsChanged();
          }
        }
      }
    }

    #endregion

    #region  Updates from Frontend 

    private void Cache_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {

      if (_UpdatingSemaphore == true) {
        return;
      }

      if (_NoDataSource) {
        return;
      }

      if (e.OldItems != null) {
        foreach (var oi in e.OldItems.OfType<TItem>())
          this.OnItemRemoved(oi);
      }

      if (e.NewItems != null) {
        foreach (var ni in e.NewItems.OfType<TItem>())
          this.OnItemAdded(ni);
      }

      this.OnItemListChanged(true);
    }

    public void ItemWasEdited(TItem item) {

      if (_NoDataSource) {
        return;
      }

      this.OnItemEdited(item);
      this.OnItemContentChanged(item);

    }

    #endregion

    #region  Updates from Backend 

    public void ReloadItems() {

      if (_UpdatingSemaphore == true) {
        return;
      }
      else {
        _UpdatingSemaphore = true;
        _Cache.CollectionChanged -= this.Cache_CollectionChanged;
      }

      lock (_Cache) {
        try {
          this.UpdateCache(_Cache);

          lock (_CurrentItems) {
            _CurrentItems = (from i in _CurrentItems
                             where _Cache.Contains(i)
                             select i).ToArray();
            this.OnCurrentItemsChanged();
          }
        }
        finally {
          _UpdatingSemaphore = false;
          _Cache.CollectionChanged += this.Cache_CollectionChanged;
        }
      }

      this.OnItemListChanged(false);
    }

    private void UpdateCache(IList<TItem> cache) {
      _Cache.Clear();
      var items = this.GetItems();
      if (items == null) {
        _NoDataSource = true;
        return;
      }
      _NoDataSource = false;
      foreach (var item in this.ApplySorting(items))
        _Cache.Add(item);
    }

    protected virtual void Cleanup() {

    }

    #endregion

    #region  IDisposable 

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private bool _AlreadyDisposed = false;

    /// <summary>
  /// Dispose the current object instance
  /// </summary>
    protected virtual void Dispose(bool disposing) {
      if (!_AlreadyDisposed) {
        if (disposing) {
          this.Cleanup();
        }
        _AlreadyDisposed = true;
      }
    }

    /// <summary>
  /// Dispose the current object instance and suppress the finalizer
  /// </summary>
    public void Dispose() {
      this.Dispose(true);
      GC.SuppressFinalize(this);
    }

    #endregion

  }
}