using System.Diagnostics;

namespace FreeSpace
{
    public class RdoPrecision
    {
        private string myVar;

        public string Precision
        {
            get
            {
                Debug.WriteLine("In Precision get");
                return myVar;
            }
            set
            {
                Debug.WriteLine("In Precision set");
                myVar = value;
            }
        }

    }
}
