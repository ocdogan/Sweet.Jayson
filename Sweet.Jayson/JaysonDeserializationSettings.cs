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
using System.Reflection;
using System.Runtime.Serialization;

namespace Sweet.Jayson
{
    # region JaysonDeserializationSettings

    public class JaysonDeserializationSettings : JaysonSerDeserSettings, ICloneable
    {
        # region Constants

        public static readonly JaysonDeserializationSettings Default = new JaysonDeserializationSettings();
        private static readonly JaysonDeserializationSettings Initial = new JaysonDeserializationSettings();

        # endregion Constants

        # region Field Members

        public SerializationBinder Binder;

        public string DateTimeFormat;
        public string DateTimeUTCFormat;
        public string DateTimeOffsetFormat;

        public bool CaseSensitive = true;
        public bool ConvertDecimalToDouble = false;
        public bool IgnoreAnonymousTypes = true;
        public bool IgnoreBackingFields = false;
        public bool IgnoreFields = false;
        public bool IgnoreNonPublicFields = false;
        public bool IgnoreNonPublicProperties = false;
        public bool RaiseErrorOnMissingMember = false;
        public bool UseDefaultValues = true;

        public int MaxObjectDepth;

        public ArrayDeserializationType ArrayType = ArrayDeserializationType.List;
        public JaysonDateTimeZoneType DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;
        public DictionaryDeserializationType DictionaryType = DictionaryDeserializationType.Dictionary;
        public JaysonCommentHandling CommentHandling = JaysonCommentHandling.Ignore;

        public JaysonDeSerErrorHandler ErrorHandler;
        public JaysonObjectActivator ObjectActivator;
        public JaysonCtorParamMatcher CtorParamMatcher;

        # endregion Field Members

        public JaysonDeserializationSettings()
            : this(null)
        { }

        public JaysonDeserializationSettings(JaysonTypeOverride[] typeOverrides)
            : base(typeOverrides)
        { }

        protected internal override void AssignTo(JaysonSerDeserSettings destination)
        {
            var dsDestination = destination as JaysonDeserializationSettings;
            if (dsDestination != null)
            {
                dsDestination.ArrayType = ArrayType;
                dsDestination.Binder = Binder;
                dsDestination.CaseSensitive = CaseSensitive;
                dsDestination.CommentHandling = CommentHandling;
                dsDestination.ConvertDecimalToDouble = ConvertDecimalToDouble;
                dsDestination.DateTimeFormat = DateTimeFormat;
                dsDestination.DateTimeUTCFormat = DateTimeUTCFormat;
                dsDestination.DateTimeOffsetFormat = DateTimeOffsetFormat;
                dsDestination.DateTimeZoneType = DateTimeZoneType;
                dsDestination.DictionaryType = DictionaryType;
                dsDestination.ErrorHandler = ErrorHandler;
                dsDestination.IgnoreAnonymousTypes = IgnoreAnonymousTypes;
                dsDestination.IgnoreBackingFields = IgnoreBackingFields;
                dsDestination.IgnoreFields = IgnoreFields;
                dsDestination.IgnoreNonPublicFields = IgnoreNonPublicFields;
                dsDestination.IgnoreNonPublicProperties = IgnoreNonPublicProperties;
                dsDestination.MaxObjectDepth = MaxObjectDepth;
                dsDestination.ObjectActivator = ObjectActivator;
                dsDestination.RaiseErrorOnMissingMember = RaiseErrorOnMissingMember;
                dsDestination.UseDefaultValues = UseDefaultValues;
            }

            base.AssignTo(destination);
        }

        public object Clone()
        {
            var clone = new JaysonDeserializationSettings();
            AssignTo(clone);

            return clone;
        }

        public void Reset()
        {
            Initial.AssignTo(this);
        }

        public static JaysonDeserializationSettings DefaultClone()
        {
            return (JaysonDeserializationSettings)Default.Clone();
        }
    }

    # endregion JaysonDeserializationSettings
}