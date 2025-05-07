using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Fuse.WinForms.Internal;

using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace System.Data.Fuse {

  public partial class FuseTable : UserControl {

    public FuseTable() {
      this.InitializeComponent();
    }

#if NET5_0_OR_GREATER
//TEMPORÄR, BIN ES FUSE FÜR .NET 4.8 GIBT

    private System.Data.ModelDescription.EntitySchema _Schema = null;
    public System.Data.ModelDescription.EntitySchema EntitySchema {
      get {
        return _Schema;
      }
      set {
        _Schema = value;
      }
    }

    public void BindToRepository<TEntity, TKey>(IRepository<TEntity, TKey> repository) where TEntity: class {




      //TODO: das hier implementieren und dann die fuse-repo-features nachbauen



      //this.InnerDataGrid.DataSource = new CollectionViewDataSource<TEntity>()














    }


#endif
  }

}
