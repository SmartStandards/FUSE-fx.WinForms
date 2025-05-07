using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;

namespace System.Data.Fuse.WinForms.Internal {

  internal abstract class PropertyDescriptorFactory {

    public abstract PropertyDescriptor BuildDescriptor();
    public abstract int GetDisplayIndex();

  }

  internal class ColumnPropertyDescriptorFactory<TComponent, TProperty> : PropertyDescriptorFactory, IFluentDisplayFieldDefinition<TComponent, TProperty> where TComponent : class {

    #region  Getter (via Constructor) 

    private Expression<Func<TComponent, TProperty>> _PropertyExperession = null;
    private Func<TComponent, TProperty> _ValueGetter = null;
    private string _ColumnName = null;

    public ColumnPropertyDescriptorFactory(Expression<Func<TComponent, TProperty>> propertyExperession, string columnName = null) {
      _PropertyExperession = propertyExperession;
      _ColumnName = columnName;
      try {
        _ValueGetter = _PropertyExperession.Compile();
      }
      catch (Exception ex) {
        throw new Exception(string.Format("Lambda-Ausdruck {0} kann nicht kompiliert werden!", _PropertyExperession.Body.ToString()), ex);
      }
    }

    public ColumnPropertyDescriptorFactory(Func<TComponent, TProperty> valueGetter, string columnName = null) {
      _ColumnName = columnName;
      _ValueGetter = valueGetter;
    }

    #endregion

    #region  Caption & Width 

    private string _ColumnDisplayTitle = null;
    private int _ColumnWidth = -1;
    private int _DisplayIndex = 10000;

    private IFluentDisplayFieldDefinition<TComponent, TProperty> HasCaption(string caption) {
      _ColumnDisplayTitle = caption;
      return this;
    }

    IFluentDisplayFieldDefinition<TComponent, TProperty> IFluentDisplayFieldDefinition<TComponent, TProperty>.HasCaption(string caption) => this.HasCaption(caption);

    private IFluentDisplayFieldDefinition<TComponent, TProperty> HasColumnWidth(int columnWidth) {
      _ColumnWidth = columnWidth;
      return this;
    }

    IFluentDisplayFieldDefinition<TComponent, TProperty> IFluentDisplayFieldDefinition<TComponent, TProperty>.HasColumnWidth(int columnWidth) => this.HasColumnWidth(columnWidth);

    public IFluentDisplayFieldDefinition<TComponent, TProperty> HasDisplayIndex(int index) {
      _DisplayIndex = index;
      return this;
    }

    #endregion

    #region  Setter 

    private Action<TComponent, TProperty> _Setter = null;

    private IFluentDisplayFieldDefinition<TComponent, TProperty> IsWritableVia(Action<TComponent, TProperty> setter) {
      _Setter = setter;
      return this;
    }

    IFluentDisplayFieldDefinition<TComponent, TProperty> IFluentDisplayFieldDefinition<TComponent, TProperty>.IsWritableVia(Action<TComponent, TProperty> setter) => this.IsWritableVia(setter);

    private IFluentDisplayFieldDefinition<TComponent, TProperty> IsReadOnly() {
      _Setter = null;
      return this;
    }

    IFluentDisplayFieldDefinition<TComponent, TProperty> IFluentDisplayFieldDefinition<TComponent, TProperty>.IsReadOnly() => this.IsReadOnly();

    #endregion

    #region  Sorting 

    private Func<TComponent, object> _SortingMethod = null;

    private IFluentDisplayFieldDefinition<TComponent, TProperty> IsSortableVia(Func<TComponent, object> sortingMethod) {
      _SortingMethod = sortingMethod;
      return this;
    }

    IFluentDisplayFieldDefinition<TComponent, TProperty> IFluentDisplayFieldDefinition<TComponent, TProperty>.IsSortableVia(Func<TComponent, object> sortingMethod) => this.IsSortableVia(sortingMethod);

    #endregion

    #region  Selection Binding 

    private Selection _Selection = (Selection)null;

    private IFluentDisplayFieldDefinition<TComponent, TProperty> IsBoundTo(Selection selection) {
      _Selection = selection;
      return this;
    }

    IFluentDisplayFieldDefinition<TComponent, TProperty> IFluentDisplayFieldDefinition<TComponent, TProperty>.IsBoundTo(Selection selection) => this.IsBoundTo(selection);

    private IFluentDisplayFieldDefinition<TComponent, TProperty> IsNotBound() {
      _Selection = (Selection)null;
      return this;
    }

    IFluentDisplayFieldDefinition<TComponent, TProperty> IFluentDisplayFieldDefinition<TComponent, TProperty>.IsNotBound() => this.IsNotBound();

    #endregion

    #region  Attributes 

    private List<Attribute> _Attributes = new List<Attribute>();

    private IFluentDisplayFieldDefinition<TComponent, TProperty> HasAttribute(Attribute attribute) {
      if (!_Attributes.Contains(attribute)) {
        _Attributes.Add(attribute);
      }
      return this;
    }

    IFluentDisplayFieldDefinition<TComponent, TProperty> IFluentDisplayFieldDefinition<TComponent, TProperty>.HasAttribute(Attribute attribute) => this.HasAttribute(attribute);

    private IFluentDisplayFieldDefinition<TComponent, TProperty> HasAttributes(params Attribute[] attributes) {
      foreach (var attr in attributes)
        this.HasAttribute(attr);
      return this;
    }

    IFluentDisplayFieldDefinition<TComponent, TProperty> IFluentDisplayFieldDefinition<TComponent, TProperty>.HasAttributes(params Attribute[] attributes) => this.HasAttributes(attributes);

    private IFluentDisplayFieldDefinition<TComponent, TProperty> HasAttributes(IEnumerable<Attribute> attributes) {
      foreach (var attr in attributes)
        this.HasAttribute(attr);
      return this;
    }

    IFluentDisplayFieldDefinition<TComponent, TProperty> IFluentDisplayFieldDefinition<TComponent, TProperty>.HasAttributes(IEnumerable<Attribute> attributes) => this.HasAttributes(attributes);

    private IFluentDisplayFieldDefinition<TComponent, TProperty> HasNoAttributes() {
      _Attributes.Clear();
      return this;
    }

    IFluentDisplayFieldDefinition<TComponent, TProperty> IFluentDisplayFieldDefinition<TComponent, TProperty>.HasNoAttributes() => this.HasNoAttributes();

    #endregion

    #region  Event Handler 

    private Action<TComponent, Action> _ClickDelegate = null;
    public IFluentDisplayFieldDefinition<TComponent, TProperty> HandlesChickEventsVia(Action<TComponent, Action> clickDelegate) {
      _ClickDelegate = clickDelegate;
      return this;
    }

    private Action<TComponent> _AfterEditDelegate = null;
    public IFluentDisplayFieldDefinition<TComponent, TProperty> HandlesEditEndVia(Action<TComponent> afterEditDelegate) {
      _AfterEditDelegate = afterEditDelegate;
      return this;
    }

    #endregion

    #region  (FATORY) 

    public override PropertyDescriptor BuildDescriptor() {

      if (string.IsNullOrWhiteSpace(_ColumnName) && _PropertyExperession != null) {
        _ColumnName = StrongTypingHelper.GetProperty(_PropertyExperession).Name;
      }
      if (string.IsNullOrWhiteSpace(_ColumnName)) {
        _ColumnName = _ColumnDisplayTitle;
      }
      if (_ColumnDisplayTitle == null) {
        _ColumnDisplayTitle = _ColumnName;
      }
      if (_ColumnDisplayTitle == null) {
        _ColumnDisplayTitle = string.Empty;
      }
      if (string.IsNullOrWhiteSpace(_ColumnName)) {
        _ColumnName = Guid.NewGuid().ToString();
      }

      var attributes = new List<Attribute>();
      attributes.Add(new DisplayNameAttribute(_ColumnDisplayTitle));
      if (_Selection != null) {
        attributes.Add(new SelectionAttribute(_Selection));
      }

      if (typeof(TProperty) == typeof(bool)) {
        ColumnPropertyDescriptor<TComponent, System.Drawing.Image> descriptor;

        if (_Attributes.Any()) {
          descriptor = new ColumnPropertyDescriptor<TComponent, System.Drawing.Image>(_ColumnName, _ColumnDisplayTitle, attributes.Concat(_Attributes));
        }
        else {
          descriptor = new ColumnPropertyDescriptor<TComponent, System.Drawing.Image>(_ColumnName, _ColumnDisplayTitle, attributes);
        }

        descriptor.SetDelegate = (Action<TComponent, System.Drawing.Image>)null;

        descriptor.GetDelegate = new Func<TComponent, System.Drawing.Image>(
          e => {
            if (_ValueGetter.Invoke(e).CastTo<TProperty, bool>()) {
#if NET5_0_OR_GREATER
              return System.Drawing.Image.FromStream(new MemoryStream(Properties.Resources.CheckBoxOn));
#else
              return Properties.Resources.CheckBoxOn; 
#endif
            }
            else {
#if NET5_0_OR_GREATER
              return System.Drawing.Image.FromStream(new MemoryStream(Properties.Resources.CheckBoxOff));
#else
              return Properties.Resources.CheckBoxOff; 
#endif
            } 
          }
        );

        if (_SortingMethod == null) {
          descriptor.GetDelegateForSort = (e) => _ValueGetter.Invoke(e);
        }
        else {
          descriptor.GetDelegateForSort = _SortingMethod;
        }
        descriptor.ClickDelegate = _ClickDelegate;
        descriptor.AfterEditHandler = _AfterEditDelegate;

        descriptor.ColumnWidth = _ColumnWidth;

        return descriptor;
      }
      else {

        ColumnPropertyDescriptor<TComponent, TProperty> descriptor;
        if (_Attributes.Any()) {
          descriptor = new ColumnPropertyDescriptor<TComponent, TProperty>(_ColumnName, _ColumnDisplayTitle, attributes.Concat(_Attributes));
        }
        else {
          descriptor = new ColumnPropertyDescriptor<TComponent, TProperty>(_ColumnName, _ColumnDisplayTitle, attributes);
        }

        descriptor.SetDelegate = _Setter;
        descriptor.GetDelegate = _ValueGetter;
        descriptor.GetDelegateForSort = _SortingMethod;
        descriptor.ClickDelegate = _ClickDelegate;
        descriptor.AfterEditHandler = _AfterEditDelegate;
        descriptor.ColumnWidth = _ColumnWidth;

        return descriptor;
      }
    }

    public override int GetDisplayIndex() {
      return _DisplayIndex;
    }

#endregion

  }
}