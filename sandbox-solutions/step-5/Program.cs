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

                        // *** STEP 4: Value Normalization
                        double? maxLife = null;
                        double? maxFemaleWeight = null;

                        if (attrs.TryGetProperty("life", out var lifeProp))
                            maxLife = ParseMax(lifeProp);

                        if (attrs.TryGetProperty("female_weight", out var fwProp))
                            maxFemaleWeight = ParseMax(fwProp);

                        // Removed debug Console.WriteLine
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

        // *** STEP 5: Ranking & Sorting

        // STEP 5a: Top 10 by Max Life Span
        var topLifespan = new List<BreedInfo>();
        foreach (var b in breeds)
            if (b.MaxLife.HasValue)
                topLifespan.Add(b);

        // Sort descending by MaxLife,
        // tie-break by name ascending
        topLifespan.Sort((a, b) =>
        {
            int cmp = b.MaxLife!.Value.CompareTo(a.MaxLife!.Value);
            return cmp != 0 ? cmp : string.Compare(a.Name, b.Name, StringComparison.Ordinal);
        });

        // STEP 5b: Top 10 by Max Female Weight
        var topWeight = new List<BreedInfo>();
        foreach (var b in breeds)
            if (b.MaxFemaleWeight.HasValue)
                topWeight.Add(b);

        // Sort descending by MaxFemaleWeight,
        // tie-break by name ascending
        topWeight.Sort((a, b) =>
        {
            int cmp = b.MaxFemaleWeight!.Value.CompareTo(a.MaxFemaleWeight!.Value);
            return cmp != 0 ? cmp : string.Compare(a.Name, b.Name, StringComparison.Ordinal);
        });

        // ✅ Confirm ranking worked (remove in Step 6)
        Console.WriteLine($"Breeds with valid lifespan:      {topLifespan.Count}");
        Console.WriteLine($"Breeds with valid female weight: {topWeight.Count}");

        return 0;
    }

    // ******* HELPERS & UTILITY METHODS

    // STEP 4 HELPER: ParseMax
    static double? ParseMax(JsonElement element)
    {
        // Case 1: Object { "min": x, "max": y }
        if (element.ValueKind == JsonValueKind.Object)
        {
            if (element.TryGetProperty("max", out var maxProp))
            {
                // max is a num
                if (maxProp.ValueKind == JsonValueKind.Number &&
                    maxProp.TryGetDouble(out double val))
                    return val;

                // max is a string num
                if (maxProp.ValueKind == JsonValueKind.String)
                {
                    var s = maxProp.GetString();
                    if (TryParseRangeMax(s, out double parsed))
                        return parsed;
                    if (double.TryParse(StripUnits(s ?? ""),
                            System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture,
                            out double dbl))
                        return dbl;
                }
            }
            return null;
        }

        // Case 2: Range string
        if (element.ValueKind == JsonValueKind.String)
        {
            var s = element.GetString();
            if (TryParseRangeMax(s, out double parsed))
                return parsed;

            // Single num with possible unit
            if (double.TryParse(StripUnits(s ?? ""),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out double single))
                return single;

            return null;
        }

        // Case 3: Raw num
        if (element.ValueKind == JsonValueKind.Number)
        {
            if (element.TryGetDouble(out double val))
                return val;
        }

        return null;
    }

    // STEP 4 HELPER: TryParseRangeMax
    static bool TryParseRangeMax(string? input, out double result)
    {
        result = 0;
        if (string.IsNullOrWhiteSpace(input))
            return false;

        var match = Regex.Match(
            input,
            @"([\d]*\.?[\d]+)\s*(?:—|–|-|to)\s*([\d]*\.?[\d]+)\s*(?:kgs?)?",
            RegexOptions.IgnoreCase);

        if (match.Success)
        {
            var maxStr = match.Groups[2].Value;
            if (double.TryParse(maxStr,
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out double max))
            {
                result = max;
                return true;
            }
        }

        return false;
    }

    // STEP 4 HELPER: StripUnits
    static string StripUnits(string s)
    {
        return Regex.Replace(s.Trim(), @"\s*kgs?\s*$", "", RegexOptions.IgnoreCase).Trim();
    }

    // STEP 1: BreedInfo Record
    record BreedInfo(string Name, double? MaxLife, double? MaxFemaleWeight);
}
