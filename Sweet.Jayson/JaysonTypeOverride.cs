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
using System.Text;
#if !(NET3500 || NET3000 || NET2000)
using System.Threading.Tasks;
#endif

namespace Sweet.Jayson
{
    public class JaysonTypeOverride : ICloneable
    {
        # region Field Members

        private Type m_Type;
        private Type m_BindToType;
        private object m_SyncRoot = new object();

        private Dictionary<string, bool> m_IgnoredMembers = new Dictionary<string, bool>();
        private Dictionary<string, string> m_AliasToMemberName = new Dictionary<string, string>();
        private Dictionary<string, string> m_MemberNameToAlias = new Dictionary<string, string>();

        # endregion Field Members

        # region Properties

        public Type Type 
        { 
            get { return m_Type; } 
        }

        public Type BindToType
        {
            get { return m_BindToType; }
            set { m_BindToType = value; }
        }

        # endregion Properties

        public JaysonTypeOverride(Type type, Type bindToType = null)
        {
            m_Type = type;
            m_BindToType = bindToType;
        }

        public bool IsMemberIgnored(string memberName)
        {
            bool result;
            if (m_IgnoredMembers.TryGetValue(memberName, out result))
            {
                return result;
            }
            return false;
        }

        public JaysonTypeOverride IgnoreMember(string memberName)
        {
            m_IgnoredMembers[memberName] = true;
            return this;
        }

        public JaysonTypeOverride IncludeMember(string memberName)
        {
            m_IgnoredMembers[memberName] = false;
            return this;
        }

        public JaysonTypeOverride SetMemberAlias(string memberName, string alias)
        {
            if (!String.IsNullOrEmpty(alias))
            {
                m_MemberNameToAlias[memberName] = alias;
                m_AliasToMemberName[alias] = memberName;
                return this;
            }
            return RemoveMemberAlias(memberName);
        }

        public JaysonTypeOverride RemoveMemberAlias(string memberName)
        {
            lock (m_SyncRoot)
            {
                string alias;
                if (m_MemberNameToAlias.TryGetValue(memberName, out alias))
                {
                    m_MemberNameToAlias.Remove(memberName);
                    if (m_AliasToMemberName.ContainsKey(alias))
                    {
                        m_AliasToMemberName.Remove(alias);
                    }
                }
            }
            return this;
        }

        public string GetMemberAlias(string memberName)
        {
            string result;
            m_MemberNameToAlias.TryGetValue(memberName, out result);
            return result;
        }

        public string GetAliasMember(string alias)
        {
            string result;
            m_AliasToMemberName.TryGetValue(alias, out result);
            return result;
        }

        public object Clone()
        {
            JaysonTypeOverride result = new JaysonTypeOverride(m_Type, m_BindToType);
            foreach (var iKvp in m_IgnoredMembers)
            {
                result.m_IgnoredMembers.Add(iKvp.Key, iKvp.Value);
            }

			foreach (var aKvp in m_AliasToMemberName)
			{
				result.m_AliasToMemberName.Add(aKvp.Key, aKvp.Value);
			}

            foreach (var aKvp in m_MemberNameToAlias)
            {
                result.m_MemberNameToAlias.Add(aKvp.Key, aKvp.Value);
            }

            return result;
        }

        public JaysonTypeOverride Clone(Type insteadType)
        {
            JaysonTypeOverride result = (JaysonTypeOverride)Clone();
            result.m_BindToType = insteadType;
            return result;
        }
    }
}
