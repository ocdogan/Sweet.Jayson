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
using System.ComponentModel;
using System.Reflection;

namespace Sweet.Jayson
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class JaysonDefaultValueAttribute : DefaultValueAttribute
    {
        private Type m_ValueType;

        public JaysonDefaultValueAttribute(bool value) : base(value) { SetValueType(typeof(bool)); }

        public JaysonDefaultValueAttribute(byte value) : base(value) { SetValueType(typeof(byte)); }

        public JaysonDefaultValueAttribute(char value) : base(value) { SetValueType(typeof(char)); }

        public JaysonDefaultValueAttribute(double value) : base(value) { SetValueType(typeof(double)); }

        public JaysonDefaultValueAttribute(float value) : base(value) { SetValueType(typeof(float)); }

        public JaysonDefaultValueAttribute(int value) : base(value) { SetValueType(typeof(int)); }

        public JaysonDefaultValueAttribute(long value) : base(value) { SetValueType(typeof(long)); }

        public JaysonDefaultValueAttribute(short value) : base(value) { SetValueType(typeof(short)); }

        public JaysonDefaultValueAttribute(string value) : base(value) { SetValueType(typeof(string)); }

        public JaysonDefaultValueAttribute(Type type, string value) : base(type, value) { SetValueType(type); }

        public JaysonDefaultValueAttribute(object value) : base(value) { SetValue(value, null); }

        public JaysonDefaultValueAttribute(Type type)
            : base(null)
        {
            try
            {
                SetValue(Activator.CreateInstance(type, true), type);
            }
            catch (Exception)
            { }
        }

        public Type ValueType { get { return m_ValueType; } }

        protected virtual void SetValueType(Type valueType = null)
        {
            m_ValueType = valueType;
        }

        protected virtual void SetValue(object value, Type valueType = null)
        {
            base.SetValue(value);
            SetValueType((valueType != null) ? valueType :
                ((value != null) ? value.GetType() : null));
        }

        public override bool Equals(object obj)
        {
            if (obj == this) return true;

            DefaultValueAttribute attr = obj as DefaultValueAttribute;
            if (attr != null) obj = attr.Value;

            var value = base.Value;
            if (value != null)
            {
                return value.Equals(obj) ||
                    ((value is IComparable) && (((IComparable)value).CompareTo(obj) == 0));
            }
            return (obj == null) || obj.Equals(null) ||
                   ((obj is IComparable) && (((IComparable)obj).CompareTo(null) == 0));
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
