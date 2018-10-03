// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Edit
{
    public abstract class EditRulesetContainer : CompositeDrawable
    {
        public Playfield Playfield => RulesetContainer.Playfield;

        protected abstract RulesetContainer RulesetContainer { get; }

        internal EditRulesetContainer()
        {
            RelativeSizeAxes = Axes.Both;
        }

        public abstract void AddHitObject(HitObject hitObject);
    }

    public abstract class EditRulesetContainer<TObject> : EditRulesetContainer
        where TObject : HitObject
    {
        private readonly Ruleset ruleset;

        private readonly RulesetContainer<TObject> rulesetContainer;
        protected override RulesetContainer RulesetContainer => rulesetContainer;

        private Beatmap<TObject> beatmap => rulesetContainer.Beatmap;

        protected EditRulesetContainer(Ruleset ruleset, WorkingBeatmap workingBeatmap)
        {
            this.ruleset = ruleset;

            InternalChild = rulesetContainer = CreateRulesetContainer(ruleset, workingBeatmap);
        }

        public override void AddHitObject(HitObject hitObject)
        {
            var tObject = (TObject)hitObject;

            // Insert into beatmap while maintaining sorting order
            var insertionIndex = beatmap.HitObjects.FindLastIndex(h => h.StartTime <= hitObject.StartTime);
            beatmap.HitObjects.Insert(insertionIndex + 1, tObject);

            var processor = ruleset.CreateBeatmapProcessor(beatmap);

            processor.PreProcess();
            tObject.ApplyDefaults(beatmap.ControlPointInfo, beatmap.BeatmapInfo.BaseDifficulty);
            processor.PostProcess();

            rulesetContainer.Playfield.Add(rulesetContainer.GetVisualRepresentation(tObject));
            rulesetContainer.Playfield.PostProcess();
        }

        /// <summary>
        /// Creates the underlying <see cref="RulesetContainer"/>.
        /// </summary>
        /// <returns></returns>
        protected abstract RulesetContainer<TObject> CreateRulesetContainer(Ruleset ruleset, WorkingBeatmap workingBeatmap);
    }
}
