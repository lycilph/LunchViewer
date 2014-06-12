using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Core
{
    public class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        public void VerifyPropertyName(string property_name)
        {
            if (!string.IsNullOrEmpty(property_name) && GetType().GetTypeInfo().GetDeclaredProperty(property_name) == null)
                throw new ArgumentException("Property not found", property_name);
        }

        protected virtual void RaisePropertyChanged(string property_name)
        {
            VerifyPropertyName(property_name);

            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(property_name));
        }

        protected virtual void RaisePropertyChanged<T>(Expression<Func<T>> property_expression)
        {
            var handler = PropertyChanged;
            if (handler == null) return;

            var property_name = GetPropertyName(property_expression);
            handler(this, new PropertyChangedEventArgs(property_name));
        }

        protected static string GetPropertyName<T>(Expression<Func<T>> property_expression)
        {
            if (property_expression == null)
                throw new ArgumentNullException("property_expression");

            var body = property_expression.Body as MemberExpression;
            if (body == null)
                throw new ArgumentException("Invalid argument", "property_expression");

            var property = body.Member as PropertyInfo;
            if (property == null)
                throw new ArgumentException("Argument is not a property", "property_expression");

            return property.Name;
        }

        protected bool Set<T>(Expression<Func<T>> property_expression, ref T field, T new_value)
        {
            if (EqualityComparer<T>.Default.Equals(field, new_value))
                return false;

            field = new_value;
            RaisePropertyChanged(property_expression);
            return true;
        }

        protected bool Set<T>(string property_name, ref T field, T new_value)
        {
            if (EqualityComparer<T>.Default.Equals(field, new_value))
                return false;

            field = new_value;
            RaisePropertyChanged(property_name);
            return true;
        }

        protected bool Set<T>(ref T field, T new_value, [CallerMemberName] string property_name = null)
        {
            return Set(property_name, ref field, new_value);
        }
    }
}
