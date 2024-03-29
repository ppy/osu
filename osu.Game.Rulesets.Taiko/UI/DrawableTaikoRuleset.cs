﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Input.Handlers;
using osu.Game.Replays;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Replays;
using osu.Game.Rulesets.Timing;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Skinning;
using osu.Game.Storyboards;
using osuTK;

namespace osu.Game.Rulesets.Taiko.UI
{
    public partial class DrawableTaikoRuleset : DrawableScrollingRuleset<TaikoHitObject>
    {
        public new BindableDouble TimeRange => base.TimeRange;

        public readonly BindableBool LockPlayfieldAspectRange = new BindableBool(true);

        public new TaikoInputManager KeyBindingInputManager => (TaikoInputManager)base.KeyBindingInputManager;

        protected new TaikoPlayfieldAdjustmentContainer PlayfieldAdjustmentContainer => (TaikoPlayfieldAdjustmentContainer)base.PlayfieldAdjustmentContainer;

        protected override bool UserScrollSpeedAdjustment => false;

        [CanBeNull]
        private SkinnableDrawable scroller;

        public DrawableTaikoRuleset(Ruleset ruleset, IBeatmap beatmap, IReadOnlyList<Mod> mods = null)
            : base(ruleset, beatmap, mods)
        {
            Direction.Value = ScrollingDirection.Left;
            VisualisationMethod = ScrollVisualisationMethod.Overlapping;
        }

        [BackgroundDependencyLoader(true)]
        private void load([CanBeNull] GameplayState gameplayState)
        {
            new BarLineGenerator<BarLine>(Beatmap).BarLines.ForEach(bar => Playfield.Add(bar));

            var spriteElements = gameplayState?.Storyboard.Layers.Where(l => l.Name != @"Overlay")
                                              .SelectMany(l => l.Elements)
                                              .OfType<StoryboardSprite>()
                                              .DistinctBy(e => e.Path) ?? Enumerable.Empty<StoryboardSprite>();

            if (spriteElements.Count() < 10)
            {
                FrameStableComponents.Add(scroller = new SkinnableDrawable(new TaikoSkinComponentLookup(TaikoSkinComponents.Scroller), _ => Empty())
                {
                    RelativeSizeAxes = Axes.X,
                    Depth = float.MaxValue,
                });
            }

            KeyBindingInputManager.Add(new DrumTouchInputArea());
        }

        protected override void Update()
        {
            base.Update();

            TimeRange.Value = ComputeTimeRange();
        }

        protected virtual double ComputeTimeRange() => PlayfieldAdjustmentContainer.ComputeTimeRange();

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            var playfieldScreen = Playfield.ScreenSpaceDrawQuad;

            if (scroller != null)
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
            LockPlayfieldAspectRange = { BindTarget = LockPlayfieldAspectRange }
        };

        protected override PassThroughInputManager CreateInputManager() => new TaikoInputManager(Ruleset.RulesetInfo);

        protected override Playfield CreatePlayfield() => new TaikoPlayfield();

        public override DrawableHitObject<TaikoHitObject> CreateDrawableRepresentation(TaikoHitObject h) => null;

        protected override ReplayInputHandler CreateReplayInputHandler(Replay replay) => new TaikoFramedReplayInputHandler(replay);

        protected override ReplayRecorder CreateReplayRecorder(Score score) => new TaikoReplayRecorder(score);

        protected override ResumeOverlay CreateResumeOverlay() => new DelayedResumeOverlay();
    }
}
