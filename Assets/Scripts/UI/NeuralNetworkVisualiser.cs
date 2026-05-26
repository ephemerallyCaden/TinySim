using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NeuralNetworkVisualiser : MonoBehaviour
{
    [Header("Visualisation Variables")]
    public GameObject nodePrefab; // UI element for nodes
    public GameObject connectionPrefab; // UILineRenderer for connections
    public Transform canvasParent; // UI parent for drawing
    private List<GameObject> instantiatedObjects = new List<GameObject>();

    [Header("Positioning Variables")]
    public float leftAnchor = -300;
    private float inputStartingY;
    private float outputStartingY;
    private float hiddenStartingY;
    private float spacingY = 32f;
    public float offsetY = 200f;
    private float hiddenLayerSpacing = 50f; // Space between hidden layers

    public void Visualise(NeuralNetwork network)
    {
        ClearVisualisation();

        Dictionary<int, GameObject> nodeObjects = new Dictionary<int, GameObject>();

        // Calculate starting Y positions based on number of nodes
        int hiddenCount = network.nodes.Count - network.inputNodes.Count - network.outputNodes.Count;
        inputStartingY = 0.5f * network.inputNodes.Count * spacingY + offsetY;
        outputStartingY = 0.5f * network.outputNodes.Count * spacingY + offsetY;
        hiddenStartingY = 0.5f * hiddenCount * hiddenLayerSpacing + offsetY;

        // Create input node visualisers
        for (int i = 0; i < network.inputNodes.Count; i++)
        {
            Node node = network.inputNodes[i];
            nodeObjects[node.id] = CreateNode(node, new Vector2(leftAnchor, inputStartingY - (i * spacingY)));
        }

        // Create output node visualisers
        for (int o = 0; o < network.outputNodes.Count; o++)
        {
            Node node = network.outputNodes[o];
            nodeObjects[node.id] = CreateNode(node, new Vector2(leftAnchor + 600f, outputStartingY - (o * spacingY)));
        }

        // Create hidden node visualisers with deterministic positioning based on node ID
        int hiddenIndex = 0;
        foreach (Node node in network.nodes)
        {
            if (node.type == NodeType.Hidden)
            {
                // Deterministic X: distribute across the hidden area using node ID as seed
                float t = (node.id * 137.5f) % 200f; // Golden angle distribution
                float xPos = leftAnchor + 150f + t;
                float yPos = hiddenStartingY - hiddenIndex * hiddenLayerSpacing;
                nodeObjects[node.id] = CreateNode(node, new Vector2(xPos, yPos));
                hiddenIndex++;
            }
        }

        // Create connections visualiser
        foreach (Connection connection in network.connections)
        {
            if (connection.enabled &&
                nodeObjects.ContainsKey(connection.link.source) &&
                nodeObjects.ContainsKey(connection.link.target))
            {
                CreateConnection(nodeObjects[connection.link.source], nodeObjects[connection.link.target], connection.weight);
            }
        }
    }

    private GameObject CreateNode(Node node, Vector2 position)
    {
        GameObject nodeObj = Instantiate(nodePrefab, canvasParent);
        RectTransform rectTransform = nodeObj.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = position;

        // Use descriptive label from NeuralNetworkLabels if available
        string label = NeuralNetworkLabels.GetLabel(node.id);
        TMP_Text text = nodeObj.GetComponentInChildren<TMP_Text>();
        text.text = label ?? node.id.ToString();
        text.overflowMode = TextOverflowModes.Overflow;

        // Output nodes: text to the right. Input/hidden nodes: text to the left.
        RectTransform textRect = text.GetComponent<RectTransform>();
        if (node.type == NodeType.Output)
        {
            textRect.anchoredPosition = new Vector2(80f, 0f);
            text.alignment = TextAlignmentOptions.MidlineLeft;
        }
        else
        {
            textRect.anchoredPosition = new Vector2(-80f, 0f);
            text.alignment = TextAlignmentOptions.MidlineRight;
        }

        instantiatedObjects.Add(nodeObj);
        return nodeObj;
    }

    private void CreateConnection(GameObject source, GameObject target, double weight)
    {
        //Instantiate line object
        GameObject lineObj = Instantiate(connectionPrefab, canvasParent);
        UILineRenderer line = lineObj.GetComponent<UILineRenderer>();

        //Fetch node visualiser positions
        Vector3 startPos = source.GetComponent<RectTransform>().anchoredPosition;
        Vector3 endPos = target.GetComponent<RectTransform>().anchoredPosition;
        line.points = new Vector2[] { startPos, endPos };

        //Fetch line colour
        line.color = WeightToColour(weight);


        instantiatedObjects.Add(lineObj);
    }

    private Color WeightToColour(double weight)
    {
        float normalisedWeight = Mathf.Clamp((float)weight, -1f, 1f); // Keep in range [-1, 1]
        return Color.Lerp(Color.red, Color.green, (normalisedWeight + 1f) / 2f); // -1 = Red, 1 = Green, 0 = Yellow
    }

    private void ClearVisualisation()
    {
        // Destroy all visualiser objects
        foreach (GameObject obj in instantiatedObjects)
        {
            Destroy(obj);
        }
        instantiatedObjects.Clear();
    }
}
