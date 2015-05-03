using System;

namespace Jayson
{
	# region JsonMemberIgnoreAttribute

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class JaysonIgnoreMemberAttribute : Attribute
	{
		public JaysonIgnoreMemberAttribute()
			: base()
		{ }
	}

	# endregion JsonMemberIgnoreAttribute
}

