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

        Console.WriteLine("Hello, Dog Breeds!");
        return 0;
    }
}

// Step 1: BreedInfo Record
record BreedInfo(string Name, double? MaxLife, double? MaxFemaleWeight);