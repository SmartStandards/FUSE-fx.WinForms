using System;

namespace System.Data.Fuse.WinForms.Internal {

  internal interface IColumnPropertyDescriptor {

    object GetValueForSort(object component);

    int ColumnWidth { get; }

    Action<object> AfterEditHandler { get; }

    Action<object, Action> ClickDelegate { get; }

  }

}
