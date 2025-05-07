
using System.Data.Fuse.WinForms.Internal;

namespace System.Data.Fuse {

  partial class FuseTable {
    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary> 
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing) {
      if (disposing && (components != null)) {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent() {
      InnerDataGrid = new CollectionViewControl();
      panel1 = new System.Windows.Forms.Panel();
      panel2 = new System.Windows.Forms.Panel();
      this.SuspendLayout();
      // 
      // InnerDataGrid
      // 
      InnerDataGrid.Dock = Windows.Forms.DockStyle.Fill;
      InnerDataGrid.Location = new System.Drawing.Point(0, 64);
      InnerDataGrid.Name = "InnerDataGrid";
      InnerDataGrid.Size = new System.Drawing.Size(846, 495);
      InnerDataGrid.TabIndex = 0;
      // 
      // panel1
      // 
      panel1.Dock = Windows.Forms.DockStyle.Top;
      panel1.Location = new System.Drawing.Point(0, 0);
      panel1.Name = "panel1";
      panel1.Size = new System.Drawing.Size(846, 64);
      panel1.TabIndex = 1;
      // 
      // panel2
      // 
      panel2.BackColor = Drawing.Color.Gray;
      panel2.Dock = Windows.Forms.DockStyle.Bottom;
      panel2.Location = new System.Drawing.Point(0, 559);
      panel2.Name = "panel2";
      panel2.Size = new System.Drawing.Size(846, 32);
      panel2.TabIndex = 2;
      // 
      // FuseTable
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
      this.AutoScaleMode = Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(InnerDataGrid);
      this.Controls.Add(panel1);
      this.Controls.Add(panel2);
      this.Name = "FuseTable";
      this.Size = new System.Drawing.Size(846, 591);
      this.ResumeLayout(false);
    }

    #endregion

    private CollectionViewControl InnerDataGrid;
    private Windows.Forms.Panel panel1;
    private Windows.Forms.Panel panel2;
  }

}
