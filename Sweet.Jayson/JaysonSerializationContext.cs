using System;
using System.Text;
using System.Reflection;

namespace Sweet.Jayson
{
	# region JaysonSerializationContext

	internal sealed class JaysonSerializationContext
	{
		public readonly StringBuilder Builder;
		public readonly Func<string, object, object> Filter;
		public readonly JaysonFormatter Formatter;
		public readonly JaysonSerializationSettings Settings;
		public readonly JaysonStackList Stack;

		public int ObjectDepth;
		public Type CurrentType;
		public JaysonObjectType ObjectType;

		public JaysonSerializationContext(JaysonSerializationSettings settings, JaysonStackList stack,
			Func<string, object, object> filter, JaysonFormatter formatter = null, 
			StringBuilder builder = null, Type currentType = null,
			JaysonObjectType objectType = JaysonObjectType.Object)
		{
			Builder = builder;
			Filter = filter;
			Formatter = formatter;
			Settings = settings;
			Stack = stack;

			CurrentType = currentType;
			ObjectType = objectType;
		}
	}

	# endregion JaysonSerializationContext
}