using System;
using System.IO;
using System.Linq;
using Ionic.Zip;
using osu.Game.Beatmaps.Formats;

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
                    if (!ZipFile.IsZipFile(stream, false))
                        return false;
                    using (ZipFile zip = ZipFile.Read(stream))
                        return zip.Entries.Any(e => e.FileName.EndsWith(".osu"));
                }
            });
        }
    
        private ZipFile Archive { get; set; }
        private string[] Beatmaps { get; set; }
        private Beatmap FirstMap { get; set; }
    
        public OszArchiveReader(Stream archive)
        {
            Archive = ZipFile.Read(archive);
            Beatmaps = Archive.Entries.Where(e => e.FileName.EndsWith(".osu"))
                .Select(e => e.FileName).ToArray();
            if (Beatmaps.Length == 0)
                throw new FileNotFoundException("This directory contains no beatmaps");
            using (var stream = new StreamReader(ReadFile(Beatmaps[0])))
            {
                var decoder = BeatmapDecoder.GetDecoder(stream);
                FirstMap = decoder.Decode(stream);
            }
        }

        public override string[] ReadBeatmaps()
        {
            return Beatmaps;
        }

        public override Stream ReadFile(string name)
        {
            ZipEntry entry = Archive.Entries.SingleOrDefault(e => e.FileName == name);
            if (entry == null)
                throw new FileNotFoundException();
            return entry.OpenReader();
        }

        public override BeatmapMetadata ReadMetadata()
        {
            return FirstMap.Metadata;
        }
    }
}