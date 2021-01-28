// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.Taiko.Replays;
using osu.Framework.Input;
using osu.Game.Configuration;
using osu.Game.Input.Handlers;
using osu.Game.Replays;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Scoring;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Taiko.UI
{
    public class DrawableTaikoRuleset : DrawableScrollingRuleset<TaikoHitObject>
    {
        private SkinnableDrawable scroller;

        protected override ScrollVisualisationMethod VisualisationMethod => ScrollVisualisationMethod.Overlapping;

        protected override bool UserScrollSpeedAdjustment => false;

        public DrawableTaikoRuleset(Ruleset ruleset, IBeatmap beatmap, IReadOnlyList<Mod> mods = null)
            : base(ruleset, beatmap, mods)
        {
            Direction.Value = ScrollingDirection.Left;
            TimeRange.Value = 7000;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            new BarLineGenerator<BarLine>(Beatmap).BarLines.ForEach(bar => Playfield.Add(bar));

            FrameStableComponents.Add(scroller = new SkinnableDrawable(new TaikoSkinComponent(TaikoSkinComponents.Scroller), _ => Empty())
            {
                RelativeSizeAxes = Axes.X,
                Depth = float.MaxValue
            });
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            var playfieldScreen = Playfield.ScreenSpaceDrawQuad;
            scroller.Height = ToLocalSpace(playfieldScreen.TopLeft + new Vector2(0, playfieldScreen.Height / 20)).Y;
        }

        public override PlayfieldAdjustmentContainer CreatePlayfieldAdjustmentContainer() => new TaikoPlayfieldAdjustmentContainer();

        protected override PassThroughInputManager CreateInputManager() => new TaikoInputManager(Ruleset.RulesetInfo);

        protected override Playfield CreatePlayfield() => new TaikoPlayfield(Beatmap.ControlPointInfo);

        public override DrawableHitObject<TaikoHitObject> CreateDrawableRepresentation(TaikoHitObject h) => null;

        protected override ReplayInputHandler CreateReplayInputHandler(Replay replay) => new TaikoFramedReplayInputHandler(replay);

        protected override ReplayRecorder CreateReplayRecorder(Score score) => new TaikoReplayRecorder(score);
    }
}
