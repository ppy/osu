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
        public Playfield Playfield => rulesetContainer.Playfield;

        private readonly RulesetContainer rulesetContainer;

        internal EditRulesetContainer(RulesetContainer rulesetContainer)
        {
            this.rulesetContainer = rulesetContainer;

            RelativeSizeAxes = Axes.Both;

            InternalChild = rulesetContainer;
        }

        public abstract DrawableHitObject AddHitObject(HitObject obj);

        public abstract DrawableHitObject RemoveHitObject(HitObject obj);
    }

    public class EditRulesetContainer<T> : EditRulesetContainer
        where T : HitObject
    {
        private readonly Ruleset ruleset;
        private readonly RulesetContainer<T> rulesetContainer;

        private Beatmap<T> beatmap => rulesetContainer.Beatmap;

        public EditRulesetContainer(Ruleset ruleset, RulesetContainer<T> rulesetContainer)
            : base(rulesetContainer)
        {
            this.ruleset = ruleset;
            this.rulesetContainer = rulesetContainer;
        }

        public override DrawableHitObject AddHitObject(HitObject obj)
        {
            var tObj = (T)obj;

            var insertionIndex = beatmap.HitObjects.IndexOf(tObj);
            if (insertionIndex < 0)
                insertionIndex = ~insertionIndex;

            beatmap.HitObjects.Insert(insertionIndex, tObj);

            IBeatmapProcessor processor = ruleset.CreateBeatmapProcessor(beatmap);

            processor.PreProcess();
            obj.ApplyDefaults(beatmap.ControlPointInfo, beatmap.BeatmapInfo.BaseDifficulty);
            processor.PostProcess();

            var drawableObject = rulesetContainer.GetVisualRepresentation(tObj);

            rulesetContainer.Playfield.Add(drawableObject);
            rulesetContainer.Playfield.PostProcess();

            return drawableObject;
        }

        public override DrawableHitObject RemoveHitObject(HitObject obj)
        {
            throw new System.NotImplementedException();
        }
    }
}
