using System;

namespace Jayson
{
	# region IJaysonFastMember

	internal interface IJaysonFastMember
	{
		JaysonFastMemberType Type { get; }
		Type MemberType { get; }

		bool CanRead { get; }
		bool CanWrite { get; }

		object Get(object instance);
		void Set(object instance, object value);
	}

	# endregion IJaysonFastMember
}

