// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.IO.Stores;
using osu.Framework.Testing;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Skinning;
using osu.Game.Storyboards;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public abstract class LegacySkinPlayerTestScene : PlayerTestScene
    {
        protected LegacySkin LegacySkin { get; private set; }

        private ISkinSource legacySkinSource;

        protected override TestPlayer CreatePlayer(Ruleset ruleset) => new SkinProvidingPlayer(legacySkinSource);

        protected override WorkingBeatmap CreateWorkingBeatmap(IBeatmap beatmap, Storyboard storyboard = null)
            => new LegacySkinWorkingBeatmap(beatmap, storyboard, Clock, Audio);

        [BackgroundDependencyLoader]
        private void load(OsuGameBase game, SkinManager skins)
        {
            LegacySkin = new DefaultLegacySkin(new NamespacedResourceStore<byte[]>(game.Resources, "Skins/Legacy"), skins);
            legacySkinSource = new SkinProvidingContainer(LegacySkin);
        }

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            // check presence of a random legacy HUD component to ensure this is using legacy skin.
            AddAssert("using legacy skin", () => this.ChildrenOfType<LegacyScoreCounter>().Any());
        }

        public class SkinProvidingPlayer : TestPlayer
        {
            [Cached(typeof(ISkinSource))]
            private readonly ISkinSource skinSource;

            public SkinProvidingPlayer(ISkinSource skinSource)
            {
                this.skinSource = skinSource;
            }
        }

        private class LegacySkinWorkingBeatmap : ClockBackedTestWorkingBeatmap
        {
            public LegacySkinWorkingBeatmap(IBeatmap beatmap, Storyboard storyboard, IFrameBasedClock frameBasedClock, AudioManager audio)
                : base(beatmap, storyboard, frameBasedClock, audio)
            {
            }

            protected override ISkin GetSkin() => new LegacyBeatmapSkin(BeatmapInfo, null, null);
        }
    }
}
