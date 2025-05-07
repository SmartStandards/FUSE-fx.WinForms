using System;
using System.Collections.Generic;

namespace System.Data.Fuse.WinForms.Internal {

  internal delegate void ItemListChangedEventHandler<TItem>(object sender, ItemListChangedEventargs<TItem> e);
  internal delegate void CurrentItemsChangedEventHandler(object sender, EventArgs e);
  internal delegate void ItemContentChangedEventHandler<TItem>(object sender, ItemContentChangedEventargs<TItem> e);

  internal interface IListEditingController<TItem> : IDisposable {

    event ItemListChangedEventHandler<TItem> ItemListChanged;


    event ItemContentChangedEventHandler<TItem> ItemContentChanged;


    event CurrentItemsChangedEventHandler CurrentItemsChanged;


    void ReloadItems();

    TItem[] CurrentItems { get; set; }

    bool IsItemDeleteable(TItem item);

    void ItemWasEdited(TItem item);

    void InitializeItemPrototype(TItem prototype);

    IEnumerable<TItem> Items { get; }

    string GetItemIdentString(TItem item);

  }

  internal class ItemContentChangedEventargs<TItem> : EventArgs {

    private TItem _Item;

    public ItemContentChangedEventargs(TItem item) {
      _Item = item;
    }

    public TItem Item {
      get {
        return _Item;
      }
    }

  }
  internal class ItemListChangedEventargs<TItem> : EventArgs {

    private bool _CollectionDirectEdited;

    public ItemListChangedEventargs(bool collectionDirectEdited) {
      _CollectionDirectEdited = collectionDirectEdited;
    }

    public bool CollectionDirectEdited {
      get {
        return _CollectionDirectEdited;
      }
    }

  }

}