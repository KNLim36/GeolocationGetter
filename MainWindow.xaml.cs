using Itinero;
using Itinero.IO.Osm;
using Itinero.Osm.Vehicles;
using OpenCage.Geocode;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;

namespace Testing2
{
    public partial class MainWindow : Window
    {
        static readonly string mapsFilePath = @"C:\Users\Hmin\Desktop\Testing2\MapTextFile.txt";
        static readonly string openCageDataApiKey = "12255245316c47bd8122a89a0f3c3c0c";
        private static RouterDb routerDb;
        Geocoder gc;
        public MainWindow()
        {
            InitializeComponent();

            routerDb = new RouterDb();


            //GetOsmData();
            //ContractRouterDb();
            //GenerateRouterDb();
            LoadFromGeneratedRouterDb();

            var router = new Router(routerDb);
            gc = new Geocoder(openCageDataApiKey);

            //read the text file with provided pathway
            string allAddInfo = File.ReadAllText(mapsFilePath);
            string[] mapsTextList = Regex.Split(allAddInfo, "\r\n");

            //get start location lat&long
            GeocoderResponse startLocResult = gc.Geocode(ProcessStartLocationFile(mapsTextList));
            float startLocLat = GetLat(startLocResult);
            float startLocLong = GetLong(startLocResult);

            //get end location lat&long
            GeocoderResponse endLocResult = gc.Geocode(ProcessEndLocationFile(mapsTextList));
            float endLocLat = GetLat(endLocResult);
            float endLocLong = GetLong(endLocResult);

            //Convert or write them from float to string then can just print in string form
            string startLocLatString = startLocLat.ToString();
            string startLocLongString = startLocLong.ToString();
            string fullStartCoordinate = ("GPS coordinate for departure: (" + startLocLatString + ", " + startLocLongString + ")");

            string endLocLatString = endLocLat.ToString();
            string endLocLongString = endLocLong.ToString();
            string fullEndCoordinate = ("GPS coordinate for arrival: (" + endLocLatString + ", " + endLocLongString + ")");


            var profile = Vehicle.Car.Shortest();
            var route = router.Calculate(profile, startLocLat, startLocLong, endLocLat, endLocLong);

            var geoJson = route.ToGeoJson();



            //change the meters into km
            float totalDistance = route.TotalDistance;
            double totalDistanceInKm = Math.Round(totalDistance / 1000, 2);
            string distanceInKm = "Estimated Route Distance: " + totalDistanceInKm.ToString() + " km";

            //change the seconds to hour and minute
            float totalTimeSec = route.TotalTime;
            float totalTimeMin = totalTimeSec / 60;
            int roundedTotalTimeMin = (int)Math.Round(totalTimeMin, 0);

            int totalTimeHour = roundedTotalTimeMin / 60;
            float totalTimeRem = totalTimeMin % 60;
            float roundedTotalTimeRem = (int)Math.Round(totalTimeRem, 0);



            //Special Condition for travel duration
            string etdString = "Estimated Travel Duration: ";
            string hourString = "hour";
            string minuteString = "minute";

            if (totalTimeHour > 1)
            {
                hourString = "hours";
            }
            if (roundedTotalTimeRem > 1)
            {
                minuteString = "minutes";
            }

            //Combination of travel time
            string finalTextToPrint = etdString + totalTimeHour.ToString() + " " + hourString + " " + roundedTotalTimeRem.ToString() + " " + minuteString;

            string[] textToPrint = { fullStartCoordinate, fullEndCoordinate, distanceInKm, finalTextToPrint };



            //print or write all needed infos into an auto-generated file(giving specific pathway)
            File.WriteAllLines(@"C:\Users\Hmin\Desktop\Testing2\PrintData.txt", textToPrint);
            this.Close();
        }

        private static void GetOsmData()
        {
            using (var stream = new FileInfo("MsiaSingBrun.osm.pbf").OpenRead())
            {
                routerDb.LoadOsmData(stream, Vehicle.Car);
            }
        }

        private static void ContractRouterDb()
        {
            routerDb.AddContracted(routerDb.GetSupportedProfile("car"));
        }

        private static void GenerateRouterDb()
        {
            using (var stream = new FileInfo(@"MsiaSingBrun.routerdb").Open(FileMode.Create))
            {
                routerDb.Serialize(stream);
            }
        }

        private static void LoadFromGeneratedRouterDb()
        {
            using (var stream = new FileInfo("MsiaSingBrun.routerdb").OpenRead())
            {
                routerDb = RouterDb.Deserialize(stream);
            }
        }

        //method of changing value from double to float
        private static float ToSingle(double value)
        {
            return (float)value;
        }

        private static float GetLat(GeocoderResponse result)
        {
            return ToSingle(result.Results[0].Geometry.Latitude);
        }

        private static float GetLong(GeocoderResponse result)
        {
            return ToSingle(result.Results[0].Geometry.Longitude);
        }



        private static string ProcessStartLocationFile(string[] mapsTextList)
        {
            string tempRow = mapsTextList[0];
            foreach (string row in mapsTextList)
            {
                if (row.Length > 0)
                {
                    string code = row.Substring(0, 5);
                    if (code == "START")
                    {
                        tempRow = row.Substring(6);
                        tempRow = tempRow.TrimEnd();
                    }
                }
            }
            return tempRow;
        }

        private static string ProcessEndLocationFile(string[] mapsTextList)
        {
            string tempRow2 = mapsTextList[1];
            foreach (string row in mapsTextList)
            {
                if (row.Length > 0)
                {
                    string code = row.Substring(0, 3);
                    if (code == "END")
                    {
                        tempRow2 = row.Substring(4);
                        tempRow2 = tempRow2.TrimEnd();
                    }
                }
            }
            return tempRow2;
        }


        //calculation of travel distance with both start and end locations via Open Cage Geocoder Geocoordinate
        //private static double CalculateDistance(double startLat, double startLong, double endLat, double endLong)
        //{
        //    var startLocation = new GeoCoordinate(startLat, startLong);
        //    var endLocation = new GeoCoordinate(endLat, endLong);

        //    //Math.Round(startLocation.GetDistanceTo(endLocation) / 1000, 2).ToString() + "Km";

        //    return Math.Round(startLocation.GetDistanceTo(endLocation) / 1000, 2);
        //}
    }
}
