// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Linq;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Configuration;
using osu.Game.Input.Handlers;
using osu.Game.Replays;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Replays;
using osu.Game.Rulesets.Taiko.Mods;
using osu.Game.Rulesets.Timing;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Scoring;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Taiko.UI
{
    public class DrawableTaikoRuleset : DrawableScrollingRuleset<TaikoHitObject>
    {
        public const double DEFAULT_TIME_RANGE = 7000;

        public new BindableDouble TimeRange => base.TimeRange;

        public Bindable<float> AspectRatioLimit = new Bindable<float>(TaikoPlayfieldAdjustmentContainer.DEFAULT_ASPECT);

        public Bindable<AspectRatioAdjustmentMethod> AdjustmentMethod = new Bindable<AspectRatioAdjustmentMethod>(AspectRatioAdjustmentMethod.Scale);

        protected override ScrollVisualisationMethod VisualisationMethod => ScrollVisualisationMethod.Overlapping;

        protected override bool UserScrollSpeedAdjustment => false;

        private SkinnableDrawable scroller;

        public DrawableTaikoRuleset(Ruleset ruleset, IBeatmap beatmap, IReadOnlyList<Mod> mods = null)
            : base(ruleset, beatmap, mods)
        {
            Direction.Value = ScrollingDirection.Left;
            TimeRange.Value = DEFAULT_TIME_RANGE;
        }

        public static double AspectRatioToTimeRange(double aspectRatio)
        {
            return aspectRatio / TaikoPlayfieldAdjustmentContainer.DEFAULT_ASPECT * DEFAULT_TIME_RANGE;
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

            KeyBindingInputManager.Add(new DrumTouchInputArea());
            applyClassicMods(Mods);
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            var playfieldScreen = Playfield.ScreenSpaceDrawQuad;
            scroller.Height = ToLocalSpace(playfieldScreen.TopLeft + new Vector2(0, playfieldScreen.Height / 20)).Y;
        }

        public MultiplierControlPoint ControlPointAt(double time)
        {
            int result = ControlPoints.BinarySearch(new MultiplierControlPoint(time));
            if (result < 0)
                result = Math.Clamp(~result - 1, 0, ControlPoints.Count);
            return ControlPoints[result];
        }

        public override PlayfieldAdjustmentContainer CreatePlayfieldAdjustmentContainer() => new TaikoPlayfieldAdjustmentContainer
        {
            AdjustmentMethod = { BindTarget = AdjustmentMethod },
            AspectRatioLimit = { BindTarget = AspectRatioLimit }
        };

        protected override PassThroughInputManager CreateInputManager() => new TaikoInputManager(Ruleset.RulesetInfo);

        protected override Playfield CreatePlayfield() => new TaikoPlayfield();

        public override DrawableHitObject<TaikoHitObject> CreateDrawableRepresentation(TaikoHitObject h) => null;

        protected override ReplayInputHandler CreateReplayInputHandler(Replay replay) => new TaikoFramedReplayInputHandler(replay);

        protected override ReplayRecorder CreateReplayRecorder(Score score) => new TaikoReplayRecorder(score);

        // TODO: Review this - this introduces a circular dependency between DrawableTaikoRuleset and TaikoModClassic
        private void applyClassicMods(IReadOnlyList<Mod> mods)
        {
            foreach (TaikoModClassic classic in mods.OfType<TaikoModClassic>())
            {
                foreach (IApplicableToTaikoClassic mod in mods.OfType<IApplicableToTaikoClassic>())
                {
                    mod.ApplyToTaikoModClassic(classic);
                }

                // Return early since there shouldn't be more than one TaikoModClassic.
                return;
            }
        }
    }
}
