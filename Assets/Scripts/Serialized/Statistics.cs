using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PokeAmie.Serialization;

[System.Serializable]
public class Statistics : JsonSerializedFile {
    public Statistics(string path) : base(path) { }
    public List<ViewerStats> viewerStats = new();

    public ViewerStats GetStatsOfViewer(string viewerId) {
        foreach (ViewerStats stats in viewerStats) {
            if (stats.viewerId.ToLower() == viewerId.ToLower())
                return stats;
        }
        ViewerStats newStats = new() { viewerId = viewerId };
        viewerStats.Add(newStats);
        return newStats;
    }
}

[System.Serializable]
public class ViewerStats {
    public string viewerId;
    public int pokepuffsFed, petsGiven;
}