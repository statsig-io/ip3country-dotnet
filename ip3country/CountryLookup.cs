using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace IP3Country
{
    public static class CountryLookup
    {
        static bool initialized;
        static List<string> countryCodes = new List<string>();
        static List<Int64> ipRanges = new List<long>();
        static List<string> countryTable = new List<string>();

        public static void Initialize()
        {
            if (initialized)
            {
                return;
            }

            var assembly = Assembly.GetExecutingAssembly();
            var resName = assembly.GetName().Name + ".ip_supalite.table";
            using (var stream = assembly.GetManifestResourceStream(resName))
            {
                var memStream = new MemoryStream();
                stream.CopyTo(memStream);
                var buffer = memStream.ToArray();
                InitializeWithBuffer(buffer);
            }
        }

        public static string LookupIPStr(string ipaddrStr)
        {
            Initialize();
            if (string.IsNullOrWhiteSpace(ipaddrStr))
            {
                return null;
            }

            var components = ipaddrStr.Split('.');
            if (components.Length != 4)
            {
                return null;
            }

            try
            {
                Int64 ipNumber = Int64.Parse(components[0]) * 16777216 +
                    Int64.Parse(components[1]) * 65536 +
                    Int64.Parse(components[2]) * 256 +
                    Int64.Parse(components[3]);
                return LookupIPNumber(ipNumber);
            } 
            catch (FormatException)
            {
                return null;
            }
        }

        public static string LookupIPNumber(Int64 ipNumber)
        {
            Initialize();
            var index = BinarySearch(ipNumber);
            var cc = countryCodes[index];
            return (cc == "--") ? null : cc;
        }


        /*
            The binary is packed as follows:
            c1.c2.c3.....**: Country code look up table, terminated by **
   
            n1.c: if n is < 240, c is country code index
            242.n2.n3.c: if n >= 240 but < 65536. n2 being lower order byte
            243.n2.n3.n4.c: if n >= 65536. n2 being lower order byte
        */
        static void InitializeWithBuffer(byte[] buffer)
        {
            var index = 0;
            while (index < buffer.Length)
            {
                var c1 = buffer[index++];
                var c2 = buffer[index++];

                countryTable.Add(new string(new char[] { (char)c1, (char)c2 }));
                if (c1 == '*')
                {
                    break;
                }
            }

            Int64 lastEndRange = 0;
            while (index < buffer.Length)
            {
                Int64 count = 0;
                var n1 = buffer[index++];
                if (n1 < 240)
                {
                    count = n1;
                } 
                else if (n1 == 242)
                {
                    var n2 = buffer[index++];
                    var n3 = buffer[index++];

                    count = n2 | (n3 << 8);
                } 
                else if (n1 == 243)
                {
                    var n2 = buffer[index++];
                    var n3 = buffer[index++];
                    var n4 = buffer[index++];

                    count = n2 | (n3 << 8) | (n4 << 16);
                }

                lastEndRange += (count * 256);
                var cc = buffer[index++];

                ipRanges.Add(lastEndRange);
                countryCodes.Add(countryTable[cc]);
            }

            initialized = true;
        }

        static int BinarySearch(Int64 ipNumber)
        {
            var min = 0;
            var max = ipRanges.Count - 1;
            
            while (min < max)
            {
                var mid = (min + max) >> 1;
                if (ipRanges[mid] <= ipNumber)
                {
                    min = mid + 1;
                }
                else
                {
                    max = mid;
                }
            }

            return min;
        }
    }
}
