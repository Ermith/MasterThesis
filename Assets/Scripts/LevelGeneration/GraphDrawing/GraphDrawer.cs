using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using GraphPlanarityTesting.PlanarityTesting.BoyerMyrvold;
using Planarity = GraphPlanarityTesting.Graphs.DataStructures;

public struct GraphDrawing<T>
{
    public HashSet<(int x, int yFrom, int yTo)> VerticalLines;
    public HashSet<(int xFrom, int xTo, int y)> HorizontalLines;
    public Dictionary<T, (int, int)> VertexPositions;
    public int MaximumX;
    public int MaximumY;
    public BaseVertex StartPosition;
    public BaseVertex EndPosition;
}

class GraphDrawer<T>
{
    private IGraph<T> _graph;
    private Dictionary<T, (int, int)> _vertexCoordinates;
    private Dictionary<(T, T), ((int, int), (int, int))> _edgeCoordinates;

    public GraphDrawer(IGraph<T> graph)
    {
        _graph = graph;
    }

    private static int Mod(int x, int m)
    {
        return (x % m + m) % m;
    }

    private class Face
    {
        public List<T> Vertices;
        public bool SwitchWinding;

        private Dictionary<T, int> _stNumbering;
        private PlanarEmbedding<T> _embedding;

        private static char C = 'I';
        private char _c;

        public override string ToString()
        {
            return $"{_c}";
        }

        public Face(List<T> vertices, Dictionary<T, int> stNumbering, PlanarEmbedding<T> embedding, bool switchWinding = false)
        {
            Vertices = vertices;
            _stNumbering = stNumbering;
            _embedding = embedding;
            _c = C++;
            SwitchWinding = switchWinding;
        }

        private IEnumerable<T> PropperEmbedding(T vertex)
        {
            int start = 0;
            List<GraphPlanarityTesting.Graphs.DataStructures.IEdge<T>> edges = _embedding.GetEdgesAroundVertex(vertex);
            int count = edges.Count;

            for (int i = 0; i < count; i++)
            {
                GraphPlanarityTesting.Graphs.DataStructures.IEdge<T> edge = edges[i];
                T neighbor = edge.Source.Equals(vertex)
                    ? edge.Target
                    : edge.Source;

                GraphPlanarityTesting.Graphs.DataStructures.IEdge<T> nextEdge = edges[Mod(i + 1, count)];
                T nextNeighbor = nextEdge.Source.Equals(vertex)
                    ? nextEdge.Target
                    : nextEdge.Source;

                if (_stNumbering[neighbor] < _stNumbering[vertex] &&
                    _stNumbering[nextNeighbor] > _stNumbering[vertex])
                {
                    start = Mod(i + 1, count);
                    break;
                }
            }

            for (int i = 0; i < count; i++)
            {
                GraphPlanarityTesting.Graphs.DataStructures.IEdge<T> edge = edges[Mod(start + i, count)];
                T neighbor = edge.Source.Equals(vertex)
                    ? edge.Target
                    : edge.Source;

                if (_stNumbering[neighbor] < _stNumbering[vertex])
                    break;

                yield return neighbor;
            }
        }

        private (int, int) GetLowPointDirection(bool getRight = false)
        {
            int min = int.MaxValue;
            int minIndex = 0;
            for (int i = 0; i < Vertices.Count; i++)
                if (_stNumbering[Vertices[i]] < min)
                {
                    min = _stNumbering[Vertices[i]];
                    minIndex = i;
                }

            T leftVertex = Vertices[Mod(minIndex - 1, Vertices.Count)];
            T rightVertex = Vertices[Mod(minIndex + 1, Vertices.Count)];
            int left = -1;
            int right = 1;

            /*/
            T[] propperEmbedding = PropperEmbedding(Vertices[minIndex]).ToArray();

            foreach (T neighbor in propperEmbedding)
            {
                if (neighbor.Equals(rightVertex))
                {
                    T temp = leftVertex;
                    leftVertex = rightVertex;
                    rightVertex = temp;
                    left = 1; right = -1;
                    break;
                } else if (neighbor.Equals(leftVertex))
                {
                    break;
                }
            }
            //*/

            if (getRight)
                return (minIndex, right);
            else
                return (minIndex, left);
        }

        public bool Adjecent(Face other)
        {
            bool getRightThis = !SwitchWinding; // normally right direction
            bool getRightOther = other.SwitchWinding; // normally left direction

            (int minIndexThis, int dirThis) = GetLowPointDirection(getRightThis);
            (int minIndexOther, int dirOther) = other.GetLowPointDirection(getRightOther);

            int i = minIndexThis;
            int countThis = Vertices.Count;
            int countOther = other.Vertices.Count;
            T vThis = Vertices[i];
            T vNextThis = Vertices[Mod(i + dirThis, countThis)];
            while (_stNumbering[vThis] < _stNumbering[vNextThis])
            {
                int j = minIndexOther;
                T vOther = other.Vertices[j];
                T vNextOther = other.Vertices[Mod(j + dirOther, countOther)];

                while (_stNumbering[vOther] < _stNumbering[vNextOther])
                {
                    if (vThis.Equals(vOther) && vNextThis.Equals(vNextOther))
                        return true;

                    j = Mod(j + dirOther, countOther);
                    vOther = other.Vertices[j];
                    vNextOther = other.Vertices[Mod(j + dirOther, countOther)];
                }

                i = Mod(i + dirThis, countThis);
                vThis = Vertices[i];
                vNextThis = Vertices[Mod(i + dirThis, countThis)];
            }

            return false;
        }

        public bool ContainsEdge(T from, T to)
        {

            int count = Vertices.Count;
            for (int i = 0; i < count; i++)
            {
                T current = Vertices[i];
                T next = Vertices[Mod(i + 1, count)];
                T previous = Vertices[Mod(i - 1, count)];

                if (current.Equals(from) && (next.Equals(to) || previous.Equals(to)))
                    return true;
            }

            return false;
        }
    }

    private Dictionary<(T, T), int> EdgePositionsX(
    PlanarFaces<T> planarFaces,
    PlanarEmbedding<T> embedding,
    Dictionary<T, int> stNumbering)
    {
        AdjecencyGraph<Face> graph = new();
        List<Face> faces = new();
        foreach (List<T> planarFace in planarFaces.Faces)
        {
            var face = new Face(planarFace, stNumbering, embedding);
            faces.Add(face);
            graph.AddVertex(face);
        }

        Face outerFace = faces[0];
        outerFace.SwitchWinding = false;
        Face specialFace = new Face(outerFace.Vertices, stNumbering, embedding);
        specialFace.SwitchWinding = false;
        graph.AddVertex(specialFace);


        foreach (Face face1 in faces)
            foreach (Face face2 in faces)
            {
                if (face1 == face2)
                    continue;

                Face potentialNeighbor = (face2 == outerFace)
                    ? specialFace
                    : face2;

                if (face1.Adjecent(potentialNeighbor))
                    graph.AddEdge(face1, potentialNeighbor);
            }

        Dictionary<Face, List<(T, T)>> rightEdgesFromFace = new();

        foreach (Face face1 in faces)
            for (int i = 0; i < face1.Vertices.Count; i++)
                foreach (Face face2 in faces)
                {
                    // edge of the face1
                    T v1 = face1.Vertices[i];
                    T v2 = face1.Vertices[Mod(i + 1, face1.Vertices.Count)];
                    if (face2.ContainsEdge(v1, v2))
                    {
                        bool areNeighbors = graph.GetNeighbors(face1).Contains(face2);
                        bool neighborWithSpecial = face2 == outerFace && graph.GetNeighbors(face1).Contains(specialFace);
                        if (areNeighbors || neighborWithSpecial)
                        {

                            if (!rightEdgesFromFace.ContainsKey(face1))
                                rightEdgesFromFace[face1] = new List<(T, T)>();

                            rightEdgesFromFace[face1].Add((v1, v2));
                        }
                    }
                }

        graph.RemoveVertex(specialFace);
        DFSParams<Face> dfsParams = graph.DepthFirstSearch(outerFace);
        IEnumerable<Face> orderedFaces =
            graph.GetVertices().OrderBy((Face face) =>
            {
                if (dfsParams.ExitTimes.ContainsKey(face))
                    return -dfsParams.ExitTimes[face];
                else
                    return 0;
            });

        Dictionary<(T, T), int> edgeX = new();
        int x = 0;
        foreach (Face face in orderedFaces)
        {
            if (face == specialFace) continue;
            foreach ((T from, T to) in rightEdgesFromFace[face])
            {
                edgeX[(from, to)] = x;
                edgeX[(to, from)] = x;
                x++;
            }
        }

        return edgeX;
    }

    private (PlanarFaces<T>, PlanarEmbedding<T>) PlanarEmbedding(IGraph<T> generationGraph)
    {

        Planarity.UndirectedAdjacencyListGraph<T> graph = new();
        foreach (T vertex in generationGraph.GetVertices())
            graph.AddVertex(vertex);

        foreach ((T from, T to) in generationGraph.GetEdgePairs())
            graph.AddEdge(from, to);

        BoyerMyrvold<T> alg = new();
        alg.IsPlanar(graph, out PlanarEmbedding<T> embedding);
        alg.TryGetPlanarFaces(graph, out var faces);

        // Just Logging
        //*/
        {
            Debug.Log("PLANAR EMBEDDING ============================");
            StringBuilder sb = new();
            foreach (T vertex in generationGraph.GetVertices())
            {
                sb.Clear();
                sb.Append($"{vertex}: [ ");
                var edges = embedding.GetEdgesAroundVertex(vertex);
                foreach (var edge in edges)
                {
                    var neighbor = edge.Source.Equals(vertex) ? edge.Target : edge.Source;
                    sb.Append($"{neighbor} ");
                }
                sb.Append(" ]");
                Debug.Log(sb);
            }

            Debug.Log("FACES =============================");
            int i = 0;
            foreach (List<T> face in faces.Faces)
            {
                sb.Clear();
                sb.Append($"Face {i++}: [ ");
                foreach (T vertex in face)
                {
                    sb.Append($" {vertex} ");
                }

                sb.Append(" ]");
                Debug.Log(sb);
            }
        }
        //*/

        return (faces, embedding);
    }


    public GraphDrawing<T> Draw(BaseVertex startVertex, BaseVertex endVertex)
    {
        var (faces, embedding) = PlanarEmbedding(_graph);

        // These vertices are on the outer face
        List<T> outerFace = faces.Faces[0];
        int mid = outerFace.Count / 2;
        T start = outerFace[0];
        T end = outerFace[mid];

        Dictionary<T, int> stNumbering = _graph.STNumbering(start, end);
        Dictionary<(T, T), int> edgePositionsX = EdgePositionsX(faces, embedding, stNumbering);

        Dictionary<T, (int, int)> vertexPositions = new();
        HashSet<(int x, int yFrom, int yTo)> verticalLines = new();
        HashSet<(int xFrom, int xTo, int y)> horizontalLines = new();
        int maximumX = int.MinValue;
        int maximumY = int.MinValue;

        // Vertical Lines
        foreach ((T from, T to) in _graph.GetEdgePairs())
        {
            int x = edgePositionsX[(from, to)];
            int yFrom = stNumbering[from];
            int yTo = stNumbering[to];
            if (yFrom > yTo)
            {
                int tmp = yFrom;
                yFrom = yTo;
                yTo = tmp;
            }

            verticalLines.Add((x, yFrom, yTo));
            maximumX = Math.Max(maximumX, x);
            maximumY = Math.Max(maximumY, yFrom);
            maximumY = Math.Max(maximumY, yTo);
        }

        // Horizontal Lines + Vertex Positions
        foreach (T vertex in _graph.GetVertices())
        {
            int xmin = int.MaxValue;
            int xmax = int.MinValue;

            foreach (T neighbor in _graph.GetNeighbors(vertex))
            {
                int x = edgePositionsX[(vertex, neighbor)];
                if (x < xmin) xmin = x;
                if (x > xmax) xmax = x;
            }

            int y = stNumbering[vertex];
            int xmid = xmin + (xmax - xmin) / 2;
            vertexPositions[vertex] = (xmid, y);
            horizontalLines.Add((xmin, xmax, y));

            maximumX = Math.Max(maximumX, xmax);
            maximumY = Math.Max(maximumY, y);
        }

        return new GraphDrawing<T>
        {
            VertexPositions = vertexPositions,
            HorizontalLines = horizontalLines,
            VerticalLines = verticalLines,
            MaximumX = maximumX,
            MaximumY = maximumY,
            StartPosition = startVertex,
            EndPosition = stNumbering.Keys.ToArray()[stNumbering.Count - 1] as BaseVertex,
        };
    }
}