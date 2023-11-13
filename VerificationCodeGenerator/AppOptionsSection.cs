using System.Configuration;
using static VerificationCodeGenerator.PropertyHelper;

namespace VerificationCodeGenerator
{
    public class AppOptionsSection : ConfigurationSection
    {
        [ConfigurationProperty("properties")]
        public PropertySettingsCollection Properties
        {
            get { return (PropertySettingsCollection)this["properties"]; }
        }
    }
}
