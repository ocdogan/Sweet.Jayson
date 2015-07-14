**Sweet.Jayson**
================

 

Fast, reliable, easy to use, fully [json.org](<http://json.org/>) compliant,
thread safe C\# JSON library for server side and desktop operations.

 

Open sourced under [MIT license](<http://opensource.org/licenses/MIT>).

 

### **Supports:**

-   *.Net 3.5, 4.0, 4.5 & Mono*

-   Any *POCO* type serialization/deserialization,

-   Default .Net types (`DateTime`*,* `DateTimeOffset`*,* `TimeSpan`*,*
    `Guid`*,* `ArrayList`*,* `Hashtable`*,* `HashSet` …),

-   Default and custom Generic .Net types (`List<T>`*,* `Dictionary<T,K>`*,*
    `Stack<T>`*,* `Queue<T>` …),

-   Concurrent collection types (`ConcurrentBag<T>`*,*
    `ConcurrentDictionary<T,K>, ConcurrentStack<T>`*,* `ConcurrentQueue<T>`),

-   Any dictionary type that has key type different than `typeof(string)`, with
    special key value pair (`$k`, `$v`) object (`IDictionary<int,decimal>`*,*
    `IDictionary`*,* `Hashtable` …),

-   `DataTable`*,* `DataSet` (also *custom* `DataSets` and `DataTable`
    *relations in DataSets*),

-   `Nullable` types (`int?`*,* `DateTime?` …),

-   *Interfaces*, both default .Net and also custom interfaces (`IList`*,*
    `IList<T>`*,* `IMyCustomInterface` …),

-   Array types (both *class* and *value* types) (`int[]`*,* `char[]`*,*
    `string[]` …),

-   Byte arrays (`byte[]`) as *Base64String,*

-   Multidimensional arrays (`int[,]`),

-   Jagged arrays (`int[][]`),

-   Mixed arrays (`int[][,]` and `int[,][]`),

-   `dynamic` Types,

-   `struct` Types, both on serialization and deserialization,

-   Types without default constructor (e.g. `Tuple`),

-   Embedding type information with `"$type"` notation for exact type
    deserialization,

-   Embedding global type information with `"$types"` notation for exact type
    deserialization and compact output,

-   Including or excluding `"null"` values into/from output (both in
    `Properties` or in `Lists` separately),

-   Serializing `Guid` as byte array (`byte[]`),

-   Case sensitive/in-case sensitive property names

~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
{
    "state": "open",
    "base": {
        "label": "technoweenie:master",
        "ref": "master",
        "sha": "53397635da83a2f4b5e862b5e59cc66f6c39f9c6"
    }
}
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

or

~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
{
    "State": "open",
    "Base": {
        "Label": "technoweenie:master",
        "Ref": "master",
        "SHA": "53397635da83a2f4b5e862b5e59cc66f6c39f9c6"
    }
}
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

-   Enable/Disable *Formating* and *beautify output* without performance loss

~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
{"state"":"open","base"":{"label":"technoweenie:master","ref"":"master","sha"":"53397635da83a2f4b5e862b5e59cc66f6c39f9c6"}}
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

or

~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
{
    "state": "open",
    "base": {
        "label": "technoweenie:master",
        "ref": "master",
        "sha": "53397635da83a2f4b5e862b5e59cc66f6c39f9c6"
    }
}
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

-   Changing types in deserialization using
    `System.Runtime.Serialization.SerializationBinder`,

-   Changing `Property` and `Field` names into any value without using custom
    `Attribute` classes by `JaysonTypeOverride`,

~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
var dto1 = TestClasses.GetTypedContainerDto();

JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
jaysonSerializationSettings.
    AddTypeOverride(
        new JaysonTypeOverride<TextElementDto>().
            IgnoreMember("ElementType").
            SetMemberAlias("ElementId", "id"));

JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
jaysonDeserializationSettings.
    AddTypeOverride(
        new JaysonTypeOverride<TextElementDto, TextElementDto2>()).
    AddTypeOverride(new JaysonTypeOverride<TextElementDto2>().
        SetMemberAlias("ElementId", "id").
        IgnoreMember("ElementType")); 

string json = JaysonConverter.ToJsonString(dto1, jaysonSerializationSettings);
var dto2 = JaysonConverter.ToObject<TypedContainerDto>(json, jaysonDeserializationSettings); 
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

-   Changing types in serialization/deserialization using `JaysonTypeOverride`,

-   Ignoring `Field` and `Properties` over `JaysonTypeOverride`,

-   Including or excluding read-only members (`Field` & `Properties`) into/from
    serialization,

-   `AnonymousType` serialization/deserialization (deserialization is only
    possible if the type already exists in the current process),

-   Custom `Number` (`byte`, `short`, `int`, `long`, `float`, `double`,
    `decimal`, `sbyte`, `ushort`, `uint`, `ulong`), `DateTime`, `DateTimeOffset`
    and `TimeSpan` formatting in serialization,

-   Various `DateTime` serialization/deserialization formats over
    `JaysonDateFormatType`:

*Iso8601*

~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    Local date: "1972-10-25T16:00:30.345+0300" 
    UTC date: "1972-10-25T13:00:30.345Z"
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

*Microsoft*

~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    Local date: "/Date(1224043200000+0300)/"
    UTC date: "/Date(1224043200000)/"
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

*JScript*

~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    Local date: new Date(1224043200000+0300)
    UTC date: new Date(1224043200000)
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

*UnixEpoch* 

~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    Any long number: 1224043200000
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

*CustomDate*

~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    Any date custom date format defined in serialization settings
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

*CustomUnixEpoch*

~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    Any custom long number format defined in serialization settings
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

-   Converting `Decimal` values to `Double` or keep them in `Decimal` form,

-   Enable/Disable `AnonymousTypes`*,* `DynamicObject` and `ExpandoObject`
    *Types,*

-   Enable/Disable *Unicode Character Escaping,*

~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    \u0357
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

-   *Sorting* `Field` and `Property` names descending,

-   Ignoring `Circular References` or raise exception on Circular Reference,

-   Use `Enum` Type names or numeric `Enum` values,

-   *Maximum Object Depth* in deserialization,

-   Option to work with default types such as `Dictionary<string,object>` or
    `ExpandoObject`, `List<object>`*,* `ArrayList` or `Array` instead of
    converting the object to a custom .Net type

    -   Option to parse *JSON arrays* `[]` conversion between `List<object>`*,*
        `ArrayList` or `Array`

    -   Option to parse *JSON objects* `{}` to `Dictionary<string,object>` or
        `ExpandoObject`

~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
{
    "State": "open",
    "Base": {
        "Label": "technoweenie:master",
        "Ref": "master",
        "SHA": "53397635da83a2f4b5e862b5e59cc66f6c39f9c6",
        "Items": [
            "Dev",
            "Test",
            "Prod"
        ]
    }
}
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

will be parsed to

~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
new Dictionary<string, object> { 
    { "State": "open" },
    { "Base": 
        {
            "Label": "technoweenie:master",
            "Ref": "master",
            "SHA": "53397635da83a2f4b5e862b5e59cc66f6c39f9c6",
            "Items": new List<object> { "Dev", "Test", "Prod" }
        }
    }
};
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

-   Custom object creation outside the conversion process using
    `JaysonObjectActivator`*,*

-   Converting any .Net object to `Dictionary<string,object>` and `List<object>`
    using `JaysonConverter.ToJsonObject` function

~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
public class CustomBaseType
{
    public string Label { get; set; }
    public string Ref { get; set; }
    public string SHA { get; set; }
    public List<string> Items { get; set; }
}

public class CustomType
{ 
    public string State;
    public CustomBaseType Base { get; set; }
}

public static class Program
{
    public static void Main(params string[] args)
    {
        var dto = new CustomType() { 
            { 
                State = "open" 
                Base = new CustomBaseType() 
                    {
                        Label = "technoweenie:master",
                        Ref = "master",
                        SHA = "53397635da83a2f4b5e862b5e59cc66f6c39f9c6",
                        Items = new List<object> { "Dev", "Test", "Prod" }
                    }
            };

        var jsonObj = 
            (Dictionary<string, object>)JaysonConverter.ToJsonObject(dto);
    }
}
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

-   Object referencing with `$id` and `$ref` tags,

~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
{
    "$type": "ObjectToDict.A, ObjectToDict",  
    "$id": 1,  
    "O2": {  
        "1": {  
            "$type": "System.Decimal, mscorlib",  
            "$value": 2  
        },  
        "$id": 2,  
        "a": "b",  
        "true": false,  
        "#self": {  
            "$ref": 2  
        },  
        "#list": {  
            "$type": "System.Collections.Generic.List`1[[System.Object, mscorlib]], mscorlib",  
            "$id": 3,  
            "$values": [  
                {  
                    "$ref": 2  
                }  
            ]  
        }  
    },  
    "O1": true,  
    "E1": "EnumB, EnumC",  
    "D2": 23456.78901,  
    "L2": {  
        "$id": 4,  
        "$values": [  
            1,  
            null,  
            2,  
            null  
        ]  
    },  
    "L1": {  
        "$type": "System.Int64, mscorlib",  
        "$value": 12345678909876544  
    },  
    "D4": {  
        "$ref": 2  
    },  
    "D1": {  
        "$type": "System.Decimal, mscorlib",  
        "$value": 12345.6789  
    },  
    "I1": 123456789  
}
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

-   Enable/Disable `KV` mode for `Dictionary<?,object>`

~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
{  
    "$type": "Sweet.Jayson.Tests.A, Sweet.Jayson.Tests",  
    "D3": {  
        "$type": "System.Collections.Generic.Dictionary`2[[System.Object, mscorlib],[System.Object, mscorlib]], mscorlib",  
        "1": {  
            "$type": "System.Decimal, mscorlib",  
            "$value": 2  
        },  
        "a": "b",  
        "True": false  
    }  
}
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

or

~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
{  
    "$type": "Sweet.Jayson.Tests.A, Sweet.Jayson.Tests",  
    "D3": {  
        "$type": "System.Collections.Generic.Dictionary`2[[System.Object, mscorlib],[System.Object, mscorlib]], mscorlib",  
        "$kv": [  
            {  
                "$k": 1,  
                "$v": {  
                    "$type": "System.Decimal, mscorlib",  
                    "$value": 2  
                }  
            },  
            {  
                "$k": "a",  
                "$v": "b"  
            },  
            {  
                "$k": true,  
                "$v": false  
            }  
        ]  
    }  
}
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

 

Details can be found at :
[Sweet.Jayson](<http://www.codeproject.com/Articles/998316/Sweet-Jayson>) link

Revision History
----------------

#### v1.0.0.9

14 July 2015

-   `System.Runtime.Serialization.ISerializable` interface support

-   `System.Type` serialization support

-   `System.Reflection.ConstuctorInfo`, `System.Reflection.MethodInfo`,
    `System.Reflection.PropertyInfo`, `System.Reflection.FieldInfo`
    serialization support

-   `System.Exception` serialization support

-   Fix for `System.Type.GetType` which works different than Mono version

-   Detailed test cases

#### v1.0.0.8

17 June 2015

-   Object referencing support

-   Performance optimisation

-   Setting for enable/disable KV mode for Dictionary\<?,object\>

-   Detailed test cases

#### v1.0.0.7

11 June 2015

-   Performance improvements

-   `JaysonOrderedDictionary` implemented for member names

-   Detailed test cases

#### v1.0.0.6

10 June 2015

-   New tests added

-   `IDictionary<object,object>` Key conversion fix

-   `ConvertToPrimitive `fix for `enum`s

#### v1.0.0.5

9 June 2015

-   Long number serialization fix

-   `AsciiToLower` & `AsciiToUpper` fix

#### v1.0.0.4

9 June 2015

-   Some refactoring

-   Sealed some classes

-   Error message class implemented

-   License header added to *JaysonError *file

#### v1.0.0.3

-   8 June 2015, Performance optimization on GUID serialization

#### v1.0.0.2

-   6 June 2015, Minor fix for list handling, when the last item is null or
    boolean

#### v1.0.0.1

-   6 June 2015, Added support for special types; Stack, Queue and Concurrent
    lists

#### v1.0.0.0

-   3 May 2015, Initial release
