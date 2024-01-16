using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using URandom = UnityEngine.Random;

public class BaseVertex
{
    static char c = 'A';
    char _c;

    private List<ILock> _locks = new();
    private List<IKey> _keys = new();

    public void AddLock(ILock l)
    {
        _locks.Add(l);
    }

    public void AddKey(IKey k)
    {
        _keys.Add(k);
    }

    public IEnumerable<ILock> GetLocks()
    {
        foreach(ILock l in _locks)
            yield return l;
    }

    public IEnumerable<IKey> GetKeys()
    {
        foreach(IKey k in _keys)
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
    public Dictionary<ILock, BaseVertex> LockMapping = new();
    public Dictionary<IKey, BaseVertex> KeyMapping = new();
    private BaseVertex _start;
    private BaseVertex _end;

    public GraphGenerator(IGraph<BaseVertex> graph)
    {
        Graph = graph;
    }

    public void RegisterLock(ILock l, BaseVertex vertex) => LockMapping[l] = vertex;
    public void RegisterKey(IKey k, BaseVertex vertex) => KeyMapping[k] = vertex;

    public BaseVertex GetLockVertex(ILock l) => LockMapping[l];
    public BaseVertex GetKeyVertex(IKey k) => KeyMapping[k];

    public BaseVertex GetStartVertex() => _start;
    public BaseVertex GetEndVertex() => _end;

    public void Generate()
    {
        _start = new BaseVertex();
        _end = new BaseVertex();
        Graph.AddVertex(_start);
        Graph.AddVertex(_end);
        Graph.AddEdge(_start, _end);

        CycleRule cycleRule = new(this);
        ExtensionRule extensionRule = new(this);

        cycleRule.Apply(Graph.GetEdge(_start, _end), Graph, new DoorLock());


        for (int i = 0; i < 2; i++)
        {
            ILock @lock = URandom.value > 0.5f ? new DoorLock() : new DoorLock();
            IEdge<BaseVertex> edge;
            do
            {
                edge = Graph.GetRandomEdge();
            } while (edge.From == _end || edge.To == _start || edge.From == _start);
        
            if (URandom.value > 0.5f)
                cycleRule.Apply(edge, Graph, @lock);
            else
                extensionRule.Apply(edge, Graph, @lock);
        }
        
        /*/
        BaseVertex A = new();
        BaseVertex B = new();
        BaseVertex C = new();
        BaseVertex D = new();
        _start = A;
        _end = D;

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

        int count = 1;
        for (int i = 0; i < count; i++)
        {
            cycleRule.Apply(Graph.GetRandomEdge(), Graph, new DoorLock());
            extensionRule.Apply(Graph.GetRandomEdge(), Graph, new SecurityCameraLock());
        }

        //*/
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
