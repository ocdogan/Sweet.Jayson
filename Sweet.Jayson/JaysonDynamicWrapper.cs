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
#if !(NET3500 || NET3000 || NET2000)
using System.Dynamic;
#endif
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

#if !(NET3500 || NET3000 || NET2000)
using Microsoft.CSharp.RuntimeBinder;
#endif

namespace Sweet.Jayson
{
    # region JaysonDynamicWrapper

    internal sealed class JaysonDynamicWrapper
    {
#if !(NET3500 || NET3000 || NET2000)
        # region Constants

        private const int CacheCapacity = 1000;

        # endregion Constants

        # region CallSiteCache

        private static class CallSiteCache
        {
            private static readonly object s_GetterLock = new object();
            private static readonly Dictionary<string, CallSite<Func<CallSite, object, object>>> s_Getters =
                new Dictionary<string, CallSite<Func<CallSite, object, object>>>(JaysonConstants.CacheInitialCapacity);

            private static readonly object s_SetterLock = new object();
            private static readonly Dictionary<string, CallSite<Func<CallSite, object, object, object>>> s_Setters =
                new Dictionary<string, CallSite<Func<CallSite, object, object, object>>>(JaysonConstants.CacheInitialCapacity);

            internal static object GetValue(string name, object target)
            {
                CallSite<Func<CallSite, object, object>> callSite;
                if (!s_Getters.TryGetValue(name, out callSite))
                {
                    lock (s_GetterLock)
                    {
                        if (!s_Getters.TryGetValue(name, out callSite))
                        {
                            callSite =
                                CallSite<Func<CallSite, object, object>>.Create(
                                    Microsoft.CSharp.RuntimeBinder.Binder.GetMember(CSharpBinderFlags.None, name,
                                        typeof(CallSiteCache), new CSharpArgumentInfo[] { 
											CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) 
										}));

                            s_Getters[name] = callSite;
                        }
                    }
                }

                return callSite.Target(callSite, target);
            }

            internal static void SetValue(string name, object target, object value)
            {
                CallSite<Func<CallSite, object, object, object>> callSite;
                if (!s_Setters.TryGetValue(name, out callSite))
                {
                    lock (s_SetterLock)
                    {
                        if (!s_Setters.TryGetValue(name, out callSite))
                        {
                            callSite =
                                CallSite<Func<CallSite, object, object, object>>.Create(
                                    Microsoft.CSharp.RuntimeBinder.Binder.SetMember(CSharpBinderFlags.None, name,
                                        typeof(CallSiteCache), new CSharpArgumentInfo[] { 
											CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null), 
											CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null) 
										}));

                            s_Setters[name] = callSite;
                        }
                    }
                }
                callSite.Target(callSite, target, value);
            }
        }

        # endregion CallSiteCache

        private readonly IDynamicMetaObjectProvider m_Target;

        public object Target
        {
            get { return m_Target; }
        }

        public JaysonDynamicWrapper(IDynamicMetaObjectProvider target)
        {
            m_Target = target;
        }

        public object this[string name]
        {
            get
            {
                return CallSiteCache.GetValue(name, m_Target);
            }
            set
            {
                CallSiteCache.SetValue(name, m_Target, value);
            }
        }
#endif
    }

    # endregion JaysonDynamicWrapper
}