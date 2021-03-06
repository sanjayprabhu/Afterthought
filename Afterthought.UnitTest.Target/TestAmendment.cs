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
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;

namespace Afterthought.UnitTest.Target
{
	public class TestAmendment<T> : Amendment<T,T>
		where T : Calculator
	{
		public override void Amend()
		{
			// public int Count { get; set; }
			AddProperty(new Property<int>("Count"));

			// public int DoubleCount { get { return Count * 2; } }
			AddProperty(new Property<int>("Three")
			{
				Getter = (instance, property) => instance.One + instance.Two
			});

			// public string LazyRandomName { get { if (lazyRandomName == null) lazyRandomName = "r" + new Random().Next(); return lazyRandomName; } }
			AddProperty(new Property<string>("LazyRandomName")
			{
				LazyInitializer = (instance, property) => "r" + new Random().Next()
			});

			// public int SetResult { set { Result = value; } }
			AddProperty(new Property<int>("SetResult")
			{
				Setter = (instance, property, value) => instance.Result = value
			});

			// public int InitiallyTwelve { get; set; } = 12
			AddProperty(new Property<int>("InitiallyTwelve")
			{
				Initializer = (instance, property) => 12
			});

			// Implement IMath interface
			ImplementInterface<IMath>(

				// Pi
				new Property<decimal>("Pi") { Getter = (instance, property) => 3.14159m },

				// Subtract()
				Method.Create<decimal, decimal, decimal>("Subtract", (instance, method, parameters) => parameters.Param1 - parameters.Param2)
			);

			// Add attributes
			AddAttribute(Attribute<TestAttribute>.Create(typeof(string)));
			AddAttribute(Attribute<TestAttribute>.Create());
			AddAttribute(Attribute<TestAttribute>.Create(5));
			AddAttribute(Attribute<TestAttribute>.Create(new string[] { "Testing", "Two" }));
		}

		public override void Amend<F>(Field<F> field)
		{
			switch (field.Name)
			{
				case "holding1":
					field.AddAttribute(Attribute<TestAttribute>.Create(typeof(string)));
					field.AddAttribute(Attribute<TestAttribute>.Create());
					field.AddAttribute(Attribute<TestAttribute>.Create(5));
					field.AddAttribute(Attribute<TestAttribute>.Create(new string[] { "Testing", "Two" }));
					break;
			}
		}

		public override void Amend(Constructor constructor)
		{
			if (constructor.ConstructorInfo.GetParameters().Length == 0)
			{
				constructor.AddAttribute(Attribute<TestAttribute>.Create(typeof(string)));
				constructor.AddAttribute(Attribute<TestAttribute>.Create());
				constructor.AddAttribute(Attribute<TestAttribute>.Create(5));
				constructor.AddAttribute(Attribute<TestAttribute>.Create(new string[] { "Testing", "Two" }));
			}
		}

		/// <summary>
		/// Apply amendments to existing properties to implement the behavior being tested.
		/// </summary>
		/// <typeparam name="P"></typeparam>
		/// <param name="property"></param>
		public override void Amend<P>(Property<P> property)
		{
			switch (property.Name)
			{
				// Modify Random1 getter set value of Random1 to GetRandom(1) before returning the underlying property value
				case "Random1":
					property.BeforeGet = (instance, propertyName) => instance.Random1 = instance.GetRandom(1);
					property.AddAttribute(Attribute<TestAttribute>.Create(typeof(string)));
					property.AddAttribute(Attribute<TestAttribute>.Create());
					property.AddAttribute(Attribute<TestAttribute>.Create(5));
					property.AddAttribute(Attribute<TestAttribute>.Create(new string[] { "Testing", "Two" }));
					break;

				// Modify Random2 to calculate a random number based on an assigned seed value
				case "Random2":
					property.OfType<int>().AfterGet = (instance, propertyName, result) => instance.GetRandom(result);
					break;

				// Modify ILog.e property
				case "Afterthought.UnitTest.Target.ILog.e":
					property.OfType<decimal>().Getter = (instance, propertyName) => 2.71828m;
					break;

				// Modify Random5 to be just a simple getter/setter using Result property as the backing store
				case "Random5":
					property.OfType<int>().Getter = (instance, propertyName) => instance.Result;
					property.OfType<int>().Setter = (instance, propertyName, value) => instance.Result = value;
					break;

				// Update Result to equal the value assigned to CopyToResult
				case "CopyToResult":
					property.OfType<int>().BeforeSet = (instance, propertyName, oldValue, value) => { instance.Result = value; return value; };
					break;

				// Modify Add to add the old value to the value being assigned
				case "Add":
					property.OfType<int>().BeforeSet = (instance, propertyName, oldValue, value) => oldValue + value;
					break;

				// Initialize InitiallyThirteen to 13
				case "InitiallyThirteen":
					property.OfType<int>().Initializer = (instance, propertyName) => 13;
					break;

				// Initialize ExistingLazyRandomName
				case "ExistingLazyRandomName":
					property.OfType<string>().LazyInitializer = (instance, propertyName) => "r" + new Random().Next();
					break;
			}
		}
			
		/// <summary>
		/// Apply amendments to existing methods to implement the behavior being tested.
		/// </summary>
		/// <param name="method"></param>
		public override void Amend(Method method)
		{
			switch (method.Name)
			{
				// Modify Multiply to also set the Result property to the resulting value
				case "Multiply" :
					method.Before<int, int>((instance, methodName, parameters) =>
					{
						instance.Result = parameters.Param1 * parameters.Param2;

						// Return null to indicate that the original parameters should not be modified
						return null;
					});

					method.AddAttribute(Attribute<TestAttribute>.Create(typeof(string)));
					method.AddAttribute(Attribute<TestAttribute>.Create());
					method.AddAttribute(Attribute<TestAttribute>.Create(5));
					method.AddAttribute(Attribute<TestAttribute>.Create(new string[] { "Testing", "Two" }));
					break;

				// Modify Multiply2 to also set the Result property to the resulting value
				case "Multiply2":
					method.Before((instance, methodName, parameters) =>
					{
						instance.Result = (int)parameters[0] * (int)parameters[1];

						// Return null to indicate that the original parameters should not be modified
						return null;
					});
					break;

				// Modify Divide to change the second parameter value to 1 every time
				case "Divide":
				    method.Before<int, int>((instance, methodName, parameters) =>
				    {
				        parameters.Param2 = 1;

				        // Return the updated parameters to cause the new values to be used by the original implementation
				        return parameters;
				    });
				    break;

				// Modify Divide2 to change the second parameter value to 1 every time
				case "Divide2":
					method.Before((instance, methodName, parameters) =>
					{
						parameters[1] = 1;

						// Return the updated parameters to cause the new values to be used by the original implementation
						return parameters;
					});
					break;

				// Replace implementation of Square to correct coding error
				case "Square":
					method.Implement<int, int>((instance, methodName, parameters) => parameters.Param1 * parameters.Param1);
					break;

				// Modify Double to double each of the input values
				case "Double":
					method.After<int[]>((instance, methodName, parameters) =>
					{
						for (int i = 0; i < parameters.Param1.Length; i++)
							parameters.Param1[i] = parameters.Param1[i] * 2;
					});
					break;

				// Modify Double to double each of the input values
				case "Double2":
					method.After((instance, methodName, parameters) =>
					{
						for (int i = 0; i < ((int[])parameters[0]).Length; i++)
							((int[])parameters[0])[i] = ((int[])parameters[0])[i] * 2;
					});
					break;

				// Modify Sum to return the sum of the input values
				case "Sum":
					method.After<int[], long>((instance, methodName, parameters, returnValue) =>
					{
						return parameters.Param1.Sum();
					});
					break;


				// Modify Sum to return the sum of the input values
				case "Sum2":
					method.After<long>((instance, methodName, parameters, returnValue) =>
					{
						return ((int[])parameters[0]).Sum();
					});
					break;

				// Modify the input values but ignore the return value
				case "Sum3":
					method.After((instance, methodName, parameters) =>
					{
						for (int i = 1; i < ((int[])parameters[0]).Length; i++)
							((int[])parameters[0])[i] = ((int[])parameters[0])[i - 1] + ((int[])parameters[0])[i];
					});
					break;
			}
		}
	}

	public class LogTracker
	{
		public DateTime Split { get; set; }
	}

	public class TestAttribute : System.Attribute
	{
		public int IntValue { get; private set; }
		public Type TypeValue { get; private set; }
		public string[] StringArValue { get; private set; }
		public object ObjectValue { get; private set; }

		public TestAttribute()
		{
			IntValue = -1;
		}

		public TestAttribute(int number)
		{
			IntValue = number;
		}

		public TestAttribute(Type type)
		{
			TypeValue = type;
		}

		public TestAttribute(string[] values)
		{
			StringArValue = values;
		}

		public TestAttribute(object values)
		{
			ObjectValue = values;
		}
	}
}
