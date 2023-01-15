// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.UI;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Taiko.Tests
{
    public abstract partial class DrawableTaikoRulesetTestScene : OsuTestScene
    {
        protected const int DEFAULT_PLAYFIELD_CONTAINER_HEIGHT = 768;

        protected DrawableTaikoRuleset DrawableRuleset { get; private set; }
        protected Container PlayfieldContainer { get; private set; }

        private ControlPointInfo controlPointInfo { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            controlPointInfo = new ControlPointInfo();
            controlPointInfo.Add(0, new TimingControlPoint());

            IWorkingBeatmap beatmap = CreateWorkingBeatmap(CreateBeatmap(new TaikoRuleset().RulesetInfo));

            Add(PlayfieldContainer = new Container
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                RelativeSizeAxes = Axes.X,
                Height = DEFAULT_PLAYFIELD_CONTAINER_HEIGHT,
                Children = new[] { DrawableRuleset = new DrawableTaikoRuleset(new TaikoRuleset(), beatmap.GetPlayableBeatmap(beatmap.BeatmapInfo.Ruleset)) }
            });
        }

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset)
        {
            return new Beatmap
            {
                HitObjects = new List<HitObject> { new Hit { Type = HitType.Centre } },
                BeatmapInfo = new BeatmapInfo
                {
                    Difficulty = new BeatmapDifficulty(),
                    Metadata = new BeatmapMetadata
                    {
                        Artist = @"Unknown",
                        Title = @"Sample Beatmap",
                        Author = { Username = @"peppy" },
                    },
                    Ruleset = ruleset
                },
                ControlPointInfo = controlPointInfo
            };
        }
    }
}
