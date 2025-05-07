using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;

namespace System.Data.Fuse.WinForms.Internal {

  internal class DirectlyPersistingListEditingController<ModelType> : GenericListEditingController<ModelType>, ISelectionParticipant<ModelType> where ModelType : new() {

    private IPersistenceService<ModelType> _PersistenceService;

    public DirectlyPersistingListEditingController(IPersistenceService<ModelType> persistenceService) {
      _PersistenceService = persistenceService;
      this.CurrentItemsChanged += ListEditingPersistenceAdapter_CurrentItemsChanged;
    }

    public Expression<Func<ModelType, bool>> SearchFilter { get; set; } = null;
    public Expression<Func<ModelType, bool>> ScopeFilter { get; set; } = null;
    public Action<ModelType> ScopeFixup { get; set; } = null;
    public Action<ModelType> NewModelInitializer { get; set; } = null;

    public override void InitializeItemPrototype(ModelType prototype) {
      base.InitializeItemPrototype(prototype);
      if (this.NewModelInitializer != null) {
        this.NewModelInitializer.Invoke(prototype);
      }
      if (this.ScopeFixup != null) {
        this.ScopeFixup.Invoke(prototype);
      }
    }

    protected override IEnumerable<ModelType> GetItems() {

      Expression<Func<ModelType, bool>> combinedFilter = null;
      if (this.ScopeFilter != null) {

        if (this.SearchFilter != null) {
          //TODO: wieder Reparieren!
          //combinedFilter = this.ScopeFilter.AndAlso(this.SearchFilter);
          combinedFilter = this.ScopeFilter;
        }
        else {
          combinedFilter = this.ScopeFilter;
        }
      }
      else if (this.SearchFilter != null) {
        combinedFilter = this.SearchFilter;
      }

      var items = new List<ModelType>();
      if (combinedFilter == null) {
        _PersistenceService.GetAllItems(items);
      }
      else {
        _PersistenceService.GetItemsMatchingFilter(combinedFilter, items);
      }

      return items;
    }

    // todo im gegendsatz zum store kennt der EditingController seinen scope (z.b. das parent item) über ein delegat
    // sodass ein master-slave update stattdinden kann und auch das parent neuer items gleich geserzt wird!!!!

    protected override void OnItemAdded(ModelType item) {
      if (this.ScopeFixup != null) {
        this.ScopeFixup.Invoke(item);
      }
      _PersistenceService.AddNewItem(item);
    }

    protected override void OnItemEdited(ModelType item) {
      // Dim id As StoreSpecificId = Nothing
      // If (_PersistenceService.TryIdentifyByContent(item, id)) Then
      // _PersistenceService.CheckInChanges(id, item, "", False)
      // Else
      // _PersistenceService.CheckInNewItem(item, "", False)
      // ' Throw New Exception("Cannot checkin changes because it does not exist in the underlying database or the key properties have been changed")
      // End If
    }

    protected override void OnItemRemoved(ModelType item) {
      // Dim id As StoreSpecificId = Nothing
      // If (_PersistenceService.TryIdentifyByContent(item, id)) Then
      // _PersistenceService.Delete(id, "")
      // Else
      // 'Throw New Exception("Cannot checkin changes because it does not exist in the underlying database or the key properties have been changed")
      // End If
    }

    public override string GetItemIdentString(ModelType item) {
      return item.ToString();
    }

    public override bool IsItemDeleteable(ModelType item) {
      return default;
      // Dim id As StoreSpecificId = Nothing
      // If (_PersistenceService.TryIdentifyByContent(item, id)) Then
      // Return _PersistenceService.IsDeleteable(id)
      // Else
      // Return True
      // Throw New Exception("Cannot checkin changes because it does not exist in the underlying database or the key properties have been changed")
      // End If
    }




    #region  ISelectionParticipant 

   public event TransmitNewSelectionEventHandler<ModelType> TransmitNewSelection;

    private void ListEditingPersistenceAdapter_CurrentItemsChanged(object sender, EventArgs e) {
      if (this.TransmitNewSelection != null) {
        TransmitNewSelection?.Invoke(this, this.CurrentItems);
      }
    }

    private void ReceiveNewSelection(ModelType[] newSelection) {
      this.CurrentItems = newSelection;
    }

    void ISelectionParticipant<ModelType>.ReceiveNewSelection(ModelType[] newSelection) => this.ReceiveNewSelection(newSelection);

    #endregion

  }
}


// Public Class DirectlyPersistingListEditingController(Of ModelType As New)
// Inherits GenericListEditingController(Of ModelType)
// Implements ISelectionParticipant(Of ModelType)

// Private _PersistenceService As IPersistenceService(Of ModelType)

// Public Sub New(persistenceService As IPersistenceService(Of ModelType))
// _PersistenceService = persistenceService
// End Sub

// Public Property SearchFilter As Func(Of IEnumerable(Of ModelType), IEnumerable(Of ModelType)) = Nothing
// Public Property ScopeFilter As Func(Of IEnumerable(Of ModelType), IEnumerable(Of ModelType)) = Nothing
// Public Property ScopeFixup As Action(Of ModelType) = Nothing
// Public Property NewModelInitializer As Action(Of ModelType) = Nothing

// Private Iterator Function IterateItems(ids As IEnumerable(Of StoreSpecificId)) As IEnumerable(Of ModelType)
// For Each id In ids
// Dim instance As New ModelType
// _PersistenceService.Load(id, instance)
// Yield instance
// Next
// End Function

// Public Overrides Sub InitializeItemPrototype(prototype As ModelType)
// MyBase.InitializeItemPrototype(prototype)
// If (Me.NewModelInitializer IsNot Nothing) Then
// Me.NewModelInitializer.Invoke(prototype)
// End If
// If (Me.ScopeFixup IsNot Nothing) Then
// Me.ScopeFixup.Invoke(prototype)
// End If
// End Sub

// Protected Overrides Function GetItems() As IEnumerable(Of ModelType)
// Dim result = Me.IterateItems(_PersistenceService.FindAllIds())
// If (Me.ScopeFilter IsNot Nothing) Then
// result = Me.ScopeFilter.Invoke(result)
// End If
// If (Me.SearchFilter IsNot Nothing) Then
// result = Me.SearchFilter.Invoke(result)
// End If
// Return result
// End Function

// 'todo im gegendsatz zum store kennt der EditingController seinen scope (z.b. das parent item) über ein delegat
// 'sodass ein master-slave update stattdinden kann und auch das parent neuer items gleich geserzt wird!!!!

// Protected Overrides Sub OnItemAdded(item As ModelType)
// If (Me.ScopeFixup IsNot Nothing) Then
// Me.ScopeFixup.Invoke(item)
// End If
// _PersistenceService.CheckInNewItem(item, "", False)
// End Sub

// Protected Overrides Sub OnItemEdited(item As ModelType)
// Dim id As StoreSpecificId = Nothing
// If (_PersistenceService.TryIdentifyByContent(item, id)) Then
// _PersistenceService.CheckInChanges(id, item, "", False)
// Else
// _PersistenceService.CheckInNewItem(item, "", False)
// ' Throw New Exception("Cannot checkin changes because it does not exist in the underlying database or the key properties have been changed")
// End If
// End Sub

// Protected Overrides Sub OnItemRemoved(item As ModelType)
// Dim id As StoreSpecificId = Nothing
// If (_PersistenceService.TryIdentifyByContent(item, id)) Then
// _PersistenceService.Delete(id, "")
// Else
// 'Throw New Exception("Cannot checkin changes because it does not exist in the underlying database or the key properties have been changed")
// End If
// End Sub

// Public Overrides Function GetItemIdentString(item As ModelType) As String
// Return item.ToString()
// End Function

// Public Overrides Function IsItemDeleteable(item As ModelType) As Boolean
// Dim id As StoreSpecificId = Nothing
// If (_PersistenceService.TryIdentifyByContent(item, id)) Then
// Return _PersistenceService.IsDeleteable(id)
// Else
// Return True
// Throw New Exception("Cannot checkin changes because it does not exist in the underlying database or the key properties have been changed")
// End If
// End Function

// Protected Overrides Sub Cleanup()
// '_PersistenceService.ReleaseLock()
// End Sub

// #Region " ISelectionParticipant "

// Private Event TransmitNewSelection(sender As ISelectionParticipant(Of ModelType), newSelection() As ModelType) Implements ISelectionParticipant(Of ModelType).TransmitNewSelection

// Private Sub ListEditingPersistenceAdapter_CurrentItemsChanged(sender As Object, e As EventArgs) Handles Me.CurrentItemsChanged
// If (TransmitNewSelectionEvent IsNot Nothing) Then
// RaiseEvent TransmitNewSelection(Me, Me.CurrentItems)
// End If
// End Sub

// Private Sub ReceiveNewSelection(newSelection() As ModelType) Implements ISelectionParticipant(Of ModelType).ReceiveNewSelection
// Me.CurrentItems = newSelection
// End Sub

// #End Region

// End Class
