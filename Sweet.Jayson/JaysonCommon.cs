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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace Sweet.Jayson
{
	public static class JaysonCommon
	{
		# region ExpressionAssigner<> for .Net3.5

		private static class ExpressionAssigner<T>
		{
			public static T Assign(ref T left, T right)
			{
				return (left = right);
			}
		}

		# endregion ExpressionAssigner<> for .Net3.5

		# region Static Members

		private static int s_IsMono = -1;

		private static readonly int LowerCaseDif = (int)'a' - (int)'A';

		private static TimeSpan s_UtcOffsetUpdate;
		private static long s_LastUtcOffsetUpdate = -1;

		private static readonly Dictionary<Type, bool> s_IsGenericCollection = new Dictionary<Type, bool>(JaysonConstants.CacheInitialCapacity);
		private static readonly Dictionary<Type, bool> s_IsGenericDictionary = new Dictionary<Type, bool>(JaysonConstants.CacheInitialCapacity);
		private static readonly Dictionary<Type, bool> s_IsGenericList = new Dictionary<Type, bool>(JaysonConstants.CacheInitialCapacity);

		private static readonly Dictionary<Type, Action<object, object[]>> s_ICollectionAdd = new Dictionary<Type, Action<object, object[]>>(JaysonConstants.CacheInitialCapacity);
		private static readonly Dictionary<Type, Action<object, object[]>> s_IDictionaryAdd = new Dictionary<Type, Action<object, object[]>>(JaysonConstants.CacheInitialCapacity);

		private static readonly Dictionary<string, Type> s_TypeCache = new Dictionary<string, Type>(JaysonConstants.CacheInitialCapacity);

		private static readonly Dictionary<Type, Type> s_GenericListArgs = new Dictionary<Type, Type>(JaysonConstants.CacheInitialCapacity);
		private static readonly Dictionary<Type, Type> s_GenericCollectionArgs = new Dictionary<Type, Type>(JaysonConstants.CacheInitialCapacity);
		private static readonly Dictionary<Type, Type[]> s_GenericDictionaryArgs = new Dictionary<Type, Type[]>(JaysonConstants.CacheInitialCapacity);

		private static readonly Dictionary<Type, MethodInfo> s_ExpressionAssignCache = new Dictionary<Type, MethodInfo>(JaysonConstants.CacheInitialCapacity);

		# endregion Static Members

		# region Helper Methods

		# region .Net3.5 Methods

		public static BinaryExpression ExpressionAssign(Expression left, Expression right)
		{
			MethodInfo assign;
			if (!s_ExpressionAssignCache.TryGetValue (left.Type, out assign)) {
				assign = typeof(ExpressionAssigner<>).MakeGenericType (left.Type).GetMethod ("Assign");
				s_ExpressionAssignCache [left.Type] = assign;
			}
			return Expression.Add(left, right, assign);
		}

		# endregion .Net3.5 Methods

		# region DateTime Methods

		public static TimeSpan GetUtcOffset(DateTime dateTime)
		{
			if (dateTime.Kind == DateTimeKind.Utc) {
				return TimeSpan.Zero;
			}
			if ((dateTime.Ticks / TimeSpan.TicksPerHour) == (DateTime.UtcNow.Ticks / TimeSpan.TicksPerHour)) {
				return GetCurrentUtcOffset ();
			}
			return JaysonConstants.CurrentTimeZone.GetUtcOffset(dateTime);
		}

		public static TimeSpan GetCurrentUtcOffset()
		{
			if (s_LastUtcOffsetUpdate < 0)
			{
				s_LastUtcOffsetUpdate = DateTime.UtcNow.Ticks;
				s_UtcOffsetUpdate = JaysonConstants.CurrentTimeZone.GetUtcOffset(DateTime.Now);
			}
			else
			{
				long utcNow = DateTime.UtcNow.Ticks;
				if (utcNow - s_LastUtcOffsetUpdate > 10000000)
				{
					s_LastUtcOffsetUpdate = utcNow;
					s_UtcOffsetUpdate = JaysonConstants.CurrentTimeZone.GetUtcOffset(DateTime.Now);
				}
			}
			return s_UtcOffsetUpdate;
		}

		public static DateTime ToLocalTime(DateTime dateTime)
		{
			DateTimeKind kind = dateTime.Kind;
			if (kind == DateTimeKind.Local) {
				return dateTime;
			}

			TimeSpan utcOffset = GetUtcOffset(dateTime);

			long utcOffsetTicks = utcOffset.Ticks;
			if (utcOffsetTicks == 0) {
				return new DateTime (dateTime.Ticks, DateTimeKind.Local);
			}

			if (utcOffsetTicks > 0) {
				if (DateTime.MaxValue - utcOffset < dateTime) {
					return new DateTime (DateTime.MaxValue.Ticks, DateTimeKind.Local);
				}
			}
			else if (dateTime.Ticks + utcOffsetTicks < DateTime.MinValue.Ticks) {
				return new DateTime (DateTime.MinValue.Ticks, DateTimeKind.Local);
			}

			return new DateTime (dateTime.AddTicks (utcOffsetTicks).Ticks, DateTimeKind.Local);			
		}

		public static DateTime ToUniversalTime(DateTime dateTime)
		{
			DateTimeKind kind = dateTime.Kind;
			if (kind == DateTimeKind.Utc) {
				return dateTime;
			}
			if (dateTime == DateTime.MinValue) {
				return JaysonConstants.DateTimeUtcMinValue;
			}
			if (kind == DateTimeKind.Unspecified) {
				return new DateTime (dateTime.Subtract (GetUtcOffset (dateTime)).Ticks, DateTimeKind.Utc);
			}

			long ticks = dateTime.Ticks - GetUtcOffset(dateTime).Ticks;
			if (ticks > 3155378975999999999L) {
				return new DateTime(3155378975999999999L, DateTimeKind.Utc);
			}
			if (ticks < 0L) {
				return new DateTime(0L, DateTimeKind.Utc);
			}
			return new DateTime(ticks, DateTimeKind.Utc);
		}

		public static TimeSpan SinceUnixEpochStart(DateTime dateTime)
		{
			if (dateTime.Kind == DateTimeKind.Utc) {
				return dateTime.Subtract (JaysonConstants.DateTimeUnixEpochMinValue);
			}
			return ToUniversalTime (dateTime).Subtract (JaysonConstants.DateTimeUnixEpochMinValue);
		}

		public static long ToUnixTimeSec(DateTime dateTime)
		{
			return SinceUnixEpochStart(dateTime).Ticks / TimeSpan.TicksPerSecond;
		}

		public static long ToUnixTimeMsec(DateTime dateTime)
		{
			return (long)(SinceUnixEpochStart(dateTime).TotalMilliseconds);
		}

		public static long ToUnixTimeMsec(long ticks)
		{
			return (ticks - JaysonConstants.UnixEpochMinValue) / TimeSpan.TicksPerMillisecond;
		}

		public static DateTime FromUnixTimeSec(long unixTime)
		{
			return JaysonConstants.DateTimeUnixEpochMinValue + TimeSpan.FromSeconds(unixTime);
		}

		public static DateTime FromUnixTimeMsec(long msecSince1970)
		{
			return JaysonConstants.DateTimeUnixEpochMinValue + TimeSpan.FromMilliseconds(msecSince1970);
		}

		public static DateTime FromUnixTimeMsec(long msecSince1970, TimeSpan offset)
		{
			return new DateTime(JaysonConstants.DateTimeUnixEpochMinValueUnspecified.Ticks + 
				TimeSpan.FromMilliseconds(msecSince1970).Ticks + offset.Ticks, DateTimeKind.Local);
		}

		// Supports: yyyy-MM-ddTHH:mm:ss.fffffff%K, yyyyMMddTHHmmss.fffffff%K and
		// dd-MM-yyyyTHH:mm:ss.fffffff%K 
		public static DateTime ParseIso8601DateTime(string str, 
			JaysonDateTimeZoneType timeZoneType = JaysonDateTimeZoneType.KeepAsIs)
		{
			DateTime dateTime;
			TimeSpan timeSpan;
			ParseIso8601DateTimeOffset (str, out dateTime, out timeSpan);

			switch (timeZoneType) {
			case JaysonDateTimeZoneType.ConvertToUtc:
				{
					if (timeSpan == TimeSpan.Zero) {
						return new DateTime(dateTime.Ticks, DateTimeKind.Utc);
					}
					return new DateTime(dateTime.Subtract (timeSpan).Ticks, DateTimeKind.Utc);
				}
			case JaysonDateTimeZoneType.ConvertToLocal:
				{
					if (timeSpan == TimeSpan.Zero) {
						return JaysonCommon.ToLocalTime(dateTime);
					}
					return JaysonCommon.ToLocalTime (dateTime.Subtract (timeSpan));
				}
			default:
				{
					if (timeSpan == TimeSpan.Zero) {
						return dateTime;
					}
					return JaysonCommon.ToLocalTime (dateTime.Subtract (timeSpan));
				}
			}
		}

		// Supports: yyyy-MM-ddTHH:mm:ss.fffffff%K, yyyyMMddTHHmmss.fffffff%K and
		// dd-MM-yyyyTHH:mm:ss.fffffff%K 
		public static DateTimeOffset ParseIso8601DateTimeOffset(string str)
		{
			DateTime dateTime;
			TimeSpan timeSpan;
			ParseIso8601DateTimeOffset (str, out dateTime, out timeSpan);
			return new DateTimeOffset (dateTime, timeSpan);
		}

		// Supports: yyyy-MM-ddTHH:mm:ss.fffffff%K, yyyyMMddTHHmmss.fffffff%K and
		// dd-MM-yyyyTHH:mm:ss.fffffff%K 
		public static void ParseIso8601DateTimeOffset(string str, out DateTime dateTime, out TimeSpan timeSpan)
		{
			timeSpan = TimeSpan.Zero;
			if (str == null) {
				dateTime = default(DateTime);
				return;
			}

			int length = str.Length;
			if (length == 0) {
				dateTime = default(DateTime);
				return;
			}

			if (length < 10) {
				throw new JaysonException ("Invalid ISO8601 date format.");
			}

			DateTimeKind kind = DateTimeKind.Unspecified;
			if (str [length - 1] == 'Z') {
				kind = DateTimeKind.Utc;
			}

			try {
				int year = 0;
				int month = 1;
				int day = 1;

				char ch;
				int pos = 0;
				bool basic = false;

				if (str [2] == '-') {
					day = 10 * (int)(str [0] - '0') + (int)(str [1] - '0');
					month = 10 * (int)(str [3] - '0') + (int)(str [4] - '0');

					year = 1000 * (int)(str [6] - '0') + 100 * (int)(str [7] - '0') +
						10 * (int)(str [8] - '0') + (int)(str [9] - '0');
					pos = 10;
				} else {
					year = 1000 * (int)(str [0] - '0') + 100 * (int)(str [1] - '0') +
						10 * (int)(str [2] - '0') + (int)(str [3] - '0');

					ch = str[4];
					basic = ch >= '0' && ch <= '9';
					if (basic) {
						month = 10 * (int)(str [4] - '0') + (int)(str [5] - '0');
						day = 10 * (int)(str [6] - '0') + (int)(str [7] - '0');
						pos = 8;
					} else {
						month = 10 * (int)(str [5] - '0') + (int)(str [6] - '0');
						day = 10 * (int)(str [8] - '0') + (int)(str [9] - '0');
						pos = 10;
					}
				}

				if (month > 12 && day < 13) {
					int tmp = month;
					month = day;
					day = tmp;
				}

				if (length == pos) {
					dateTime = new DateTime (year, month, day, 0, 0, 0, kind);
					return;
				}

				ch = str [pos];
				if (!(ch == 'T' || ch == ' ')) {
					dateTime = new DateTime (year, month, day, 0, 0, 0, kind);
					return;
				}

				int minute = 0;
				int second = 0;
				int millisecond = 0;

				int hour = 10 * (int)(str [pos + 1] - '0') + (int)(str [pos + 2] - '0');
				pos += 3;

				if (pos < length) {
					ch = str [pos];
					if (ch == ':') {
						minute = 10 * (int)(str [pos + 1] - '0') + (int)(str [pos + 2] - '0');
						pos += 3;

						if (pos < length) {
							ch = str [pos];
							if (ch == ':') {
								second = 10 * (int)(str [pos + 1] - '0') + (int)(str [pos + 2] - '0');
								pos += 3;
							}
						}
					} else if (basic) {
						minute = 10 * (int)(str [pos] - '0') + (int)(str [pos + 1] - '0');
						pos += 2;

						if (pos < length) {
							ch = str [pos];
							if (ch >= '0' || ch <= '9') {
								second = 10 * (int)(str [pos] - '0') + (int)(str [pos + 1] - '0');
								pos += 2;
							}
						}
					}

					if (pos < length) {
						ch = str [pos];
						if (ch == 'Z') {
							dateTime = new DateTime (year, month, day, hour, minute, second, kind);
							return;
						}

						if (ch == '.') {
							int msIndex = 0;
							while (++pos < length) {
								ch = str [pos];
								if (ch < '0' || ch > '9') {
									break;
								}

								msIndex++;
								if (msIndex < 4) {
									millisecond *= 10;
									millisecond += (int)(ch - '0');
								}
							}
						}

						if (pos < length) {
							ch = str [pos];
							if (ch == '+' || ch == '-') {
								int tzHour = 10 * (int)(str [pos + 1] - '0') + (int)(str [pos + 2] - '0');
								int tzMinute = 0;
								pos += 3;

								if (pos < length) {
									ch = str [pos];
									if (ch == ':') {
										pos++;
									}

									tzMinute = 10 * (int)(str [pos] - '0') + (int)(str [pos + 1] - '0');
								}

								timeSpan = new TimeSpan (tzHour, tzMinute, 0);
								dateTime = new DateTime (year, month, day, hour, minute, 
									second, millisecond, DateTimeKind.Unspecified);
								return;
							}
						}
					}
				}

				dateTime = new DateTime (year, month, day, hour, minute, second, millisecond, kind);
			} catch (Exception) {
				throw new JaysonException ("Invalid ISO8601 date format.");
			}
		}

		public static DateTime ParseUnixEpoch(string str)
		{
			if (str == null) {
				return default(DateTime);
			}

			int length = str.Length;
			if (length == 0)
			{
				return default(DateTime);
			}

			char ch;
			long l = 0;
			int timeZonePos = -1;
			int timeZoneSign = 1;

			for (int i = 0; i < length; i++)
			{
				ch = str[i];
				if (ch == '-')
				{
					timeZonePos = i;
					timeZoneSign = -1;
					break;
				}

				if (ch == '+')
				{
					timeZonePos = i;
					break;
				}

				if (ch < '0' || ch > '9')
				{
					if (l == 0 && IsWhiteSpace(ch))
						continue;
					throw new JaysonException("Invalid Unix Epoch date format.");
				}

				l *= 10;
				l += (long)(ch - '0');
			}

			if (timeZonePos == -1)
			{
				DateTime dt1 = JaysonCommon.FromUnixTimeMsec(l);
				if (dt1 > JaysonConstants.DateTimeUnixEpochMaxValue)
				{
					throw new JaysonException("Invalid Unix Epoch date format.");
				}
				return dt1;
			}

			if (timeZonePos > length - 5)
			{
				throw new JaysonException("Invalid Unix Epoch date format.");
			}

			TimeSpan tz = new TimeSpan(10 * (str[timeZonePos + 1] - '0') + (str[timeZonePos + 2] - '0'),
				10 * (str[timeZonePos + 3] - '0') + (str[timeZonePos + 4] - '0'), 0);

			if (timeZoneSign == -1)
			{
				tz = new TimeSpan(-tz.Ticks);
			}

			DateTime dt2 = JaysonCommon.FromUnixTimeMsec(l, tz);
			if (dt2 > JaysonConstants.DateTimeUnixEpochMaxValue)
			{
				throw new JaysonException("Invalid Unix Epoch date format.");
			}
			return dt2;
		}

		private static DateTime DefaultDateTime (JaysonDateTimeZoneType timeZoneType)
		{
			switch (timeZoneType) {
			case JaysonDateTimeZoneType.ConvertToUtc:
				return new DateTime (0, DateTimeKind.Utc);
			case JaysonDateTimeZoneType.ConvertToLocal:
				return new DateTime (0, DateTimeKind.Local);
			default:
				return default(DateTime);
			}
		}

		public static DateTime TryConvertDateTime (object value, JaysonDateTimeZoneType timeZoneType)
		{
			if (value == null) {
				return DefaultDateTime(timeZoneType);
			}

			DateTime dateTime;

			string str = value as string;
			if (str != null) {
				if (str.Length == 0) {
					return DefaultDateTime (timeZoneType);
				}

				if (StartsWith (str, JaysonConstants.MicrosoftDateFormatStart) &&
					EndsWith (str, JaysonConstants.MicrosoftDateFormatEnd)) {
					str = str.Substring (JaysonConstants.MicrosoftDateFormatStartLen, 
						str.Length - JaysonConstants.MicrosoftDateFormatLen);
					dateTime = ParseUnixEpoch (str);
				} else {
					dateTime = ParseIso8601DateTime (str, timeZoneType);
				}
			} else {
				if (value is DateTime) {
					dateTime = (DateTime)value;
				} else if (value is DateTime?) {
					dateTime = ((DateTime?)value).Value;
				} else if (value is int) {
					dateTime = FromUnixTimeSec ((int)value);
				} else if (value is long) {
					dateTime = FromUnixTimeSec ((long)value);
				} else {
					dateTime = Convert.ToDateTime (value);
				}
			}

			switch (timeZoneType) {
			case JaysonDateTimeZoneType.ConvertToUtc:
				return ToUniversalTime (dateTime);
			case JaysonDateTimeZoneType.ConvertToLocal:
				return ToLocalTime (dateTime);
			default:
				return dateTime;
			}
		}

		public static DateTime TryConvertDateTime (object value, string dateFormat, 
			JaysonDateTimeZoneType timeZoneType)
		{
			if (value == null) {
				return DefaultDateTime(timeZoneType);
			}

			DateTime dateTime;

			string str = value as string;
			if (str != null) {
				if (str.Length == 0) {
					return DefaultDateTime (timeZoneType);
				}

				if (StartsWith (str, JaysonConstants.MicrosoftDateFormatStart) &&
				    EndsWith (str, JaysonConstants.MicrosoftDateFormatEnd)) {
					str = str.Substring (JaysonConstants.MicrosoftDateFormatStartLen, 
						str.Length - JaysonConstants.MicrosoftDateFormatLen);				
					dateTime = ParseUnixEpoch (str);
				} else if (String.IsNullOrEmpty (dateFormat)) {
					dateTime = ParseIso8601DateTime (str, timeZoneType);	
				} else {
					DateTimeStyles dtStyle = DateTimeStyles.None;
					if (EndsWith (str, 'Z')) {
						dtStyle = DateTimeStyles.AdjustToUniversal;
					} else if (timeZoneType == JaysonDateTimeZoneType.ConvertToLocal) {
						dtStyle = DateTimeStyles.AssumeLocal;
					}

					if (!DateTime.TryParseExact (str, dateFormat, JaysonConstants.InvariantCulture, 
						dtStyle, out dateTime)) {
						throw new JaysonException ("Invalid date format.");
					}
				}
			} else {
				if (value is DateTime) {
					dateTime = (DateTime)value;
				} else if (value is DateTime?) {
					dateTime = ((DateTime?)value).Value;
				} else if (value is int) {
					dateTime = FromUnixTimeSec ((int)value);
				} else if (value is long) {
					dateTime = FromUnixTimeSec ((long)value);
				} else {
					dateTime = Convert.ToDateTime (value);
				}
			}

			switch (timeZoneType) {
			case JaysonDateTimeZoneType.ConvertToUtc:
				return ToUniversalTime (dateTime);
			case JaysonDateTimeZoneType.ConvertToLocal:
				return ToLocalTime (dateTime);
			default:
				return dateTime;
			}
		}

		public static DateTime TryConvertDateTime (object value, string[] dateFormats, 
			JaysonDateTimeZoneType timeZoneType)
		{
			if (value == null) {
				return DefaultDateTime(timeZoneType);
			}

			DateTime dateTime;

			string str = value as string;
			if (str != null) {
				if (str.Length == 0) {
					return DefaultDateTime (timeZoneType);
				}

				if (StartsWith (str, JaysonConstants.MicrosoftDateFormatStart) &&
				    EndsWith (str, JaysonConstants.MicrosoftDateFormatEnd)) {
					str = str.Substring (JaysonConstants.MicrosoftDateFormatStartLen, 
						str.Length - JaysonConstants.MicrosoftDateFormatLen);
					dateTime = ParseUnixEpoch (str);
				} else if (dateFormats == null || dateFormats.Length == 0) {
					dateTime = ParseIso8601DateTime (str, timeZoneType);	
				} else {
					DateTimeStyles dtStyle = DateTimeStyles.None;
					if (EndsWith (str, 'Z')) {
						dtStyle = DateTimeStyles.AdjustToUniversal;
					} else if (timeZoneType == JaysonDateTimeZoneType.ConvertToLocal) {
						dtStyle = DateTimeStyles.AssumeLocal;
					} 

					if (!DateTime.TryParseExact (str, dateFormats, JaysonConstants.InvariantCulture, 
						dtStyle, out dateTime)) {
						throw new JaysonException ("Invalid date format.");
					}
				}
			} else {
				if (value is DateTime) {
					dateTime = (DateTime)value;
				} else if (value is DateTime?) {
					dateTime = ((DateTime?)value).Value;
				} else if (value is int) {
					dateTime = FromUnixTimeSec ((int)value);
				} else if (value is long) {
					dateTime = FromUnixTimeSec ((long)value);
				} else {
					dateTime = Convert.ToDateTime (value);
				}
			}

			switch (timeZoneType) {
			case JaysonDateTimeZoneType.ConvertToUtc:
				return ToUniversalTime (dateTime);
			case JaysonDateTimeZoneType.ConvertToLocal:
				return ToLocalTime (dateTime);
			default:
				return dateTime;
			}
		}

		public static DateTimeOffset TryConvertDateTimeOffset (object value, out bool converted)
		{
			converted = true;

			string str = value as string;
			if (str != null) {
				if (str.Length == 0) {
					return default(DateTime);
				}

				if (StartsWith (str, JaysonConstants.MicrosoftDateFormatStart) && 
					EndsWith (str, JaysonConstants.MicrosoftDateFormatEnd)) {
					str = str.Substring (JaysonConstants.MicrosoftDateFormatStartLen, 
						str.Length - JaysonConstants.MicrosoftDateFormatLen);
					return new DateTimeOffset(ParseUnixEpoch (str));
				}

				return ParseIso8601DateTimeOffset (str);
			}

			if (value == null) {
				return default(DateTimeOffset);
			}

			if (value is DateTimeOffset) {
				return (DateTimeOffset)value;
			}

			if (value is DateTime?) {
				return ((DateTimeOffset?)value).Value;
			}

			if (value is DateTime) {
				return new DateTimeOffset((DateTime)value);
			}

			if (value is DateTime?) {
				return new DateTimeOffset(((DateTime?)value).Value);
			}

			if (value is int) {
				return new DateTimeOffset(FromUnixTimeSec ((int)value));
			}

			if (value is long) {
				return new DateTimeOffset(FromUnixTimeSec ((long)value));
			}

			return new DateTimeOffset(Convert.ToDateTime (value));
		}

		public static DateTimeOffset TryConvertDateTimeOffset (object value, string dateFormat, out bool converted)
		{
			converted = true;

			string str = value as string;
			if (str != null) {
				if (str.Length == 0) {
					return default(DateTime);
				}
				if (StartsWith (str, JaysonConstants.MicrosoftDateFormatStart) && 
					EndsWith (str, JaysonConstants.MicrosoftDateFormatEnd)) {
					str = str.Substring (JaysonConstants.MicrosoftDateFormatStartLen, 
						str.Length - JaysonConstants.MicrosoftDateFormatLen);
					return new DateTimeOffset(ParseUnixEpoch (str));
				}

				if (String.IsNullOrEmpty (dateFormat)) {
					return ParseIso8601DateTimeOffset (str);
				}

				DateTimeStyles dtStyle = DateTimeStyles.None;
				if (EndsWith (str, 'Z')) {
					dtStyle = DateTimeStyles.AdjustToUniversal;
				}

				DateTimeOffset result;
				converted = DateTimeOffset.TryParseExact (str,
					!String.IsNullOrEmpty (dateFormat) ? dateFormat : JaysonConstants.DateIso8601Format,
					JaysonConstants.InvariantCulture, dtStyle, out result);
				return result;
			}

			if (value == null) {
				return default(DateTimeOffset);
			}

			if (value is DateTimeOffset) {
				return (DateTimeOffset)value;
			}

			if (value is DateTimeOffset?) {
				return ((DateTimeOffset?)value).Value;
			}

			if (value is DateTime) {
				return new DateTimeOffset((DateTime)value);
			}

			if (value is DateTime?) {
				return new DateTimeOffset(((DateTime?)value).Value);
			}

			if (value is int) {
				return new DateTimeOffset(FromUnixTimeSec ((int)value));
			}

			if (value is long) {
				return new DateTimeOffset(FromUnixTimeSec ((long)value));
			}

			return new DateTimeOffset(Convert.ToDateTime (value));
		}

		public static DateTimeOffset TryConvertDateTimeOffset (object value, string[] dateFormats, out bool converted)
		{
			converted = true;

			string str = value as string;
			if (str != null) {
				if (str.Length == 0) {
					return default(DateTime);
				}
				if (StartsWith (str, JaysonConstants.MicrosoftDateFormatStart) && 
					EndsWith (str, JaysonConstants.MicrosoftDateFormatEnd)) {
					str = str.Substring (JaysonConstants.MicrosoftDateFormatStartLen, 
						str.Length - JaysonConstants.MicrosoftDateFormatLen);
					return new DateTimeOffset(ParseUnixEpoch (str));
				}

				if (dateFormats == null || dateFormats.Length == 0) {
					return ParseIso8601DateTimeOffset (str);
				}

				DateTimeStyles dtStyle = DateTimeStyles.None;
				if (EndsWith (str, 'Z')) {
					dtStyle = DateTimeStyles.AdjustToUniversal;
				}

				DateTimeOffset result;
				converted = DateTimeOffset.TryParseExact (str, dateFormats,
					JaysonConstants.InvariantCulture, dtStyle, out result);
				return result;
			}

			if (value == null) {
				return default(DateTimeOffset);
			}

			if (value is DateTimeOffset) {
				return (DateTimeOffset)value;
			}

			if (value is DateTimeOffset?) {
				return ((DateTimeOffset?)value).Value;
			}

			if (value is DateTime) {
				return new DateTimeOffset((DateTime)value);
			}

			if (value is DateTime?) {
				return new DateTimeOffset(((DateTime?)value).Value);
			}

			if (value is int) {
				return new DateTimeOffset(FromUnixTimeSec ((int)value));
			}

			if (value is long) {
				return new DateTimeOffset(FromUnixTimeSec ((long)value));
			}

			return new DateTimeOffset(Convert.ToDateTime (value));
		}

		# endregion DateTime Methods

		# region String Methods

		public static bool StartsWith(string str1, string str2)
		{
			if (str1 != null && str2 != null)
			{
				int length2 = str2.Length;
				if (str1.Length >= length2)
				{
					for (int i = 0; i < length2; i++)
					{
						if (str1[i] != str2[i])
							return false;
					}
					return true;
				}
			}
			return false;
		}

		public static bool StartsWith(string str, char ch)
		{
			return !String.IsNullOrEmpty(str) && str [0] == ch;
		}

		public static bool EndsWith(string str1, string str2)
		{
			if (str1 != null && str2 != null)
			{
				int length2 = str2.Length;
				int offset = str1.Length - length2;

				if (offset == 0)
				{
					for (int i = 0; i < length2; i++)
					{
						if (str1[i] != str2[i])
							return false;
					}
					return true;
				}
				else if (offset > 0)
				{
					for (int i = length2 - 1; i > -1; i--)
					{
						if (str1[offset + i] != str2[i])
							return false;
					}
					return true;
				}
			}
			return false;
		}

		public static bool EndsWith(string str, char ch)
		{
			if (str != null)
			{
				int length = str.Length;
				if (length > 0)
				{
					return str [length - 1] == ch;
				}
			}
			return false;
		}

		public static string AsciiToLower(string asciiStr)
		{
			if (asciiStr == null) {
				return asciiStr;
			}

			int length = asciiStr.Length;
			if (length == 0) {
				return asciiStr;
			}

			char ch;
			int len;
			int start = 0;

			StringBuilder builder = new StringBuilder (20, int.MaxValue);
			for (int i = 0; i < length; i++) {
				ch = asciiStr [i];
				if (ch >= 'A' && ch <= 'Z') {
					len = i - start;
					if (len > 1) {
						builder.Append (asciiStr, start, len);
					} 
					builder.Append ((char)((int)ch + LowerCaseDif));
					start = i + 1;
				}
			}

			if (start == 0) {
				return asciiStr;
			}

			if (start < length) {
				builder.Append (asciiStr, start, length - start);
			}
			return builder.ToString ();
		}

		public static bool ParseBoolean(string str) 
		{
			if (!String.IsNullOrEmpty (str)) {
				char ch;
				int pos = 0;
				int length = str.Length;

				while (pos < length) {
					ch = str [pos];

					if (IsWhiteSpace (ch)) {
						pos++;
						continue;
					}

					if (ch == 't' || ch == 'T') {
						if (pos < length - 3 &&
						   (str [++pos] == 'r' || str [pos] == 'R') &&
						   (str [++pos] == 'u' || str [pos] == 'U') &&
						   (str [++pos] == 'e' || str [pos] == 'E')) {
							if (++pos < length) {
								do {
									if (!IsWhiteSpace (str [pos++])) {
										throw new JaysonException ("Invalid boolean string.");
									}
								} while (pos < length);
							}
							return true;
						}
						throw new JaysonException ("Invalid boolean string.");
					}

					if (ch == 'f' || ch == 'F') {
						if (pos < length - 4 &&
						   (str [++pos] == 'a' || str [pos] == 'A') &&
						   (str [++pos] == 'l' || str [pos] == 'L') &&
						   (str [++pos] == 's' || str [pos] == 'S') &&
						   (str [++pos] == 'e' || str [pos] == 'E')) {
							if (++pos < length) {
								do {
									if (!IsWhiteSpace (str [pos++])) {
										throw new JaysonException ("Invalid boolean string.");
									}
								} while (pos < length);
							}
							return false;
						}
						throw new JaysonException ("Invalid boolean string.");
					}

					throw new JaysonException ("Invalid boolean string.");
				}
			}
			throw new JaysonException ("Invalid boolean string.");
		}

		# endregion String Methods

		public static bool IsOnMono()
		{
			if (s_IsMono == -1) {
				s_IsMono = Type.GetType ("Mono.Runtime") != null ? 1 : 0;
			}
			return (s_IsMono == -1);
		}

		public static object EnumToObject(Type enumType, object value)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}

			JaysonTypeCode jtc = JaysonTypeInfo.GetJTypeCode(value.GetType());
			switch (jtc)
			{
			case JaysonTypeCode.Long:
				{
					return Enum.ToObject(enumType, (long)value);
				}
			case JaysonTypeCode.Int:
				{
					return Enum.ToObject(enumType, (long)((int)value));
				}
			case JaysonTypeCode.Short:
				{
					return Enum.ToObject(enumType, (long)((short)value));
				}
			case JaysonTypeCode.Byte:
				{
					return Enum.ToObject(enumType, (long)((byte)value));
				}
			case JaysonTypeCode.ULong:
				{
					return Enum.ToObject(enumType, (long)((ulong)value));
				}
			case JaysonTypeCode.UInt:
				{
					return Enum.ToObject(enumType, (long)((uint)value));
				}
			case JaysonTypeCode.UShort:
				{
					return Enum.ToObject(enumType, (long)((ushort)value));
				}
			case JaysonTypeCode.SByte:
				{
					return Enum.ToObject(enumType, (long)((sbyte)value));
				}
			case JaysonTypeCode.Char:
				{
					return Enum.ToObject(enumType, (long)((char)value));
				}
			case JaysonTypeCode.Double:
				{
					return Enum.ToObject(enumType, (long)((double)value));
				}
			case JaysonTypeCode.Float:
				{
					return Enum.ToObject(enumType, (long)((float)value));
				}
			case JaysonTypeCode.LongNullable:
				{
					return Enum.ToObject(enumType, ((long?)value).Value);
				}
			case JaysonTypeCode.IntNullable:
				{
					return Enum.ToObject(enumType, (long)((int?)value).Value);
				}
			case JaysonTypeCode.ShortNullable:
				{
					return Enum.ToObject(enumType, (long)((short?)value).Value);
				}
			case JaysonTypeCode.ByteNullable:
				{
					return Enum.ToObject(enumType, (long)((byte?)value).Value);
				}
			case JaysonTypeCode.ULongNullable:
				{
					return Enum.ToObject(enumType, (long)((ulong?)value).Value);
				}
			case JaysonTypeCode.UIntNullable:
				{
					return Enum.ToObject(enumType, (long)((uint?)value).Value);
				}
			case JaysonTypeCode.UShortNullable:
				{
					return Enum.ToObject(enumType, (long)((ushort?)value).Value);
				}
			case JaysonTypeCode.SByteNullable:
				{
					return Enum.ToObject(enumType, (long)((sbyte?)value).Value);
				}
			case JaysonTypeCode.CharNullable:
				{
					return Enum.ToObject(enumType, (long)((char?)value).Value);
				}
			case JaysonTypeCode.BoolNullable:
				{
					return Enum.ToObject(enumType, ((bool?)value).Value ? 1L : 0L);
				}
			case JaysonTypeCode.DoubleNullable:
				{
					return Enum.ToObject(enumType, (long)((double?)value).Value);
				}
			case JaysonTypeCode.FloatNullable:
				{
					return Enum.ToObject(enumType, (long)((float?)value).Value);
				}
			default:
				throw new JaysonException("Argument must be Enum base type or Enum");
			}
		}

		public static Type GetType(string typeName, SerializationBinder binder = null)
		{
			Type result;
			if (!s_TypeCache.TryGetValue (typeName, out result)) {
				if (binder != null)
				{
					string[] typeParts = typeName.Split(',');
					if (typeParts.Length > 1)
					{
						result = binder.BindToType(typeParts[0], typeParts[1]);
					}
				}

				if (result == null)
				{
					result = Type.GetType(typeName, false);
				}

				s_TypeCache[typeName] = result;
			}

			return result;
		}

		public static object ConvertToPrimitive(object value, Type toPrimitiveType, out bool converted)
		{
			var info = JaysonTypeInfo.GetTypeInfo (toPrimitiveType);

			// Do not change the type check order
			converted = false;
			switch (info.JTypeCode) {
			case JaysonTypeCode.Int:
				{
					converted = true;
					if (value is int) {
						return value;
					}
					if (value == null) {
						return 0;
					}
					if (value is string) {
						string s = (string)value;
						if (s.Length == 0) {
							return 0;
						}
						return int.Parse (s, NumberStyles.Integer, JaysonConstants.InvariantCulture);
					}
					return Convert.ToInt32 (value);
				}
			case JaysonTypeCode.Bool:
				{
					converted = true;
					if (value is bool) {
						return value;
					}
					if (value == null) {
						return false;
					}
					if (value is string) {
						string s = (string)value;
						if (s.Length == 0) {
							return false;
						}
						return ParseBoolean (s);
					}
					return Convert.ToBoolean (value);
				}
			case JaysonTypeCode.Long:
				{
					converted = true;
					if (value is long) {
						return value;
					}
					if (value == null) {
						return 0L;
					}
					if (value is string) {
						string s = (string)value;
						if (s.Length == 0) {
							return 0L;
						}
						return long.Parse (s, NumberStyles.Integer, JaysonConstants.InvariantCulture);
					}
					return Convert.ToInt64 (value);
				}
			case JaysonTypeCode.Double:
				{
					converted = true;
					if (value == null) {
						return 0d;
					}
					if (value is string) {
						string s = (string)value;
						if (s.Length == 0) {
							return 0d;
						}
						return double.Parse (s, NumberStyles.Float, JaysonConstants.InvariantCulture);
					}
					return Convert.ToDouble (value);
				}
			case JaysonTypeCode.DateTime:
				{
					converted = true;
					return TryConvertDateTime (value, JaysonDateTimeZoneType.KeepAsIs);
				}
			case JaysonTypeCode.Short:
				{
					converted = true;
					if (value == null) {
						return (short)0;
					}
					if (value is string) {
						string s = (string)value;
						if (s.Length == 0) {
							return (short)0;
						}
						return short.Parse (s, NumberStyles.Integer, JaysonConstants.InvariantCulture);
					}
					return Convert.ToInt16 (value);
				}
			case JaysonTypeCode.IntNullable:
				{
					converted = true;
					if (value == null) {
						return (int?)value;
					}
					if (value is string) {
						string s = (string)value;
						if (s.Length == 0) {
							return (int?)null;
						}
						return (int?)int.Parse (s, NumberStyles.Integer, JaysonConstants.InvariantCulture);
					}
					return (int?)Convert.ToInt32 (value);
				}
			case JaysonTypeCode.BoolNullable:
				{
					converted = true;
					if (value == null) {
						return (bool?)value;
					}
					if (value is string) {
						string s = (string)value;
						if (s.Length == 0) {
							return (bool?)null;
						}
						return (bool?)ParseBoolean (s);
					}
					return (bool?)Convert.ToBoolean (value);
				}
			case JaysonTypeCode.LongNullable:
				{
					converted = true;
					if (value == null) {
						return (long?)value;
					}
					if (value is string) {
						string s = (string)value;
						if (s.Length == 0) {
							return (long?)null;
						}
						return (long?)long.Parse (s, NumberStyles.Integer, JaysonConstants.InvariantCulture);
					}
					return (long?)Convert.ToInt64 (value);
				}
			case JaysonTypeCode.DoubleNullable:
				{
					converted = true;
					if (value == null) {
						return (double?)value;
					}
					if (value is string) {
						string s = (string)value;
						if (s.Length == 0) {
							return (double?)null;
						}
						return (double?)decimal.Parse (s, NumberStyles.Float, JaysonConstants.InvariantCulture);
					}
					return (double?)Convert.ToDouble (value);
				}
			case JaysonTypeCode.DateTimeNullable:
				{
					converted = true;
					DateTime dt = TryConvertDateTime (value, JaysonDateTimeZoneType.KeepAsIs);
					if (dt == default(DateTime)) {
						return null;
					}
					return (DateTime?)dt;
				}
			case JaysonTypeCode.ShortNullable:
				{
					converted = true;
					if (value == null) {
						return (short?)value;
					}
					if (value is string) {
						string s = (string)value;
						if (s.Length == 0) {
							return (short?)null;
						}
						return (short?)short.Parse (s, NumberStyles.Integer, JaysonConstants.InvariantCulture);
					}
					return (short?)Convert.ToInt16 (value);
				}

			case JaysonTypeCode.Float:
				{
					converted = true;
					if (value == null) {
						return 0f;
					}
					if (value is string) {
						string s = (string)value;
						if (s.Length == 0) {
							return 0f;
						}
						return float.Parse (s, NumberStyles.Float, JaysonConstants.InvariantCulture);
					}
					return Convert.ToSingle (value);
				}
			case JaysonTypeCode.Decimal:
				{
					converted = true;
					if (value is decimal) {
						return value;
					}
					if (value is double) {
						return Convert.ToDecimal ((double)value);
					}
					if (value == null) {
						return 0m;
					}
					if (value is string) {
						string s = (string)value;
						if (s.Length == 0) {
							return 0m;
						}
						return decimal.Parse (s, NumberStyles.Float, JaysonConstants.InvariantCulture);
					}
					return Convert.ToDecimal (value);
				}
			case JaysonTypeCode.Byte:
				{
					converted = true;
					if (value == null) {
						return (byte)0;
					}
					if (value is string) {
						string s = (string)value;
						if (s.Length == 0) {
							return (byte)0;
						}
						return byte.Parse (s, NumberStyles.Integer, JaysonConstants.InvariantCulture);
					}
					return Convert.ToByte (value);
				}
			case JaysonTypeCode.Guid:
				{
					converted = true;
					if (value is Guid) {
						return value;
					}
					if (value == null) {
						return default(Guid);
					}
					if (value is string) {
						string s = (string)value;
						if (s.Length == 0) {
							return default(Guid);
						}
						if (s [0] == '!') {
							return new Guid(Convert.FromBase64String (s.Substring (1)));
						}
						return Guid.Parse(s);
					}
					if (value is byte[]) {
						return new Guid ((byte[])value);
					}
					return Guid.Parse(value.ToString ());
				}
			case JaysonTypeCode.Char:
				{
					converted = true;
					if (value == null) {
						return (char)0;
					}
					if (value is string) {
						string s = (string)value;
						if (s.Length == 0) {
							return (char)0;
						}
						return s [0];
					}
					return Convert.ToChar (value);
				}
			case JaysonTypeCode.TimeSpan:
				{
					converted = true;
					if (value is TimeSpan) {
						return value;
					}
					if (value == null) {
						return default(TimeSpan);
					}
					if (value is string) {
						string s = (string)value;
						if (s.Length == 0) {
							return default(TimeSpan);
						}
						#if !(NET3500 || NET3000 || NET2000)
						return TimeSpan.Parse (s, JaysonConstants.InvariantCulture);
						#else
						return TimeSpan.Parse(s);
						#endif
					}
					return new TimeSpan (Convert.ToInt64 (value));
				}

			case JaysonTypeCode.FloatNullable:
				{
					converted = true;
					if (value == null) {
						return (float?)value;
					}
					if (value is string) {
						string s = (string)value;
						if (s.Length == 0) {
							return (float?)null;
						}
						return (float?)float.Parse (s, NumberStyles.Float, JaysonConstants.InvariantCulture);
					}
					return (float?)Convert.ToSingle (value);
				}
			case JaysonTypeCode.DecimalNullable:
				{
					converted = true;
					if (value is decimal) {
						return (decimal?)((decimal)value);
					}
					if (value is double) {
						return (decimal?)Convert.ToDecimal ((double)value);
					}
					if (value == null) {
						return (decimal?)value;
					}
					if (value is string) {
						string s = (string)value;
						if (s.Length == 0) {
							return (decimal?)null;
						}
						return (decimal?)decimal.Parse (s, NumberStyles.Float, JaysonConstants.InvariantCulture);
					}
					return (decimal?)Convert.ToDecimal (value);
				}
			case JaysonTypeCode.ByteNullable:
				{
					converted = true;
					if (value == null) {
						return (byte?)value;
					}
					if (value is string) {
						string s = (string)value;
						if (s.Length == 0) {
							return (byte?)null;
						}
						return (byte?)byte.Parse (s, NumberStyles.Integer, JaysonConstants.InvariantCulture);
					}
					return (byte?)Convert.ToByte (value);
				}
			case JaysonTypeCode.GuidNullable:
				{
					converted = true;
					if (value is Guid) {
						return (Guid?)((Guid)value);
					}
					if (value == null) {
						return (Guid?)value;
					}
					if (value is string) {
						string s = (string)value;
						if (s.Length == 0) {
							return default(Guid?);
						}
						if (s [0] == '!') {
							return (Guid?)(new Guid(Convert.FromBase64String (s.Substring (1))));
						}
						return (Guid?)Guid.Parse(s);
					}
					if (value is byte[]) {
						return (Guid?)(new Guid ((byte[])value));
					}
					return (Guid?)Guid.Parse(value.ToString ());
				}
			case JaysonTypeCode.CharNullable:
				{
					converted = true;
					if (value == null) {
						return (char?)null;
					}
					if (value is string) {
						string s = (string)value;
						if (s.Length == 0) {
							return (char?)null;
						}
						return (char?)s [0];
					}
					return Convert.ToChar (value);
				}
			case JaysonTypeCode.TimeSpanNullable:
				{
					converted = true;
					if (value is TimeSpan) {
						return (TimeSpan?)((TimeSpan)value);
					}
					if (value == null) {
						return (TimeSpan?)value;
					}
					if (value is string) {
						string s = (string)value;
						if (s.Length == 0) {
							return (TimeSpan?)null;
						}
						#if !(NET3500 || NET3000 || NET2000)
						return (TimeSpan?)TimeSpan.Parse (s, JaysonConstants.InvariantCulture);
						#else
						return (TimeSpan?)TimeSpan.Parse(s);
						#endif
					}
					return (TimeSpan?)(new TimeSpan (Convert.ToInt64 (value)));
				}
			case JaysonTypeCode.UInt:
				{
					converted = true;
					if (value == null) {
						return (uint)0;
					}
					if (value is string) {
						string s = (string)value;
						if (s.Length == 0) {
							return (uint)0;
						}
						return uint.Parse (s, NumberStyles.Integer, JaysonConstants.InvariantCulture);
					}
					return Convert.ToUInt32 (value);
				}
			case JaysonTypeCode.ULong:
				{
					converted = true;
					if (value == null) {
						return (ulong)0;
					}
					if (value is string) {
						string s = (string)value;
						if (s.Length == 0) {
							return (ulong)0;
						}
						return ulong.Parse (s, NumberStyles.Integer, JaysonConstants.InvariantCulture);
					}
					return Convert.ToUInt64 (value);
				}
			case JaysonTypeCode.UShort:
				{
					converted = true;
					if (value == null) {
						return (ushort)0;
					}
					if (value is string) {
						string s = (string)value;
						if (s.Length == 0) {
							return (ushort)0;
						}
						return ushort.Parse (s, NumberStyles.Integer, JaysonConstants.InvariantCulture);
					}
					return Convert.ToUInt16 (value);
				}
			case JaysonTypeCode.SByte:
				{
					converted = true;
					if (value == null) {
						return (sbyte)0;
					}
					if (value is string) {
						string s = (string)value;
						if (s.Length == 0) {
							return (sbyte)0;
						}
						return sbyte.Parse (s, NumberStyles.Integer, JaysonConstants.InvariantCulture);
					}
					return Convert.ToSByte (value);
				}
			case JaysonTypeCode.UIntNullable:
				{
					converted = true;
					if (value == null) {
						return (uint?)value;
					}
					if (value is string) {
						string s = (string)value;
						if (s.Length == 0) {
							return (uint?)null;
						}
						return (uint?)uint.Parse (s, NumberStyles.Integer, JaysonConstants.InvariantCulture);
					}
					return (uint?)Convert.ToUInt32 (value);
				}
			case JaysonTypeCode.ULongNullable:
				{
					converted = true;
					if (value == null) {
						return (ulong?)value;
					}
					if (value is string) {
						string s = (string)value;
						if (s.Length == 0) {
							return (ulong?)null;
						}
						return (ulong?)ulong.Parse (s, NumberStyles.Integer, JaysonConstants.InvariantCulture);
					}
					return (ulong?)Convert.ToUInt64 (value);
				}
			case JaysonTypeCode.UShortNullable:
				{
					converted = true;
					if (value == null) {
						return (ushort?)value;
					}
					if (value is string) {
						string s = (string)value;
						if (s.Length == 0) {
							return (ushort?)null;
						}
						return (ushort?)ushort.Parse (s, NumberStyles.Integer, JaysonConstants.InvariantCulture);
					}
					return (ushort?)Convert.ToUInt16 (value);
				}
			case JaysonTypeCode.SByteNullable:
				{
					converted = true;
					if (value == null) {
						return (sbyte?)value;
					}
					if (value is string) {
						string s = (string)value;
						if (s.Length == 0) {
							return (sbyte?)null;
						}
						return (sbyte?)sbyte.Parse (s, NumberStyles.Integer, JaysonConstants.InvariantCulture);
					}
					return (sbyte?)Convert.ToSByte (value);
				}
			case JaysonTypeCode.DateTimeOffset:
				{
					return TryConvertDateTimeOffset (value, out converted);
				}
			case JaysonTypeCode.DateTimeOffsetNullable:
				{
					if (value == null) {
						return (DateTimeOffset?)value;
					}
					DateTimeOffset dto = TryConvertDateTimeOffset (value, out converted);
					if (dto == default(DateTimeOffset)) {
						return null;
					}
					return dto;
				}
			}

			if (info.Enum) {
				if (value == null) {
					return Enum.ToObject (toPrimitiveType, 0L);
				}

				if (value is long) {
					return Enum.ToObject (toPrimitiveType, (long)value);
				}
				if (value is int) {
					return Enum.ToObject (toPrimitiveType, (long)((int)value));
				}

				if (value is string) {
					string s = (string)value;
					if (s.Length == 0) {
						return Enum.ToObject (toPrimitiveType, 0L);
					}
					return JaysonEnumCache.Parse (s, toPrimitiveType);
				}

				return Enum.ToObject (toPrimitiveType, Convert.ToInt64 (value));
			}
			return value;
		}

		public static bool IsWhiteSpaceChar(char ch)
		{
			return IsWhiteSpace(ch);
			// return ch == ' ' || (ch >= '\x0009' && ch <= '\x000d') || ch == '\x00a0' || ch == '\x0085';
		}

		public static bool IsWhiteSpace(int ch)
		{
			if (ch != 32 && (ch < 9 || ch > 13) && ch != 160)
				return ch == 133;
			return true; 
		}

		internal static bool FindInterface(Type objType, Type interfaceType, out Type[] arguments)
		{
			arguments = null;
			if (objType.IsGenericType)
			{
				JaysonTypeInfo iInfo;
				var iTypes = JaysonTypeInfo.GetInterfaces(objType);

				for (int i = iTypes.Length - 1; i > -1; i--)
				{
					iInfo = JaysonTypeInfo.GetTypeInfo(iTypes[i]);
					if (iInfo.Type == interfaceType || 
						(iInfo.Generic && iInfo.GenericTypeDefinitionType == interfaceType))
					{
						arguments = iInfo.GenericArguments;
						if (arguments.Length == 0)
						{
							arguments = null;
						}
						return true;
					}
				}
			}
			return false;
		}

		private static void UpdateGenericCollectionInfo(Type objType)
		{
			Type[] arguments;
			bool found = FindInterface(objType, typeof(ICollection<>), out arguments);

			s_IsGenericCollection[objType] = found;
			s_GenericCollectionArgs[objType] = found ? arguments[0] : null;
		}

		private static void UpdateGenericDictionaryInfo(Type objType)
		{
			Type[] arguments;
			bool found = FindInterface(objType, typeof(IDictionary<,>), out arguments);

			s_IsGenericDictionary[objType] = found;
			s_GenericDictionaryArgs[objType] = arguments;
		}

		private static void UpdateGenericListInfo(Type objType)
		{
			Type[] arguments;
			bool found = FindInterface(objType, typeof(IList<>), out arguments);

			s_IsGenericList[objType] = found;
			s_GenericListArgs[objType] = found ? arguments[0] : null;
		}

		internal static bool IsGenericCollection(Type objType)
		{
			bool result;
			if (!s_IsGenericCollection.TryGetValue(objType, out result))
			{
				UpdateGenericCollectionInfo(objType);
				s_IsGenericCollection.TryGetValue(objType, out result);
			}
			return result;
		}

		internal static bool IsGenericDictionary(Type objType)
		{
			bool result;
			if (!s_IsGenericDictionary.TryGetValue(objType, out result))
			{
				UpdateGenericDictionaryInfo(objType);
				s_IsGenericDictionary.TryGetValue(objType, out result);
			}
			return result;
		}

		internal static bool IsGenericList(Type objType)
		{
			bool result;
			if (!s_IsGenericList.TryGetValue(objType, out result))
			{
				UpdateGenericListInfo(objType);
				s_IsGenericList.TryGetValue(objType, out result);
			}
			return result;
		}

		internal static Type GetGenericCollectionArgs(Type objType)
		{
			Type result;
			if (!s_GenericCollectionArgs.TryGetValue(objType, out result))
			{
				UpdateGenericCollectionInfo(objType);
				s_GenericCollectionArgs.TryGetValue(objType, out result);
			}
			return result;
		}

		internal static Type[] GetGenericDictionaryArgs(Type objType)
		{
			Type[] result;
			if (!s_GenericDictionaryArgs.TryGetValue(objType, out result))
			{
				UpdateGenericDictionaryInfo(objType);
				s_GenericDictionaryArgs.TryGetValue(objType, out result);
			}
			return result;
		}

		internal static Type GetGenericListArgs(Type objType)
		{
			Type result;
			if (!s_GenericListArgs.TryGetValue(objType, out result))
			{
				UpdateGenericListInfo(objType);
				s_GenericListArgs.TryGetValue(objType, out result);
				return result;
			}
			return result;
		}

		internal static bool StackContains(ArrayList stack, object obj)
		{
			for (int i = stack.Count - 1; i > -1; i--)
			{
				if (obj == stack[i])
					return true;
			}
			return false;
		}

		public static Func<object[], object> CreateActivator (ConstructorInfo ctor)
		{
			Type declaringT = ctor.DeclaringType;
			ParameterInfo[] ctorParams = ctor.GetParameters ();

			// Create a single param of type object[]
			ParameterExpression paramExp = Expression.Parameter(typeof(object[]), "args");

			int length = ctorParams.Length;
			Expression[] argsExp = new Expression[length];

			Type paramType;
			Expression paramAccessorExp;
			UnaryExpression paramCastExp;

			// Pick each arg from the params array and create a typed expression of them
			for (int i = 0; i < length; i++)
			{
				paramType = ctorParams[i].ParameterType;

				#if !(NET3500 || NET3000 || NET2000)
				paramAccessorExp = Expression.ArrayAccess(paramExp, Expression.Constant(i));
				#else
				paramAccessorExp = Expression.ArrayIndex(paramExp, Expression.Constant (i));
				#endif
				paramCastExp = !paramType.IsValueType ?
					Expression.TypeAs(paramAccessorExp, paramType) : Expression.Convert(paramAccessorExp, paramType);

				argsExp[i] = paramCastExp;
			}                  

			// Make a NewExpression that calls the ctor with the args we just created
			NewExpression newExp = Expression.New(ctor, argsExp);                  
			Expression returnExp = !declaringT.IsValueType ? (Expression)newExp :
				Expression.Convert (newExp, typeof(object));

			// Create a lambda with the New Expression as body and our param object[] as arg
			var lambda = Expression.Lambda<Func<object[], object>> (returnExp, paramExp);

			return lambda.Compile ();
		}

		#if !(NET3500 || NET3000 || NET2000)
		public static Action<object, object[]> PrepareMethodCall(MethodInfo methodInfo)
		{
			Type declaringT = methodInfo.DeclaringType;
			ParameterInfo[] parameters = methodInfo.GetParameters();

			ParameterExpression argsExp = Expression.Parameter(typeof(object[]), "args");
			ParameterExpression inputObjExp = Expression.Parameter(typeof(object), "inputObj");
			ParameterExpression tVariable = Expression.Variable(declaringT);

			List<ParameterExpression> variableList = new List<ParameterExpression> { tVariable };

			Expression inputCastExp = !declaringT.IsValueType ?
				Expression.TypeAs (inputObjExp, declaringT) : 
				Expression.Convert(inputObjExp, declaringT);

			Expression assignmentExp = Expression.Assign(tVariable, inputCastExp);

			List<Expression> bodyExps = new List<Expression> { assignmentExp };

			Expression callExp = null;
			if (parameters.Length == 0)
			{
				callExp = Expression.Call(tVariable, methodInfo);
			}
			else 
			{
				Type paramType;
				Expression arrayAccessExp;
				Expression arrayValueCastExp;
				Expression variableAssignExp;
				ParameterExpression newVariableExp;

				List<ParameterExpression> callArguments = new List<ParameterExpression>();

				for (int i = 0; i < parameters.Length; i++ )
				{
					paramType = parameters[i].ParameterType;

					newVariableExp = Expression.Variable(paramType, "param" + i);

					callArguments.Add(newVariableExp);

					arrayAccessExp = Expression.ArrayAccess(argsExp, Expression.Constant(i));
					arrayValueCastExp = !paramType.IsValueType ?
						Expression.TypeAs (arrayAccessExp, paramType) : Expression.Convert(arrayAccessExp, paramType);

					variableAssignExp = Expression.Assign(newVariableExp, arrayValueCastExp);
					bodyExps.Add(variableAssignExp);
				}

				variableList.AddRange(callArguments);
				callExp = Expression.Call(tVariable, methodInfo, callArguments);
			}

			bodyExps.Add(callExp);

			BlockExpression body = Expression.Block(variableList, bodyExps);
			return Expression.Lambda<Action<object, object[]>>(body, inputObjExp, argsExp).Compile();
		}
		#else
		public static Action<object, object[]> PrepareMethodCall(MethodInfo methodInfo)
		{
			var declaringT = methodInfo.DeclaringType;
			var methodParams = methodInfo.GetParameters();

			var paramExp = Expression.Parameter(typeof (object[]), "args");
			var inputObjExp = Expression.Parameter(typeof(object), "inputObj");

			var inputCastExp = !declaringT.IsValueType ?
			Expression.TypeAs (inputObjExp, declaringT) : Expression.Convert(inputObjExp, declaringT);

			Expression callExp;
			if (methodParams.Length == 0) {
				callExp = Expression.Call (inputCastExp, methodInfo);
			} else {
				var callArguments = new Expression[methodParams.Length];

				Type paramType;
				Expression arrayAccessExp;
				Expression arrayValueCastExp;

				for (var i = 0; i < methodParams.Length; i++) {
					paramType = methodParams [i].ParameterType;

					arrayAccessExp = Expression.ArrayIndex(paramExp, Expression.Constant(i));

					arrayValueCastExp = !paramType.IsValueType ?
						Expression.TypeAs (arrayAccessExp, paramType) : Expression.Convert (arrayAccessExp, paramType);

					callArguments [i] = arrayValueCastExp;
				}

				callExp = Expression.Call (inputCastExp, methodInfo, callArguments);
			}
			return Expression.Lambda<Action<object, object[]>>(callExp, paramExp).Compile ();            
		}
		#endif

		internal static Action<object, object[]> GetICollectionAddMethod(Type objType)
		{
			Action<object, object[]> result;
			if (!s_ICollectionAdd.TryGetValue(objType, out result))
			{
				MethodInfo method;
				MethodInfo[] methods = objType.GetMethods();

				for (int i = methods.Length - 1; i > -1; i--)
				{
					method = methods[i];
					if (method.Name == "Add" && method.GetParameters().Length == 1)
					{
						result = PrepareMethodCall(method);
						break;
					}
				}

				s_ICollectionAdd[objType] = result;
			}
			return result;
		}

		internal static Action<object, object[]> GetIDictionaryAddMethod(Type objType)
		{
			Action<object, object[]> result;
			if (!s_IDictionaryAdd.TryGetValue(objType, out result))
			{
				MethodInfo method;
				MethodInfo[] methods = objType.GetMethods();

				for (int i = methods.Length - 1; i > -1; i--)
				{
					method = methods[i];
					if (method.Name == "Add" && method.GetParameters().Length == 2)
					{
						result = PrepareMethodCall(method);
						break;
					}
				}
				s_ICollectionAdd[objType] = result;
			}
			return result;
		}

		internal static JaysonDictionaryType GetDictionaryType(IEnumerable obj, out Type entryType)
		{
			entryType = null;
			IEnumerator enumerator = obj.GetEnumerator();
			try
			{
				if (enumerator.MoveNext())
				{
					if (enumerator.Current is DictionaryEntry)
					{
						return JaysonDictionaryType.IDictionary;
					}

					entryType = enumerator.Current.GetType();

					IDictionary<string, IJaysonFastMember> members = JaysonFastMemberCache.GetMembers(entryType);
					if (members.ContainsKey("Key") && members.ContainsKey("Value"))
					{
						return JaysonDictionaryType.IGenericDictionary;
					}
				}
			}
			finally
			{
				if (enumerator is IDisposable)
				{
					((IDisposable)enumerator).Dispose();
				}
			}
			return JaysonDictionaryType.Undefined;
		}

		# endregion Helper Methods
	}
}