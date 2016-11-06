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
using System.Linq;
using System.Reflection;

namespace Sweet.Jayson
{
    # region JaysonFastMemberCache

    internal static class JaysonFastMemberCache
    {
        # region Constants

        private const int CacheCapacity = 10000;

        # endregion Constants

        # region Static Members

        private static readonly object s_TypeMembersLock = new object();
        private static Dictionary<Type, JaysonTypeMemberCache> s_TypeMembers =
            new Dictionary<Type, JaysonTypeMemberCache>(JaysonConstants.CacheInitialCapacity);

        # endregion Static Members

        public static JaysonTypeMemberCache GetCache(Type objType)
        {
            if (objType != null)
            {
                JaysonTypeMemberCache cache;
                if (!s_TypeMembers.TryGetValue(objType, out cache))
                {
                    lock (s_TypeMembers)
                    {
                        if (!s_TypeMembers.TryGetValue(objType, out cache))
                        {
                            cache = new JaysonTypeMemberCache(objType);
                            s_TypeMembers[objType] = cache;
                        }
                    }
                }
                return cache;
            }
            return null;
        }

        public static IJaysonFastMember GetAnyMember(Type objType, string memberName, bool caseSensitive = true)
        {
            if ((objType != null) && !String.IsNullOrEmpty(memberName))
            {
                JaysonTypeMemberCache cache;
                if (!s_TypeMembers.TryGetValue(objType, out cache))
                {
                    lock (s_TypeMembers)
                    {
                        if (!s_TypeMembers.TryGetValue(objType, out cache))
                        {
                            cache = new JaysonTypeMemberCache(objType);
                            s_TypeMembers[objType] = cache;
                        }
                    }
                }

                if (cache != null)
                {
                    return cache.GetAnyMember(memberName);
                }
            }
            return null;
        }

        public static IJaysonFastMember[] GetMembers(Type objType)
        {
            if (objType != null)
            {
                JaysonTypeMemberCache cache;
                if (!s_TypeMembers.TryGetValue(objType, out cache))
                {
                    lock (s_TypeMembers)
                    {
                        if (!s_TypeMembers.TryGetValue(objType, out cache))
                        {
                            cache = new JaysonTypeMemberCache(objType);
                            s_TypeMembers[objType] = cache;
                        }
                    }
                }

                if (cache != null)
                {
                    return cache.AllMembers;
                }
            }
            return null;
        }

        public static IJaysonFastMember[] GetFields(Type objType)
        {
            if (objType != null)
            {
                JaysonTypeMemberCache cache;
                if (!s_TypeMembers.TryGetValue(objType, out cache))
                {
                    lock (s_TypeMembers)
                    {
                        if (!s_TypeMembers.TryGetValue(objType, out cache))
                        {
                            cache = new JaysonTypeMemberCache(objType);
                            s_TypeMembers[objType] = cache;
                        }
                    }
                }

                if (cache != null)
                {
                    return cache.Fields;
                }
            }
            return null;
        }

        public static IJaysonFastMember[] GetProperties(Type objType)
        {
            if (objType != null)
            {
                JaysonTypeMemberCache cache;
                if (!s_TypeMembers.TryGetValue(objType, out cache))
                {
                    lock (s_TypeMembers)
                    {
                        if (!s_TypeMembers.TryGetValue(objType, out cache))
                        {
                            cache = new JaysonTypeMemberCache(objType);
                            s_TypeMembers[objType] = cache;
                        }
                    }
                }

                if (cache != null)
                {
                    return cache.Properties;
                }
            }
            return null;
        }
    }

    # endregion JaysonFastMemberCache
}