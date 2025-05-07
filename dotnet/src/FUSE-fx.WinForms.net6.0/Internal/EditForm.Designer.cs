using System.Runtime.CompilerServices;

namespace System.Data.Fuse.WinForms.Internal {

  internal partial class EditForm : System.Windows.Forms.Form {

    // Form overrides dispose to clean up the component list.
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
      gLblEdit = new System.Windows.Forms.Label();
      gBtnOk = new System.Windows.Forms.Button();
      _gPanControls = new System.Windows.Forms.Panel();
      this.SuspendLayout();
      // 
      // gLblEdit
      // 
      gLblEdit.AutoSize = true;
      gLblEdit.Location = new System.Drawing.Point(12, 9);
      gLblEdit.Name = "gLblEdit";
      gLblEdit.Size = new System.Drawing.Size(58, 13);
      gLblEdit.TabIndex = 0;
      gLblEdit.Text = "Bearbeiten";
      // 
      // gBtnOk
      // 
      gBtnOk.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
      gBtnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      gBtnOk.Location = new System.Drawing.Point(497, 348);
      gBtnOk.Name = "gBtnOk";
      gBtnOk.Size = new System.Drawing.Size(75, 23);
      gBtnOk.TabIndex = 2;
      gBtnOk.Text = "OK";
      gBtnOk.UseVisualStyleBackColor = true;
      // 
      // gPanControls
      // 
      _gPanControls.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;

      _gPanControls.Location = new System.Drawing.Point(12, 34);
      _gPanControls.Name = "_gPanControls";
      _gPanControls.Size = new System.Drawing.Size(560, 308);
      _gPanControls.TabIndex = 3;
      // 
      // EditForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6.0f, 13.0f);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(584, 383);
      this.Controls.Add(_gPanControls);
      this.Controls.Add(gBtnOk);
      this.Controls.Add(gLblEdit);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.MinimumSize = new System.Drawing.Size(600, 100);
      this.Name = "EditForm";
      this.ShowIcon = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "Edit";
      this.ResumeLayout(false);
      this.PerformLayout();

    }
    internal System.Windows.Forms.Label gLblEdit;
    internal System.Windows.Forms.Button gBtnOk;
    private System.Windows.Forms.Panel _gPanControls;

    public virtual System.Windows.Forms.Panel gPanControls {
      [MethodImpl(MethodImplOptions.Synchronized)]
      get {
        return _gPanControls;
      }

      [MethodImpl(MethodImplOptions.Synchronized)]
      set {
        _gPanControls = value;
      }
    }
  }
}