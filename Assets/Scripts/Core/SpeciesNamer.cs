using System.Collections.Generic;

// Generates procedural species names from syllable combinations.
// Child species inherit their ancestor's prefix.
// Names are guaranteed unique.
public static class SpeciesNamer
{
    private static readonly string[] prefixes = {
        "Xena", "Nino", "Mouo", "Felo", "Nubb", "Wooh", "Puff", "Dodo",
        "Flee", "Mipp", "Krii", "Chuu", "Shle", "Pomm", "Snou", "Bunn",
        "Lilo", "Gouo", "Humb", "Regi", "Kitt", "Lopi", "Nimm", "Yifa",
        "Quii", "Coel", "Squr", "Tota", "Uumi", "Bott", "Sime", "Gaia",
        "Coca"
    };

    private static readonly string[] suffixes = {
        "ble", "can", "chi", "lexus", "kin", "tot", "pom", "loo",
        "cicus", "licanth", "fie", "ple", "boo", "mew", "dot", "ling",
        "nut", "pod", "pien", "wee", "oki", "ino", "dron", "ums",
        "anne", 
    };

    private static HashSet<string> usedNames = new HashSet<string>();
    private static int nameSeed = SimRandom.NextInt(1000);

    // Appends a suffix to a prefix based on speciesId.
    private static string AddSuffix(string prefix, int speciesId)
    {
        int suffixIndex = ((speciesId + nameSeed) * 7 + 3) % suffixes.Length;
        return prefix + suffixes[suffixIndex];
    }

    // Cycles through suffixes until a unique name is found, then registers it.
    private static string ResolveUniqueName(string prefix, int speciesId)
    {
        string name = AddSuffix(prefix, speciesId);

        int attempt = 0;
        int suffixIndex = (speciesId * 7 + 3) % suffixes.Length;
        while (usedNames.Contains(name))
        {
            attempt++;
            suffixIndex = (suffixIndex + 1) % suffixes.Length;
            name = prefix + suffixes[suffixIndex];
            if (attempt >= suffixes.Length)
            {
                int suffix2 = (speciesId * 3 + 7) % suffixes.Length;
                name = prefix + suffixes[suffixIndex] + suffixes[suffix2];
                break;
            }
        }

        usedNames.Add(name);
        return name;
    }

    // Generate a random unique species name for a root species (no ancestor).
    public static string GenerateName(int speciesId)
    {
        int prefixIndex = (speciesId + nameSeed) % prefixes.Length;
        return ResolveUniqueName(prefixes[prefixIndex], speciesId);
    }

    // Generate a name for a child species that inherits its ancestor's prefix.
    public static string GenerateChildName(int speciesId, string ancestorName)
    {
        string ancestorPrefix = ancestorName.Length >= 4 ? ancestorName.Substring(0, 4) : ancestorName;
        return ResolveUniqueName(ancestorPrefix, speciesId);
    }

    // Generate a successor name for anagenesis, adds a new suffix onto the full parent name.
    public static string GenerateSuccessorName(int speciesId, string parentName)
    {
        return ResolveUniqueName(parentName, speciesId);
    }
}
