// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
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
        /// <summary>
        /// The <see cref="Playfield"/> contained by this <see cref="EditRulesetContainer"/>.
        /// </summary>
        public abstract Playfield Playfield { get; }

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

        /// <summary>
        /// Removes a <see cref="HitObject"/> from the <see cref="Beatmap"/> and the display.
        /// </summary>
        /// <param name="hitObject">The <see cref="HitObject"/> to remove.</param>
        /// <returns>The visual representation of the removed <paramref name="hitObject"/>.</returns>
        internal abstract DrawableHitObject Remove(HitObject hitObject);
    }

    public class EditRulesetContainer<TObject> : EditRulesetContainer
        where TObject : HitObject
    {
        public override Playfield Playfield => rulesetContainer.Playfield;

        private Ruleset ruleset => rulesetContainer.Ruleset;
        private Beatmap<TObject> beatmap => rulesetContainer.Beatmap;

        private readonly RulesetContainer<TObject> rulesetContainer;

        public EditRulesetContainer(RulesetContainer<TObject> rulesetContainer)
        {
            this.rulesetContainer = rulesetContainer;

            InternalChild = rulesetContainer;

            Playfield.DisplayJudgements.Value = false;
        }

        internal override DrawableHitObject Add(HitObject hitObject)
        {
            var tObject = (TObject)hitObject;

            // Add to beatmap, preserving sorting order
            var insertionIndex = beatmap.HitObjects.FindLastIndex(h => h.StartTime <= hitObject.StartTime);
            beatmap.HitObjects.Insert(insertionIndex + 1, tObject);

            // Process object
            var processor = ruleset.CreateBeatmapProcessor(beatmap);

            processor?.PreProcess();
            tObject.ApplyDefaults(beatmap.ControlPointInfo, beatmap.BeatmapInfo.BaseDifficulty);
            processor?.PostProcess();

            // Add visual representation
            var drawableObject = rulesetContainer.GetVisualRepresentation(tObject);

            rulesetContainer.Playfield.Add(drawableObject);
            rulesetContainer.Playfield.PostProcess();

            return drawableObject;
        }

        internal override DrawableHitObject Remove(HitObject hitObject)
        {
            var tObject = (TObject)hitObject;

            // Remove from beatmap
            beatmap.HitObjects.Remove(tObject);

            // Process the beatmap
            var processor = ruleset.CreateBeatmapProcessor(beatmap);

            processor?.PreProcess();
            processor?.PostProcess();

            // Remove visual representation
            var drawableObject = Playfield.AllHitObjects.Single(d => d.HitObject == hitObject);

            rulesetContainer.Playfield.Remove(drawableObject);
            rulesetContainer.Playfield.PostProcess();

            return drawableObject;
        }
    }
}
