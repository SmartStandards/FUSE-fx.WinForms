using System;

namespace System.Data.Fuse.WinForms.Internal {

  internal class SelectionAttribute : Attribute {

    private Selection _Selection;

    public SelectionAttribute(Selection s) {
      _Selection = s;
    }

    public Selection Selection {
      get {
        return _Selection;
      }
      set {
        _Selection = value;
      }
    }

  }
}