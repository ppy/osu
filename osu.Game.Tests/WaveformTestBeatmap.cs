// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using System.Linq;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.Video;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Formats;
using osu.Game.IO;
using osu.Game.IO.Archives;
using osu.Game.Rulesets.Catch;
using osu.Game.Tests.Beatmaps;
using osu.Game.Tests.Resources;

namespace osu.Game.Tests
{
    /// <summary>
    /// A <see cref="WorkingBeatmap"/> that is used for test scenes that include waveforms.
    /// </summary>
    public class WaveformTestBeatmap : WorkingBeatmap
    {
        private readonly Beatmap beatmap;
        private readonly ITrackStore trackStore;

        public WaveformTestBeatmap(AudioManager audioManager)
            : this(audioManager, new WaveformBeatmap())
        {
        }

        public WaveformTestBeatmap(AudioManager audioManager, Beatmap beatmap)
            : base(beatmap.BeatmapInfo, audioManager)
        {
            this.beatmap = beatmap;
            trackStore = audioManager.GetTrackStore(getZipReader());
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            trackStore?.Dispose();
        }

        private static Stream getStream() => TestResources.GetTestBeatmapStream();

        private static ZipArchiveReader getZipReader() => new ZipArchiveReader(getStream());

        protected override IBeatmap GetBeatmap() => beatmap;

        protected override Texture GetBackground() => null;

        protected override VideoSprite GetVideo() => null;

        protected override Waveform GetWaveform() => new Waveform(trackStore.GetStream(firstAudioFile));

        protected override Track GetTrack() => trackStore.Get(firstAudioFile);

        private string firstAudioFile
        {
            get
            {
                using (var reader = getZipReader())
                    return reader.Filenames.First(f => f.EndsWith(".mp3"));
            }
        }

        private class WaveformBeatmap : TestBeatmap
        {
            public WaveformBeatmap()
                : base(new CatchRuleset().RulesetInfo)
            {
            }

            protected override Beatmap CreateBeatmap()
            {
                using (var reader = getZipReader())
                using (var beatmapStream = reader.GetStream(reader.Filenames.First(f => f.EndsWith(".osu"))))
                using (var beatmapReader = new LineBufferedReader(beatmapStream))
                    return Decoder.GetDecoder<Beatmap>(beatmapReader).Decode(beatmapReader);
            }
        }
    }
}
