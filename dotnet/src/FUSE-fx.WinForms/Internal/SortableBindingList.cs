using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;

namespace System.Data.Fuse.WinForms.Internal {

  internal class SortableBindingList<T> : BindingList<T>, ITypedList {

    #region  Static 

    private static ListSortDirection _SortDirection;
    private static PropertyDescriptor _PropertyDescriptor;

    public static SortableBindingList<T> CreateFor(IEnumerable<T> source, IEnumerable<PropertyDescriptor> propertyDescriptors = null) {
      SortableList<T> sourceSortingWrapper;

      if (source is IList<T>) {
        sourceSortingWrapper = new SortableList<T>((IList<T>)source);
      }
      else {
        sourceSortingWrapper = new SortableList<T>(source.ToList());
      }

      return new SortableBindingList<T>(sourceSortingWrapper, propertyDescriptors);
    }

    #endregion

    private SortableList<T> _SourceSortingWrapper = null;
    private ColumnDescriptors<T> _ColumnDescriptors;
    private bool _IsSorted;
    // Private _CollectionChangedDisabled As Boolean

    private SortableBindingList(SortableList<T> sourceSortingWrapper, IEnumerable<PropertyDescriptor> propertyDescriptors = null) : base(sourceSortingWrapper) {
      _SourceSortingWrapper = sourceSortingWrapper;
      this.InitColumnDescriptors(propertyDescriptors);
    }

    public void Refresh() {
      this.ResetBindings();
    }

    #region  Hook for Init of new Lines 

    public Action<T> NewItemInitializer { get; set; } = null;

    protected override object AddNewCore() {

      if (!(_SourceSortingWrapper is IList<T>)) {
        throw new Exception("Addnew is only supported when the datasource is a 'IList(Of )'");
      }

      T newObj = (T)base.AddNewCore();
      if (this.NewItemInitializer != null) {
        this.NewItemInitializer.Invoke(newObj);
      }

      return newObj;
    }

    #endregion

    #region  Descriptors 

    private void InitColumnDescriptors(IEnumerable<PropertyDescriptor> propertyDescriptors) {
      if (propertyDescriptors == null) {
        _ColumnDescriptors = new ColumnDescriptors<T>();
      }
      else {
        _ColumnDescriptors = new ColumnDescriptors<T>(propertyDescriptors);
      }
    }

    public void AddColumnDescriptor(PropertyDescriptor pd) {
      _ColumnDescriptors.Add(pd);
    }

    public void RemoveColumnDescriptor(PropertyDescriptor pd) {
      _ColumnDescriptors.Remove(pd);
    }

    public void AddColumnDescriptor(string caption, string propertyName, bool readOnly) {
      _ColumnDescriptors.Add(caption, propertyName, readOnly);
    }

    public void RemoveColumnDescriptor(string propertyName) {
      _ColumnDescriptors.Remove(propertyName);
    }

    public IEnumerable<PropertyDescriptor> GetColumnDescriptors() {
      PropertyDescriptor[] propertyDescriptorArray;
      propertyDescriptorArray = _ColumnDescriptors.PropertyDescriptorsDictionary.Values.ToArray();
      return new PropertyDescriptorCollection(propertyDescriptorArray).OfType<PropertyDescriptor>();
    }

    private PropertyDescriptorCollection GetItemProperties(PropertyDescriptor[] listAccessors) {
      return new PropertyDescriptorCollection(_ColumnDescriptors.PropertyDescriptorsDictionary.Values.ToArray());
    }

    PropertyDescriptorCollection ITypedList.GetItemProperties(PropertyDescriptor[] listAccessors) => this.GetItemProperties(listAccessors);

    private string GetListName(PropertyDescriptor[] listAccessors) {
      return "DynamicRowCollectionViewDataSource_" + typeof(T).Name;
    }

    string ITypedList.GetListName(PropertyDescriptor[] listAccessors) => this.GetListName(listAccessors);

    #endregion

    #region  Sorting 

    protected override bool SupportsSortingCore {
      get {
        return true;
      }
    }

    protected override void ApplySortCore(PropertyDescriptor @property, ListSortDirection direction) {

      _PropertyDescriptor = @property;
      _SortDirection = direction;

      this.Sort(@property, direction);
    }

    protected override ListSortDirection SortDirectionCore {
      get {
        return _SortDirection;
      }
    }

    protected override PropertyDescriptor SortPropertyCore {
      get {
        return _PropertyDescriptor;
      }
    }

    protected override bool IsSortedCore {
      get {
        return _IsSorted;
      }
    }

    protected override void RemoveSortCore() {

      _IsSorted = base.IsSortedCore;
      _SortDirection = base.SortDirectionCore;
      _PropertyDescriptor = base.SortPropertyCore;

      _SourceSortingWrapper.SortingDelegate = null;
      _IsSorted = true;
      base.OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));

    }

    public void Sort(PropertyDescriptor propertyDescriptor, ListSortDirection direction) {
      _SortDirection = direction;
      this.Sort(new Tuple<PropertyDescriptor, ListSortDirection>(propertyDescriptor, direction));
    }

    public void Sort(params Tuple<PropertyDescriptor, ListSortDirection>[] descriptorDirectionTuples) {
      this.Sort(descriptorDirectionTuples.ToList());
    }

    public void Sort(IEnumerable<Tuple<PropertyDescriptor, ListSortDirection>> descriptorDirectionTuples) {
      Tuple<PropertyDescriptor, ListSortDirection>[] tuples = descriptorDirectionTuples.ToArray();


      _SourceSortingWrapper.SortingDelegate = new Func<IEnumerable<T>, IEnumerable<T>>((itemsToSort) => {

        foreach (Tuple<PropertyDescriptor, ListSortDirection> sortTuple in tuples) {

          var propertyDescriptor = sortTuple.Item1;
          bool isColumnPropertyDescriptor = propertyDescriptor is IColumnPropertyDescriptor;
          var columnPropertyDescriptor = isColumnPropertyDescriptor ? (IColumnPropertyDescriptor)propertyDescriptor : (IColumnPropertyDescriptor)null;
          IOrderedEnumerable<T> itemsAsIOrderedEnumerable = itemsToSort as IOrderedEnumerable<T>;
          bool @ascending = sortTuple.Item2 == ListSortDirection.Ascending;

          itemsToSort = ascending ? itemsAsIOrderedEnumerable != null ? isColumnPropertyDescriptor ? itemsAsIOrderedEnumerable.ThenBy(x => columnPropertyDescriptor.GetValueForSort(x)) : itemsAsIOrderedEnumerable.ThenBy(x => propertyDescriptor.GetValue(x)) : isColumnPropertyDescriptor ? Items.OrderBy(x => columnPropertyDescriptor.GetValueForSort(x)) : Items.OrderBy(x => propertyDescriptor.GetValue(x)) : itemsAsIOrderedEnumerable != null ? isColumnPropertyDescriptor ? itemsAsIOrderedEnumerable.ThenByDescending(x => columnPropertyDescriptor.GetValueForSort(x)) : itemsAsIOrderedEnumerable.ThenByDescending(x => propertyDescriptor.GetValue(x)) : isColumnPropertyDescriptor ? Items.OrderByDescending(x => columnPropertyDescriptor.GetValueForSort(x)) : Items.OrderByDescending(x => propertyDescriptor.GetValue(x));



        }

        return itemsToSort;
      });

      _IsSorted = true;
      base.OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));

      // Dim collection = Items.[Select](Function(x) x)

      // For Each sortTuple As Tuple(Of PropertyDescriptor, ListSortDirection) In descriptorDirectionTuples

      // Dim propertyDescriptor = sortTuple.Item1
      // Dim isColumnPropertyDescriptor = TypeOf propertyDescriptor Is IColumnPropertyDescriptor
      // Dim columnPropertyDescriptor As IColumnPropertyDescriptor = If(isColumnPropertyDescriptor, DirectCast(propertyDescriptor, IColumnPropertyDescriptor), Nothing)
      // Dim itemsAsIOrderedEnumerable = TryCast(collection, IOrderedEnumerable(Of T))
      // Dim ascending = sortTuple.Item2 = ListSortDirection.Ascending

      // collection = If(ascending,
      // (If(itemsAsIOrderedEnumerable IsNot Nothing,
      // (If(isColumnPropertyDescriptor,
      // itemsAsIOrderedEnumerable.
      // ThenBy(Function(x) columnPropertyDescriptor.GetValueForSort(x)),
      // itemsAsIOrderedEnumerable.
      // ThenBy(Function(x) propertyDescriptor.GetValue(x)))),
      // (If(isColumnPropertyDescriptor, Items.OrderBy(Function(x) columnPropertyDescriptor.GetValueForSort(x)),
      // Items.OrderBy(Function(x) propertyDescriptor.GetValue(x)))))),
      // (If(itemsAsIOrderedEnumerable IsNot Nothing,
      // (If(isColumnPropertyDescriptor, itemsAsIOrderedEnumerable.ThenByDescending(Function(x) columnPropertyDescriptor.GetValueForSort(x)),
      // itemsAsIOrderedEnumerable.ThenByDescending(Function(x) propertyDescriptor.GetValue(x)))),
      // (If(isColumnPropertyDescriptor, Items.OrderByDescending(Function(x) columnPropertyDescriptor.GetValueForSort(x)),
      // Items.OrderByDescending(Function(x) propertyDescriptor.GetValue(x)))))))

      // Me.ApplySort(collection)

      // Next

    }

    // Private Sub ApplySort(sortedItems As IEnumerable(Of T))
    // Dim sortedItemsList = sortedItems.ToList()

    // _IsSorted = True

    // Me.Clear()
    // Me.AddRange(sortedItemsList)

    // End Sub

    // Public Sub AddRange(collection As IEnumerable(Of T))
    // Me.ForceDeselectAndDisableCollectionChangedEvent()

    // For Each o As T In collection.ToArray()
    // Me.Add(o)
    // Next

    // Me.EnableCollectionChangedEvent()

    // MyBase.OnListChanged(New ListChangedEventArgs(ListChangedType.Reset, -1))
    // End Sub

    // Private Sub ForceDeselectAndDisableCollectionChangedEvent()

    // If (_CollectionChangedDisabled) Then
    // Return
    // End If

    // Dim collection = Items.ToList()

    // Me.Clear()

    // _CollectionChangedDisabled = True

    // For Each o As T In collection
    // Me.Add(o)
    // Next

    // MyBase.OnListChanged(New ListChangedEventArgs(ListChangedType.Reset, -1))
    // End Sub

    // Private Sub EnableCollectionChangedEvent()

    // If (Not _CollectionChangedDisabled) Then
    // Return
    // End If

    // _CollectionChangedDisabled = False
    // End Sub

    #endregion

  }
}