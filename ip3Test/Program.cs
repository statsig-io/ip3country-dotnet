using IP3Country;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace ip3Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var p = new Program();
            // p.RunRandomTests();
            p.RunLookupTests();
        }

        void RunLookupTests()
        {
            var testTable = new Dictionary<string, string>
            {
                ["1.1.1.1"] = "US",
                ["2.2.2.2"] = "FR",
                ["3.3.3.3"] = "US",
                ["4.4.4.4"] = "US",
                ["5.5.5.5"] = "DE",
                ["6.6.6.6"] = "US",
                ["7.7.7.7"] = "US",
                ["8.8.8.8"] = "US",
                ["9.9.9.9"] = "US",
                ["11.11.11.11"] = "US",
                ["12.12.12.12"] = "US",
                ["13.13.13.13"] = "US",
                ["14.14.14.14"] = "JP",
                ["15.15.15.15"] = "US",
                ["16.16.16.16"] = "US",
                ["17.17.17.17"] = "US",
                ["18.18.18.18"] = "US",
                ["19.19.19.19"] = "US",
                ["20.20.20.20"] = "US",
                ["21.21.21.21"] = "US",
                ["22.22.22.22"] = "US",
                ["23.23.23.23"] = "US",
                ["24.24.24.24"] = "US",
                ["25.25.25.25"] = "GB",
                ["26.26.26.26"] = "US",
                ["27.27.27.27"] = "CN",
                ["28.28.28.28"] = "US",
                ["29.29.29.29"] = "US",
                ["30.30.30.30"] = "US",
                ["31.31.31.31"] = "MD",
                ["41.41.41.41"] = "EG",
                ["42.42.42.42"] = "KR",
                ["45.45.45.45"] = "CA",
                ["46.46.46.46"] = "RU",
                ["49.49.49.49"] = "TH",
                ["101.101.101.101"] = "TW",
                ["110.110.110.110"] = "CN",
                ["111.111.111.111"] = "JP",
                ["112.112.112.112"] = "CN",
                ["150.150.150.150"] = "KR",
                ["200.200.200.200"] = "BR",
                ["202.202.202.202"] = "CN",
                ["45.85.95.65"] = "CH",
                ["58.96.74.25"] = "AU",
                ["88.99.77.66"] = "DE",
                ["25.67.94.211"] = "GB",
                ["27.67.94.211"] = "VN",
                ["27.62.93.211"] = "IN",
            };

            foreach (var kv in testTable)
            {
                var expected = kv.Value;
                var result = CountryLookup.LookupIPStr(kv.Key);

                if (expected != result)
                {
                    Console.WriteLine($"F: {kv.Key}: {result}, Expected: {expected}");
                }
            }
        }

        void RunRandomTests()
        {
            CountryLookup.Initialize();
            var type = typeof(CountryLookup);
            
            var ipRangesFi = type.GetField("ipRanges", BindingFlags.Static | BindingFlags.NonPublic);
            var ipRanges = ipRangesFi.GetValue(null) as List<Int64>;

            var countryCodesFi = type.GetField("countryCodes", BindingFlags.Static | BindingFlags.NonPublic);
            var countryCodes = countryCodesFi.GetValue(null) as List<string>;

            var rand = new Random((int)DateTime.Now.Ticks);
            var failed = false;
            for (var ii = 1; ii < ipRanges.Count; ii++)
            {
                var max = ipRanges[ii];
                var min = ipRanges[ii - 1];

                var expected = countryCodes[ii];

                failed |= !Verify(CountryLookup.LookupIPNumber(min), expected, min);
                failed |= !Verify(CountryLookup.LookupIPNumber(max - 1), expected, max - 1);
                for (var jj = 0; jj < 100; jj++)
                {
                    var index = min + rand.Next((int)(max - min));
                    failed |= !Verify(CountryLookup.LookupIPNumber(index), expected, index);
                }
            }

            Console.WriteLine(failed ? "--- Random Tests Failed ---" : "--- Random Tests Passed ---");
        }

        bool Verify(string result, string expected, Int64 index)
        {
            if (result == null)
            {
                result = "--";
            }

            if (expected == result)
            {
                return true;
            }

            Console.WriteLine($"F: {index}, {result}, Expected: {expected}");
            return false;
        }
    }
}
