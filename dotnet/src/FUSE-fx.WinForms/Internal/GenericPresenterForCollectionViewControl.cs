using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace System.Data.Fuse.WinForms.Internal {

  internal class GenericPresenterForCollectionViewControl<TModel> {

    private IListEditingController<TModel> __Backend;

    private IListEditingController<TModel> _Backend {
      [MethodImpl(MethodImplOptions.Synchronized)]
      get {
        return __Backend;
      }

      [MethodImpl(MethodImplOptions.Synchronized)]
      set {
        if (__Backend != null) {
          __Backend.ItemContentChanged -= Backend_ItemContentChanged;
          __Backend.ItemListChanged -= Backend_ItemsChanged;
          __Backend.CurrentItemsChanged -= Backend_CurrentArticlesChanged;
        }

        __Backend = value;
        if (__Backend != null) {
          __Backend.ItemContentChanged += Backend_ItemContentChanged;
          __Backend.ItemListChanged += Backend_ItemsChanged;
          __Backend.CurrentItemsChanged += Backend_CurrentArticlesChanged;
        }
      }
    }
    private CollectionViewDataSource<TModel> __DataSource;

    private CollectionViewDataSource<TModel> _DataSource {
      [MethodImpl(MethodImplOptions.Synchronized)]
      get {
        return __DataSource;
      }

      [MethodImpl(MethodImplOptions.Synchronized)]
      set {
        if (__DataSource != null) {
          __DataSource.ModelEdited -= DataSource_ModelEdited;
        }

        __DataSource = value;
        if (__DataSource != null) {
          __DataSource.ModelEdited += DataSource_ModelEdited;
        }
      }
    }

    private CollectionViewControl __Frontend;

    private CollectionViewControl _Frontend {
      [MethodImpl(MethodImplOptions.Synchronized)]
      get {
        return __Frontend;
      }

      [MethodImpl(MethodImplOptions.Synchronized)]
      set {
        if (__Frontend != null) {
          __Frontend.CellFormatting -= Frontend_CellFormatting;
          __Frontend.KeyDown -= Frontend_KeyDown;
          __Frontend.CellMouseClick -= Frontend_CellMouseClick;
          __Frontend.CellEndEdit -= Frontend_CellEndEdit;
          __Frontend.UserDeletingRow -= Frontend_UserDeletingRow;
          __Frontend.SelectedItemChanged -= Frontend_SelectedIndexChanged;
        }

        __Frontend = value;
        if (__Frontend != null) {
          __Frontend.CellFormatting += Frontend_CellFormatting;
          __Frontend.KeyDown += Frontend_KeyDown;
          __Frontend.CellMouseClick += Frontend_CellMouseClick;
          __Frontend.CellEndEdit += Frontend_CellEndEdit;
          __Frontend.UserDeletingRow += Frontend_UserDeletingRow;
          __Frontend.SelectedItemChanged += Frontend_SelectedIndexChanged;
        }
      }
    }

    private IModelDisplaySchema _DisplaySchema;

    public GenericPresenterForCollectionViewControl(IListEditingController<TModel> backend, CollectionViewControl frontend, IModelDisplaySchema displaySchema, bool allowAdd, bool allowDelete) {

      this._Frontend = frontend;
      this._Backend = backend;
      _DisplaySchema = displaySchema;

      this._Frontend.Grid.AllowUserToAddRows = allowAdd;
      this._Frontend.Grid.AllowUserToDeleteRows = allowDelete;

      this._Backend.ReloadItems();
      this.EnsureDataSourceIsInitialized();
    }

    private void EnsureDataSourceIsInitialized() {
      if (this._Backend.Items == null) {
        this._DataSource = (CollectionViewDataSource<TModel>)null;
        this._Frontend.DataSource = (object)null;
      }
      else {
        if (this._DataSource == null) {
          this._DataSource = new CollectionViewDataSource<TModel>(this._Backend.Items, _DisplaySchema);
          this._DataSource.NewItemInitializer = new Action<TModel>(model => this._Backend.InitializeItemPrototype(model));
          this._Frontend.DataSource = this._DataSource.BindingList;
        }
        this._DataSource.Refresh();
      }
      this._Frontend.Enabled = this._Frontend.DataSource != null;
    }

    #region  Reload 

    public void Reload() {
      this._Backend.ReloadItems();
      this.EnsureDataSourceIsInitialized();
    }

    private void Backend_ItemContentChanged(object sender, ItemContentChangedEventargs<TModel> e) {
      int rowIndex = this._DataSource.GetRowIndexOfObject(e.Item);
      this._DataSource.BindingList.ResetItem(rowIndex);
    }

    private void Backend_ItemsChanged(object sender, ItemListChangedEventargs<TModel> e) {

      if (e.CollectionDirectEdited) {
        // this came already from a direct edit on the _DataSource
        return;
      }

      this.EnsureDataSourceIsInitialized();

      if (this._DataSource == null || this._Backend.Items == null) {
        this._Frontend.Enabled = false;
      }
      else {
        this._Frontend.Enabled = true;
      }

    }

    #endregion

    #region  DataGrid Events 

    private Dictionary<string, List<Action<TModel, DataGridViewCellStyle, object>>> _FormatersPerColumn = null;
    private List<Action<string, TModel, DataGridViewCellStyle, object>> _GlobalFormaters = new List<Action<string, TModel, DataGridViewCellStyle, object>>();

    /// <summary>
  /// The Formating Method will receive (the model for the current row, the cellstyle to modify, the curent cells value)
  /// </summary>
    public void AddCustomFormatter(string columnName, Action<TModel, DataGridViewCellStyle, object> formatingMethod) {
      if (_FormatersPerColumn == null) {
        _FormatersPerColumn = new Dictionary<string, List<Action<TModel, DataGridViewCellStyle, object>>>();
      }
      if (!_FormatersPerColumn.ContainsKey(columnName)) {
        _FormatersPerColumn.Add(columnName, new List<Action<TModel, DataGridViewCellStyle, object>>());
      }
      _FormatersPerColumn[columnName].Add(formatingMethod);
    }

    /// <summary>
  /// The Formating Method will receive (columnName, the model for the current row, the cellstyle to modify, the curent cells value)
  /// </summary>
    public void AddCustomFormatter(Action<string, TModel, DataGridViewCellStyle, object> formatingMethod) {
      _GlobalFormaters.Add(formatingMethod);
    }

    private void Frontend_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e) {

      if (e.ColumnIndex >= this._DataSource.ColumnCount) {
        // this indicates that were confronted with a bug of the datagridview which
        // is re-formating on an old datasource while applying a new datasource 
        return;
      }

      TModel data = default;
      string columnName = null;
      if (_GlobalFormaters.Count > 0) {
        data = this._DataSource.GetObjectAtRowIndex(e.RowIndex);
        columnName = this._DataSource.GetColumnName(e.ColumnIndex);
        foreach (var globalFormater in _GlobalFormaters)
          globalFormater.Invoke(columnName, data, e.CellStyle, e.Value);
      }

      if (_FormatersPerColumn == null) {
        return;
      }

      if (columnName == null) {
        columnName = this._DataSource.GetColumnName(e.ColumnIndex);
      }
      if (_FormatersPerColumn.ContainsKey(columnName)) {
        if (data == null) {
          data = this._DataSource.GetObjectAtRowIndex(e.RowIndex);
        }
        foreach (var formatingMethod in _FormatersPerColumn[columnName])
          formatingMethod.Invoke(data, e.CellStyle, e.Value);
      }
    }

    private void Frontend_KeyDown(object sender, KeyEventArgs e) {
      try {
        switch (e.KeyCode) {
          case Keys.F5: {
              this.Reload();
              break;
            }
          case Keys.Return: {
              break;
            }
        }
      }
      catch (Exception ex) {
        MessageBox.Show(this._Frontend.ParentForm, ex.Message, "Unhadled Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private void Frontend_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e) {
      try {
        // triggers the click events handlers defined inside of the display schema
        this._DataSource.NotifyCellClicked(e.RowIndex, e.ColumnIndex);
      }
      catch (Exception ex) {
        MessageBox.Show(this._Frontend.ParentForm, ex.Message, "Unhadled Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private void Frontend_CellEndEdit(object sender, DataGridViewCellEventArgs e) {
      try {
        this._DataSource.NotifyCellEdited(e.RowIndex, e.ColumnIndex);
      }
      catch (Exception ex) {
        MessageBox.Show(this._Frontend.ParentForm, ex.Message, "Unhadled Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private void Frontend_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e) {
      TModel item = (TModel)e.Row.DataBoundItem;
      if (this._Backend.IsItemDeleteable(item) == false) {
        e.Cancel = true;
        MessageBox.Show(this._Frontend.ParentForm, string.Format("Deletion of item '{0}' is not allowed!", this._Backend.GetItemIdentString(item)), "Invalid Operation", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
      }
    }

    private void DataSource_ModelEdited(TModel model, string columnName, int rowIndex, object value) {

      try {
        this._DataSource.BindingList.ResetItem(rowIndex);
        this.ModelEdited(model);
      }
      catch (Exception ex) {
        MessageBox.Show(this._Frontend.ParentForm, ex.Message, "Unhadled Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }

    }

    #endregion

    #region  Edit / Add / Remove 

    private void ModelEdited(TModel model) {
      this._Backend.ItemWasEdited(model);

      // required when the lis is sorted and the change will affect the position of our item
      // so we need tpo fixup the current selection index
      this.Backend_CurrentArticlesChanged(null, EventArgs.Empty);

    }

    #endregion

    #region  Selection Handling 

    private bool _SelectionChangedSemaphore = false;

    private void Frontend_SelectedIndexChanged(object sender, EventArgs e) {

      try {

        if (_SelectionChangedSemaphore) {
          return;
        }
        else {
          _SelectionChangedSemaphore = true;
        }

        try {
          TModel[] selection = _Frontend.SelectedItems.OfType<TModel>().ToArray();
          this._Backend.CurrentItems = selection;
        }
        finally {
          _SelectionChangedSemaphore = false;
        }
      }

      catch (Exception ex) {
        MessageBox.Show(this._Frontend.ParentForm, ex.Message, "Unhadled Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private void Backend_CurrentArticlesChanged(object sender, EventArgs e) {

      if (_SelectionChangedSemaphore) {
        return;
      }
      else {
        _SelectionChangedSemaphore = true;
      }

      try {
        this._Frontend.SelectMultiple(this._Backend.CurrentItems);
      }
      finally {
        _SelectionChangedSemaphore = false;
      }

    }

    #endregion

  }
}