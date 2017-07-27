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
    public enum JaysonTypeNameFormatFlags : int
    {
        Basic = 0,
        Namespace = 1 << 0,
        GenericParams = 1 << 1,
        Assembly = 1 << 2,
        Version = Assembly | 1 << 3,
        Culture = Assembly | 1 << 4,
        PublicKeyToken = Assembly | 1 << 5,

        ParameterAssembly = 1 << 6,
        ParameterVersion = ParameterAssembly | 1 << 7,
        ParameterCulture = ParameterAssembly | 1 << 8,
        ParameterPublicKeyToken = ParameterAssembly | 1 << 9,

        Simple =
            Basic |
            Namespace |
            GenericParams,

        FullName =
            Simple |
            Assembly |
            ParameterAssembly |
            ParameterVersion |
            ParameterCulture |
            ParameterPublicKeyToken,

        AssemblyQualifiedName =
            FullName |
            Version |
            Culture |
            PublicKeyToken,
    }
}
