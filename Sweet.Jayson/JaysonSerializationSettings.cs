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
    # region JaysonSerializationSettings

    public sealed class JaysonSerializationSettings : JaysonSerDeserSettings, ICloneable
    {
        public static readonly JaysonSerializationSettings Default = new JaysonSerializationSettings();
        private static readonly JaysonSerializationSettings Initial = new JaysonSerializationSettings();

        public string DateTimeFormat;
        public string DateTimeUTCFormat;
        public string DateTimeOffsetFormat;
        public string NumberFormat;
        public string TimeSpanFormat;

        public bool CaseSensitive = true;
        public bool ConvertDecimalToDouble = false;
        public bool EscapeChars = true;
        public bool EscapeUnicodeChars = false;
        public bool GuidAsByteArray = false;
        public bool IgnoreAnonymousTypes = true;
        public bool IgnoreBackingFields = true;
        public bool IgnoreDefaultValues = false;
#if !(NET3500 || NET3000 || NET2000)
        public bool IgnoreDynamicObjects = false;
        public bool IgnoreExpandoObjects = false;
#endif
        public bool IgnoreEmptyCollections = false;
        public bool IgnoreFields = false;
        public bool IgnorePrimitiveTypeNames = true;
        public bool IgnoreReadOnlyMembers = false;
        public bool IgnoreNonPublicFields = false;
        public bool IgnoreNonPublicProperties = false;
        public bool IgnoreNullValues = true;
        public bool IgnoreNullListItems = false;
        public bool OrderNames = false;
        public bool RaiseErrorOnCircularRef = false;
        public bool RaiseErrorOnMaxObjectDepth = true;
        public bool UseEnumNames = true;
        public bool UseGlobalTypeNames = false;
        public bool UseKVModelForISerializable = true;
        public bool UseKVModelForJsonObjects = true;
        public bool UseObjectReferencing = false;

        public int MaxObjectDepth;

        public JaysonFloatSerStrategy FloatNanStrategy = JaysonFloatSerStrategy.Error;
        public JaysonFloatSerStrategy FloatInfinityStrategy = JaysonFloatSerStrategy.Error;

        public JaysonFormatting Formatting = JaysonFormatting.None;
        public JaysonDateFormatType DateFormatType = JaysonDateFormatType.Iso8601;
        public JaysonDateTimeZoneType DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;
        public JaysonTypeNameSerialization TypeNames = JaysonTypeNameSerialization.None;
        public JaysonTypeNameInfo TypeNameInfo = JaysonTypeNameInfo.TypeNameWithAssembly;

        public JaysonSerializationSettings()
            : this(null)
        { }

        public JaysonSerializationSettings(JaysonTypeOverride[] typeOverrides)
            : base(typeOverrides)
        { }

        protected internal override void AssignTo(JaysonSerDeserSettings destination)
        {
            var sDestination = destination as JaysonSerializationSettings;
            if (sDestination != null)
            {
                sDestination.CaseSensitive = CaseSensitive;
                sDestination.ConvertDecimalToDouble = ConvertDecimalToDouble;
                sDestination.DateFormatType = DateFormatType;
                sDestination.DateTimeFormat = DateTimeFormat;
                sDestination.DateTimeUTCFormat = DateTimeUTCFormat;
                sDestination.DateTimeOffsetFormat = DateTimeOffsetFormat;
                sDestination.DateTimeZoneType = DateTimeZoneType;
                sDestination.TimeSpanFormat = TimeSpanFormat;
                sDestination.EscapeChars = EscapeChars;
                sDestination.EscapeUnicodeChars = EscapeUnicodeChars;
                sDestination.FloatNanStrategy = FloatNanStrategy;
                sDestination.FloatInfinityStrategy = FloatInfinityStrategy;
                sDestination.Formatting = Formatting;
                sDestination.GuidAsByteArray = GuidAsByteArray;
                sDestination.IgnoreAnonymousTypes = IgnoreAnonymousTypes;
                sDestination.IgnoreBackingFields = IgnoreBackingFields;
#if !(NET3500 || NET3000 || NET2000)
                sDestination.IgnoreDynamicObjects = IgnoreDynamicObjects;
                sDestination.IgnoreExpandoObjects = IgnoreExpandoObjects;
#endif
                sDestination.IgnoreDefaultValues = IgnoreDefaultValues;
                sDestination.IgnoreFields = IgnoreFields;
                sDestination.IgnorePrimitiveTypeNames = IgnorePrimitiveTypeNames;
                sDestination.IgnoreNonPublicFields = IgnoreNonPublicFields;
                sDestination.IgnoreNonPublicProperties = IgnoreNonPublicProperties;
                sDestination.IgnoreNullListItems = IgnoreNullListItems;
                sDestination.IgnoreNullValues = IgnoreNullValues;
                sDestination.IgnoreReadOnlyMembers = IgnoreReadOnlyMembers;
                sDestination.MaxObjectDepth = MaxObjectDepth;
                sDestination.NumberFormat = NumberFormat;
                sDestination.OrderNames = OrderNames;
                sDestination.RaiseErrorOnCircularRef = RaiseErrorOnCircularRef;
                sDestination.RaiseErrorOnMaxObjectDepth = RaiseErrorOnMaxObjectDepth;
                sDestination.UseEnumNames = UseEnumNames;
                sDestination.UseGlobalTypeNames = UseGlobalTypeNames;
                sDestination.UseKVModelForISerializable = UseKVModelForISerializable;
                sDestination.UseKVModelForJsonObjects = UseKVModelForJsonObjects;
                sDestination.UseObjectReferencing = UseObjectReferencing;
                sDestination.TypeNameInfo = TypeNameInfo;
                sDestination.TypeNames = TypeNames;
            }

            base.AssignTo(destination);
        }

        public object Clone()
        {
            var clone = new JaysonSerializationSettings();
            AssignTo(clone);

            return clone;
        }

        public void Reset()
        {
            Initial.AssignTo(this);
        }

        public static JaysonSerializationSettings DefaultClone()
        {
            return (JaysonSerializationSettings)Default.Clone();
        }
    }

    # endregion JaysonSerializationSettings
}