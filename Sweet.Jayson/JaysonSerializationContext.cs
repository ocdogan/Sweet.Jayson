﻿# region License
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
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace Sweet.Jayson
{
    # region JaysonSerializationContext

    internal sealed class JaysonSerializationContext : IDisposable
    {
        public readonly StringBuilder Builder;
        public readonly Func<string, object, object> Filter;
        public readonly JaysonFormatter Formatter;
        public readonly JaysonSerializationTypeList GlobalTypes;
        public readonly JaysonSerializationSettings Settings;
        public readonly JaysonStackList<object> Stack;
        public readonly JaysonSerializationReferenceMap ReferenceMap = new JaysonSerializationReferenceMap();
        public readonly StreamingContext StreamingContext = new StreamingContext();

        public int ObjectDepth;
        public Type CurrentType;
        public JaysonObjectType ObjectType;

        public bool SkipCurrentType;

        public JaysonSerializationContext(JaysonSerializationSettings settings, JaysonStackList<object> stack,
            Func<string, object, object> filter, JaysonFormatter formatter = null,
            StringBuilder builder = null, Type currentType = null,
            JaysonObjectType objectType = JaysonObjectType.Object,
            JaysonSerializationTypeList globalTypes = null)
        {
            Builder = builder;
            Filter = filter;
            Formatter = formatter;
            GlobalTypes = globalTypes;
            Settings = settings;
            Stack = stack;

            CurrentType = currentType;
            ObjectType = objectType;
        }

        public void Dispose()
        {
            ReferenceMap.Dispose();
        }
    }

    # endregion JaysonSerializationContext
}