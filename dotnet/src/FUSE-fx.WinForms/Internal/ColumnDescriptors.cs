using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace System.Data.Fuse.WinForms.Internal {

  internal class ColumnDescriptors<T> {

    private Dictionary<string, PropertyDescriptor> _PropertyDescriptorsDictionary;

    public Dictionary<string, PropertyDescriptor> PropertyDescriptorsDictionary {
      get {
        return _PropertyDescriptorsDictionary;
      }
      private set {
        _PropertyDescriptorsDictionary = value;
      }
    }
    public ColumnDescriptors() : this(false) {
    }

    public ColumnDescriptors(bool injectExtensionMethods) {
      this.PropertyDescriptorsDictionary = new Dictionary<string, PropertyDescriptor>();

      var targetType = typeof(T);

      var properties = from p in targetType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                       where !p.Name.StartsWith("p_")
                       where !p.PropertyType.IsGenericType
                       where !p.PropertyType.IsInterface
                       select p;

      foreach (PropertyInfo @property in properties)
        this.Add(this.CreateColumnPropertyDescriptor(@property));

      if (injectExtensionMethods) {

        //TODO: hier war früher ComponentDiscovery - brauchen wir das überhaupt noch?

        //foreach (MethodInfo @method in targetType.GetExtensionMethods()) {
        //  if (!(@method.ReturnType == typeof(void)) && @method.GetParameters().Count() == 1) {
        //    this.Add(this.CreateColumnPropertyDescriptor(@method));
        //  }
        //}

      }

    }

    public ColumnDescriptors(IEnumerable<PropertyDescriptor> propertyDescriptors) {
      this.PropertyDescriptorsDictionary = new Dictionary<string, PropertyDescriptor>();

      foreach (PropertyDescriptor pd in propertyDescriptors)
        this.Add(pd);

    }

    public void Add(string caption, string propertyName, bool readOnly) {
      if (this.PropertyDescriptorsDictionary.ContainsKey(propertyName)) {
        throw new ArgumentException(string.Format("Es existiert schon ein PropertyDescriptor mit Name: {0}, ein PropertyDescriptor mit dem selben Namen kann nicht hinzugefügt werden.", propertyName));
      }

      if (!this.PropertyDescriptorsDictionary.ContainsKey(propertyName)) {
        this.PropertyDescriptorsDictionary.Add(propertyName, this.CreateColumnPropertyDescriptor(caption, propertyName, readOnly));
      }
    }

    public void Add(PropertyDescriptor propertyDescriptor) {

      if (this.PropertyDescriptorsDictionary.ContainsKey(propertyDescriptor.Name) && !this.PropertyDescriptorsDictionary.Values.Contains(propertyDescriptor)) {
        throw new ArgumentException(string.Format("Es existiert schon ein PropertyDescriptor mit Name {0}, ein PropertyDesscriptor mit demselben Namen kann nicht hinzugefügt werden.", propertyDescriptor.Name));
      }

      if (!this.PropertyDescriptorsDictionary.ContainsKey(propertyDescriptor.Name)) {
        this.PropertyDescriptorsDictionary.Add(propertyDescriptor.Name, propertyDescriptor);
      }

    }

    public void Remove(string propertyName) {
      if (this.PropertyDescriptorsDictionary.ContainsKey(propertyName)) {
        this.PropertyDescriptorsDictionary.Remove(propertyName);
      }
    }

    public void Remove(PropertyDescriptor propertyDescriptor) {
      this.Remove(propertyDescriptor.Name);
    }

    private PropertyDescriptor CreateColumnPropertyDescriptor(string caption, string propertyName, bool readOnly) {

      var t = typeof(T);
      var @property = t.GetProperty(propertyName);

      if (@property == null) {
        throw new ArgumentException(string.Format("Eigenschaft mit Name: {0} ist nicht vorhanden!", @property.Name));
      }

      var columnPropertyDescriptorType = typeof(ColumnPropertyDescriptor<,>).MakeGenericType(t, @property.PropertyType);

      try {

        if (readOnly) {
          return (PropertyDescriptor)Activator.CreateInstance(
            columnPropertyDescriptorType, 
            new object[] { caption, this.CreateGetDelegateExpression(t, @property) }
          );
        }
        else {
          return (PropertyDescriptor)Activator.CreateInstance(
            columnPropertyDescriptorType,
            new object[] {
              caption,
              this.CreateGetDelegateExpression(t, @property),
              this.CreateSetDelegate(t, @property)
            }
          );
        }
      }

      catch (Exception ex) {
        throw new Exception(string.Format("Error while creating an instance of columnPropertyDescriptorType '{0}': {1}", columnPropertyDescriptorType.Name, ex.Message), ex);
      }

    }

    private PropertyDescriptor CreateColumnPropertyDescriptor(string caption, MethodInfo @method) {
      var t = typeof(T);
      var columnPropertyDescriptorType = typeof(ColumnPropertyDescriptor<,>).MakeGenericType(t, @method.ReturnType);

      try {
        return (PropertyDescriptor)Activator.CreateInstance(columnPropertyDescriptorType, new[] { @method.Name, this.CreateGetDelegate(t, @method), null, (object)null });
      }
      catch (Exception ex) {
        throw new Exception(string.Format("Error while creating an instance of columnPropertyDescriptorType '{0}' ({1}): {2}", columnPropertyDescriptorType.Name, @method.Name, ex.Message), ex);
      }

    }

    private PropertyDescriptor CreateColumnPropertyDescriptor(PropertyInfo @property) {
      return this.CreateColumnPropertyDescriptor(@property.Name, @property.Name, true);
    }

    private PropertyDescriptor CreateColumnPropertyDescriptor(MethodInfo @method) {
      return this.CreateColumnPropertyDescriptor(@method.Name, @method);
    }

    private LambdaExpression CreateGetDelegateExpression(Type t, PropertyInfo @property) {

      var objParm = Expression.Parameter(@property.DeclaringType, "o");
      var delegateType = typeof(Func<,>).MakeGenericType(t, @property.PropertyType);
      var lambda = Expression.Lambda(delegateType, Expression.Property(objParm, @property.Name), objParm);

      return lambda;
    }

    private Delegate CreateGetDelegate(Type t, MethodInfo @method) {

      var delegateType = typeof(Func<,>).MakeGenericType(t, @method.ReturnType);

      return Delegate.CreateDelegate(delegateType, @method);
    }

    private Delegate CreateSetDelegate(Type t, PropertyInfo @property) {

      if (!@property.CanWrite) {
        return null;
      }

      var objParm = Expression.Parameter(@property.DeclaringType, "o");
      var valueParm = Expression.Parameter(@property.PropertyType, "value");
      var delegateType = typeof(Action<,>).MakeGenericType(@property.DeclaringType, @property.PropertyType);
      var lambda = Expression.Lambda(delegateType, Expression.Assign(Expression.Property(objParm, @property.Name), valueParm), objParm, valueParm);

      return lambda.Compile();
    }

  }
}