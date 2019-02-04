// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using System.Linq;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Formats;
using osu.Game.IO.Archives;
using osu.Game.Tests.Resources;

namespace osu.Game.Tests
{
    /// <summary>
    /// A <see cref="WorkingBeatmap"/> that is used for testcases that include waveforms.
    /// </summary>
    public class WaveformTestBeatmap : WorkingBeatmap
    {
        private readonly ZipArchiveReader reader;
        private readonly Stream stream;

        public WaveformTestBeatmap()
            : base(new BeatmapInfo())
        {
            stream = TestResources.GetTestBeatmapStream();
            reader = new ZipArchiveReader(stream);
        }

        public override void Dispose()
        {
            base.Dispose();
            stream?.Dispose();
            reader?.Dispose();
        }

        protected override IBeatmap GetBeatmap() => createTestBeatmap();

        protected override Texture GetBackground() => null;

        protected override Waveform GetWaveform() => new Waveform(getAudioStream());

        protected override Track GetTrack() => new TrackBass(getAudioStream());

        private Stream getAudioStream() => reader.GetStream(reader.Filenames.First(f => f.EndsWith(".mp3")));
        private Stream getBeatmapStream() => reader.GetStream(reader.Filenames.First(f => f.EndsWith(".osu")));

        private Beatmap createTestBeatmap()
        {
            using (var beatmapStream = getBeatmapStream())
            using (var beatmapReader = new StreamReader(beatmapStream))
                return Decoder.GetDecoder<Beatmap>(beatmapReader).Decode(beatmapReader);
        }
    }
}
