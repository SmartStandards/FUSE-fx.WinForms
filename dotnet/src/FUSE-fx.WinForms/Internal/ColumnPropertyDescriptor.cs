using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace System.Data.Fuse.WinForms.Internal {

  internal class ColumnPropertyDescriptor<TComponent, TProperty> : PropertyDescriptor, IColumnPropertyDescriptor where TComponent : class {

    #region  Methods 

    private string _ColumnName;
    private string _ColumnDisplayTitle;

    public ColumnPropertyDescriptor(string columnName, string columnDisplayTitle, IEnumerable<Attribute> attributes) : base(columnName, attributes.ToArray()) {
      _ColumnName = columnName;
      _ColumnDisplayTitle = columnDisplayTitle;
    }

    public override string Name {
      get {
        return _ColumnName;
      }
    }

    public override string DisplayName {
      get {
        return _ColumnDisplayTitle;
      }
    }

    public TValue GetValue<TValue>(object component) {
      return (TValue)this.GetValue(component);
    }

    public override object GetValue(object component) {
      TComponent castedComponent = component as TComponent;
      if (castedComponent != null) {
        return this.GetDelegate(castedComponent);
      }
      else {
        return null;
      }
    }

    public object GetValueForSort(object component) {
      if (this.GetDelegateForSort != null) {
        return this.GetDelegateForSort(component as TComponent);
      }
      else {
        return this.GetValue(component);
      }
    }

    public override bool CanResetValue(object component) {
      return false;
    }

    public override void ResetValue(object component) {
      throw new NotImplementedException();
    }

    public override bool ShouldSerializeValue(object component) {
      return !typeof(System.Drawing.Image).IsAssignableFrom(typeof(TProperty));
    }

    public override void SetValue(object component, object value) {
      this.SetDelegate.Invoke(component as TComponent, (TProperty)value);
      if (this.AfterEditHandler != null) {
        this.AfterEditHandler.Invoke(component as TComponent);
      }
    }

    #endregion

    #region  Properties 

    public Action<TComponent, TProperty> SetDelegate { get; set; } = null;
    public Func<TComponent, TProperty> GetDelegate { get; set; } = null;
    public int ColumnWidth { get; set; } = -1;
    public Func<TComponent, object> GetDelegateForSort { get; set; } = null;

    public Action<TComponent> AfterEditHandler { get; set; } = null;
    private Action<object> IAfterEditHandler {
      get {
        if (this.AfterEditHandler == null) {
          return null;
        }
        return new Action<object>(obj => this.AfterEditHandler.Invoke((TComponent)obj));
      }
    }

    Action<object> IColumnPropertyDescriptor.AfterEditHandler { get => this.IAfterEditHandler; }


    public Action<TComponent, Action> ClickDelegate { get; set; } = null;

    private Action<object, Action> IClickDelegate {
      get {
        if (this.ClickDelegate == null) {
          return null;
        }
        return new Action<object, Action>((obj, wasModified) => this.ClickDelegate.Invoke((TComponent)obj, wasModified));
      }
    }

    Action<object, Action> IColumnPropertyDescriptor.ClickDelegate { get => this.IClickDelegate; }

    public object get_Value(object component) {
      return this.GetValue(component);
    }
    public void set_Value(object component, object value) {
      this.SetValue(component, value);
    }

    public override Type ComponentType {
      get {
        return typeof(TComponent);
      }
    }

    public override Type PropertyType {
      get {
        return typeof(TProperty);
      }
    }

    public override bool IsReadOnly {
      get {
        return this.SetDelegate == null;
      }
    }

    private int IColumnWidth {
      get {
        return this.ColumnWidth;
      }
    }

    int IColumnPropertyDescriptor.ColumnWidth { get => this.IColumnWidth; }

    #endregion


  }

}
