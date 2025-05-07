
namespace System.Data.Fuse.WinForms.Internal {

  internal partial class CollectionViewControl : System.Windows.Forms.UserControl {

    // UserControl overrides dispose to clean up the component list.
    [System.Diagnostics.DebuggerNonUserCode()]
    protected override void Dispose(bool disposing) {
      try {
        if (disposing && components != null) {
          components.Dispose();
        }
      }
      finally {
        base.Dispose(disposing);
      }
    }

    // Required by the Windows Form Designer
    private System.ComponentModel.IContainer components;

    // NOTE: The following procedure is required by the Windows Form Designer
    // It can be modified using the Windows Form Designer.  
    // Do not modify it using the code editor.
    [System.Diagnostics.DebuggerStepThrough()]
    private void InitializeComponent() {
      components = new System.ComponentModel.Container();
      gDataGridView = new System.Windows.Forms.DataGridView();
      gDataGridView.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(gDataGridView_CellFormatting);
      gDataGridView.KeyPress += new System.Windows.Forms.KeyPressEventHandler(DataGridView_KeyPress);
      gDataGridView.KeyDown += new System.Windows.Forms.KeyEventHandler(DataGridView_KeyDown);
      gDataGridView.KeyUp += new System.Windows.Forms.KeyEventHandler(DataGridView_KeyUp);
      gDataGridView.SelectionChanged += new System.EventHandler(DataGridView_SelectionChanged);
      gDataGridView.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(gDataGridView_CellEndEdit);
      gDataGridView.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(gDataGridView_CellContentClick);
      gDataGridView.CellMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(gDataGridView_CellMouseClick);
      gDataGridView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(gDataGridView_MouseDoubleClick);
      gDataGridView.CellMouseDoubleClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(DataGridView_CellMouseDoubleClick);
      gDataGridView.UserDeletingRow += new System.Windows.Forms.DataGridViewRowCancelEventHandler(gDataGridView_UserDeletingRow);
      gDataGridView.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(gDataGridView_DataError);
      gBindingSource = new System.Windows.Forms.BindingSource(components);
      gBindingSource.ListChanged += new System.ComponentModel.ListChangedEventHandler(BindingSource_ListChanged);
      ((System.ComponentModel.ISupportInitialize)gDataGridView).BeginInit();
      ((System.ComponentModel.ISupportInitialize)gBindingSource).BeginInit();
      this.SuspendLayout();
      // 
      // gDataGridView
      // 
      gDataGridView.AllowUserToDeleteRows = false;
      gDataGridView.BackgroundColor = System.Drawing.SystemColors.Window;
      gDataGridView.BorderStyle = System.Windows.Forms.BorderStyle.None;
      gDataGridView.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
      gDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
      gDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
      gDataGridView.GridColor = System.Drawing.SystemColors.GradientInactiveCaption;
      gDataGridView.Location = new System.Drawing.Point(0, 0);
      gDataGridView.Name = "gDataGridView";
      gDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
      gDataGridView.Size = new System.Drawing.Size(317, 278);
      gDataGridView.TabIndex = 0;
      // 
      // gBindingSource
      // 
      // 
      // CollectionViewControl
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6.0f, 13.0f);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Controls.Add(gDataGridView);
      this.Name = "CollectionViewControl";
      this.Size = new System.Drawing.Size(317, 278);
      ((System.ComponentModel.ISupportInitialize)gDataGridView).EndInit();
      ((System.ComponentModel.ISupportInitialize)gBindingSource).EndInit();
      this.ResumeLayout(false);

    }

    internal System.Windows.Forms.DataGridView gDataGridView;
    internal System.Windows.Forms.BindingSource gBindingSource;

  }
}