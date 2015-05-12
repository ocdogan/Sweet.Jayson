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