// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Input;
using osu.Game.Beatmaps;
using osu.Game.Input.Handlers;
using osu.Game.Replays;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Configuration;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.Mania.Replays;
using osu.Game.Rulesets.Mania.Scoring;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.UI.Scrolling;
using osuTK;

namespace osu.Game.Rulesets.Mania.UI
{
    public class DrawableManiaRuleset : DrawableScrollingRuleset<ManiaHitObject>
    {
        protected new ManiaPlayfield Playfield => (ManiaPlayfield)base.Playfield;

        public new ManiaBeatmap Beatmap => (ManiaBeatmap)base.Beatmap;

        public IEnumerable<BarLine> BarLines;

        protected override bool RelativeScaleBeatLengths => true;

        protected new ManiaRulesetConfigManager Config => (ManiaRulesetConfigManager)base.Config;

        private readonly Bindable<ManiaScrollingDirection> configDirection = new Bindable<ManiaScrollingDirection>();

        public DrawableManiaRuleset(Ruleset ruleset, IWorkingBeatmap beatmap, IReadOnlyList<Mod> mods)
            : base(ruleset, beatmap, mods)
        {
            BarLines = new BarLineGenerator<BarLine>(Beatmap).BarLines;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            BarLines.ForEach(Playfield.Add);

            Config.BindWith(ManiaRulesetSetting.ScrollDirection, configDirection);
            configDirection.BindValueChanged(direction => Direction.Value = (ScrollingDirection)direction.NewValue, true);

            Config.BindWith(ManiaRulesetSetting.ScrollTime, TimeRange);
        }

        /// <summary>
        /// Retrieves the column that intersects a screen-space position.
        /// </summary>
        /// <param name="screenSpacePosition">The screen-space position.</param>
        /// <returns>The column which intersects with <paramref name="screenSpacePosition"/>.</returns>
        public Column GetColumnByPosition(Vector2 screenSpacePosition) => Playfield.GetColumnByPosition(screenSpacePosition);

        public override PlayfieldAdjustmentContainer CreatePlayfieldAdjustmentContainer() => new ManiaPlayfieldAdjustmentContainer();

        protected override Playfield CreatePlayfield() => new ManiaPlayfield(Beatmap.Stages);

        public override ScoreProcessor CreateScoreProcessor() => new ManiaScoreProcessor(this);

        public override int Variant => (int)(Beatmap.Stages.Count == 1 ? PlayfieldType.Single : PlayfieldType.Dual) + Beatmap.TotalColumns;

        protected override PassThroughInputManager CreateInputManager() => new ManiaInputManager(Ruleset.RulesetInfo, Variant);

        public override DrawableHitObject<ManiaHitObject> CreateDrawableRepresentation(ManiaHitObject h)
        {
            switch (h)
            {
                case HoldNote holdNote:
                    return new DrawableHoldNote(holdNote);

                case Note note:
                    return new DrawableNote(note);

                default:
                    return null;
            }
        }

        protected override ReplayInputHandler CreateReplayInputHandler(Replay replay) => new ManiaFramedReplayInputHandler(replay);
    }
}
