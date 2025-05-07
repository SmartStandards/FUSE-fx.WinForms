using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace System.Data.Fuse.WinForms.Internal {

  internal static class ExtensionsGeneric {

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static T CastTo<TSource, T>(this TSource anyObj) {
      return (T)(object)anyObj;
    }

    //[EditorBrowsable(EditorBrowsableState.Advanced)]
    //public static IEnumerable<TargetType> TransformTo<TSource, TargetType>(this IEnumerable<TSource> value, Func<TSource, TargetType> conversionMethod) {
    //  return new Collections.Generic.EnumerableProxy<TSource, TargetType>(value, conversionMethod);
    //}

    //[EditorBrowsable(EditorBrowsableState.Advanced)]
    //public static IEnumerable<string> ToString(this IEnumerable<object> value) {
    //  return new Collections.Generic.EnumerableProxy<object, string>(value, s => s.ToString());
    //}

    #region  Cloning 

    //[EditorBrowsable(EditorBrowsableState.Advanced)]
    //public static T CloneBinary<T>(this T anyObj) {
    //  if (anyObj == null) {
    //    return default;
    //  }
    //  IFormatter formatter = new BinaryFormatter();
    //  Stream stream = new MemoryStream();
    //  using (stream) {
    //    formatter.Serialize(stream, anyObj);
    //    stream.Seek(0L, SeekOrigin.Begin);
    //    return (T)formatter.Deserialize(stream);
    //  }
    //}

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static T CloneValueMembers<T>(this T sourceObject) where T : class, new() {
      var targetObject = Activator.CreateInstance<T>();
      sourceObject.CopyValueMembersTo(targetObject);
      return targetObject;
    }

    public static object CloneValueMembers(object sourceObject) {
      var targetObject = Activator.CreateInstance(sourceObject.GetType());
      CopyValueMembers(sourceObject, targetObject);
      return targetObject;
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static void CopyValueMembersTo<T>(this T sourceObject, T targetObject) where T : class {
      CopyValueMembers(sourceObject, targetObject);
    }

    public static void CopyValueMembers(object sourceObject, object targetObject) {

      PropertyInfo[] sourceObjectProperties = sourceObject.GetType().GetProperties();
      PropertyInfo[] targetObjectProperties = targetObject.GetType().GetProperties();
      PropertyInfo matchingTargetObjectProperty;
      object objectValue;

      foreach (PropertyInfo sourceObjectProperty in sourceObjectProperties) {

        if (sourceObjectProperty.CanRead && CopyAlowed(sourceObjectProperty.PropertyType) && sourceObjectProperty.GetIndexParameters().Count() == 0) {

          matchingTargetObjectProperty = null;
          foreach (PropertyInfo targetObjectProperty in sourceObjectProperties) {
            if ((targetObjectProperty.Name ?? "") == (sourceObjectProperty.Name ?? "")) {
              matchingTargetObjectProperty = targetObjectProperty;
              break;
            }
          }

          if (matchingTargetObjectProperty != null && matchingTargetObjectProperty.CanWrite && matchingTargetObjectProperty.GetIndexParameters().Count() == 0 && matchingTargetObjectProperty.PropertyType == sourceObjectProperty.PropertyType) {

            objectValue = sourceObjectProperty.GetValue(sourceObject, null);
            CloneIfNessecary(ref objectValue);
            matchingTargetObjectProperty.SetValue(targetObject, objectValue, null);
          }

        }

      }

    }

    private static bool CopyAlowed(Type @type) {

      if (type.IsValueType) {
        return true;
      }
      else if (type.IsCollection()) {
        return false;
      }

      switch (type) {

        // non-valuetypes which are allowed to be copied
        case var @case when @case == typeof(string): {
            break;
          }
        case var case1 when case1 == typeof(Version): {
            break;
          }

        default: {

            return false;
          }
      }

      return true;
    }

    private static void CloneIfNessecary(ref object objectInstance) {
      if (objectInstance != null && !objectInstance.GetType().IsValueType) {
        switch (objectInstance.GetType()) {

          // non-valuetypes which are allowed to be copied and requireing a manual clone operation
          case var @case when @case == typeof(Version): {
              objectInstance = ((Version)objectInstance).Clone();
              break;
            }

        }
      }
    }

    #endregion

    #region  Conversion 

    /// <summary>
    /// accepts a format pattern which will be resolved via reflection and can be used like this:
    /// car.ToString("Car: {obj}{br}({AnyPropertyName}/{AnyFunctionName()})")
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Always)]
    public static string ToString<T>(this T anyObj, string format) {

      var foundPlaceHolders = new List<string>();
      var contentString = new StringBuilder();
      var placeHolderString = new StringBuilder();
      bool terminated = false;
      bool inPlaceHolder = false;
      Type targetType;

      foreach (char c in format) {

        if (terminated) {
          terminated = false;
        }
        else {

          switch (c) {

            case '/': {
                terminated = true;
                continue;
              }

            case '{': {
                inPlaceHolder = true;
                continue;
              }

            case '}': {
                if (inPlaceHolder) {
                  if (placeHolderString.Length > 0) {
                    foundPlaceHolders.Add(placeHolderString.ToString());
                    placeHolderString.Clear();
                    contentString.Append("{" + (foundPlaceHolders.Count - 1).ToString() + "}");
                  }
                  inPlaceHolder = false;
                }
                continue;
              }

          }

        }

        if (inPlaceHolder) {
          placeHolderString.Append(c);
        }
        else {
          contentString.Append(c);
        }

      }

      targetType = anyObj.GetType();

      if (placeHolderString.Length > 0) {
        foundPlaceHolders.Add(placeHolderString.ToString());
        placeHolderString.Clear();
        contentString.Append("{" + (foundPlaceHolders.Count - 1).ToString() + "}");
      }

      for (int i = 0, loopTo = foundPlaceHolders.Count - 1; i <= loopTo; i++) {
        if (foundPlaceHolders[i].ToLower() == "br") {
          foundPlaceHolders[i] = Environment.NewLine;
        }
        else if (foundPlaceHolders[i].ToLower() == "obj") {
          foundPlaceHolders[i] = anyObj.ToString();
        }
        else if (foundPlaceHolders[i].EndsWith("()")) {
          foundPlaceHolders[i] = targetType.GetMethod(foundPlaceHolders[i]).Invoke(anyObj, null).ToString();
        }
        else {
          foundPlaceHolders[i] = targetType.GetProperty(foundPlaceHolders[i]).GetValue(anyObj).ToString();
        }
      }

      return string.Format(contentString.ToString(), foundPlaceHolders.ToArray());
    }

    #endregion

  }

}