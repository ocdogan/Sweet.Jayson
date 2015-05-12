# region License
//	The MIT License (MIT)
//
//	Copyright (c) 2015, Cagatay Dogan
//
//	Permission is hereby granted, free of charge, to any person obtaining a copy
//	of this software and associated documentation files (the "Software"), to deal
//	in the Software without restriction, including without limitation the rights
//	to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//	copies of the Software, and to permit persons to whom the Software is
//	furnished to do so, subject to the following conditions:
//
//		The above copyright notice and this permission notice shall be included in
//		all copies or substantial portions of the Software.
//
//		THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//		IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//		FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//		AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//		LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//		OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//		THE SOFTWARE.
# endregion License

using System;

namespace Sweet.Jayson
{
	# region JaysonSerializationSettings

	public class JaysonSerializationSettings : ICloneable
	{
		public static readonly JaysonSerializationSettings Default = new JaysonSerializationSettings();
		private static readonly JaysonSerializationSettings Initial = new JaysonSerializationSettings ();

		public string DateTimeFormat;
		public string DateTimeOffsetFormat;
		public string NumberFormat;
		public string TimeSpanFormat;

		public bool CaseSensitive = true;
		public bool ConvertDecimalToDouble = false;
		public bool DisableAnonymousTypes = false;
		#if !(NET3500 || NET3000 || NET2000)
		public bool DisableDynamicObjects = false;
		public bool DisableExpandoObjects = false;
		#endif
		public bool EscapeChars = true;
		public bool EscapeUnicodeChars = false;
		public bool Formatting = false;
		public bool IgnorePrimitiveTypeNames = true;
		public bool IgnoreNullValues = true;
        public bool IgnoreNullListItems = false;
		public bool OrderNames = false;
		public bool RaiseErrorOnCircularRef = false;
		public bool RaiseErrorOnMaxObjectDepth = true;
		public bool UseEnumNames = true;

		public int MaxObjectDepth;

		public JaysonDateFormatType DateFormatType = JaysonDateFormatType.Iso8601;
		public JaysonDateTimeZoneType DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;
		public JaysonTypeNameSerialization TypeNames = JaysonTypeNameSerialization.None;
        public JaysonTypeNameInfo TypeNameInfo = JaysonTypeNameInfo.TypeNameWithAssembly;

		internal void AssignTo(JaysonSerializationSettings destination)
		{
			destination.CaseSensitive = CaseSensitive;
			destination.ConvertDecimalToDouble = ConvertDecimalToDouble;
			destination.DateFormatType = DateFormatType;
			destination.DateTimeFormat = DateTimeFormat;
			destination.DateTimeOffsetFormat = DateTimeOffsetFormat;
			destination.DateTimeZoneType = DateTimeZoneType;
			destination.TimeSpanFormat = TimeSpanFormat;
			destination.DisableAnonymousTypes = DisableAnonymousTypes;
			#if !(NET3500 || NET3000 || NET2000)
			destination.DisableDynamicObjects = DisableDynamicObjects;
			destination.DisableExpandoObjects = DisableExpandoObjects;
			#endif
			destination.EscapeChars = EscapeChars;
			destination.EscapeUnicodeChars = EscapeUnicodeChars;
			destination.Formatting = Formatting;
			destination.IgnorePrimitiveTypeNames = IgnorePrimitiveTypeNames;
            destination.IgnoreNullListItems = IgnoreNullListItems;
			destination.IgnoreNullValues = IgnoreNullValues;
			destination.MaxObjectDepth = MaxObjectDepth;
			destination.NumberFormat = NumberFormat;
			destination.OrderNames = OrderNames;
			destination.RaiseErrorOnCircularRef = RaiseErrorOnCircularRef;
			destination.RaiseErrorOnMaxObjectDepth = RaiseErrorOnMaxObjectDepth;
			destination.UseEnumNames = UseEnumNames;
            destination.TypeNameInfo = TypeNameInfo;
			destination.TypeNames = TypeNames;
		}

		public object Clone()
		{
			JaysonSerializationSettings clone = new JaysonSerializationSettings();
			AssignTo (clone);

			return clone;
		}

		public void Reset()
		{
			Initial.AssignTo (this);
		}

		public static JaysonSerializationSettings DefaultClone()
		{
			return (JaysonSerializationSettings)Default.Clone ();
		}
	}

	# endregion JaysonSerializationSettings
}