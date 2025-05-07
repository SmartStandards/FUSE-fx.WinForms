using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace System.Data.Fuse.WinForms.Internal {

  internal partial class CollectionViewControl : UserControl {

    #region  Events 

    public event EventHandler SelectedItemChanged;
    public event DataGridViewCellMouseEventHandler CellMouseDoubleClick;
    public event CellFormattingEventHandler CellFormatting;

    public delegate void CellFormattingEventHandler(object sender, DataGridViewCellFormattingEventArgs e);
    public event UserDeletingRowEventHandler UserDeletingRow;

    public delegate void UserDeletingRowEventHandler(object sender, DataGridViewRowCancelEventArgs e);

    // proxy for the wrapped control
    public new event KeyEventHandler KeyUp;
    public new event KeyEventHandler KeyDown;
    public new event KeyPressEventHandler KeyPress;

    #endregion

    #region  Constructor 

    public CollectionViewControl() {

      this.InitializeComponent();

      {
        var withBlock = gDataGridView;

        withBlock.BorderStyle = BorderStyle.None;
        withBlock.AutoGenerateColumns = false;
        withBlock.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        withBlock.RowHeadersVisible = false;
        withBlock.AllowUserToResizeRows = false;
        withBlock.AllowUserToResizeColumns = true;
        withBlock.AllowUserToOrderColumns = false;

        withBlock.AllowUserToAddRows = false;
        withBlock.AllowUserToDeleteRows = false;
        withBlock.ClipboardCopyMode = DataGridViewClipboardCopyMode.Disable;

        withBlock.AlternatingRowsDefaultCellStyle.BackColor = Color.AliceBlue;
        withBlock.ShowRowErrors = false; // NEU

        DesktpModeFont = withBlock.Font;
        TabletModeFont = new Font(DesktpModeFont.Name, 20f);

        DesktpModeRowHeight = withBlock.RowTemplate.Height;
        TabletModeRowHeight = Convert.ToInt32(
          (double)TabletModeFont.CalculateStringSize(
            "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz"
          ).Height * 1.2d
        );

      }

    }

    #endregion

    public override void Refresh() {
      base.Refresh();
      gDataGridView.Refresh();
    }

    #region  Properties 

    private Font DesktpModeFont;
    private Font TabletModeFont;
    private int DesktpModeRowHeight;
    private int TabletModeRowHeight;

    private bool _TabletMode = false;

    public void AutosizeColumnByAvailableSpace(int columnIndex, int minsize = 10) {
      // With Me.gDataGridView.Columns(columnIndex)
      // .Width = .GetPreferredWidth(DataGridViewAutoSizeColumnMode.Fill, True)
      // If (.Width < minsize) Then
      // .Width = minsize
      // End If
      // End With

      {
        var withBlock = gDataGridView;

        int reserved = SystemInformation.VerticalScrollBarWidth;
        switch (withBlock.BorderStyle) {
          case BorderStyle.Fixed3D: {
              reserved += SystemInformation.Border3DSize.Width * 2;
              break;
            }
          case BorderStyle.FixedSingle: {
              reserved += SystemInformation.BorderSize.Width * 2;
              break;
            }
        }
        for (int i = 0, loopTo = withBlock.Columns.Count - 1; i <= loopTo; i++) {
          if (i != columnIndex && withBlock.Columns[i].Displayed) {
            if (withBlock.Columns[i].Width < withBlock.Columns[i].MinimumWidth) {
              reserved += withBlock.Columns[i].MinimumWidth;
            }
            else {
              reserved += withBlock.Columns[i].Width;
            }
          }
        }


        int available = withBlock.Width - reserved;
        if (available > minsize) {
          withBlock.Columns[columnIndex].Width = available;
        }
        else {
          withBlock.Columns[columnIndex].Width = minsize;
        }
      }
    }

    public void AutosizeColumnByContent(int columnIndex, bool byHeaderContent, bool byDataContent, int minsize = 10) {
      {
        var withBlock = gDataGridView.Columns[columnIndex];
        if (byHeaderContent) {
          if (byDataContent) {
            withBlock.Width = withBlock.GetPreferredWidth(DataGridViewAutoSizeColumnMode.AllCells, true);
          }
          else {
            withBlock.Width = withBlock.GetPreferredWidth(DataGridViewAutoSizeColumnMode.ColumnHeader, true);
          }
        }
        else if (byDataContent) {
          withBlock.Width = withBlock.GetPreferredWidth(DataGridViewAutoSizeColumnMode.AllCellsExceptHeader, true);
        }
        if (withBlock.Width < minsize) {
          withBlock.Width = minsize;
        }
      }
    }

    public bool TabletMode {
      get {
        return _TabletMode;
      }
      set {
        if (_TabletMode == value) {
          return;
        }
        _TabletMode = value;

        {
          var withBlock = gDataGridView;
          if (_TabletMode) {
            withBlock.Font = TabletModeFont;
            withBlock.RowTemplate.Height = TabletModeRowHeight;
          }
          else {
            withBlock.Font = DesktpModeFont;
            withBlock.RowTemplate.Height = DesktpModeRowHeight;
          }

          withBlock.ColumnHeadersHeight = withBlock.RowTemplate.Height;
          foreach (DataGridViewRow r in withBlock.Rows)
            r.Height = withBlock.RowTemplate.Height;

        }

      }
    }

    public object DataSource {
      get {
        if (gDataGridView.DataSource != null && gDataGridView.DataSource is BindingSource) {
          return ((BindingSource)gDataGridView.DataSource).DataSource;
        }
        return gDataGridView.DataSource;
      }
      set {

        gBindingSource.ListChanged -= this.BindingSource_ListChanged;
        gBindingSource.DataSource = value;
        gDataGridView.DataSource = gBindingSource;
        gBindingSource.ListChanged += this.BindingSource_ListChanged;

        if (!gDataGridView.AutoGenerateColumns) {
          this.SetupColumns();
        }

      }
    }

    public object[] RowTemplateValues { get; set; }



    #region  Selector 

    public void SelectFirst() {
      gDataGridView.ClearSelection();
      gDataGridView.Rows[0].Selected = true;
    }

    public void SelectLast() {
      gDataGridView.ClearSelection();
      gDataGridView.Rows[gDataGridView.Rows.Count - 1].Selected = true;
    }

    public void SelectIndex(int index) {
      gDataGridView.ClearSelection();
      gDataGridView.Rows[index].Selected = true;
    }

    public void SelectMultiple<T>(Func<T, bool> selector) {
      gDataGridView.ClearSelection();

      foreach (DataGridViewRow row in gDataGridView.Rows) {

        var model = this.GetBoundItemFailsave(row);

        if (model is T && selector.Invoke((T)model)) {
          row.Selected = true;
        }
        else {
          // row.Selected = False  << seems to be a bug, beacause this kills any selection
        }

      }
    }

    public void SelectMultiple<T>(T[] selectedItems) {
      this.SelectMultiple((T item) => selectedItems.Contains(item));
    }

    #endregion

    public bool MultiSelect {
      get {
        return gDataGridView.MultiSelect;
      }
      set {
        gDataGridView.MultiSelect = value;
      }
    }

    public object CurrentItem {
      get {
        return gBindingSource.Current;
      }
    }

    public IEnumerable<object> SelectedItems {
      get {
        return from dataRow in this.SelectedDataRows
               select this.GetBoundItemFailsave(dataRow);
      }
    }

    private object GetBoundItemFailsave(DataGridViewRow dataRow) {
      try {
        return dataRow.DataBoundItem; // this occours sometimes when editing in a prototype-row
      }
      catch {
        return null;
      }
    }

    public IEnumerable<DataGridViewRow> SelectedDataRows {
      get {
        return gDataGridView.SelectedRows.OfType<DataGridViewRow>();
      }
    }

    public IEnumerable<DataGridViewRow> Rows {
      get {
        return gDataGridView.Rows.OfType<DataGridViewRow>();
      }
    }

    public override ContextMenuStrip ContextMenuStrip {
      get {
        return gDataGridView.ContextMenuStrip;
      }
      set {
        gDataGridView.ContextMenuStrip = value;
      }
    }

    public DataGridView Grid {
      get {
        return gDataGridView;
      }
    }

    #endregion

    #region  Methods 

    #region  Proxy for Events 

    private void gDataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e) {
      if (this.CellFormatting != null) {
        CellFormatting?.Invoke(this, e);
      }
    }

    private void DataGridView_KeyPress(object sender, KeyPressEventArgs e) {
      if (this.KeyPress != null) {
        KeyPress?.Invoke(this, e);
      }
    }

    private void DataGridView_KeyDown(object sender, KeyEventArgs e) {
      if (this.KeyDown != null) {
        KeyDown?.Invoke(this, e);
      }
      if (!e.Handled) {

        if (e.KeyCode == Keys.C && e.Control && !gDataGridView.IsCurrentCellInEditMode) {
          var content = new StringBuilder();

          bool firstRow = true;
          foreach (var selectedRow in gDataGridView.SelectedRows.OfType<DataGridViewRow>().Where(r => r.Visible)) {

            if (firstRow) {
              firstRow = false;
            }
            else {
              content.AppendLine();
            }

            bool firstCell = true;
            foreach (var cell in selectedRow.Cells.OfType<DataGridViewCell>().Where(c => c.Visible)) {

              if (firstCell) {
                firstCell = false;
              }
              else {
                content.Append(((char)9).ToString());
              }

              if (cell.Value != null && !typeof(Image).IsAssignableFrom(cell.ValueType)) {
                content.Append(cell.FormattedValue.ToString().Replace(((char)9).ToString(), "    "));
              }

            }

          }

          if (content != null) {
            Clipboard.SetText(content.ToString(), TextDataFormat.Text);
          }
          e.Handled = true;
        }

      }
    }

    // Private Sub CurrentEditingControl_KeyDown(sender As Object, e As KeyEventArgs) Handles _CurrentEditingControl.KeyDown
    // If (e.KeyCode = Keys.C AndAlso e.Control) Then
    // If (gDataGridView.IsCurrentCellInEditMode) Then
    // If (gDataGridView.CurrentCell.Value IsNot Nothing) Then
    // Clipboard.SetText(gDataGridView.CurrentCell.Value.ToString(), TextDataFormat.Text)
    // End If
    // End If
    // e.Handled = True
    // End If
    // End Sub


    // Private WithEvents _CurrentEditingControl As Control

    // Private Sub gDataGridView_EditingControlShowing(sender As Object, e As DataGridViewEditingControlShowingEventArgs) Handles gDataGridView.EditingControlShowing

    // _CurrentEditingControl = e.Control

    // End Sub

    private void DataGridView_KeyUp(object sender, KeyEventArgs e) {
      if (this.KeyUp != null) {
        KeyUp?.Invoke(this, e);
      }
    }

    private void DataGridView_SelectionChanged(object sender, EventArgs e) {
      if (this.SelectedItemChanged != null) {
        SelectedItemChanged?.Invoke(this, e);
      }
    }

    public event CellEndEditEventHandler CellEndEdit;

    public delegate void CellEndEditEventHandler(object sender, DataGridViewCellEventArgs e);
    private void gDataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e) {
      if (this.CellEndEdit != null) {
        CellEndEdit?.Invoke(this, e);
      }
    }

    public event CellContentClickEventHandler CellContentClick;

    public delegate void CellContentClickEventHandler(object sender, DataGridViewCellEventArgs e);
    private void gDataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e) {
      if (this.CellContentClick != null) {
        CellContentClick?.Invoke(this, e);
      }
    }

    public event CellMouseClickEventHandler CellMouseClick;

    public delegate void CellMouseClickEventHandler(object sender, DataGridViewCellMouseEventArgs e);
    private void gDataGridView_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e) {
      if (this.CellMouseClick != null) {
        CellMouseClick?.Invoke(this, e);
      }
    }

    public new event MouseDoubleClickEventHandler MouseDoubleClick;

    public delegate void MouseDoubleClickEventHandler(object sender, MouseEventArgs e);
    private void gDataGridView_MouseDoubleClick(object sender, MouseEventArgs e) {
      if (this.MouseDoubleClick != null) {
        MouseDoubleClick?.Invoke(this, e);
      }
    }

    private void DataGridView_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e) {
      if (this.CellMouseDoubleClick != null) {
        CellMouseDoubleClick?.Invoke(this, e);
      }
    }

    private void gDataGridView_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e) {
      if (this.UserDeletingRow != null) {
        UserDeletingRow?.Invoke(this, e);
      }
    }

    #endregion

    private void SetupColumns() {
      gDataGridView.Columns.Clear();

      var properties = gBindingSource.GetItemProperties(null);
      // Dim defaultValues As New List(Of Object)

      foreach (PropertyDescriptor pd in properties) {

        var column = new DataGridViewColumn();
        SelectionAttribute selectionAttribute = pd.Attributes[typeof(SelectionAttribute)] as SelectionAttribute;
        object defaultValue = null;

        if (selectionAttribute != null) {

          column = new DataGridViewComboBoxColumn();

          ((DataGridViewComboBoxColumn)column).DataSource = selectionAttribute.Selection.DataSource;
          ((DataGridViewComboBoxColumn)column).DisplayMember = selectionAttribute.Selection.DisplayMember;
          ((DataGridViewComboBoxColumn)column).ValueMember = selectionAttribute.Selection.ValueMember;
        }

        // defaultValues.Add(Nothing)
        else if (typeof(Image).IsAssignableFrom(pd.PropertyType)) {
          // column = New DataGridViewImageColumn()
          column = new DataGridViewImageColumnWithoutErrorImage();
        }


        // defaultValues.Add(New Bitmap(1, 1))
        // defaultValue = New Bitmap(1, 1)
        // column.DefaultCellStyle.NullValue = New Bitmap(1, 1)
        // DirectCast(column, DataGridViewImageColumn).DefaultCellStyle.NullValue = defaultValue

        else {
          column = new DataGridViewTextBoxColumn();
          // defaultValues.Add(String.Empty)
        }

        if (pd is IColumnPropertyDescriptor) {
          if (((IColumnPropertyDescriptor)pd).ColumnWidth >= 0) {
            column.Width = ((IColumnPropertyDescriptor)pd).ColumnWidth;
          }
        }

        column.HeaderText = pd.DisplayName;
        column.DataPropertyName = pd.Name;

        gDataGridView.Columns.Add(column);



      }




      // Me.gDataGridView.RowsDefaultCellStyle.NullValue = ""
      // 'Me.gDataGridView.RowTemplate.SetValues(defaultValues.ToArray())
      // '  Me.gDataGridView.

      // For Each col In Me.gDataGridView.Columns

      // If (TypeOf (col) Is DataGridViewImageColumn) Then
      // DirectCast(col, DataGridViewImageColumn).DefaultCellStyle.NullValue = Nothing
      // DirectCast(col, DataGridViewImageColumn).CellTemplate.DefaultNewRowValue
      // End If

      // Next


      // Me.gDataGridView.RowsDefaultCellStyle.NullValue = Nothing
      // Me.gDataGridView.AlternatingRowsDefaultCellStyle.NullValue = Nothing

    }

    private class DataGridViewImageColumnWithoutErrorImage : DataGridViewImageColumn {

      public DataGridViewImageColumnWithoutErrorImage() {
        base.CellTemplate = new ImgCell();
      }

      public override DataGridViewCell CellTemplate {
        get {
          return base.CellTemplate;
        }
        set {
          base.CellTemplate = value;
        }
      }

      private class ImgCell : DataGridViewImageCell {

        private Image _Original;

        public override object DefaultNewRowValue {
          get {
            return new Bitmap(_Original.Width, _Original.Height);
          }
        }

        public ImgCell() {
          _Original = (Image)base.DefaultNewRowValue;
        }

      }

    }

    public void ClearSelection() {
      gDataGridView.ClearSelection();
    }

    public event NewRowAddedEventHandler NewRowAdded;

    public delegate void NewRowAddedEventHandler(object sender, EventArgs e);

    private void OnNewRowAdded(int rowIndex) {

      // Me.gDataGridView.Rows(rowIndex).InheritedStyle.NullValue = ""

      // For Each col In Me.gDataGridView.Columns

      // If (TypeOf (col) Is DataGridViewImageColumn) Then
      // DirectCast(col, DataGridViewImageColumn).DefaultCellStyle.NullValue = Nothing
      // End If

      // Next



      if (this.NewRowAdded != null) {
        NewRowAdded?.Invoke(this, EventArgs.Empty);
      }
    }

    private void BindingSource_ListChanged(object sender, ListChangedEventArgs e) {

      if (e.ListChangedType == ListChangedType.ItemAdded) {

        if (this.Rows.ElementAtOrDefault(e.NewIndex).IsNewRow) {
          // Me.Rows(e.NewIndex).InheritedStyle.NullValue = Nothing
          this.OnNewRowAdded(e.NewIndex);
          return;
        }

        if (gDataGridView.Rows.OfType<DataGridViewRow>().Any()) {

          try {
            gDataGridView.ClearSelection();
            gDataGridView.Rows.OfType<DataGridViewRow>().Last().Selected = true;
            gDataGridView.CurrentCell = gDataGridView.Rows.OfType<DataGridViewRow>().Last().Cells.OfType<DataGridViewCell>().First();
            gDataGridView.CurrentCell.Selected = true;
          }
          catch {
          }

        }
      }

      else if (e.ListChangedType == ListChangedType.ItemDeleted) {

        if (gDataGridView.Rows.OfType<DataGridViewRow>().Any()) {

          try {
            gDataGridView.ClearSelection();
            gDataGridView.Rows.OfType<DataGridViewRow>().First().Selected = true;
            gDataGridView.CurrentCell = gDataGridView.Rows.OfType<DataGridViewRow>().First().Cells.OfType<DataGridViewCell>().First();
            gDataGridView.CurrentCell.Selected = true;
          }
          catch {
          }

        }

      }

    }

    #endregion

    private void gDataGridView_DataError(object sender, DataGridViewDataErrorEventArgs e) {
      e.ThrowException = false;
    }



















    // Private Sub gDataGridView_CellStyleChanged(sender As Object, e As DataGridViewCellEventArgs) Handles gDataGridView.CellStyleChanged
    // 'Dim col = gDataGridView.Columns(e.ColumnIndex)
    // 'If (TypeOf (col) Is DataGridViewImageColumn) Then
    // '  '    DirectCast(col, DataGridViewImageColumn).DefaultCellStyle.NullValue = Nothing
    // '  gDataGridView.Rows(e.RowIndex).Cells(e.ColumnIndex).Style.NullValue = Nothing
    // 'End If

    // End Sub

    // Private Sub gDataGridView_CellParsing(sender As Object, e As DataGridViewCellParsingEventArgs) Handles gDataGridView.CellParsing
    // If (e.RowIndex = 1) Then

    // e.ToString()

    // End If

    // End Sub
    // Private Sub gDataGridView_CellErrorTextNeeded(sender As Object, e As DataGridViewCellErrorTextNeededEventArgs) Handles gDataGridView.CellErrorTextNeeded
    // '  e.ErrorText = "FOO"
    // End Sub




    // Private Sub gDataGridView_ColumnAdded(sender As Object, e As DataGridViewColumnEventArgs) Handles gDataGridView.ColumnAdded
    // If (TypeOf (e.Column) Is DataGridViewImageColumn) Then
    // Dim col = DirectCast(e.Column, DataGridViewImageColumn)
    // Dim cellTemplate = DirectCast(col.CellTemplate, DataGridViewImageCell)


    // col.DefaultCellStyle.NullValue = Nothing
    // col.InheritedStyle.NullValue = Nothing


    // cellTemplate.Style.NullValue = Nothing

    // 'DirectCast(e.Column, DataGridViewImageColumn).CellTemplate.DefaultNewRowValue = Nothing
    // 'DirectCast(e.Column, DataGridViewImageColumn).CellTemplate.FormattedValue = Nothing
    // 'DirectCast(e.Column, DataGridViewImageColumn).CellTemplate.ErrorIconBounds
    // ' gDataGridView.Rows(e.RowIndex).Cells(e.ColumnIndex).Style.NullValue = Nothing
    // End If
    // End Sub

    // Private Sub gDataGridView_DefaultValuesNeeded(sender As Object, e As DataGridViewRowEventArgs) Handles gDataGridView.DefaultValuesNeeded
    // '  e.Row.Cells(0).Value = Nothing
    // End Sub

    // Private Sub gDataGridView_NewRowNeeded(sender As Object, e As DataGridViewRowEventArgs) Handles gDataGridView.NewRowNeeded

    // End Sub

  }
}