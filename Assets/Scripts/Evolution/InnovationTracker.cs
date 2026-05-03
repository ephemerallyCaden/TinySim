using System.Collections.Generic;

public static class InnovationTracker
{
    private static int nextNodeId;
    private static int nextLinkId = 0;
    private static Dictionary<(int, int), int> linkIds = new Dictionary<(int, int), int>();

    /// <summary>
    /// Initialise the tracker with the correct starting node ID.
    /// Must be called before any agents are created.
    /// </summary>
    public static void Initialise(int inputCount, int outputCount)
    {
        nextNodeId = inputCount + outputCount;
        nextLinkId = 0;
        linkIds.Clear();
    }

    //Fetches an existing linkid, or creates a new one if the connection is unique
    public static int GetInnovation(int source, int target)
    {
        if (!linkIds.TryGetValue((source, target), out int linkId))
        {
            linkId = nextLinkId++;
            linkIds[(source, target)] = linkId;
        }
        return linkId;
    }

    //Increment the Node ID for a new node
    public static int GetNextNodeId()
    {
        return nextNodeId++;
    }
}