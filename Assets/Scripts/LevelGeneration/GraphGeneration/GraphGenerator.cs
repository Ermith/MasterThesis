using System;
using System.Collections.Generic;
using Unity.VisualScripting;

public class BaseVertex
{
    static char c = 'A';
    char _c;

    public List<Lock> Locks = new();
    public List<Key> Keys = new();

    public BaseVertex()
    {
        _c = c++;
    }

    public override string ToString()
    {
        return $"{_c}";
    }
}
class GraphGenerator
{
    public IGraph<BaseVertex> Graph { get; private set; }

    public GraphGenerator(IGraph<BaseVertex> graph)
    {
        Graph = graph;
    }

    public void Generate()
    {
        //*/
        BaseVertex A = new();
        BaseVertex B = new();
        BaseVertex C = new();
        BaseVertex D = new();

        Graph.AddVertex(A);
        Graph.AddVertex(B);
        Graph.AddVertex(C);
        Graph.AddVertex(D);
        Graph.AddEdge(A, B);
        Graph.AddEdge(A, C);
        Graph.AddEdge(B, D);
        Graph.AddEdge(C, D);

        CycleRule cycleRule = new();
        ExtensionRule extensionRule = new();
        UnityEngine.Random.InitState(7);

        int count = 4;
        for (int i = 0; i < count; i++)
        {
            cycleRule.Apply(Graph.GetRandomEdge(), Graph);
            extensionRule.Apply(Graph.GetRandomEdge(), Graph);
        }

        // Graph from the paper
        //PredefinedGraph();
    }

    // For Debugging
    private void PredefinedGraph()
    {
        int count = 8;
        var nodes = new BaseVertex[count];
        for (int i = 0; i < count; i++)
        {
            BaseVertex vertex = new();
            nodes[i] = vertex;
            Graph.AddVertex(vertex);
        }

        Graph.AddEdge(nodes[0], nodes[1]);
        Graph.AddEdge(nodes[0], nodes[3]);
        Graph.AddEdge(nodes[0], nodes[6]);
        Graph.AddEdge(nodes[1], nodes[3]);
        Graph.AddEdge(nodes[1], nodes[5]);
        Graph.AddEdge(nodes[1], nodes[2]);
        Graph.AddEdge(nodes[2], nodes[5]);
        Graph.AddEdge(nodes[2], nodes[7]);
        Graph.AddEdge(nodes[3], nodes[6]);
        Graph.AddEdge(nodes[3], nodes[4]);
        Graph.AddEdge(nodes[4], nodes[6]);
        Graph.AddEdge(nodes[4], nodes[7]);
        Graph.AddEdge(nodes[4], nodes[5]);
        Graph.AddEdge(nodes[6], nodes[7]);
    }
}
