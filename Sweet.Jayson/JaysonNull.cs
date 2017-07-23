using System;

namespace Sweet.Jayson
{
    public sealed class JaysonNull
    {
        public static readonly JaysonNull Value = new JaysonNull();

        private JaysonNull()
        { }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(obj, null) || ReferenceEquals(obj, JaysonNull.Value) ||
                ReferenceEquals(obj, DBNull.Value);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}

