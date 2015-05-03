using System;

namespace Jayson
{
	# region JaysonSerializationSettings

	public class JaysonSerializationSettings : ICloneable
	{
		public static readonly JaysonSerializationSettings Default = new JaysonSerializationSettings();
		private static readonly JaysonSerializationSettings Initial = new JaysonSerializationSettings ();

		public string DateTimeFormat;
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
	}

	# endregion JaysonSerializationSettings
}