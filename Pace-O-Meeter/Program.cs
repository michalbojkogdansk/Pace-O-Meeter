using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

class PaceOMeterDetailed
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== Pace-o-Meter: Segment Analyzer ===");

        // Get user inputs
        Console.Write("Enter start location: ");
        string origin = Console.ReadLine();

        Console.Write("Enter destination location: ");
        string destination = Console.ReadLine();

        Console.Write("Enter your Google Maps API Key: ");
        string apiKey = Console.ReadLine();

        // Get route details
        var longestStep = await GetLongestStep(origin, destination, apiKey);

        if (longestStep == null)
        {
            Console.WriteLine("Could not retrieve route information.");
            return;
        }

        double distanceKm = longestStep.Item1;
        double durationMin = longestStep.Item2;
        string instruction = longestStep.Item3;

        double originalSpeed = distanceKm / (durationMin / 60.0);

        Console.WriteLine($"\nLongest Step: {instruction}");
        Console.WriteLine($"Distance: {distanceKm:F2} km");
        Console.WriteLine($"Duration: {durationMin:F1} min");
        Console.WriteLine($"Average Speed: {originalSpeed:F1} km/h");

        // User input: target speed
        Console.Write("\nEnter your planned speed for this segment (km/h): ");
        double newSpeed = Convert.ToDouble(Console.ReadLine());

        double newTimeMinutes = (distanceKm / newSpeed) * 60.0;
        double timeSaved = durationMin - newTimeMinutes;

        Console.WriteLine($"\nIf you travel at {newSpeed} km/h on this segment:");
        Console.WriteLine($"New Segment Time: {newTimeMinutes:F1} min");
        Console.WriteLine($"Time Saved on this Segment: {timeSaved:F1} min");

        Console.WriteLine("\n=== End ===");
    }

    static async Task<Tuple<double, double, string>> GetLongestStep(string origin, string destination, string apiKey)
    {
        string url = $"https://maps.googleapis.com/maps/api/directions/json?origin={Uri.EscapeDataString(origin)}&destination={Uri.EscapeDataString(destination)}&key={apiKey}";

        using (HttpClient client = new HttpClient())
        {
            try
            {
                var response = await client.GetStringAsync(url);
                JObject json = JObject.Parse(response);

                var status = json["status"]?.ToString();
                if (status != "OK")
                    return null;

                var steps = json["routes"]?[0]?["legs"]?[0]?["steps"];

                if (steps == null)
                    return null;

                double maxDistance = 0;
                double maxDuration = 0;
                string maxInstruction = "";

                foreach (var step in steps)
                {
                    double stepDistance = step["distance"]?["value"]?.ToObject<double>() ?? 0;
                    double stepDuration = step["duration"]?["value"]?.ToObject<double>() ?? 0;
                    string instruction = step["html_instructions"]?.ToString();

                    if (stepDistance > maxDistance)
                    {
                        maxDistance = stepDistance;
                        maxDuration = stepDuration;
                        maxInstruction = instruction;
                    }
                }

                return Tuple.Create(maxDistance / 1000.0, maxDuration / 60.0, maxInstruction);
            }
            catch
            {
                return null;
            }
        }
    }
}
