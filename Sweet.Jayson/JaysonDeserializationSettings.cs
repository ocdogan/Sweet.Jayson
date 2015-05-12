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
using System.Runtime.Serialization;

namespace Sweet.Jayson
{
	# region JaysonDeserializationSettings

	public class JaysonDeserializationSettings : ICloneable
	{
		public static readonly JaysonDeserializationSettings Default = new JaysonDeserializationSettings();
		private static readonly JaysonDeserializationSettings Initial = new JaysonDeserializationSettings ();

        public SerializationBinder Binder;
        
		public string DateTimeFormat;
		public string DateTimeOffsetFormat;

		public bool CaseSensitive = true;
		public bool ConvertDecimalToDouble = false;
        public bool IgnoreAnonymousTypes = true;
        public bool RaiseErrorOnMissingMember = false;

		public int MaxObjectDepth;

		public ArrayDeserializationType ArrayType = ArrayDeserializationType.List;
		public JaysonDateTimeZoneType DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;
		public DictionaryDeserializationType DictionaryType = DictionaryDeserializationType.Dictionary;

		public JaysonObjectActivator ObjectActivator;

		internal void AssignTo(JaysonDeserializationSettings destination)
		{
			destination.ArrayType = ArrayType;
            destination.Binder = Binder;
			destination.CaseSensitive = CaseSensitive;
			destination.ConvertDecimalToDouble = ConvertDecimalToDouble;
			destination.DateTimeFormat = DateTimeFormat;
			destination.DateTimeOffsetFormat = DateTimeOffsetFormat;
			destination.DateTimeZoneType = DateTimeZoneType;
			destination.DictionaryType = DictionaryType;
			destination.IgnoreAnonymousTypes = IgnoreAnonymousTypes;
			destination.MaxObjectDepth = MaxObjectDepth;
			destination.ObjectActivator = ObjectActivator;
            destination.RaiseErrorOnMissingMember = RaiseErrorOnMissingMember;
        }

		public object Clone()
		{
			JaysonDeserializationSettings clone = new JaysonDeserializationSettings ();
			AssignTo (clone);

			return clone;
		}

		public void Reset()
		{
			Initial.AssignTo (this);
		}

		public static JaysonDeserializationSettings DefaultClone()
		{
			return (JaysonDeserializationSettings)Default.Clone ();
		}
	}

	# endregion JaysonDeserializationSettings
}