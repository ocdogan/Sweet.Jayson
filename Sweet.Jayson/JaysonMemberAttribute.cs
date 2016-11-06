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
using System.Collections.Generic;

namespace Sweet.Jayson
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class JaysonMemberAttribute : Attribute
    {
        # region Field Members

        private string m_Alias;
        private object m_DefaultValue;

        # endregion Field Members

        public JaysonMemberAttribute()
        { }

        public JaysonMemberAttribute(object defaultValue)
        {
            DefaultValue = defaultValue;
        }

        public JaysonMemberAttribute(string alias)
        {
            Alias = alias;
        }

        public JaysonMemberAttribute(string alias, object defaultValue)
        {
            Alias = alias;
            DefaultValue = defaultValue;
        }

        public JaysonMemberAttribute(bool ignored)
        {
            Ignored = ignored;
        }

        public JaysonMemberAttribute(bool ignored, string alias)
        {
            Alias = alias;
            Ignored = ignored;
        }

        public JaysonMemberAttribute(bool ignored, object defaultValue)
        {
            Ignored = ignored;
            DefaultValue = defaultValue;
        }

        public JaysonMemberAttribute(bool ignored, string alias, object defaultValue)
		{
            Alias = alias;
            Ignored = ignored;
            DefaultValue = defaultValue;
		}

        public object DefaultValue
        {
            get { return m_DefaultValue; }
            set
            {
                m_DefaultValue = value;
                HasDefaultValue = true;
            }
        }

        public bool HasDefaultValue { get; private set; }

        public bool? Ignored { get; set; }

        public string Alias
        {
            get { return m_Alias; }
            set
            {
                if (value != null)
                {
                    value = value.Trim();
                    if (value == String.Empty)
                    {
                        value = null;
                    }
                }
                m_Alias = value;
            }
        }
    }
}

