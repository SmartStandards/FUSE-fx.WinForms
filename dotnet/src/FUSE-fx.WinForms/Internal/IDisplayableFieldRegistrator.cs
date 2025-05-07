using System;
using System.Linq.Expressions;

namespace System.Data.Fuse.WinForms.Internal {

  internal interface IDisplayableFieldRegistrator {

    IFluentDisplayFieldDefinition<TComponent, TProperty> HasPropertyField<TComponent, TProperty>(Expression<Func<TComponent, TProperty>> propertyExpression, string columnName = null) where TComponent : class;
    IFluentDisplayFieldDefinition<TComponent, TProperty> HasCalculatedField<TComponent, TProperty>(Func<TComponent, TProperty> valueGetter, string columnName) where TComponent : class;

    [Obsolete("PLEASE SPECYFY THE 'columnName'")]
    IFluentDisplayFieldDefinition<TComponent, TProperty> HasCalculatedField<TComponent, TProperty>(Func<TComponent, TProperty> valueGetter) where TComponent : class;

    void UnDefineField<TComponent, TProperty>(Expression<Func<TComponent, TProperty>> propertyExpression) where TComponent : class;
    void UnDefineField<TComponent, TProperty>(Func<TComponent, TProperty> valueGetter) where TComponent : class;


    // Sub Register(Of TComponent As Class, TProperty)(caption As String, getDelegateExpression As Expression(Of Func(Of TComponent, TProperty)), Optional columnWidth As Integer = -1)

    // Sub Register(Of TComponent As Class, TProperty)(caption As String, getDelegateExpression As Expression(Of Func(Of TComponent, TProperty)), s As Selection, Optional columnWidth As Integer = -1)

    // Sub Register(Of TComponent As Class, TProperty)(caption As String, getDelegateExpression As Expression(Of Func(Of TComponent, TProperty)), setDelegate As Action(Of TComponent, TProperty), Optional columnWidth As Integer = -1)

    // Sub Register(Of TComponent As Class, TProperty)(caption As String, getDelegateExpression As Expression(Of Func(Of TComponent, TProperty)), setDelegate As Action(Of TComponent, TProperty), s As Selection, Optional columnWidth As Integer = -1)

    // Sub Register(Of TComponent As Class, TProperty)(caption As String, getDelegateExpression As Expression(Of Func(Of TComponent, TProperty)), setDelegate As Action(Of TComponent, TProperty), attributes As IEnumerable(Of Attribute), Optional columnWidth As Integer = -1)

    // Sub Register(Of TComponent As Class, TProperty)(caption As String, getDelegate As Func(Of TComponent, TProperty), getDelegateForSort As Func(Of TComponent, Object), setDelegate As Action(Of TComponent, TProperty), Optional columnWidth As Integer = -1)

    // Sub Register(Of TComponent As Class, TProperty)(caption As String, getDelegateExpression As Expression(Of Func(Of TComponent, TProperty)), getDelegateForSort As Func(Of TComponent, Object), setDelegate As Action(Of TComponent, TProperty), attributes As IEnumerable(Of Attribute))

  }
}