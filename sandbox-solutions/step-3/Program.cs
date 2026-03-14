using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
    static async Task<int> Main(string[] args)
    {
        // *** STEP 1: Accumulate all breed data across all pages
        var breeds = new List<BreedInfo>();

        // *** STEP 2: HTTP Fetching with Pagination with Catch Block
        try
        {
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(30);
            httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.api+json");

            // JSON data source
            string? nextUrl = "https://dogapi.dog/api/v2/breeds";

            while (nextUrl != null)
            {
                Console.Error.WriteLine($"Fetching: {nextUrl}");

                // STEP 2a: Make the HTTP GET request
                HttpResponseMessage response;

                try
                {
                    response = await httpClient.GetAsync(nextUrl);
                    response.EnsureSuccessStatusCode();
                }
                catch (HttpRequestException ex)
                {
                    Console.Error.WriteLine($"HTTP error: {ex.Message}");
                    return 1;
                }
                catch (TaskCanceledException)
                {
                    Console.Error.WriteLine("Request timed out.");
                    return 1;
                }

                // STEP 2b: Read the response body as a string
                string json;
                try
                {
                    json = await response.Content.ReadAsStringAsync();
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Failed to read response: {ex.Message}");
                    return 1;
                }

                // *** STEP 3: JSON Parsing with Catch Block
                JsonDocument doc;
                try
                {
                    doc = JsonDocument.Parse(json);
                }
                catch (JsonException ex)
                {
                    Console.Error.WriteLine($"Invalid JSON: {ex.Message}");
                    return 1;
                }

                using (doc)
                {
                    var root = doc.RootElement;

                    // STEP 3a: Check if the data array exists
                    if (!root.TryGetProperty("data", out var dataArray) ||
                        dataArray.ValueKind != JsonValueKind.Array)
                    {
                        Console.Error.WriteLine("Unexpected JSON structure: missing 'data' array.");
                        return 1;
                    }

                    // STEP 3b: Loop through data array
                    foreach (var item in dataArray.EnumerateArray())
                    {
                        if (!item.TryGetProperty("attributes", out var attrs))
                            continue;

                        // STEP 3c: Extract breed name
                        string name = attrs.TryGetProperty("name", out var nameProp)
                            ? nameProp.GetString() ?? string.Empty
                            : string.Empty;

                        if (string.IsNullOrWhiteSpace(name))
                            continue;

                        // For step 3, we extract name and acknowledge life/weight fields exist
                        // but don't parse them yet
                        double? maxLife = null;
                        double? maxFemaleWeight = null;

                        if (attrs.TryGetProperty("life", out var lifeProp))
                        {
                            // Step 3: Just check that the property exists
                            // Parsing happens in Step 4
                            Console.Error.WriteLine($"  {name} has 'life' property");
                        }

                        if (attrs.TryGetProperty("female_weight", out var fwProp))
                        {
                            // Step 3: Just check that the property exists
                            // Parsing happens in Step 4
                            Console.Error.WriteLine($"  {name} has 'female_weight' property");
                        }

                        breeds.Add(new BreedInfo(name, maxLife, maxFemaleWeight));
                    }

                    // STEP 2c: Handle Pagination
                    // - Check for links.next in the response
                    // - If present, set nextUrl to keep fetching
                    // - If missing or null, stop the loop
                    nextUrl = null;
                    if (root.TryGetProperty("links", out var links) &&
                        links.TryGetProperty("next", out var nextProp) &&
                        nextProp.ValueKind == JsonValueKind.String)
                    {
                        var nextStr = nextProp.GetString();
                        if (!string.IsNullOrWhiteSpace(nextStr))
                            nextUrl = nextStr;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Catch-all for any unexpected errors
            Console.Error.WriteLine($"Unexpected error: {ex.Message}");
            return 1;
        }

        // Output results
        Console.WriteLine("Successfully Parsed Breeds from JSON:");
        Console.WriteLine();

        foreach (var breed in breeds)
        {
            Console.WriteLine($"- {breed.Name}");
        }

        Console.WriteLine();
        Console.WriteLine($"Total breeds parsed: {breeds.Count}");

        return 0;
    }


    // STEP 1: BreedInfo Record
    record BreedInfo(string Name, double? MaxLife, double? MaxFemaleWeight);
}
