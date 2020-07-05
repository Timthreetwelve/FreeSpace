using System.Xml.Serialization;

namespace FreeSpace
{
    public class Prefs
    {

        public string LogFile { get; set; }


        public string Precision { get; set; }

        [XmlElement("TimeStamp")]
        public string TimeStamp { get; set; }
    }
}
