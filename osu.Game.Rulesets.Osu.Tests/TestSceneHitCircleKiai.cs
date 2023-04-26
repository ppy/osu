// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using NUnit.Framework;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;

namespace osu.Game.Rulesets.Osu.Tests
{
    [TestFixture]
    public partial class TestSceneHitCircleKiai : TestSceneHitCircle, IBeatSyncProvider
    {
        private ControlPointInfo controlPoints { get; set; }

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            controlPoints = new ControlPointInfo();

            controlPoints.Add(0, new TimingControlPoint { BeatLength = 500 });
            controlPoints.Add(0, new EffectControlPoint { KiaiMode = true });

            Beatmap.Value = CreateWorkingBeatmap(new Beatmap
            {
                ControlPointInfo = controlPoints
            });

            // track needs to be playing for BeatSyncedContainer to work.
            Beatmap.Value.Track.Start();
        });

        ChannelAmplitudes IHasAmplitudes.CurrentAmplitudes => new ChannelAmplitudes();
        ControlPointInfo IBeatSyncProvider.ControlPoints => controlPoints;
        IClock IBeatSyncProvider.Clock => Clock;
    }
}
