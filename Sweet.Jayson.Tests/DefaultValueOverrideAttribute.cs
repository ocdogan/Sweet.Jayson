using System;
using System.ComponentModel;

namespace Sweet.Jayson.Tests
{
    public class DefaultValueOverrideAttribute : DefaultValueAttribute
    {
        public DefaultValueOverrideAttribute(Type valueType)
            : base(null)
        {
            if (valueType != null)
            {
                try
                {
                    base.SetValue(Activator.CreateInstance(valueType, true));
                }
                catch (Exception)
                { }
            }
        }
    }
}

