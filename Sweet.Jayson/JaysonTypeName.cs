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
using System.Linq;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Sweet.Jayson
{
    # region JaysonTypeName

    public class JaysonTypeName
    {
        # region Static Members

        # region Static Readonly Members

        private static JaysonSynchronizedDictionary<Type, JaysonTypeName> s_TypeNameCache;

        # endregion Static Readonly Members

        # endregion Static Members

        # region Field Members

        # region Field Readonly Members

        private readonly object m_NameCacheLock = new object();
        private JaysonSynchronizedDictionary<JaysonTypeNameFormatFlags, string> m_NameCache;

        # endregion Field Readonly Members

        # endregion Field Members

        # region .Ctors

        private JaysonTypeName()
        { }

        private JaysonTypeName(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            JaysonTypeName[] genericArgs = null;
            if (type.IsGenericType)
            {
                var argTypes = type.GetGenericArguments();
                if (argTypes != null && argTypes.Length > 0)
                {
                    genericArgs = new JaysonTypeName[argTypes.Length];
                    for (var i = 0; i < argTypes.Length; i++)
                    {
                        genericArgs[i] = GetTypeName(argTypes[i]);
                        genericArgs[i].IsGenericArgument = true;
                    }
                }
            }

            var assemblyName = type.Assembly.GetName();

            var publicKeyToken = EncodePublicKeyToken(assemblyName.GetPublicKeyToken());

            Name = type.Name;
            GenericArguments = genericArgs;
            Namespace = String.IsNullOrEmpty(type.Namespace) ? null : type.Namespace;
            AssemblyName = assemblyName.Name;
            Version = (assemblyName.Version != null) ? assemblyName.Version.ToString() : null;
            PublicKeyToken = (publicKeyToken != null) ? publicKeyToken.ToLowerInvariant() : null;
            Culture = assemblyName.CultureInfo != null ? assemblyName.CultureInfo.Name : null;
        }

        # endregion .Ctors

        # region Properties

        public string Name { get; private set; }
        public string Namespace { get; private set; }
        public string AssemblyName { get; private set; }
        public string Version { get; private set; }
        public string PublicKeyToken { get; private set; }
        public string Culture { get; private set; }
        public JaysonTypeName[] GenericArguments { get; private set; }
        public bool IsGenericArgument { get; private set; }

        public string AssemblyQualifiedName
        {
            get
            {
                return FormatTypeName(JaysonTypeNameFormatFlags.AssemblyQualifiedName);
            }
        }

        public string FullName
        {
            get
            {
                return FormatTypeName(JaysonTypeNameFormatFlags.FullName);
            }
        }

        public bool IsGeneric
        {
            get { return GenericArguments != null && GenericArguments.Length > 0; }
        }

        # endregion Properties

        # region Write Methods

        private void WriteTo(StringBuilder sBuilder, JaysonTypeNameFormatFlags flags)
        {
            if (flags.HasFlag(JaysonTypeNameFormatFlags.Namespace) &&
                !String.IsNullOrEmpty(Namespace))
            {
                sBuilder.Append(Namespace);
                sBuilder.Append('.');
            }
            sBuilder.Append(Name);

            var writeAssembly = flags.HasFlag(JaysonTypeNameFormatFlags.Assembly);
            var parameterAssembly = flags.HasFlag(JaysonTypeNameFormatFlags.ParameterAssembly);

            if (flags.HasFlag(JaysonTypeNameFormatFlags.GenericParams) && IsGeneric)
            {
                sBuilder.Append('[');

                for (var i = 0; i < GenericArguments.Length; i++)
                {
                    if (i > 0)
                    {
                        sBuilder.Append(", ");
                    }

                    if (!(writeAssembly || parameterAssembly))
                    {
                        GenericArguments[i].WriteTo(sBuilder, flags);
                    }
                    else
                    {
                        sBuilder.Append('[');
                        GenericArguments[i].WriteTo(sBuilder, flags);
                        sBuilder.Append(']');
                    }
                }
                sBuilder.Append(']');
            }

            if (!String.IsNullOrEmpty(AssemblyName) && (writeAssembly || (IsGenericArgument && parameterAssembly)))
            {
                sBuilder.Append(',');
                sBuilder.Append(' ');
                sBuilder.Append(AssemblyName);

                if (!String.IsNullOrEmpty(Version) &&
                    (flags.HasFlag(JaysonTypeNameFormatFlags.Version) ||
                        (IsGenericArgument && flags.HasFlag(JaysonTypeNameFormatFlags.ParameterVersion))))
                {
                    sBuilder.Append(", Version=");
                    sBuilder.Append(Version);
                }

                if (flags.HasFlag(JaysonTypeNameFormatFlags.Culture) ||
                    (IsGenericArgument && flags.HasFlag(JaysonTypeNameFormatFlags.ParameterCulture)))
                {
                    sBuilder.Append(", Culture=");
                    sBuilder.Append(String.IsNullOrEmpty(Culture) ? "neutral" : Culture);
                }

                if (flags.HasFlag(JaysonTypeNameFormatFlags.PublicKeyToken) ||
                    (IsGenericArgument && flags.HasFlag(JaysonTypeNameFormatFlags.ParameterPublicKeyToken)))
                {
                    sBuilder.Append(", PublicKeyToken=");
                    sBuilder.Append(String.IsNullOrEmpty(PublicKeyToken) ? JaysonConstants.Null : PublicKeyToken);
                }
            }
        }

        private JaysonSynchronizedDictionary<JaysonTypeNameFormatFlags, string> GetNameCache()
        {
            if (m_NameCache == null)
                Interlocked.Exchange(ref m_NameCache, new JaysonSynchronizedDictionary<JaysonTypeNameFormatFlags, string>());
            return m_NameCache;
        }

        public string FormatTypeName(JaysonTypeNameFormatFlags flags)
        {
            return GetNameCache().GetValueOrUpdate(flags, (f) =>
                {
                    var sBuilder = new StringBuilder(IsGeneric ? 200 : 60);
                    WriteTo(sBuilder, f);
                    return sBuilder.ToString();
                });
        }

        private static char IntToHexDigit(int num)
        {
            return (char)((num < 10) ? (num + 48) : (num + 55));
        }

        public static string EncodePublicKeyToken(byte[] pk)
        {
            if (pk != null && pk.Length > 0)
            {
                var array = new char[pk.Length * 2];

                var num = 0;
                for (var i = 0; i < pk.Length; i++)
                {
                    var num2 = (pk[i] & 240) >> 4;

                    array[num++] = IntToHexDigit(num2);

                    num2 = (int)(pk[i] & 15);
                    array[num++] = IntToHexDigit(num2);
                }
                return new string(array);
            }
            return null;
        }

        private static JaysonSynchronizedDictionary<Type, JaysonTypeName> GetTypeNameCache()
        {
            if (s_TypeNameCache == null)
                Interlocked.Exchange(ref s_TypeNameCache, new JaysonSynchronizedDictionary<Type, JaysonTypeName>());
            return s_TypeNameCache;
        }

        public static JaysonTypeName GetTypeName(Type type)
        {
            if (type != null)
                return GetTypeNameCache().GetValueOrUpdate(type, (t) => new JaysonTypeName(t));
            return null;
        }

        # endregion Write Methods

        public override string ToString()
        {
            return FormatTypeName(JaysonTypeNameFormatFlags.Simple);
        }
    }

    # endregion JaysonTypeName
}
