using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;

namespace System.Data.Fuse.WinForms.Internal {

  internal class CollectionViewDataSource<T> {

    public event ModelEditedEventHandler ModelEdited;

    public delegate void ModelEditedEventHandler(T model, string columnName, int rowIndex, object value);
    // Public Event ListChanged(sender As Object, e As ListChangedEventArgs)

    private SortableBindingList<T> __SortableBindingList;

    private SortableBindingList<T> _SortableBindingList {
      [MethodImpl(MethodImplOptions.Synchronized)]
      get {
        return __SortableBindingList;
      }

      [MethodImpl(MethodImplOptions.Synchronized)]
      set {
        __SortableBindingList = value;
      }
    }
    private IEnumerable<PropertyDescriptor> _Descriptors;

    public CollectionViewDataSource(IEnumerable<T> source) : this(source, (IModelDisplaySchema)null) {
    }

    public CollectionViewDataSource(IEnumerable<T> source, IModelDisplaySchema schema) {
      if (source == null) {
        source = Array.CreateInstance(typeof(T), 0).OfType<object>().ToList().OfType<T>();
      }
      if (schema == null) {
        this.BindingList = SortableBindingList<T>.CreateFor(source);
        _Descriptors = new List<PropertyDescriptor>();
      }
      else {
        var registrator = new DisplayableFieldRegistrator();
        schema.DefineFields(registrator);
        _Descriptors = registrator.BuildDescriptors();
        this.BindingList = SortableBindingList<T>.CreateFor(source, _Descriptors);
      }
    }

    public Action<T> NewItemInitializer {
      get {
        return this.BindingList.NewItemInitializer;
      }
      set {
        this.BindingList.NewItemInitializer = value;
      }
    }


    [Obsolete("Use the 'ModelEdited' Event instead!!")]
    public Action<T> OnModelEdited { get; set; } = t => { };


    public void NotifyCellEdited(int rowIndex, int columnIndex) {
      var model = this.GetObjectAtRowIndex(rowIndex);

      if (model == null) {
        // this occours when were editing in a prototype-row
        return;
      }



      var descriptor = this.GetDescriptorForColumnIndex(columnIndex);

      if (rowIndex >= 0 && descriptor is IColumnPropertyDescriptor) {
        var afterEditHandler = ((IColumnPropertyDescriptor)descriptor).AfterEditHandler;
        if (afterEditHandler != null) {
          afterEditHandler.Invoke(model);
        }
      }

      if (this.ModelEdited != null) {
        ModelEdited?.Invoke(model, descriptor.Name, rowIndex, descriptor.GetValue(model));
      }

      this.OnModelEdited.Invoke(model);

      // required when the lis is sorted and the change will affect the position of our item
      this.BindingList.Refresh();
    }

    public void NotifyCellClicked(int rowIndex, int columnIndex) {
      var descriptor = this.GetDescriptorForColumnIndex(columnIndex);
      if (rowIndex >= 0 && descriptor is IColumnPropertyDescriptor) {
        var clickDelegate = ((IColumnPropertyDescriptor)descriptor).ClickDelegate;
        if (clickDelegate != null) {
          var model = this.GetObjectAtRowIndex(rowIndex);
          clickDelegate.Invoke(model, () => this.NotifyCellEdited(rowIndex, columnIndex));
        }
      }
    }

    public int ColumnCount {
      get {
        return _Descriptors.Count();
      }
    }

    public string GetColumnName(int columnIndex) {
      return _Descriptors.ElementAtOrDefault(columnIndex).Name;
    }

    public PropertyDescriptor GetDescriptorForColumnIndex(int columnIndex) {
      return _Descriptors.ElementAtOrDefault(columnIndex);
    }

    public T GetObjectAtRowIndex(int rowIndex) {
      if (rowIndex < 0) {
        return default;
      }
      else if (rowIndex >= this.BindingList.Count) {
        // maybe we have a prototype of a new row (at the end of out grid) which hasnt already added to the bindinglist
        return default;
      }
      else {
        return this.BindingList[rowIndex];
      }
    }

    public int GetRowIndexOfObject(T o) {
      return this.BindingList.IndexOf(o);
    }

    // Public Sub ApplyNewSource(source As IEnumerable(Of T))
    // Me.BindingList.Clear()
    // Me.BindingList.AddRange(source)
    // Me.Refresh()
    // End Sub

    public SortableBindingList<T> BindingList {
      get {
        return this._SortableBindingList;
      }
      set {
        this._SortableBindingList = value;
      }
    }

    public void AddColumn(PropertyDescriptor pd) {
      this._SortableBindingList.AddColumnDescriptor(pd);
    }

    public void RemoveColumn(PropertyDescriptor pd) {
      this._SortableBindingList.RemoveColumnDescriptor(pd);
    }

    public void AddColumn(string caption, string propertyName, bool readOnly = true) {
      this._SortableBindingList.AddColumnDescriptor(caption, propertyName, readOnly);
    }

    public void RemoveColumn(string propertyName) {
      this._SortableBindingList.RemoveColumnDescriptor(propertyName);
    }

    public IEnumerable<PropertyDescriptor> GetPropertyDescriptors() {
      return this._SortableBindingList.GetColumnDescriptors();
    }

    public void Insert(int index, T item) {
      this._SortableBindingList.Insert(index, item);
    }

    public void Add(T item) {
      this._SortableBindingList.Add(item);
    }

    public void Remove(T item) {
      this._SortableBindingList.Remove(item);
    }

    public bool Contains(T item) {
      return this._SortableBindingList.Contains(item);
    }

    public void Refresh() {
      this._SortableBindingList.Refresh();
    }

    // Private Sub SortableBindingList_ListChanged(sender As Object, e As ListChangedEventArgs) Handles _SortableBindingList.ListChanged
    // If (ListChangedEvent IsNot Nothing) Then
    // RaiseEvent ListChanged(Me, e)
    // End If
    // End Sub

  }
}