using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using URandom = UnityEngine.Random;

/// <summary>
/// Class for generation of <see cref="GridGraph"/> using <see cref="Pattern"/> classes.
/// Generates based on <see cref="GridGraphGenerator"/>.
/// </summary>
public class GridGraphGenerator
{
    public GridGraph Graph { get; private set; }
    public Dictionary<ILock, GridVertex> LockMapping = new();
    public Dictionary<IKey, GridVertex> KeyMapping = new();
    private GridVertex _start;
    private GridVertex _end;

    public GridGraphGenerator(GridGraph graph)
    {
        Graph = graph;
    }

    public void RegisterLock(ILock l, GridVertex vertex) => LockMapping[l] = vertex;
    public void RegisterKey(IKey k, GridVertex vertex) => KeyMapping[k] = vertex;

    public GridVertex GetLockVertex(ILock l) => LockMapping[l];
    public GridVertex GetKeyVertex(IKey k) => KeyMapping[k];

    public GridVertex GetStartVertex() => _start;
    public GridVertex GetEndVertex() => _end;

    /// <summary>
    /// Generates a <see cref="GridGraph"/> based on <see cref="GridGraphGenerator"/> using <see cref="Pattern"/> classes.
    /// </summary>
    public void Generate()
    {
        List<Pattern> patterns = new();
        if (GenerationSettings.PatternDoubleLock) patterns.Add(new DoubleLockCyclePattern());
        if (GenerationSettings.PatternLockedCycle) patterns.Add(new LockedCyclePattern());
        if (GenerationSettings.PatternHiddenShortcut) patterns.Add(new HiddenPathPattern());
        if (GenerationSettings.PatternLockedFork) patterns.Add(new LockedForkPattern());
        if (GenerationSettings.PatternAlternativePath) patterns.Add(new AlternatePathPattern());

        List<Pattern> floorPatterns = new();
        if (GenerationSettings.FloorPatternHiddenShortcut) floorPatterns.Add(new FloorHiddenPathPattern());
        if (GenerationSettings.FloorPatternLockedCycle) floorPatterns.Add(new FloorLockedCyclePattern());
        if (GenerationSettings.FloorPatternLockedFork) floorPatterns.Add(new FloorLockedForkPattern());

        List<DangerType> dangerTypes = new();
        if (GenerationSettings.DangerCameras) dangerTypes.Add(DangerType.SecurityCameras);
        if (GenerationSettings.DangerSoundTraps) dangerTypes.Add(DangerType.SoundTraps);
        if (GenerationSettings.DangerDeathTraps) dangerTypes.Add(DangerType.DeathTraps);

        if (GenerationSettings.FloorPatternCount == 0)
        { // Start with a graph with a single edge
            _start = Graph.AddGridVertex(0, 0);
            _end = Graph.AddGridVertex(0, GridGraph.STEP);
            Graph.AddGridEdge(_start, _end, Directions.North, Directions.South);
        } else
        { // Start with a graph that has two floors.
            var f1 = Graph.AddGridVertex(0, 0, 0);
            var f2 = Graph.AddGridVertex(0, 0, GridGraph.STEP);

            _start = Graph.AddGridVertex(0, -GridGraph.STEP, 0);
            _end = Graph.AddGridVertex(0, GridGraph.STEP, GridGraph.STEP);

            Graph.AddInterFloorEdge(f1, f2);
            Graph.AddGridEdge(_start, f1, Directions.North, Directions.South);
            Graph.AddGridEdge(f2, _end, Directions.North, Directions.South);
        }

        if (floorPatterns.Count > 0)
            for (int i = 0; i < GenerationSettings.FloorPatternCount; i++)
            {
                int index = URandom.Range(0, floorPatterns.Count);
                int dangerIndex = URandom.Range(0, dangerTypes.Count);
                GridEdge e = Graph.GetRandomInterfloorEdge();
                var pattern = floorPatterns[index];
                if (dangerTypes.Count > 0)
                    pattern.DangerType = dangerTypes[dangerIndex];
                pattern.Apply(e, Graph);
            }

        if (patterns.Count > 0)
            for (long floor = 0; floor < Graph.FloorCount; floor++)
            {
                for (int i = 0; i < GenerationSettings.PatternCount; i++)
                {
                    int index = URandom.Range(0, patterns.Count);
                    int dangerIndex = URandom.Range(0, dangerTypes.Count);
                    GridEdge e = Graph.GetRandomFloorEdge(floor, allowHidden: false);
                    var pattern = patterns[index];
                    if (dangerTypes.Count > 0)
                        pattern.DangerType = dangerTypes[dangerIndex];
                    pattern.Apply(e, Graph);
                }
            }
    }
}
