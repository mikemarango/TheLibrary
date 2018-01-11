using System.Collections.Generic;

namespace Library.API.Helpers
{
    public interface IPropertyMapping
    {
        Dictionary<string, PropertyMappingValue> MappingDictionary { get; }
    }
}