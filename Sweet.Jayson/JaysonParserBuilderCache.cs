using System;
using System.Text;

namespace Sweet.Jayson
{
	# region JaysonParserBuilderCache

	internal static class JaysonParserBuilderCache
	{
		#if !(NET3500 || NET3000 || NET2000)
		private const int MAX_BUILDER_SIZE = 360;

		[ThreadStatic]
		private static StringBuilder s_Cached;
		#endif

		public static StringBuilder Acquire(int capacity = 20)
		{
			#if !(NET3500 || NET3000 || NET2000)
			if (capacity <= MAX_BUILDER_SIZE)
			{
				StringBuilder builder = JaysonParserBuilderCache.s_Cached;
				if (builder != null)
				{
					if (capacity <= builder.Capacity)
					{
						JaysonParserBuilderCache.s_Cached = null;
						builder.Clear();
						return builder;
					}
				}
			}
			#endif
			return new StringBuilder(Math.Max(capacity, 20), int.MaxValue);
		}

		public static void Release(StringBuilder builder)
		{
			#if !(NET3500 || NET3000 || NET2000)
			if (builder.Capacity <= MAX_BUILDER_SIZE)
			{
				JaysonParserBuilderCache.s_Cached = builder;
			}
			#endif
		}

		public static string GetStringAndRelease(StringBuilder builder)
		{
			string result = builder.ToString();
			Release(builder);
			return result;
		}
	}

	# endregion JaysonParserBuilderCache
}