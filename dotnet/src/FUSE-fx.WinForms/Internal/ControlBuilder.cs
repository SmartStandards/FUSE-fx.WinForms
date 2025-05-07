using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace System.Data.Fuse.WinForms.Internal {

  internal class ControlBuilder<T> {

    #region  Declarations 

    private T _Item;
    private CollectionViewDataSource<T> _CollectionViewDataSource;
    private BindingSource _BindingSource;

    #endregion

    #region  Constructors 

    private ControlBuilder() {
    }

    public ControlBuilder(T item, IModelDisplaySchema schema) {
      _Item = item;

      var items = new List<T>();
      items.Add(_Item);

      _CollectionViewDataSource = new CollectionViewDataSource<T>(items, schema);
      _BindingSource = new BindingSource();
      _BindingSource.DataSource = _CollectionViewDataSource.BindingList;

    }

    #endregion

    #region  Public Methods 

    public Control Render() {

      var groupBox = new GroupBox();
      var @type = typeof(T);

      groupBox.Text = type.Name;
      groupBox.Dock = DockStyle.Fill;
      groupBox.AutoSize = true;

      var table = this.CreateTable();
      int iRow = 0;

      foreach (PropertyDescriptor propDescr in _BindingSource.GetItemProperties(null)) {
        // Label
        var lbl = new Label();

        lbl.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        lbl.AutoSize = true;
        lbl.Margin = new Padding(3);
        lbl.Padding = new Padding(0, 1, 0, 3);
        lbl.Text = propDescr.DisplayName;

        table.Controls.Add(lbl, 0, iRow);

        // EditingControl
        table.Controls.Add(this.CreateEditingControl(propDescr), 1, iRow);

        iRow += 1;
      }

      groupBox.Controls.Add(table);

      return groupBox;
    }

    #endregion

    #region  Private Methods 

    private TableLayoutPanel CreateTable() {
      var table = new TableLayoutPanel();

      table.Dock = DockStyle.Fill;
      table.Location = new Point(0, 0);
      table.Size = new Size(1101, 266);
      table.TabIndex = 0;

      return table;
    }

    private Control CreateEditingControl(PropertyDescriptor pd) {
      SelectionAttribute selectionAttribute = pd.Attributes[typeof(SelectionAttribute)] as SelectionAttribute;

      if (selectionAttribute != null) {
        var comboBox = new ComboBox();

        comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        comboBox.Enabled = !pd.IsReadOnly;
        comboBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        comboBox.AutoSize = true;
        comboBox.Margin = new Padding(3);
        comboBox.Padding = new Padding(0, 1, 0, 3);
        comboBox.FormattingEnabled = true;
        comboBox.DataSource = selectionAttribute.Selection.DataSource;
        comboBox.DisplayMember = selectionAttribute.Selection.DisplayMember;
        comboBox.ValueMember = selectionAttribute.Selection.ValueMember;

        var binding = new Binding("Text", _BindingSource, pd.Name, true, DataSourceUpdateMode.OnPropertyChanged);
        comboBox.DataBindings.Add(binding);

        return comboBox;
      }
      else {
        var textBox = new TextBox();

        textBox.ReadOnly = pd.IsReadOnly;
        textBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        textBox.AutoSize = true;
        textBox.Margin = new Padding(3);
        textBox.Padding = new Padding(0, 1, 0, 3);

        var binding = new Binding("Text", _BindingSource, pd.Name, true, DataSourceUpdateMode.OnPropertyChanged);
        textBox.DataBindings.Add(binding);

        return textBox;
      }

    }

    #endregion

    #region  Properties 

    public T Item {
      get {
        return (T)_BindingSource.Current;
      }
    }

    public CollectionViewDataSource<T> DataSource {
      get {
        return _CollectionViewDataSource;
      }
    }

    #endregion

  }

}
