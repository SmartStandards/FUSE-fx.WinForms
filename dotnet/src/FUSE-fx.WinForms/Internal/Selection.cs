using System.Collections;

namespace System.Data.Fuse.WinForms.Internal {

  internal class Selection {

    private IList _DataSource;
    private string _DisplayMember;
    private string _ValueMember;

    public Selection(IList dataSource) {
      _DataSource = dataSource;
    }

    public Selection(IList dataSource, string displayMember, string valueMember) {
      _DataSource = dataSource;
      _DisplayMember = displayMember;
      _ValueMember = valueMember;
    }

    public IList DataSource {
      get {
        return _DataSource;
      }
      set {
        _DataSource = value;
      }
    }

    public string DisplayMember {
      get {
        return _DisplayMember;
      }
      set {
        _DisplayMember = value;
      }
    }

    public string ValueMember {
      get {
        return _ValueMember;
      }
      set {
        _ValueMember = value;
      }
    }

  }
}