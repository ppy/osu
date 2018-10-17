// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
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

        /// <summary>
        /// Adds a <see cref="HitObject"/> to the <see cref="Beatmap"/> and displays a visual representation of it.
        /// </summary>
        /// <param name="hitObject">The <see cref="HitObject"/> to add.</param>
        /// <returns>The visual representation of <paramref name="hitObject"/>.</returns>
        internal abstract DrawableHitObject Add(HitObject hitObject);
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

        internal override DrawableHitObject Add(HitObject hitObject)
        {
            var tObject = (TObject)hitObject;

            // Add to beatmap, preserving sorting order
            var insertionIndex = beatmap.HitObjects.FindLastIndex(h => h.StartTime <= hitObject.StartTime);
            beatmap.HitObjects.Insert(insertionIndex + 1, tObject);

            // Process object
            var processor = ruleset.CreateBeatmapProcessor(beatmap);

            processor.PreProcess();
            tObject.ApplyDefaults(beatmap.ControlPointInfo, beatmap.BeatmapInfo.BaseDifficulty);
            processor.PostProcess();

            // Add visual representation
            var drawableObject = rulesetContainer.GetVisualRepresentation(tObject);

            rulesetContainer.Playfield.Add(drawableObject);
            rulesetContainer.Playfield.PostProcess();

            return drawableObject;
        }

        /// <summary>
        /// Creates the underlying <see cref="RulesetContainer"/>.
        /// </summary>
        /// <returns></returns>
        protected abstract RulesetContainer<TObject> CreateRulesetContainer(Ruleset ruleset, WorkingBeatmap workingBeatmap);
    }
}
