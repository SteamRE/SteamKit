using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Globalization;

// Microsoft Patterns and Practice
// http://objectbuilder.codeplex.com/

namespace SteamKit2
{
    /// <summary>
    /// Custom <see cref="PropertyInfo"/> that wraps an existing property and provides 
    /// <c>Reflection.Emit</c>-generated <see cref="GetValue"/> and <see cref="SetValue"/> 
    /// implementations for drastically improved performance over default late-bind 
    /// invoke.
    /// </summary>
    public class FastPropertyInfo : PropertyInfo
    {
        PropertyInfo property;
        SetHandler setValueImpl = null;
        GetHandler getValueImpl = null;

        /// <summary>
        /// Initializes the property and generates the implementation for getter and setter methods.
        /// </summary>
        public FastPropertyInfo(PropertyInfo property)
        {
            this.property = property;

            if (property.CanWrite)
            {
                setValueImpl = DynamicMethodCompiler.CreateSetHandler(property.DeclaringType, property);
            }

            if (property.CanRead)
            {
                getValueImpl = DynamicMethodCompiler.CreateGetHandler(property.DeclaringType, property);
            }
        }

        /// <summary>
        /// See <see cref="PropertyInfo.SetValue(object, object, BindingFlags, Binder, object[], CultureInfo)"/>.
        /// </summary>
        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            if (CanWrite)
            {
                setValueImpl(obj, value);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// See <see cref="PropertyInfo.GetValue(object, BindingFlags, Binder, object[], CultureInfo)"/>.
        /// </summary>
        public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            if (CanRead)
            {
                return getValueImpl(obj);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        #region Pass-through members

        /// <summary>
        /// See <see cref="PropertyInfo.PropertyType"/>.
        /// </summary>
        public override Type PropertyType
        {
            get { return property.PropertyType; }
        }

        /// <summary>
        /// See <see cref="MemberInfo.DeclaringType"/>.
        /// </summary>
        public override Type DeclaringType
        {
            get { return property.DeclaringType; }
        }

        /// <summary>
        /// See <see cref="PropertyInfo.Attributes"/>.
        /// </summary>
        public override PropertyAttributes Attributes
        {
            get { return property.Attributes; }
        }

        /// <summary>
        /// See <see cref="PropertyInfo.CanRead"/>.
        /// </summary>
        public override bool CanRead
        {
            get { return property.CanRead; }
        }

        /// <summary>
        /// See <see cref="PropertyInfo.CanWrite"/>.
        /// </summary>
        public override bool CanWrite
        {
            get { return property.CanWrite; }
        }

        /// <summary>
        /// See <see cref="PropertyInfo.GetAccessors(bool)"/>.
        /// </summary>
        public override MethodInfo[] GetAccessors(bool nonPublic)
        {
            return property.GetAccessors(nonPublic);
        }

        /// <summary>
        /// See <see cref="PropertyInfo.GetGetMethod(bool)"/>.
        /// </summary>
        public override MethodInfo GetGetMethod(bool nonPublic)
        {
            return property.GetGetMethod(nonPublic);
        }

        /// <summary>
        /// See <see cref="PropertyInfo.GetIndexParameters"/>.
        /// </summary>
        public override ParameterInfo[] GetIndexParameters()
        {
            return property.GetIndexParameters();
        }

        /// <summary>
        /// See <see cref="PropertyInfo.GetSetMethod(bool)"/>.
        /// </summary>
        public override MethodInfo GetSetMethod(bool nonPublic)
        {
            return property.GetSetMethod(nonPublic);
        }

        /// <summary>
        /// See <see cref="MemberInfo.GetCustomAttributes(Type, bool)"/>.
        /// </summary>
        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return property.GetCustomAttributes(attributeType, inherit);
        }

        /// <summary>
        /// See <see cref="MemberInfo.GetCustomAttributes(bool)"/>.
        /// </summary>
        public override object[] GetCustomAttributes(bool inherit)
        {
            return property.GetCustomAttributes(inherit);
        }

        /// <summary>
        /// See <see cref="MemberInfo.IsDefined"/>.
        /// </summary>
        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return property.IsDefined(attributeType, inherit);
        }

        /// <summary>
        /// See <see cref="MemberInfo.Name"/>.
        /// </summary>
        public override string Name
        {
            get { return property.Name; }
        }

        /// <summary>
        /// See <see cref="MemberInfo.ReflectedType"/>.
        /// </summary>
        public override Type ReflectedType
        {
            get { return property.ReflectedType; }
        }

        #endregion
    }
}
