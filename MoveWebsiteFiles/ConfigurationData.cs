using System;
using System.ComponentModel;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using SandcastleBuilder.Utils;

namespace MoveWebsiteFiles
{
    internal class ConfigurationData
    {
        private ConfigurationData(IBasePathProvider basePathProvider, XPathNavigator navigator)
        {
            BasePathProvider = basePathProvider;
            UseDirectMove = GetBoolean(navigator, "configuration/useDirectMove", false);
        }

        private ConfigurationData(ConfigurationData other)
        {
            UseDirectMove = other.UseDirectMove;
        }

        public ConfigurationData Clone()
        {
            return new ConfigurationData(this);
        }
        
        [Browsable(false)]
        public IBasePathProvider BasePathProvider { get; private set; }

        [DefaultValue(false)]
        [LocalizableCategory("ConfigCategorySettings")]
        [Description("ConfigUseDirectMove")]
        public bool UseDirectMove { get; set; }

        public static ConfigurationData FromXml(IBasePathProvider basePathProvider, XPathNavigator configuration)
        {
            return new ConfigurationData(basePathProvider, configuration);
        }

        public static ConfigurationData FromXml(IBasePathProvider basePathProvider, string configuration)
        {
            using (var reader = new StringReader(configuration))
            {
                var doc = new XPathDocument(reader);
                var navigator = doc.CreateNavigator();
                return FromXml(basePathProvider, navigator);
            }
        }

        public static string ToXml(ConfigurationData configuration)
        {
            var doc = new XmlDocument();
            var configurationNode = doc.CreateElement("configuration");
            doc.AppendChild(configurationNode);

            var useDirectMoveNode = doc.CreateElement("useDirectMove");
            useDirectMoveNode.InnerText = XmlConvert.ToString(configuration.UseDirectMove);
            configurationNode.AppendChild(useDirectMoveNode);

            return doc.OuterXml;
        }

        private static bool GetBoolean(XPathNavigator navigator, string xpath, bool defaultValue)
        {
            var value = navigator.SelectSingleNode(xpath);
            return (value == null)
                    ? defaultValue
                    : value.ValueAsBoolean;
        }
    }
}
