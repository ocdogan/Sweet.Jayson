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
    # region JaysonTypeMemberCache

    internal class JaysonTypeMemberCache
    {
        # region TypeMemberIndex

        private class TypeMemberIndex
        {
            # region Field Members

            private int m_Count;

            private IJaysonFastMember[] m_AllMembers;
            private IJaysonFastMember[] m_FieldMembers;
            private IJaysonFastMember[] m_PropertyMembers;

            private Dictionary<string, IJaysonFastMember> m_AllIndex =
                new Dictionary<string, IJaysonFastMember>(JaysonConstants.CacheInitialCapacity);
            private Dictionary<string, IJaysonFastMember> m_FieldIndex =
                new Dictionary<string, IJaysonFastMember>(JaysonConstants.CacheInitialCapacity);
            private Dictionary<string, IJaysonFastMember> m_PropertyIndex =
                new Dictionary<string, IJaysonFastMember>(JaysonConstants.CacheInitialCapacity);

            # endregion Field Members

            # region Properties

            public IJaysonFastMember this[string name]
            {
                get
                {
                    IJaysonFastMember result;
                    m_AllIndex.TryGetValue(name, out result);
                    return result;
                }
                set
                {
                    m_AllMembers = null;
                    m_FieldMembers = null;
                    m_PropertyMembers = null;

                    m_AllIndex[name] = value;
                    m_Count = m_AllIndex.Count;

                    if (value == null)
                    {
                        m_FieldIndex[name] = null;
                        m_PropertyIndex[name] = null;
                    }
                    else if (value.Type == JaysonFastMemberType.Field)
                    {
                        m_FieldIndex[name] = value;
                    }
                    else
                    {
                        m_PropertyIndex[name] = value;
                    }
                }
            }

            public int Count
            {
                get { return m_Count; }
            }

            public IJaysonFastMember[] AllMembers
            {
                get
                {
                    if (m_AllMembers == null)
                    {
                        m_AllMembers = GetItemsOf(m_AllIndex);
                    }
                    return m_AllMembers;
                }
            }

            public IJaysonFastMember[] Fields
            {
                get
                {
                    if (m_FieldMembers == null)
                    {
                        m_FieldMembers = GetItemsOf(m_FieldIndex);
                    }
                    return m_FieldMembers;
                }
            }

            public IJaysonFastMember[] Properties
            {
                get
                {
                    if (m_PropertyMembers == null)
                    {
                        m_PropertyMembers = GetItemsOf(m_PropertyIndex);
                    }
                    return m_PropertyMembers;
                }
            }

            # endregion Properties

            # region Methods

            private IJaysonFastMember[] GetItemsOf(Dictionary<string, IJaysonFastMember> map)
            {
                IJaysonFastMember[] items = null;
                if ((map != null) && (map.Count > 0))
                {
                    items = map.Values.Cast<IJaysonFastMember>()
                        .Where(m => m != null)
                        .OrderBy(m => String.IsNullOrEmpty(m.Alias) ? m.Name : m.Alias)
                        .ToArray();
                }
                return items ?? new IJaysonFastMember[0];
            }

            public IJaysonFastMember GetAnyMember(string memberName)
            {
                IJaysonFastMember result = null;
                if (!String.IsNullOrEmpty(memberName))
                {
                    m_AllIndex.TryGetValue(memberName, out result);
                }
                return result;
            }

            public IJaysonFastMember GetField(string memberName)
            {
                IJaysonFastMember result = null;
                if (!String.IsNullOrEmpty(memberName))
                {
                    m_FieldIndex.TryGetValue(memberName, out result);
                }
                return result;
            }

            public IJaysonFastMember GetProperty(string memberName)
            {
                IJaysonFastMember result = null;
                if (!String.IsNullOrEmpty(memberName))
                {
                    m_PropertyIndex.TryGetValue(memberName, out result);
                }
                return result;
            }

            # endregion Methods
        }

        # endregion TypeMemberIndex

        # region Field Members

        private Type m_Type;
        private TypeMemberIndex m_Members;
        private TypeMemberIndex m_InvariantMembers;

        # endregion Field Members

        # region Properties

        public int Count
        {
            get { return (m_Members != null) ? m_Members.Count : 0; }
        }

        public IJaysonFastMember this[string name]
        {
            get
            {
                if (!String.IsNullOrEmpty(name))
                {
                    var member = m_Members[name];
                    if (member == null)
                    {
                        name = name.ToLower(JaysonConstants.InvariantCulture);
                        member = m_InvariantMembers[name];
                    }
                    return member;
                }
                return null;
            }
        }

        public IJaysonFastMember[] AllMembers
        {
            get { return (m_Members != null) ? m_Members.AllMembers : new IJaysonFastMember[0]; }
        }

        public IJaysonFastMember[] Fields
        {
            get { return (m_Members != null) ? m_Members.Fields : new IJaysonFastMember[0]; }
        }

        public IJaysonFastMember[] Properties
        {
            get { return (m_Members != null) ? m_Members.Properties : new IJaysonFastMember[0]; }
        }

        # endregion Properties

        public JaysonTypeMemberCache(Type type)
        {
            m_Type = type;
            FillMembers();
        }

        # region Methods

        # region FillMembers

        private void FillMembers()
        {
            var pis = m_Type.GetProperties(BindingFlags.Instance | 
                BindingFlags.Public | 
                BindingFlags.NonPublic | 
                BindingFlags.GetProperty | 
                BindingFlags.SetProperty);

            string name;
            IJaysonFastMember member;

            var members = new TypeMemberIndex();
            var invariantMembers = new TypeMemberIndex();

            // Properties
            PropertyInfo pi;

            for (int i = pis.Length - 1; i > -1; i--)
            {
                pi = pis[i];
                if (pi.CanRead && (pi.PropertyType != typeof(void*)))
                {
                    var iParams = pi.GetIndexParameters();

                    if ((iParams == null || iParams.Length == 0) && 
                        !pi.IsDefined(typeof(JaysonIgnoreMemberAttribute), true))
                    {
                        member = new JaysonFastProperty(pi, true, true);
                        if (!member.Ignored)
                        {
                            name = pi.Name;

                            members[name] = member;
                            invariantMembers[name.ToLower(JaysonConstants.InvariantCulture)] = member;

                            if (!String.IsNullOrEmpty(member.Alias))
                            {
                                JaysonTypeOverrideGlobal.SetMemberAlias(m_Type, name, member.Alias);
                            }
                        }
                    }
                }
            }

            // Fields
            var fis = m_Type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            FieldInfo fi;

            for (int i = fis.Length - 1; i > -1; i--)
            {
                fi = fis[i];
                if ((fi.FieldType != typeof(void*)) && !fi.IsDefined(typeof(JaysonIgnoreMemberAttribute), true))
                {
                    member = new JaysonFastField(fi, true, true);
                    if (!member.Ignored)
                    {
                        name = fi.Name;

                        members[name] = member;
                        invariantMembers[name.ToLower(JaysonConstants.InvariantCulture)] = member;

                        if (!String.IsNullOrEmpty(member.Alias))
                        {
                            JaysonTypeOverrideGlobal.SetMemberAlias(m_Type, name, member.Alias);
                        }
                    }
                }
            }

            m_Members = members;
            m_InvariantMembers = invariantMembers;
        }

        # endregion FillMembers

        # region Get Methods

        public IJaysonFastMember GetAnyMember(string memberName, bool caseSensitive = true)
        {
            if (!String.IsNullOrEmpty(memberName))
            {
                if (!caseSensitive)
                {
                    memberName = memberName.ToLower(JaysonConstants.InvariantCulture);
                    return m_InvariantMembers.GetAnyMember(memberName);
                }
                return m_Members.GetAnyMember(memberName);
            }
            return null;
        }

        public IJaysonFastMember GetProperty(string memberName, bool caseSensitive = true)
        {
            if (!String.IsNullOrEmpty(memberName))
            {
                if (!caseSensitive)
                {
                    memberName = memberName.ToLower(JaysonConstants.InvariantCulture);
                    return m_InvariantMembers.GetProperty(memberName);
                }
                return m_Members.GetProperty(memberName);
            }
            return null;
        }

        public IJaysonFastMember GetField(string memberName, bool caseSensitive = true)
        {
            if (!String.IsNullOrEmpty(memberName))
            {
                if (!caseSensitive)
                {
                    memberName = memberName.ToLower(JaysonConstants.InvariantCulture);
                    return m_InvariantMembers.GetField(memberName);
                }
                return m_Members.GetField(memberName);
            }
            return null;
        }

        # endregion Get Methods

        # endregion Methods
    }

    # endregion JaysonTypeMemberCache
}
