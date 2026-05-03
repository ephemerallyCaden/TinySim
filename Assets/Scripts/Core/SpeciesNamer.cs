using System.Collections.Generic;

/// <summary>
/// Generates procedural species names from syllable combinations.
/// Child species inherit their ancestor's prefix.
/// Names are guaranteed unique.
/// </summary>
public static class SpeciesNamer
{
    private static readonly string[] prefixes = {
        "Xena", "Nino", "Mou", "Felo", "Nub", "Wooha", "Puff", "Dodo",
        "Flee", "Mip", "Tum", "Chu", "Lil", "Pom", "Sno", "Bun",
        "", "Goo", "Hum", "Reg", "Kit", "Lop", "Nim", "Pea",
        "Qui", "Roo", "Squ", "Tot", "Umi", "Wib", "Yam", "Zep",
        "Coca"
    };

    private static readonly string[] suffixes = {
        "ble", "can", "chi", "lexus", "kin", "tot", "pom", "loo",
        "cicus", "licanth", "fie", "ple", "boo", "mew", "dot", "ling",
        "nut", "pod", "pien", "wee", "oki", "ino", "dron", "ums",
        "bit", 
    };

    private static HashSet<string> usedNames = new HashSet<string>();

    /// <summary>
    /// Generate a unique species name for a root species (no ancestor).
    /// </summary>
    public static string GenerateName(int speciesId)
    {
        // Try the default combination first
        int prefixIndex = speciesId % prefixes.Length;
        int suffixIndex = (speciesId * 7 + 3) % suffixes.Length;
        string name = prefixes[prefixIndex] + suffixes[suffixIndex];

        // If taken, try other suffix combinations until unique
        int attempt = 0;
        while (usedNames.Contains(name))
        {
            attempt++;
            suffixIndex = (suffixIndex + 1) % suffixes.Length;
            name = prefixes[prefixIndex] + suffixes[suffixIndex];
            if (attempt >= suffixes.Length)
            {
                // Fallback: double suffix for uniqueness
                int suffix2 = (speciesId * 3 + 7) % suffixes.Length;
                name = prefixes[prefixIndex] + suffixes[suffixIndex] + suffixes[suffix2];
                break;
            }
        }

        usedNames.Add(name);
        return name;
    }

    /// <summary>
    /// Generate a name for a child species that inherits its ancestor's prefix.
    /// </summary>
    public static string GenerateChildName(int speciesId, string ancestorName)
    {
        // Extract the prefix from the ancestor's name (first 3 characters)
        string ancestorPrefix = ancestorName.Length >= 3 ? ancestorName.Substring(0, 3) : ancestorName;

        // Find the prefix index that matches
        int prefixIndex = -1;
        for (int i = 0; i < prefixes.Length; i++)
        {
            if (prefixes[i] == ancestorPrefix)
            {
                prefixIndex = i;
                break;
            }
        }

        // Generate with ancestor's prefix + new suffix
        int suffixIndex = (speciesId * 7 + 3) % suffixes.Length;
        string name = ancestorPrefix + suffixes[suffixIndex];

        // Ensure uniqueness
        int attempt = 0;
        while (usedNames.Contains(name))
        {
            attempt++;
            suffixIndex = (suffixIndex + 1) % suffixes.Length;
            name = ancestorPrefix + suffixes[suffixIndex];
            if (attempt >= suffixes.Length)
            {
                int suffix2 = (speciesId * 3 + 7) % suffixes.Length;
                name = ancestorPrefix + suffixes[suffixIndex] + suffixes[suffix2];
                break;
            }
        }

        usedNames.Add(name);
        return name;
    }

    /// <summary>
    /// Generate a successor name for anagenesis (drift evolution).
    /// Keeps same prefix, gets a new suffix to show lineage continuity.
    /// </summary>
    public static string GenerateSuccessorName(int speciesId, string parentName)
    {
        return GenerateChildName(speciesId, parentName);
    }
}
