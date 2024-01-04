using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using URandom = UnityEngine.Random;

public class BaseVertex
{
    static char c = 'A';
    char _c;

    private List<Lock> _locks = new();
    private List<Key> _keys = new();

    public void AddLock(Lock l)
    {
        _locks.Add(l);
    }

    public void AddKey(Key k)
    {
        _keys.Add(k);
    }

    public IEnumerable<Lock> GetLocks()
    {
        foreach(Lock l in _locks)
            yield return l;
    }

    public IEnumerable<Key> GetKeys()
    {
        foreach(Key k in _keys)
            yield return k;
    }

    public BaseVertex()
    {
        _c = c++;
    }

    public override string ToString()
    {
        return $"{_c}";
    }
}

public class GraphGenerator
{
    public IGraph<BaseVertex> Graph { get; private set; }
    public Dictionary<Lock, BaseVertex> LockMapping = new();
    public Dictionary<Key, BaseVertex> KeyMapping = new();
    private BaseVertex _start;

    public GraphGenerator(IGraph<BaseVertex> graph)
    {
        Graph = graph;
    }

    public void RegisterLock(Lock l, BaseVertex vertex) => LockMapping[l] = vertex;
    public void RegisterKey(Key k, BaseVertex vertex) => KeyMapping[k] = vertex;

    public BaseVertex GetLockVertex(Lock l) => LockMapping[l];
    public BaseVertex GetKeyVertex(Key k) => KeyMapping[k];

    public BaseVertex GetStartVertex() => _start;

    public void Generate()
    {
        //*/
        BaseVertex A = new();
        BaseVertex B = new();
        BaseVertex C = new();
        BaseVertex D = new();
        _start = A;

        Graph.AddVertex(A);
        Graph.AddVertex(B);
        Graph.AddVertex(C);
        Graph.AddVertex(D);
        Graph.AddEdge(A, B);
        Graph.AddEdge(A, C);
        Graph.AddEdge(B, D);
        Graph.AddEdge(C, D);

        CycleRule cycleRule = new(this);
        ExtensionRule extensionRule = new(this);

        int count = 2;
        for (int i = 0; i < count; i++)
        {
            
            cycleRule.Apply(Graph.GetRandomEdge(), Graph, new DoorLock());
            extensionRule.Apply(Graph.GetRandomEdge(), Graph, new SecurityCameraLock());
        }


        //foreach (var vertex in Graph.GetVertices())
        //    if (URandom.value > 0.5f)
        //        vertex.AddLock(new EnemyLock());

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
