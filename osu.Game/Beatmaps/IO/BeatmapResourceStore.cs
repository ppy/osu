using System;
using System.Collections.Generic;
using System.IO;
using osu.Framework.IO.Stores;
using osu.Game.Database;

namespace osu.Game.Beatmaps.IO
{
    public class BeatmapResourceStore : IResourceStore<byte[]>, IDisposable
    {
        private Dictionary<int, ArchiveReader> beatmaps = new Dictionary<int, ArchiveReader>();
        private BeatmapDatabase database;
        
        public BeatmapResourceStore(BeatmapDatabase database)
        {
            this.database = database;
        }

        public void AddBeatmap(BeatmapSetInfo setInfo)
        {
            beatmaps.Add(setInfo.BeatmapSetID, database.GetReader(setInfo));
        }

        public void RemoveBeatmap(BeatmapSetInfo setInfo)
        {
            beatmaps[setInfo.BeatmapSetID].Dispose();
            beatmaps.Remove(setInfo.BeatmapSetID);
        }

        public void Dispose()
        {
            foreach (var b in beatmaps.Values)
                b.Dispose();
        }

        public byte[] Get(string name)
        {
            throw new NotImplementedException();
        }

        public Stream GetStream(string name)
        {
            string id = name.Remove(name.IndexOf(':'));
            string path = name.Substring(name.IndexOf(':') + 1);
            var reader = beatmaps[int.Parse(id)];
            return reader.ReadFile(path);
        }
    }
}