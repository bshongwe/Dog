# Dog Breeds API - Progressive Implementation Guide

This project demonstrates a progressive, step-by-step approach to building a .NET console application that fetches dog breed data from an API and produces ranked reports.

## Project Overview

The application fetches dog breed information from the [Dog API](https://dogapi.dog/), processes the data to extract lifespan and weight metrics, and outputs the top 10 breeds ranked by maximum lifespan and maximum female weight.

### Key Features
- **No third-party libraries** - Uses only .NET standard libraries
- **Robust error handling** - Gracefully handles HTTP errors, timeouts, and invalid JSON
- **Pagination support** - Automatically fetches all pages of results
- **Complex data parsing** - Handles multiple data formats (objects, ranges, strings, numbers)
- **Ranked output** - Top 10 lists sorted by multiple criteria with tie-breaking

## Step-by-Step Implementation

Each step builds progressively on the previous one, adding more functionality. This approach helps understand how the solution is constructed piece by piece.

| Step | Focus Area | Key Achievements | Location |
|------|-----------|-----------------|----------|
| **Step 0** | Project skeleton | Basic program structure and initial setup | `sandbox-solutions/step-0/` |
| **Step 1** | Data structure definition | Define `BreedInfo` record for data storage | `sandbox-solutions/step-1/` |
| **Step 2** | HTTP communication | Fetch raw JSON from API with error handling | `sandbox-solutions/step-2/` |
| **Step 3** | JSON parsing | Parse JSON response and extract breed names | `sandbox-solutions/step-3/` |
| **Step 4** | Value normalization | Parse and normalize lifespan and weight values | `sandbox-solutions/step-4/` |
| **Step 5** | Ranking & sorting | Sort breeds by metrics with tie-breaking | `sandbox-solutions/step-5/` |
| **Step 6** | Output formatting | Format and display final ranked results | `sandbox-solutions/step-6/` |

## Detailed Step Breakdown

### Step 0: Project Skeleton
**Goal**: Establish basic project structure

**Accomplishments**:
- Create basic `Program.cs` with `Main` method
- Define project configuration
- Print welcome message to verify setup

**Code structure**:
```csharp
class Program
{
    static int Main(string[] args)
    {
        Console.WriteLine("Hello, Dog Breeds!");
        return 0;
    }
}
```

---

### Step 1: Data Structure Definition
**Goal**: Define the data model for breed information

**Accomplishments**:
- Create `BreedInfo` record with three fields:
  - `Name`: Breed name (string)
  - `MaxLife`: Maximum lifespan (double?)
  - `MaxFemaleWeight`: Maximum female weight (double?)
- Initialize empty breeds list
- Verify structure compiles

**Key Code**:
```csharp
record BreedInfo(string Name, double? MaxLife, double? MaxFemaleWeight);
```

---

### Step 2: HTTP Communication
**Goal**: Establish connection to the API and fetch data

**Accomplishments**:
- Create `HttpClient` with 30-second timeout
- Set proper headers for JSON:API format
- Implement GET request with error handling
- Handle three types of exceptions:
  - `HttpRequestException`: Network/HTTP errors
  - `TaskCanceledException`: Request timeout
  - `Exception`: Generic fallback

**Features**:
- Raw JSON output to verify successful fetch
- Single page retrieval (foundation for pagination)
- Comprehensive error messages to stderr

**Sample Output**:
```
Fetching: https://dogapi.dog/api/v2/breeds
[Raw JSON response displayed]
```

---

### Step 3: JSON Parsing
**Goal**: Extract structured data from JSON response

**Accomplishments**:
- Parse JSON document with error handling for invalid JSON
- Validate presence of required `data` array
- Iterate through breed items in the array
- Extract breed names from attributes
- Filter out empty/invalid names

**Error Handling**:
- Graceful handling of missing `data` array
- Skip items missing `attributes` section
- Skip breeds with empty names

**Data Extraction**:
- Navigates JSON:API structure
- Accesses `data[*].attributes.name`
- Builds list of breed objects

---

### Step 4: Value Normalization
**Goal**: Parse and normalize lifespan and weight values from various formats

**Accomplishments**:
- Implement `ParseMax()` helper to handle three data formats:
  
| Format | Example | Handled |
|--------|---------|---------|
| **Object with numeric max** | `{ "max": 15 }` | ✓ |
| **Object with string max** | `{ "max": "15" }` | ✓ |
| **Range string** | `"10 - 15"` or `"10–15"` | ✓ |
| **Single value with unit** | `"15 kg"` | ✓ |
| **Raw number** | `15` | ✓ |

- Implement regex pattern for range extraction:
  - Supports separators: `-`, `–`, `—`, `to`
  - Extracts upper bound (max value)
  - Handles optional unit suffixes (kg, kgs)

- Implement `StripUnits()` helper to clean unit suffixes
- Implement `TryParseRangeMax()` for range string parsing

**Parsing Pipeline**:
1. Check if object with "max" property
2. Try parsing as range string (regex)
3. Try parsing as single number
4. Return null if no valid value found

---

### Step 5: Ranking & Sorting
**Goal**: Sort breeds by metrics and prepare for output

**Accomplishments**:
- Filter breeds with valid lifespan data
- Filter breeds with valid weight data
- Sort by lifespan (descending) with name as tie-breaker
- Sort by female weight (descending) with name as tie-breaker
- Verify ranking counts before output

**Sorting Criteria**:

| Ranking | Primary Sort | Direction | Tie-breaker |
|---------|-------------|-----------|------------|
| By Lifespan | `MaxLife` | Descending | Name (ascending) |
| By Weight | `MaxFemaleWeight` | Descending | Name (ascending) |

**Example Output**:
```
Breeds with valid lifespan:      xxx
Breeds with valid female weight: xxx
```

---

### Step 6: Output Formatting
**Goal**: Display final ranked results in human-readable format

**Accomplishments**:
- Format and display "Top 10 by Max Life Span"
- Format and display "Top 10 Heaviest Female Breeds"
- Show up to 10 items (or fewer if not enough data)
- Include formatted output with:
  - Rank number (1-10)
  - Breed name
  - Metric value

**Output Format**:
```
Top 10 by Max Life Span

1. Breed Name — max_lifespan=xx

2. Another Breed — max_lifespan=xx

...

Top 10 Heaviest Female Breeds

1. Large Breed — max_female_weight=xx

2. Big Breed — max_female_weight=xx

...
```

---

## Running the Application

### Run the complete solution (Step 6):
```bash
cd Dog
dotnet run
```

### Run individual steps:
```bash
cd sandbox-solutions/step-1
dotnet run

cd sandbox-solutions/step-2
dotnet run

# ... and so on
```

## Data Processing Flow

```
┌─────────────────────────────────────┐
│ Step 1: Data Structure              │
│ Define BreedInfo record             │
└────────────────┬────────────────────┘
                 │
┌────────────────▼────────────────────┐
│ Step 2: HTTP Fetching               │
│ GET /api/v2/breeds with pagination  │
└────────────────┬────────────────────┘
                 │
┌────────────────▼────────────────────┐
│ Step 3: JSON Parsing                │
│ Extract names from data[].attributes│
└────────────────┬────────────────────┘
                 │
┌────────────────▼────────────────────┐
│ Step 4: Value Normalization         │
│ Parse life & female_weight values   │
└────────────────┬────────────────────┘
                 │
┌────────────────▼────────────────────┐
│ Step 5: Ranking & Sorting           │
│ Filter & sort by metrics            │
└────────────────┬────────────────────┘
                 │
┌────────────────▼────────────────────┐
│ Step 6: Output Formatting           │
│ Display Top 10 lists                │
└─────────────────────────────────────┘
```

## Technical Highlights

### Error Handling Strategy
- HTTP errors captured at transport layer
- JSON parsing errors caught explicitly
- Structure validation at each processing step
- Graceful degradation (skip invalid items)

### Data Normalization
- Handles multiple input formats
- Regex-based range string parsing
- Unit suffix removal
- Culture-invariant number parsing

### Pagination
- Automatic page following via `links.next`
- Accumulates results across pages
- Stops when pagination ends

### Sorting
- Multi-level sort with custom comparators
- Descending metric sort
- Alphabetical tie-breaking

## Project Structure

```
Dog/
├── Program.cs                          # Final complete solution
├── Dog.csproj                          # Project file
├── .gitignore                          # Git ignore rules
└── sandbox-solutions/
    ├── step-0/                         # Skeleton
    ├── step-1/                         # Data structure
    ├── step-2/                         # HTTP fetching
    ├── step-3/                         # JSON parsing
    ├── step-4/                         # Value normalization
    ├── step-5/                         # Ranking & sorting
    └── step-6/                         # Output formatting
```

## Learning Outcomes

By following this implementation, you'll understand:

1. **Async/await patterns** in C# for HTTP operations
2. **JSON parsing** using `JsonDocument` API
3. **Regular expressions** for complex string matching
4. **Error handling** with try-catch blocks
5. **Data transformation** and normalization
6. **Sorting algorithms** with custom comparators
7. **Pagination** handling for API results
8. **LINQ-less functional** approach to data processing

## Notes

- This implementation uses **no external NuGet packages**
- Works with **.NET 8/9/10**
- Demonstrates **best practices** for API consumption
- Includes **comprehensive error handling**
- Uses **culture-invariant** number parsing for robustness
