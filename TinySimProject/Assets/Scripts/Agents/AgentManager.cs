using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class AgentManager : MonoBehaviour
{
    public static AgentManager instance;
    List<Agent> agents = new List<Agent>();
    // Start is called before the first frame update
    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }
    void Start()
    {

    }

    public void UpdateAgents(float deltaTime)
    {
        foreach (Agent agent in agents)
        {
            agent.UpdateAgent(deltaTime);
        }
    }
    public void CreateAgent(Agent agent)
    {
        agents.Add(agent);
    }
    public void RemoveAgent(Agent agent)
    {
        agents.Remove(agent);
    }
}
