﻿using System;
using System.IO;
using System.Linq;
using Ionic.Zip;
using osu.Game.Beatmaps.Formats;

namespace osu.Game.Beatmaps.IO
{
    public sealed class OszArchiveReader : ArchiveReader
    {
        public static void Register()
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
            OsuLegacyDecoder.Register();
        }
    
        private ZipFile archive { get; set; }
        private string[] beatmaps { get; set; }
        private Beatmap firstMap { get; set; }
    
        public OszArchiveReader(Stream archiveStream)
        {
            archive = ZipFile.Read(archiveStream);
            beatmaps = archive.Entries.Where(e => e.FileName.EndsWith(".osu"))
                .Select(e => e.FileName).ToArray();
            if (beatmaps.Length == 0)
                throw new FileNotFoundException("This directory contains no beatmaps");
            using (var stream = new StreamReader(ReadFile(beatmaps[0])))
            {
                var decoder = BeatmapDecoder.GetDecoder(stream);
                firstMap = decoder.Decode(stream);
            }
        }

        public override string[] ReadBeatmaps()
        {
            return beatmaps;
        }

        public override Stream ReadFile(string name)
        {
            ZipEntry entry = archive.Entries.SingleOrDefault(e => e.FileName == name);
            if (entry == null)
                throw new FileNotFoundException();
            return entry.OpenReader();
        }

        public override BeatmapMetadata ReadMetadata()
        {
            return firstMap.Metadata;
        }
        public override void Dispose()
        {
            archive.Dispose();
        }
    }
}