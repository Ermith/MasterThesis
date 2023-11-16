using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;

public class MyPlanarEmbedding<T>
{

}

public class MergeInfo
{

}

public class MyPlanarAlgo<T>
{
    public MyPlanarEmbedding<T> GetEmbedding(IGraph<T> graph)
    {
        graph = TransformGraph(graph);
        List<(T, T)> selfLoopsNew = new();
        Stack<MergeInfo> mergeStackNew = new();

        T start = graph.GetVertices().First();
        DFSParams<T> dfs = graph.DepthFirstSearch(start);

        // Sort vertices by dfs number ASC
        var verticesByEnterTime =
            graph.GetVertices().OrderBy((T vertex) => dfs.EnterTimes[vertex]);

        // Sort vertices by low point ASC
        var verticesByLowPoint =
            graph.GetVertices().OrderBy((T vertex) => dfs.Low[vertex]);

        Dictionary<T, List<T>> backEdges = new();
        Dictionary<T, int> visited = new();
        Dictionary<T, int> backedgeFlag = new();
        Dictionary<T, bool> flipped = new();


        foreach (var vertex in graph.GetVertices())
        {
            backEdges[vertex] = new List<T>();
            visited[vertex] = int.MaxValue;
            backedgeFlag[vertex] = graph.VertexCount + 1;
            flipped[vertex] = false;

            T parent = dfs.Parents[vertex];

            if (vertex.Equals(parent))
            {

            }
        }

        return null;
    }

    private IGraph<T> TransformGraph(IGraph<T> graph)
    {
        UndirectedAdjecencyGraph<T> transformedGraph = new();

        foreach (T vertex in graph.GetVertices())
            transformedGraph.AddVertex(vertex);

        foreach (IEdge<T> edge in graph.GetEdges())
            transformedGraph.AddEdge(edge.From, edge.To);

        return transformedGraph;
    }
}
