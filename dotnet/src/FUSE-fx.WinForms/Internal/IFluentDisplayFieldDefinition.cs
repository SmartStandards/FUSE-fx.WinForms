using System;
using System.Collections.Generic;

namespace System.Data.Fuse.WinForms.Internal {

  /// <summary>
  /// Interface for Fluent Configuration
  /// </summary>
  internal interface IFluentDisplayFieldDefinition<TComponent, TProperty> {

    IFluentDisplayFieldDefinition<TComponent, TProperty> HasCaption(string caption);

    IFluentDisplayFieldDefinition<TComponent, TProperty> HasColumnWidth(int columnWidth);
    IFluentDisplayFieldDefinition<TComponent, TProperty> HasDisplayIndex(int index);

    IFluentDisplayFieldDefinition<TComponent, TProperty> IsWritableVia(Action<TComponent, TProperty> setter);
    IFluentDisplayFieldDefinition<TComponent, TProperty> IsReadOnly();

    IFluentDisplayFieldDefinition<TComponent, TProperty> IsBoundTo(Selection s);
    IFluentDisplayFieldDefinition<TComponent, TProperty> IsNotBound();

    IFluentDisplayFieldDefinition<TComponent, TProperty> IsSortableVia(Func<TComponent, object> sortingMethod);

    IFluentDisplayFieldDefinition<TComponent, TProperty> HasAttribute(Attribute attribute);
    IFluentDisplayFieldDefinition<TComponent, TProperty> HasAttributes(IEnumerable<Attribute> attributes);
    IFluentDisplayFieldDefinition<TComponent, TProperty> HasAttributes(params Attribute[] attributes);
    IFluentDisplayFieldDefinition<TComponent, TProperty> HasNoAttributes();

    IFluentDisplayFieldDefinition<TComponent, TProperty> HandlesChickEventsVia(Action<TComponent, Action> clickDelegate);

    IFluentDisplayFieldDefinition<TComponent, TProperty> HandlesEditEndVia(Action<TComponent> afterEditDelegate);

  }
}