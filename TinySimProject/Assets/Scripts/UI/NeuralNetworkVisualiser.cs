using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NeuralNetworkVisualiser : MonoBehaviour
{
    [Header("Visualisation Variables")]
    public GameObject nodePrefab; // UI element for nodes
    public GameObject connectionPrefab; // LineRenderer for connections
    public Transform canvasParent; // UI parent for drawing
    private List<GameObject> instantiatedObjects = new List<GameObject>();

    [Header("Positioning Variables")]
    private float leftAnchor = -1300;
    private float inputStartingY;
    private float outputStartingY;
    private float offsetY = 32f;
    private float hiddenLayerSpacing = 150f; // Space between hidden layers

    public void Visualise(NeuralNetwork network)
    {
        ClearVisualisation();

        Dictionary<int, GameObject> nodeObjects = new Dictionary<int, GameObject>();

        // Calculate Y positions
        inputStartingY = 0.5f * network.inputNodes.Count * offsetY;
        outputStartingY = 0.5f * network.outputNodes.Count * offsetY;

        // Create input nodes
        for (int i = 0; i < network.inputNodes.Count; i++)
        {
            Node node = network.inputNodes[i];
            nodeObjects[node.id] = CreateNode(node, new Vector2(leftAnchor, inputStartingY - (i * offsetY)));
        }

        // Create output nodes
        for (int o = 0; o < network.outputNodes.Count; o++)
        {
            Node node = network.outputNodes[o];
            nodeObjects[node.id] = CreateNode(node, new Vector2(leftAnchor + 600f, outputStartingY - (o * offsetY)));
        }

        // Create hidden nodes (evenly spaced)
        int hiddenLayerIndex = 0;
        foreach (Node node in network.nodes)
        {
            if (node.type == NodeType.Hidden)
            {
                float xPos = Random.Range(leftAnchor + 200, leftAnchor + 400); // Avoid overlap
                float yPos = hiddenLayerIndex * hiddenLayerSpacing - 100; // Space out layers
                nodeObjects[node.id] = CreateNode(node, new Vector2(xPos, yPos));
                hiddenLayerIndex++;
            }
        }

        // Create connections
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
        GameObject lineObj = Instantiate(connectionPrefab, canvasParent);
        UILineRenderer line = lineObj.GetComponent<UILineRenderer>();
        RectTransform rectTransform = lineObj.GetComponent<RectTransform>();


        // Convert UI positions (anchoredPosition) to world positions
        Vector3 startPos = source.GetComponent<RectTransform>().anchoredPosition;
        Vector3 endPos = target.GetComponent<RectTransform>().anchoredPosition;

        rectTransform.anchoredPosition = new Vector2(515, 186);

        line.points = new Vector2[] { startPos, endPos };
        Debug.Log($"Positions: ({startPos.x}, {startPos.y}) ({endPos.x}, {endPos.y})");
        line.color = WeightToColor(weight);

        // Ensure the LineRenderer is visible
        instantiatedObjects.Add(lineObj);
    }

    private Color WeightToColor(double weight)
    {
        float normalizedWeight = Mathf.Clamp((float)weight, -1f, 1f); // Keep in range [-1, 1]
        return Color.Lerp(Color.red, Color.green, (normalizedWeight + 1f) / 2f); // -1 = Red, 1 = Green, 0 = Yellow
    }

    private void ClearVisualisation()
    {
        foreach (GameObject obj in instantiatedObjects)
        {
            Destroy(obj);
        }
        instantiatedObjects.Clear();
    }
}
