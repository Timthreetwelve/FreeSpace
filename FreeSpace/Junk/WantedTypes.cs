using System.Xml.Serialization;

namespace FreeSpace
{
    public partial class WantedTypes
    {
        [XmlElement("DriveType")]
        public DriveTypes[] DriveType { get; set; }
    }

    public partial class DriveTypes
    {
        [XmlAttribute()]
        public string Name { get; set; }

        [XmlAttribute()]
        public string Value { get; set; }
    }
}

namespace FreeSpace
{
    public partial class LogSettings
    {
        [XmlElement("LogSetting")]
        public LogSetting[] LogSetting { get; set; }
    }


    public partial class LogSetting
    {
        [XmlAttribute()]
        public string Name { get; set; }

        [XmlAttribute()]
        public string Value { get; set; }

    }
}
