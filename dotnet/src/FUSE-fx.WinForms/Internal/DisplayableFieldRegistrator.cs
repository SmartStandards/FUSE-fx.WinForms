using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;

namespace System.Data.Fuse.WinForms.Internal {

  internal class DisplayableFieldRegistrator : IDisplayableFieldRegistrator {

    private Dictionary<object, PropertyDescriptorFactory> _Factories = new Dictionary<object, PropertyDescriptorFactory>();

    public DisplayableFieldRegistrator() {
    }

    public IFluentDisplayFieldDefinition<TComponent, TProperty> HasPropertyField<TComponent, TProperty>(Expression<Func<TComponent, TProperty>> propertyExpression, string columnName = null) where TComponent : class {
      ColumnPropertyDescriptorFactory<TComponent, TProperty> fluentFactory = (ColumnPropertyDescriptorFactory<TComponent, TProperty>)null;

      if (_Factories.ContainsKey(propertyExpression)) {
        if (_Factories[propertyExpression] is ColumnPropertyDescriptorFactory<TComponent, TProperty>) {
          fluentFactory = (ColumnPropertyDescriptorFactory<TComponent, TProperty>)_Factories[propertyExpression];
        }
        else {
          _Factories.Remove(propertyExpression);
        }
      }

      if (fluentFactory == null) {
        fluentFactory = new ColumnPropertyDescriptorFactory<TComponent, TProperty>(propertyExpression, columnName);
        _Factories.Add(propertyExpression, fluentFactory);
      }

      return fluentFactory;
    }

    [Obsolete("PLEASE SPECYFY THE 'columnName'")]
    public IFluentDisplayFieldDefinition<TComponent, TProperty> HasCalculatedField<TComponent, TProperty>(Func<TComponent, TProperty> valueGetter) where TComponent : class {
      return this.HasCalculatedField(valueGetter, null);
    }

    public IFluentDisplayFieldDefinition<TComponent, TProperty> HasCalculatedField<TComponent, TProperty>(Func<TComponent, TProperty> valueGetter, string columnName) where TComponent : class {
      ColumnPropertyDescriptorFactory<TComponent, TProperty> fluentFactory = (ColumnPropertyDescriptorFactory<TComponent, TProperty>)null;

      if (_Factories.ContainsKey(valueGetter)) {
        if (_Factories[valueGetter] is ColumnPropertyDescriptorFactory<TComponent, TProperty>) {
          fluentFactory = (ColumnPropertyDescriptorFactory<TComponent, TProperty>)_Factories[valueGetter];
        }
        else {
          _Factories.Remove(valueGetter);
        }
      }

      if (fluentFactory == null) {
        fluentFactory = new ColumnPropertyDescriptorFactory<TComponent, TProperty>(valueGetter, columnName);
        _Factories.Add(valueGetter, fluentFactory);
      }

      return fluentFactory;
    }

    public void UnDefineField<TComponent, TProperty>(Func<TComponent, TProperty> valueGetter) where TComponent : class {
      if (_Factories.ContainsKey(valueGetter)) {
        _Factories.Remove(valueGetter);
      }
    }

    public void UnDefineField<TComponent, TProperty>(Expression<Func<TComponent, TProperty>> propertyExpression) where TComponent : class {
      if (_Factories.ContainsKey(propertyExpression)) {
        _Factories.Remove(propertyExpression);
      }
    }

    public IEnumerable<PropertyDescriptor> BuildDescriptors() {
      var descriptors = new List<Tuple<int, PropertyDescriptor>>();
      int autoOrder = 10000;
      foreach (var factory in _Factories.Values) {
        int index = factory.GetDisplayIndex();
        if (index == 10000) {
          index = autoOrder;
          autoOrder += 1;
        }
        descriptors.Add(new Tuple<int, PropertyDescriptor>(index, factory.BuildDescriptor()));
      }
      return descriptors.OrderBy(tpl => tpl.Item1).Select(tpl => tpl.Item2);
    }

  }
}