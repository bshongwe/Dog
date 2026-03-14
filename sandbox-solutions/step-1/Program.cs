using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
    static int Main(string[] args)
    {
        // *** STEP 1: Accumulate all breed data across all pages
        var breeds = new List<BreedInfo>();

        Console.WriteLine("Step 1: Data Accumulation Structure");
        Console.WriteLine($"Breeds list initialized. Count: {breeds.Count}");

        return 0;
    }

    // STEP 1: BreedInfo Record
    record BreedInfo(string Name, double? MaxLife, double? MaxFemaleWeight);
}

