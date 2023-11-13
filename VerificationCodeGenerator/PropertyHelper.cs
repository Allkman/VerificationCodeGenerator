using System.Configuration;
using System.Linq;

namespace VerificationCodeGenerator
{

    public static class PropertyHelper
    {
        public static string GetPropertyValue(string propertyName, AppOptionsSection appOptions)
        {
            PropertySettings property = appOptions.Properties
                .Cast<PropertySettings>()
                .FirstOrDefault(p => p.Name == propertyName);

            return property?.Value ?? string.Empty;
        }

        public class PropertySettings : ConfigurationElement
        {
            [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
            public string Name
            {
                get { return (string)this["name"]; }
                set { this["name"] = value; }
            }

            [ConfigurationProperty("value", IsRequired = true)]
            public string Value
            {
                get { return (string)this["value"]; }
                set { this["value"] = value; }
            }
        }

        [ConfigurationCollection(typeof(PropertySettings), AddItemName = "add", CollectionType = ConfigurationElementCollectionType.BasicMap)]
        public class PropertySettingsCollection : ConfigurationElementCollection
        {
            protected override ConfigurationElement CreateNewElement()
            {
                return new PropertySettings();
            }

            protected override object GetElementKey(ConfigurationElement element)
            {
                return ((PropertySettings)element).Name;
            }
        }
    }
}
