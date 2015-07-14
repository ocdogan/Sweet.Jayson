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

namespace Sweet.Jayson
{
    public enum JaysonTypeSerializationType : int
    {
        ClassOrStruct = 0,
        IDictionaryGeneric = 1,
        IDictionary = 2,
        DynamicObject = 3,
        DataTable = 4,
        DataSet = 5,
        StringDictionary = 6,
        NameValueCollection = 7,
        IEnumerable = 8,
        Anonymous = 9,
        ISerializable = 10,
        Enum = 11,
        Type = 12,
        FieldInfo = 13,
        PropertyInfo = 14,
        MethodInfo = 15,
        ConstructorInfo = 16,
        ParameterInfo = 17
    }
}
