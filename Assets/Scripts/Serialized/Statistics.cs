using System;
using System.Collections.Generic;
using System.Linq;

using PokeAmie.Serialization;

[Serializable]
public class Statistics : JsonSerializedFile {
    public Statistics(string path) : base(path) { }
    public List<ViewerStats> viewerStats = new();

    public ViewerStats GetStatsOfViewer(string viewerId) {
        ViewerStats stats = viewerStats.FirstOrDefault(stats => string.Equals(stats.viewerId, viewerId, StringComparison.CurrentCultureIgnoreCase));
        if (stats != null) {
            return stats;
        }

        stats = new ViewerStats() { viewerId = viewerId };
        viewerStats.Add(stats);
        return stats;
    }
}

[Serializable]
public class ViewerStats {
    public string viewerId;
    public int pokepuffsFed, petsGiven;
}
