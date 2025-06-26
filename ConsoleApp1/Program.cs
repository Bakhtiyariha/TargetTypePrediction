using System;
using System.IO;
using System.Linq;

namespace ConsoleApp1
{
    class Program
    {
        static Random random = new Random();
        string filePath = "";
        static void Main()
        {
            string filePath = "enhanced_target_data.csv";
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine("TargetType,Range(m),Speed(m/s),RadarCrossSection(sq.m),Direction(deg),Height(deg),CrossParameter(m),Size(m),Altitude(m),Maneuverability,EmissionSignature,TargetIntent,FlightPathPredictability");

                for (int i = 0; i < 100000; i++) // 100,000 data points
                {
                    string targetType = GetTargetType();
                    double range = GetRandomDouble(200, 27000);  // Range from 0 to 27,000 meters
                    double speed = GetTargetSpeed(targetType);  // Adjusted speed based on target type
                    double crossParameter = GetRandomDouble(0, 8000); // Cross parameter between 0 and 8000 meters
                    double radarCrossSection = CalculateKuBandRCS(crossParameter, range, targetType);
                    int direction = random.Next(0, 361); // Random direction in degrees
                    int height = GetRandomInt(-6, 78);  // Altitude angle from -6° to 78°
                    double size = GetTargetSize(targetType); // Size based on target type
                    double altitude = GetTargetAltitude(targetType); // Altitude based on target type
                    int maneuverability = GetTargetManeuverability(targetType); // Maneuverability based on target type
                    int emissionSignature = GetEmissionSignature(targetType); // Emission signature based on target type
                    string targetIntent = GetTargetIntent(targetType); // Intent (civilian/military)
                    string flightPathPredictability = GetFlightPathPredictability(targetType); // Flight path predictability

                    writer.WriteLine($"{targetType},{range:F2},{speed:F2},{radarCrossSection:F6},{direction},{height},{crossParameter:F2},{size:F2},{altitude:F2},{maneuverability},{emissionSignature},{targetIntent},{flightPathPredictability}");
                }

            }

            Console.WriteLine("Ku-band radar data file has been created successfully.");
        }

        static string GetTargetType()
        {
            string[] targetTypes = { "Passenger Aircraft", "Fighter Aircraft", "Cruise Missile", "Guided Bomb", "Helicopter", "Drone" };
            return targetTypes[random.Next(targetTypes.Length)];
        }

        static double GetTargetSpeed(string targetType)
        {
            switch (targetType)
            {
                case "Passenger Aircraft": return GetRandomDouble(150, 280);
                case "Fighter Aircraft": return GetRandomDouble(115, 900);  // Fighters can have high speed
                case "Cruise Missile": return GetRandomDouble(200, 300);    // Cruise missiles have lower speeds
                case "Guided Bomb": return GetRandomDouble(150, 450);        // Guided bombs slow down as they descend
                case "Helicopter": return GetRandomDouble(0, 85);          // Helicopter speed can be 0 but never above 220 m/s
                case "Drone": return GetRandomDouble(10, 250);              // Drones have low speed
                default: return GetRandomDouble(100, 300);
            }
        }

        // Updated RCS calculation to consider cross-parameter and range effects
        static double CalculateKuBandRCS(double crossParameter, double range, string targetType)
        {
            double baseRCS = 0;

            switch (targetType)
            {
                case "Passenger Aircraft":
                    baseRCS = 20.0;
                    break;
                case "Fighter Aircraft":
                    baseRCS = 1.0;
                    break;
                case "Cruise Missile":
                    baseRCS = 0.2;
                    break;
                case "Guided Bomb":
                    baseRCS = 0.01;
                    break;
                case "Helicopter":
                    baseRCS = 0.1;
                    break;
                case "Drone":
                    baseRCS = 0.03;
                    break;
            }

            // The RCS increases with crossParameter and decreases with range
            double crossParameterEffect = 1 + (crossParameter / 8000); // Max 2x increase when crossParameter is 8000
            double rangeEffect = (27000 - range) / 27000; // Decreases RCS at longer ranges

            // Calculate the RCS with effects
            double calculatedRCS = baseRCS * crossParameterEffect * rangeEffect;

            // Ensure the RCS is capped at 20 square meters
            return Math.Min(calculatedRCS, 20.0);
        }

        static double GetRandomDouble(double min, double max)
        {
            return random.NextDouble() * (max - min) + min;
        }

        static int GetRandomInt(int min, int max)
        {
            return random.Next(min, max + 1);
        }

        static double GetTargetSize(string targetType)
        {
            switch (targetType)
            {
                case "Passenger Aircraft": return GetRandomDouble(30, 80);
                case "Fighter Aircraft": return GetRandomDouble(10, 20);
                case "Cruise Missile": return GetRandomDouble(3, 10);
                case "Guided Bomb": return GetRandomDouble(1, 3);
                case "Helicopter": return GetRandomDouble(10, 25);
                case "Drone": return GetRandomDouble(0.5, 5);
                default: return 0;
            }
        }

        static double GetTargetAltitude(string targetType)
        {
            switch (targetType)
            {
                case "Passenger Aircraft": return GetRandomDouble(8000, 12000);
                case "Fighter Aircraft": return GetRandomDouble(5000, 15000);
                case "Cruise Missile": return GetRandomDouble(1000, 5000);
                case "Guided Bomb": return GetRandomDouble(100, 1000);
                case "Helicopter": return GetRandomDouble(0, 5000);
                case "Drone": return GetRandomDouble(50, 3000);
                default: return 0;
            }
        }

        static int GetTargetManeuverability(string targetType)
        {
            switch (targetType)
            {
                case "Passenger Aircraft": return 2;
                case "Fighter Aircraft": return 5;
                case "Cruise Missile": return 3;
                case "Guided Bomb": return 1;
                case "Helicopter": return 4;
                case "Drone": return 4;
                default: return 1;
            }
        }

        static int GetEmissionSignature(string targetType)
        {
            switch (targetType)
            {
                case "Passenger Aircraft": return 0;
                case "Fighter Aircraft": return 3;
                case "Cruise Missile": return 2;
                case "Guided Bomb": return 1;
                case "Helicopter": return 1;
                case "Drone": return 2;
                default: return 0;
            }
        }

        static string GetTargetIntent(string targetType)
        {
            return (targetType == "Passenger Aircraft" || targetType == "Drone") ? "Civilian" : "Military";
        }

        static string GetFlightPathPredictability(string targetType)
        {
            return (targetType == "Passenger Aircraft" || targetType == "Drone") ? "Predictable" : "Unpredictable";
        }

        public void GetMaxAndMinValueFromData()
        {
            string filePath = "enhanced_target_data.csv";
            try
            {
                // Read all lines from the CSV file
                var lines = File.ReadAllLines(filePath);

                // Skip the header if it exists
                var radarCrossSectionValues = lines
                    .Skip(1) // Assuming the first row is the header, remove this line if there's no header
                    .Select(line => line.Split(',')) // Split each line by comma
                    .Select(columns => double.Parse(columns[7])) // Select the third column and convert to double
                    .ToList();

                // Find the maximum radar cross-section value
                double maxRadarCrossSection = radarCrossSectionValues.Max();

                // Output the result
                Console.WriteLine(maxRadarCrossSection);
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
