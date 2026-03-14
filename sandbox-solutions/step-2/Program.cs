using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
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
                Console.WriteLine($"Fetching: {nextUrl}");

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

                // Print raw JSON to confirm it works (we will remove this later)
                Console.WriteLine(json);

                // Step 3: JSON parsing will go here

                // For now, just stop after first page
                nextUrl = null;
            }
        }
        catch (Exception ex)
        {
            // Catch-all for any unexpected errors
            Console.Error.WriteLine($"Unexpected error: {ex.Message}");
            return 1;
        }

        return 0;
    }
}

// STEP 1: BreedInfo Record
record BreedInfo(string Name, double? MaxLife, double? MaxFemaleWeight);
