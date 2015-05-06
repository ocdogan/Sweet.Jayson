using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
#if !(NET3500 || NET3000 || NET2000)
using System.Dynamic;
#endif
using System.Text;

namespace Jayson.Tests
{
	public static class TestClasses
	{
		public const string JsonGitResponse = @"{
  ""pulls"": [
    {
      ""state"": ""open"",
      ""base"": {
        ""label"": ""technoweenie:master"",
        ""ref"": ""master"",
        ""sha"": ""53397635da83a2f4b5e862b5e59cc66f6c39f9c6"",
      },
      ""head"": {
        ""label"": ""smparkes:synchrony"",
        ""ref"": ""synchrony"",
        ""sha"": ""83306eef49667549efebb880096cb539bd436560"",
      },
      ""discussion"": [
        {
          ""type"": ""IssueComment"",
          ""gravatar_id"": ""821395fe70906c8290df7f18ac4ac6cf"",
          ""created_at"": ""2010/10/07 07:38:35 -0700"",
          ""body"": ""Did you intend to remove net/http?  Otherwise, this looks good.  Have you tried running the LIVE tests with it?\r\n\r\n    ruby test/live_server.rb # start the demo server\r\n    LIVE=1 rake"",
          ""updated_at"": ""2010/10/07 07:38:35 -0700"",
          ""id"": 453980,
        },
        {
          ""type"": ""Commit"",
          ""created_at"": ""2010-11-04T16:27:45-07:00"",
          ""sha"": ""83306eef49667549efebb880096cb539bd436560"",
          ""author"": ""Steven Parkes"",
          ""subject"": ""add em_synchrony support"",
          ""email"": ""smparkes@smparkes.net""
        }
      ],
      ""title"": ""Synchrony"",
      ""body"": ""Here's the pull request.\r\n\r\nThis isn't generic EM: require's Ilya's synchrony and needs to be run on its own fiber, e.g., via synchrony or rack-fiberpool.\r\n\r\nI thought about a \""first class\"" em adapter, but I think the faraday api is sync right now, right? Interesting idea to add something like rack's async support to faraday, but that's an itch I don't have right now."",
      ""position"": 4.0,
      ""number"": 15,
      ""votes"": 0,
      ""comments"": 4,
      ""diff_url"": ""https://github.com/technoweenie/faraday/pull/15.diff"",
      ""patch_url"": ""https://github.com/technoweenie/faraday/pull/15.patch"",
      ""labels"": [],
      ""html_url"": ""https://github.com/technoweenie/faraday/pull/15"",
      ""issue_created_at"": ""2010-10-04T12:39:18-07:00"",
      ""issue_updated_at"": ""2010-11-04T16:35:04-07:00"",
      ""created_at"": ""2010-10-04T12:39:18-07:00"",
      ""updated_at"": ""2010-11-04T16:30:14-07:00""
    }
  ]
}";
	
		public static C GetC()
		{
			#if !(NET3500 || NET3000 || NET2000)
			dynamic dObj1 = new MyDynamicObject();
			dObj1.X = 3.3;
			dObj1.Y = "YProp";
			dObj1.DyS1 = "s1";
			dObj1.DyI1 = -101;
			dObj1.DyD1 = -202.303;
			dObj1.DyDy1 = false;

			dynamic eObj1 = new ExpandoObject();
			eObj1.EName = "xyz";
			eObj1.EAge = 33.5;
			eObj1.EBirth = new DateTime(1972, 10, 25);
			eObj1.ExS1 = "s1";
			eObj1.ExI1 = -101;
			eObj1.ExD1 = -202.303;
			eObj1.ExDy1 = true;
			#endif

			var c = new C()
			{
				I1 = 1,
				L1 = long.MaxValue,
				D1 = 34567890.01234567890123456789m,
				D2 = 4.01234567890123456789d,
				E1 = MyEnum.EnumC,
				B1 = true,
				S1 = "String",
				S2 = 5,
				B2 = 6,
				A1 = new A
				{
					// I1 = 7,
					L1 = 2*(long)int.MaxValue,
					D1 = 9,
					D2 = 10,
					O1 = new {
						A1O1 = "1O1A"
					}
				},
				L2 = new List<int?>(),
				L3 = new List<A>(),
				L4 = new DBNull[4],
				L5 = new ArrayList(),
				D3 = new Dictionary<object, object>(),
				D4 = new Hashtable(),
				O2 = new { Z = 'C', W = new { V = 4 } },
				#if !(NET3500 || NET3000 || NET2000)
				O1 = dObj1,
				O3 = eObj1,
				#endif
				O4 = new byte[6] { 
					(byte)(1 + '0'), 
					(byte)(2 + '0'), 
					(byte)(3 + '0'), 
					(byte)(4 + '0'), 
					(byte)(5 + '0'), 
					(byte)(6 + '0') }
			};

			c.A2 = c;

			c.D4.Add ("d41", 131);
			c.D4.Add (true, 132);
			c.D4.Add (133.0, 134);

			c.L2.Add(7);
			c.L2.Add(null);
			c.L2.Add(5);

			c.L3.Add(new A { I1 = 2, D2 = 3 });
			c.L3.Add(new B { I1 = 4, D2 = 5, B1 = true, B2 = (byte)'x' });

			c.L4[0] = DBNull.Value;
			c.L4[1] = DBNull.Value;
			c.L4[2] = DBNull.Value;

			c.L5.Add (1.0);
			c.L5.Add ("Hi");
			c.L5.Add (true);
			c.L5.Add (null);
			c.L5.Add (DBNull.Value);
			c.L5.Add (new ArrayList(new object[] {101, 'x'}));

			c.D3.Add(1, 11);
			c.D3.Add(2, new A
				{
					I1 = 1,
					L1 = 2,
					D1 = 3,
					D2 = 4
				});
			return c;
		}

		public static object GetTypedContainerDto()
		{
			return new TypedContainerDto {
				Date1 = new DateTime(1972, 10, 25, 14, 35, 45, DateTimeKind.Utc),
				Date2 = new DateTime(1972, 10, 25, 14, 35, 45, DateTimeKind.Local),
				Date3 = new DateTime(1972, 10, 25, 14, 35, 45),
				P1 = new ReadOnlyCollection<object>(new List<object> { "s", 2.3, true }),
				P2 = new ReadOnlyCollection<int?>(new List<int?> { null, 34 }),
				ObjectProperty = new ReadOnlyDictionary<string, object>(new Dictionary<string, object>{ 
					{"op", new List<dynamic> { "a", 1 } } }),
				DynamicProperty = 12,
				ByteArray = Encoding.UTF8.GetBytes("Hello world!"),
				Source = new List<TextElementDto> { new TextElementDto {
						ElementId = "text_1",
						ElementType = "text",
						// Raw nesting - won't be escaped
						Content = new ElementContentDto { ElementId = "text_1", Content = "text goes here" },
						Action = new ElementActionDto { ElementId = "text_1", Action = "action goes here" }
					}
				},
				Destination = "Here is the destionation"
			};
		}
	}

	# region Sample Classes

	#if !(NET3500 || NET3000 || NET2000)
	public class MyDynamicObject : DynamicObject
	{
		private Dictionary<string, object> _members = new Dictionary<string, object>();

		public string DyS1 { get; set; }
		public int DyI1 { get; set; }
		public double DyD1 { get; set; }
		public dynamic DyDy1 { get; set; }

		/// <summary>
		/// When a new property is set, 
		/// add the property name and value to the dictionary
		/// </summary>     
		public override bool TrySetMember(SetMemberBinder binder, object value)
		{
			if (!_members.ContainsKey(binder.Name))
				_members.Add(binder.Name, value);
			else
				_members[binder.Name] = value;

			return true;
		}

		/// <summary>
		/// When user accesses something, return the value if we have it
		/// </summary>      
		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			if (_members.ContainsKey(binder.Name))
			{
				result = _members[binder.Name];
				return true;
			}
			else
			{
				return base.TryGetMember(binder, out result);
			}
		}

		/// <summary>
		/// If a property value is a delegate, invoke it
		/// </summary>     
		public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
		{
			if (_members.ContainsKey(binder.Name)
				&& _members[binder.Name] is Delegate)
			{
				result = (_members[binder.Name] as Delegate).DynamicInvoke(args);
				return true;
			}
			else
			{
				return base.TryInvokeMember(binder, args, out result);
			}
		}

		/// <summary>
		/// Return all dynamic member names
		/// </summary>
		/// <returns>
		public override IEnumerable<string> GetDynamicMemberNames()
		{
			return _members.Keys;
		}
	}
	#endif

	public enum MyEnum
	{
		EnumA,
		EnumB,
		EnumC,
		EnumD
	}

	public class A
	{
		public int? I1;
		public long L1 { get; set; }
		public List<int?> L2 { get; set; }

		public decimal? D1;
		public double D2 { get; set; }
		public Dictionary<object, object> D3;

		public MyEnum E1 { get; set; }

		public object O1 { get; set; }
		public object O2 { get; set; }
	}

	public class B : A
	{
		public bool B1;
		public string S1 { get; set; }

		public short S2;
		public byte B2 { get; set; }

		public List<A> L3 { get; set; }
		public DBNull[] L4 { get; set; }

		public object O3 { get; set; }
	}

	public class C : B
	{
		public A A1 { get; set; }
		public A A2 { get; set; }
		public ArrayList L5;

		public Hashtable D4;

		public object O4 { get; set; }
	}

	public class SimpleObj
	{
		public string Value1 { get; set; }
		public string Value2 { get; set; }
	}

	public class VerySimpleJsonValue
	{
		public object Value { get; set; }
	}

	public class NestedJsonValueDto
	{
		public string ElementType { get; set; }
		public string ElementId { get; set; }

		public ElementContentDto Content { get; set; }
		public ElementActionDto Action { get; set; }

		public List<object> Scaffolding { get; set; }
	}

	public class TypedContainerDto
	{
		public DateTime Date1;
		public DateTime Date2;
		public DateTime Date3;
		public ReadOnlyCollection<object> P1 { get; set; }
		public ReadOnlyCollection<int?> P2 { get; set; }
		public byte[] ByteArray { get; set; }
		public object ObjectProperty { get; set; }
		public dynamic DynamicProperty { get; set; }
		public List<TextElementDto> Source { get; set; }
		public string Destination { get; set; } // This will be some ElementDto
	}

	// DTOs
	public class StringContainerDto // This is the request dto
	{
		public string Source { get; set; } // This will be some ElementDto
		public string Destination { get; set; } // This will be some ElementDto
	}

	// DTOs
	public class JsonValueContainerDto // This is the request dto
	{
		public object Source { get; set; } // This will be some ElementDto
		public object Destination { get; set; } // This will be some ElementDto
	}

	public class TextElementDto
	{
		public string ElementType { get; set; }
		public string ElementId { get; set; }

		public ElementContentDto Content { get; set; }
		public ElementActionDto Action { get; set; }
	}

	public class ImageElementDto
	{
		public string ElementType { get; set; }
		public string ElementId { get; set; }

		public string Content { get; set; }
		public string Action { get; set; }
	}

	public class ElementContentDto
	{
		public string ElementId { get; set; }
		public string Content { get; set; }
		// There can be more nested objects in here
	}

	public class ElementActionDto
	{
		public string ElementId { get; set; }
		public string Action { get; set; }
		// There can be more nested objects in here
	}

	# endregion Sample Classes
}

