using System.Collections.Generic;

public static class InnovationTracker
{
    private static int nextNodeId = 0;
    private static int nextLinkId = 0;
    private static Dictionary<(int, int), int> linkIds = new Dictionary<(int, int), int>();

    public static int GetInnovation(int source, int target)
    {
        if (!linkIds.TryGetValue((source, target), out int linkId))
        {
            linkId = nextLinkId++;
            linkIds[(source, target)] = linkId;
        }
        return linkId;
    }

    public static int GetNextNodeId()
    {
        return nextNodeId++;
    }
}