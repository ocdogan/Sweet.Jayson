using System;
using System.Collections.Generic;

namespace Sweet.Jayson
{
	public delegate object JaysonObjectActivator(Type toType, IDictionary<string, object> parsedObject, out bool useDefaultCtor);
}

