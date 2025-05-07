

namespace System.Data.Fuse.WinForms.Internal {

  internal interface IStateContainer {

    T Item<T>() where T : new();

  }

}