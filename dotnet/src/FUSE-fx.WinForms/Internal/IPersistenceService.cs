using System;
using System.Collections.Generic;

namespace System.Data.Fuse.WinForms.Internal {

  internal interface IPersistenceService<ModelType> : IDisposable where ModelType : new() {

    int GetAllItems(IList<ModelType> targetBuffer);
    int GetItemsMatchingFilter(System.Linq.Expressions.Expression<Func<ModelType, bool>> filterExpression, IList<ModelType> targetBuffer);

    void AddNewItem(ModelType item);

    bool ContainsItemMatchingFilter(System.Linq.Expressions.Expression<Func<ModelType, bool>> filterExpression);
    int CountItemsMatchingFilter(System.Linq.Expressions.Expression<Func<ModelType, bool>> filterExpression);
    int CountItems();

    int UpdateItemsMatchingFilter(System.Linq.Expressions.Expression<Func<ModelType, bool>> filterExpression, ModelType newData);

    int DeleteItemsMatchingFilter(System.Linq.Expressions.Expression<Func<ModelType, bool>> filterExpression);

  }

}
