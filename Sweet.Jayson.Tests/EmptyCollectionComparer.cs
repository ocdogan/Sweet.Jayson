using System;
using System.Collections;

namespace Sweet.Jayson.Tests
{
    public class EmptyCollectionComparer
    {
        public EmptyCollectionComparer()
        { }

        public override bool Equals(object obj)
        {
            if (obj != null)
            {
                var collection = obj as ICollection;
                return (collection != null) && (collection.Count == 0);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}

