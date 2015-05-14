**Sweet.Jayson**
================

 

Fast, reliable, easy to use, fully [json.org][1] compliant C\# JSON library for
server side and desktop operations.

[1]: <http://json.org/>

 

Open sourced under [MIT license][2].

[2]: <http://opensource.org/licenses/MIT>

 

##### **Supports:**

-   *.Net 3.5, 4.0, 4.5 & Mono*

-   Any *POCO* type serialization/deserialization,

-   Default .Net types (*DateTime, DateTimeOffset, TimeSpan, Guid, ArrayList,
    HashTable* …),

-   Default and custom generic .Net types (*List\<T\>, Dictionary\<T, K\>* …),

-   *DataTable, DataSet* (also *custom DataSets* and *DataTable relations in
    DataSets*),

-   *Nullable* types (*int?, DateTime?* …),

-   *Interfaces*, both default .Net and also custom interfaces (*IList,
    IList\<T\>, IMyCustomInterface* …),

-   Array types (both *class* and *value* types) (*int[], char[], string[]* …),

-   Byte arrays (*byte[]*) as *Base64String,*

-   Embedding type information with *"\$type"* notation for exact type
    deserialization,

-   Including or excluding *"null"* values into/from output (both in
    *properties* or in *lists* separately),

-   Case sensitive/in-case sensitive property names

~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
{
    "state"": "open",
    "base"": {
        "label": "technoweenie:master",
        "ref": "master",
        "sha": "53397635da83a2f4b5e862b5e59cc66f6c39f9c6"
    }
}
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

or

~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
{
    "State"": "open",
    "Base"": {
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
    "state"": "open",
    "base"": {
        "label": "technoweenie:master",
        "ref": "master",
        "sha": "53397635da83a2f4b5e862b5e59cc66f6c39f9c6"
    }
}
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

-   Changing types in deserialization using
    *System.Runtime.Serialization.SerializationBinder*

-   Changing *property* and *field names into any value* without using custom
    Attribute classes by *JaysonTypeOverride*

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

-   Changing types in serialization/deserialization using *JaysonTypeOverride*

-   Ignoring *Field* and *Properties* over *JaysonTypeOverride*

-   Including or excluding\* read-only members (Field & Properties)\* into/from
    serialization

-   *Anonymous Type serialization/deserialization* (deserialization is only
    possible if the type already exists in the current process)

-   Custom *Number, DateTime, DateTimeOffset and TimeSpan* formatting in
    serialization

-   Various *DateTime* serialization/deserialization formats over
    *JaysonDateFormatType*:

*Iso8601*

~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    Local date: “1972-10-25T16:00:30.345+0300” 
    UTC date: “1972-10-25T13:00:30.345Z”
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

*Microsoft*

~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    Local date: “/Date(1224043200000+0300)/”
    UTC date: “/Date(1224043200000)/”
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

-   Converting *Decimal* values to *Double* or keep them in *Decimal* form

-   Enable/Disable *Anonymous Types, Dynamic Objects* and *Expando Types*

-   Enable/Disable *Unicode Character Escaping*

~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    \u0357
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

-   *Sorting Field* and *Property names* descending

-   Ignoring *Circular References* or raise exception on Circular Reference

-   Use *Enum Type* names or numeric Enum values

-   *Maximum Object Depth* in deserialization

-   Option to parse *JSON arrays []* convertion between *List\<object\>,
    ArrayList* or *Array*

-   Option to parse *JSON objects {}* to *Dictionary\<string, object\>* or
    *Expando Objects*

-   Custom object creation outside the convertion process using
    *JaysonObjectActivator*
