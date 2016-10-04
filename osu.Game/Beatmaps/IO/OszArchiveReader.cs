using System;
using System.IO;

namespace osu.Game.Beatmaps.IO
{
    public class OszArchiveReader : ArchiveReader
    {
        static OszArchiveReader()
        {
            AddReader<OszArchiveReader>((storage, path) =>
            {
                using (var stream = storage.GetStream(path))
                {
                    // TODO: detect if osz
                    return false;
                }
            });
        }
    
        private Stream Archive { get; set; }
    
        public OszArchiveReader(Stream archive)
        {
            Archive = archive;
        }

        public override string[] ReadBeatmaps()
        {
            throw new NotImplementedException();
        }

        public override Stream ReadFile(string name)
        {
            throw new NotImplementedException();
        }

        public override Metadata ReadMetadata()
        {
            throw new NotImplementedException();
        }
    }
}