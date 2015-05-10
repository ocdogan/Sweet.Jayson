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
	}

	# endregion JaysonDeserializationSettings
}