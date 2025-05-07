using System;
using System.ComponentModel;

namespace System.Data.Fuse.WinForms.Internal {

  internal class PopupButton {
    #region ...
    private PopupButton(string key) {
      this.Key = key;
    }

    private string Key { get; set; }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public override int GetHashCode() {
      return this.Key.GetHashCode();
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public override string ToString() {
      return this.Key;
      //TODO: das hier wieder reparieren:
      //return My.Resources.Strings.ResourceManager.GetStringFailsafe("PopupButton_" + this.Key, this.Key);
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static bool operator ==(PopupButton one, PopupButton other) {
      return (one.Key ?? "") == (other.Key ?? "");
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static bool operator !=(PopupButton one, PopupButton other) {
      return (one.Key ?? "") != (other.Key ?? "");
    }

    #endregion

    private static PopupButton _Ok = new PopupButton("Ok");
    public static PopupButton Ok {
      get {
        return _Ok;
      }
    }

    private static PopupButton _Cancel = new PopupButton("Cancel");
    public static PopupButton Cancel {
      get {
        return _Cancel;
      }
    }

    private static PopupButton _Yes = new PopupButton("Yes");
    public static PopupButton Yes {
      get {
        return _Yes;
      }
    }

    private static PopupButton _No = new PopupButton("No");
    public static PopupButton No {
      get {
        return _No;
      }
    }

    private static PopupButton _Retry = new PopupButton("Retry");
    public static PopupButton Retry {
      get {
        return _Retry;
      }
    }

    private static PopupButton _Ignore = new PopupButton("Ignore");
    public static PopupButton Ignore {
      get {
        return _Ignore;
      }
    }

    private static PopupButton _Keep = new PopupButton("Keep");
    public static PopupButton Keep {
      get {
        return _Keep;
      }
    }

    private static PopupButton _Replace = new PopupButton("Replace");
    public static PopupButton Replace {
      get {
        return _Replace;
      }
    }

  }
}