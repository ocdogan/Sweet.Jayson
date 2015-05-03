using System;
using System.Runtime.Serialization;

namespace Jayson
{
	# region JaysonDeserializationSettings

	public class JaysonDeserializationSettings : ICloneable
	{
		public static readonly JaysonDeserializationSettings Default = new JaysonDeserializationSettings();
		private static readonly JaysonDeserializationSettings Initial = new JaysonDeserializationSettings ();

        public SerializationBinder Binder;
        
		public bool CaseSensitive = true;
		public bool ConvertDecimalToDouble = false;
        public bool IgnoreAnonymousTypes = true;
        public bool RaiseErrorOnMissingMember = false;

		public int MaxObjectDepth;

		public ArrayDeserializationType ArrayType = ArrayDeserializationType.List;
		public DictionaryDeserializationType DictionaryType = DictionaryDeserializationType.Dictionary;

		internal void AssignTo(JaysonDeserializationSettings destination)
		{
			destination.ArrayType = ArrayType;
            destination.Binder = Binder;
			destination.CaseSensitive = CaseSensitive;
			destination.ConvertDecimalToDouble = ConvertDecimalToDouble;
			destination.DictionaryType = DictionaryType;
			destination.IgnoreAnonymousTypes = IgnoreAnonymousTypes;
			destination.MaxObjectDepth = MaxObjectDepth;
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