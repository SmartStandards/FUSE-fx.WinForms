using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

namespace System.Data.Fuse.WinForms.Internal {

  internal class SelectionController<TItem> : IDisposable {

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private List<ISelectionParticipant<TItem>> _Participants = new List<ISelectionParticipant<TItem>>();

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private List<Action> _HandlerMethods = new List<Action>();

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private SelectionState<TItem> _StateContainer;

    public SelectionController() {
      _StateContainer = new SelectionState<TItem>();
    }

    public SelectionController(IStateContainer stateContainer) {
      _StateContainer = stateContainer.Item<SelectionState<TItem>>();
    }

    #region  Participants 

    public ISelectionParticipant<TItem>[] Participants {
      get {
        return _Participants.ToArray();
      }
    }

    public void AddParticipant(ISelectionParticipant<TItem> participant) {
      if (!_Participants.Contains(participant)) {
        _Participants.Add(participant);
        participant.TransmitNewSelection += this.ChangeSelection;
        participant.ReceiveNewSelection(this.CurrentSelection);
      }
    }

    public void RemoveParticipant(ISelectionParticipant<TItem> participant) {
      if (_Participants.Contains(participant)) {
        participant.TransmitNewSelection -= this.ChangeSelection;
        _Participants.Remove(participant);
      }
    }

    #endregion

    public void AddHandlerMethod(Action handler) {
      if (!_HandlerMethods.Contains(handler)) {
        _HandlerMethods.Add(handler);
      }
    }

    public void RemoveHandlerMethod(Action handler) {
      if (_HandlerMethods.Contains(handler)) {
        _HandlerMethods.Remove(handler);
      }
    }

    private void NotifySelectionChanged(ISelectionParticipant<TItem> caller, bool suppressCallback) {
      foreach (var participant in _Participants) {
        if (suppressCallback == false || !ReferenceEquals(participant, caller)) {
          participant.ReceiveNewSelection(_StateContainer.CurrentSelection);
        }
      }
      foreach (var participant in _HandlerMethods)
        participant.Invoke();
    }

    public void ChangeSelection(ISelectionParticipant<TItem> caller, TItem[] newSelection) {
      this.ChangeSelection(caller, newSelection, true);
    }

    public void ChangeSelection(ISelectionParticipant<TItem> caller, TItem[] newSelection, bool suppressCallback) {
      _StateContainer.CurrentSelection = newSelection;
      this.NotifySelectionChanged(caller, suppressCallback);
    }

    public void ClearSelection(ISelectionParticipant<TItem> caller, bool suppressCallback = true) {
      _StateContainer.CurrentSelection = Array.Empty<TItem>();
      this.NotifySelectionChanged(caller, suppressCallback);
    }

    public void ClearSelection() {
      _StateContainer.CurrentSelection = Array.Empty<TItem>();
      this.NotifySelectionChanged(null, true);
    }

    public TItem[] CurrentSelection {
      get {
        return _StateContainer.CurrentSelection;
      }
      set {
        _StateContainer.CurrentSelection = value;
        this.NotifySelectionChanged(null, true);
      }
    }

    #region  IDisposable 

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private bool _AlreadyDisposed = false;

    /// <summary>
  /// Dispose the current object instance
  /// </summary>
    protected virtual void Dispose(bool disposing) {
      if (!_AlreadyDisposed) {
        if (disposing) {
          foreach (var p in _Participants)
            this.RemoveParticipant(p);
        }
        _AlreadyDisposed = true;
      }
    }

    /// <summary>
  /// Dispose the current object instance and suppress the finalizer
  /// </summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public void Dispose() {
      this.Dispose(true);
      GC.SuppressFinalize(this);
    }

    #endregion

  }





  internal class SelectionState<TItem> {

    public TItem[] CurrentSelection { get; set; } = Array.Empty<TItem>();

  }






  internal  delegate void TransmitNewSelectionEventHandler<TItem>(
    ISelectionParticipant<TItem> sender, TItem[] newSelection
  );

  internal interface ISelectionParticipant<TItem> {

    event TransmitNewSelectionEventHandler<TItem> TransmitNewSelection;

    void ReceiveNewSelection(TItem[] newSelection);

  }

}