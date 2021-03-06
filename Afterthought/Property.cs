﻿//-----------------------------------------------------------------------------
//
// Copyright (c) VC3, Inc. All rights reserved.
// This code is licensed under the Microsoft Public License.
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//-----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Afterthought
{
	#region Amendment.Property

	/// <summary>
	/// Abstract base class for concrete <see cref="Amendment<TType, TAmended>"/> which supports amending code into 
	/// a specific <see cref="Type"/> during compilation.
	/// </summary>
	public abstract partial class Amendment
	{
		public abstract class Property : InterfaceMember, IPropertyAmendment
		{
			internal Property(string name)
				: base(name)
			{ }

			internal Property(PropertyInfo property)
				: base(property.Name)
			{
				this.PropertyInfo = property;
			}

			public abstract Type Type { get; }

			public PropertyInfo PropertyInfo { get; private set; }

			public override bool IsAmended
			{
				get
				{
					return base.IsAmended || LazyInitializerMethod != null || InitializerMethod != null || GetterMethod != null || SetterMethod != null ||
						BeforeGetMethod != null || AfterGetMethod != null || BeforeSetMethod != null || AfterSetMethod != null;
				}
			}

			PropertyInfo IPropertyAmendment.Implements { get { return Implements; } }

			MethodInfo IPropertyAmendment.LazyInitializer { get { return LazyInitializerMethod; } }

			MethodInfo IPropertyAmendment.Initializer { get { return InitializerMethod; } }

			MethodInfo IPropertyAmendment.Getter { get { return GetterMethod; } }

			MethodInfo IPropertyAmendment.Setter { get { return SetterMethod; } }
			
			MethodInfo IPropertyAmendment.BeforeGet { get { return BeforeGetMethod; } }

			MethodInfo IPropertyAmendment.AfterGet { get { return AfterGetMethod; } }

			MethodInfo IPropertyAmendment.BeforeSet { get { return BeforeSetMethod; } }

			MethodInfo IPropertyAmendment.AfterSet { get { return AfterSetMethod; } }

			PropertyInfo implements;
			internal PropertyInfo Implements
			{
				get
				{
					return implements;
				}
				set
				{
					if (implements != null)
						throw new InvalidOperationException("The property implementation may only be set once.");
					implements = value;
					if (implements != null)
						Name = implements.DeclaringType.FullName + "." + implements.Name;
				}
			}

			internal MethodInfo LazyInitializerMethod { get; set; }

			internal MethodInfo InitializerMethod { get; set; }

			internal MethodInfo GetterMethod { get; set; }

			internal MethodInfo SetterMethod { get; set; }
			
			internal MethodInfo BeforeGetMethod { get; set; }

			internal MethodInfo AfterGetMethod { get; set; }

			internal MethodInfo BeforeSetMethod { get; set; }

			internal MethodInfo AfterSetMethod { get; set; }

			/// <summary>
			/// Creates a concrete property with the specified instance type, property type, and name.
			/// </summary>
			/// <param name="instanceType"></param>
			/// <param name="propertyType"></param>
			/// <param name="name"></param>
			/// <returns></returns>
			public static Property Create(Type instanceType, Type propertyType, string name)
			{
				Type amendmentType = typeof(Amendment<,>).MakeGenericType(instanceType, instanceType);
				Type propertyAmendmentType = amendmentType.GetNestedType("Property`1").MakeGenericType(instanceType, instanceType, propertyType);
				return (Property)propertyAmendmentType.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(string) }, null).Invoke(new object[] { name });
			}

			/// <summary>
			/// Creates a new <see cref="Property"/> that implements the specified interface property.
			/// </summary>
			/// <param name="instanceType"></param>
			/// <param name="interfaceProperty"></param>
			/// <returns></returns>
			public static Property Implement(Type instanceType, PropertyInfo interfaceProperty)
			{
				// Ensure the property is declared on an interface
				if (!interfaceProperty.DeclaringType.IsInterface)
					throw new ArgumentException("Only interface properties may be implemented.");

				var property = Create(instanceType, interfaceProperty.PropertyType, interfaceProperty.Name);
				property.Implements = interfaceProperty;
				return property;
			}
		}
	}

	#endregion

	#region Amendment<TType, TAmended>.Property<TProperty>

	public partial class Amendment<TType, TAmended> : Amendment
	{
		public class Property<TProperty> : Property
		{
			public Property(string name)
				: base(name)
			{ }

			internal Property(PropertyInfo prop)
				: base(prop)
			{ }

			protected virtual Property UnderlyingProperty
			{
				get
				{
					return this;
				}
			}

			public override Type Type
			{
				get
				{
					return typeof(TProperty);
				}
			}

			public PropertyGetter Getter { set { UnderlyingProperty.GetterMethod = value.Method; } }

			public PropertySetter Setter { set { UnderlyingProperty.SetterMethod = value.Method; } }

			public PropertyInitializer Initializer { set { UnderlyingProperty.InitializerMethod = value.Method; } }

			public PropertyInitializer LazyInitializer { set { UnderlyingProperty.LazyInitializerMethod = value.Method; } }

			public BeforeProperyGet BeforeGet { set { UnderlyingProperty.BeforeGetMethod = value.Method; } }

			public AfterProperyGet AfterGet { set { UnderlyingProperty.AfterGetMethod = value.Method; } }

			public BeforePropertySet BeforeSet { set { UnderlyingProperty.BeforeSetMethod = value.Method; } }

			public AfterPropertySet AfterSet { set { UnderlyingProperty.AfterSetMethod = value.Method; } }

			public delegate TProperty PropertyInitializer(TAmended instance, string propertyName);

			public delegate TProperty PropertyGetter(TAmended instance, string propertyName);

			public delegate void PropertySetter(TAmended instance, string propertyName, TProperty value);

			public delegate void BeforeProperyGet(TAmended instance, string propertyName);

			public delegate TProperty AfterProperyGet(TAmended instance, string propertyName, TProperty returnValue);

			public delegate TProperty BeforePropertySet(TAmended instance, string propertyName, TProperty oldValue, TProperty value);

			public delegate void AfterPropertySet(TAmended instance, string propertyName, TProperty oldValue, TProperty value, TProperty newValue);

			public Property<TActual> OfType<TActual>()
			{
				return new Property<TProperty, TActual>(this);
			}
		}
	}

	#endregion

	#region Amendment<TType, TAmended>.Property<TProperty, TAmended>

	public partial class Amendment<TType, TAmended> : Amendment
	{
		internal class Property<TProperty, TAmended> : Property<TAmended>
		{
			Property<TProperty> property;

			internal Property(Property<TProperty> property)
				: base(property.PropertyInfo)
			{
				this.property = property;
			}

			protected override Property UnderlyingProperty
			{
				get
				{
					return property;
				}
			}
		}
	}

	#endregion

}
