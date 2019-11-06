// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Edit.Blueprints.HitCircles;
using osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders;
using osu.Game.Rulesets.Osu.Edit.Blueprints.Spinners;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Edit.Compose.Components;

namespace osu.Game.Rulesets.Osu.Edit
{
    public class OsuHitObjectComposer : HitObjectComposer<OsuHitObject>
    {
        public OsuHitObjectComposer(Ruleset ruleset)
            : base(ruleset)
        {
        }

        protected override DrawableRuleset<OsuHitObject> CreateDrawableRuleset(Ruleset ruleset, IWorkingBeatmap beatmap, IReadOnlyList<Mod> mods)
            => new DrawableOsuEditRuleset(ruleset, beatmap, mods);

        protected override IReadOnlyList<HitObjectCompositionTool> CompositionTools => new HitObjectCompositionTool[]
        {
            new HitCircleCompositionTool(),
            new SliderCompositionTool(),
            new SpinnerCompositionTool()
        };

        public override SelectionHandler CreateSelectionHandler() => new OsuSelectionHandler();

        public override SelectionBlueprint CreateBlueprintFor(DrawableHitObject hitObject)
        {
            switch (hitObject)
            {
                case DrawableHitCircle circle:
                    return new HitCircleSelectionBlueprint(circle);

                case DrawableSlider slider:
                    return new SliderSelectionBlueprint(slider);

                case DrawableSpinner spinner:
                    return new SpinnerSelectionBlueprint(spinner);
            }

            return base.CreateBlueprintFor(hitObject);
        }

        protected override DistanceSnapGrid CreateDistanceSnapGrid(IEnumerable<HitObject> selectedHitObjects)
        {
            var objects = selectedHitObjects.ToList();

            if (objects.Count == 0)
                return createGrid(h => h.StartTime <= EditorClock.CurrentTime);

            double minTime = objects.Min(h => h.StartTime);
            return createGrid(h => h.StartTime < minTime);
        }

        private OsuDistanceSnapGrid createGrid(Func<HitObject, bool> hitObjectSelector)
        {
            int lastIndex = -1;

            for (int i = 0; i < EditorBeatmap.HitObjects.Count; i++)
            {
                HitObject hitObject = EditorBeatmap.HitObjects[i];

                if (!hitObjectSelector(hitObject))
                    break;

                lastIndex = i;
            }

            if (lastIndex == -1)
                return null;

            OsuHitObject lastObject = EditorBeatmap.HitObjects[lastIndex];
            OsuHitObject nextObject = lastIndex == EditorBeatmap.HitObjects.Count - 1 ? null : EditorBeatmap.HitObjects[lastIndex + 1];

            return new OsuDistanceSnapGrid(lastObject, nextObject);
        }
    }
}
