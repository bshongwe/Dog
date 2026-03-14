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
        // Step 2: Fetch data here
        // Step 3: Parse JSON here
        // Step 4: Normalize values here
        // Step 5: Rank breeds here
        // Step 6: Print output here

        return 0;
    }
}

// Step 3: We will define this record fully later
record BreedInfo(string Name, double? MaxLife, double? MaxFemaleWeight);