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
    private float leftAnchor = -750;
    private float inputStartingY;
    private float outputStartingY;
    private float hiddenStartingY;
    private float spacingY = 32f;
    private float offsetY = 200f;
    private float hiddenLayerSpacing = 50f; // Space between hidden layers

    public void Visualise(NeuralNetwork network)
    {
        ClearVisualisation();

        Dictionary<int, GameObject> nodeObjects = new Dictionary<int, GameObject>();

        // Calculate starting Y positions based on number of nodes
        inputStartingY = 0.5f * network.inputNodes.Count * spacingY + offsetY;
        outputStartingY = 0.5f * network.outputNodes.Count * spacingY + offsetY;
        hiddenStartingY = 0.5f * (network.nodes.Count - 15) * hiddenLayerSpacing + offsetY;

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

        // Create hidden node visualisers (evenly spaced with hiddenLayerSpacing)
        int hiddenLayerIndex = 0;
        foreach (Node node in network.nodes)
        {
            if (node.type == NodeType.Hidden)
            {
                float xPos = Random.Range(leftAnchor + 200, leftAnchor + 400); // Avoid overlap with a random x position
                float yPos = hiddenStartingY - hiddenLayerIndex * hiddenLayerSpacing; // Space out layers
                nodeObjects[node.id] = CreateNode(node, new Vector2(xPos, yPos));
                hiddenLayerIndex++;
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
        nodeObj.GetComponentInChildren<TMP_Text>().text = node.id.ToString();
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

        //Debugging
        //line.points = new Vector2[] { startPos, endPos };
        //Debug.Log($"Positions: ({startPos.x}, {startPos.y}) ({endPos.x}, {endPos.y})");

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
