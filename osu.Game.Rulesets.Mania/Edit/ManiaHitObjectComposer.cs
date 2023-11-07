// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;

namespace osu.Game.Rulesets.Mania.Edit
{
    public partial class ManiaHitObjectComposer : ScrollingHitObjectComposer<ManiaHitObject>
    {
        private DrawableManiaEditorRuleset drawableRuleset;

        public ManiaHitObjectComposer(Ruleset ruleset)
            : base(ruleset)
        {
        }

        public new ManiaPlayfield Playfield => ((ManiaPlayfield)drawableRuleset.Playfield);

        public IScrollingInfo ScrollingInfo => drawableRuleset.ScrollingInfo;

        protected override Playfield PlayfieldAtScreenSpacePosition(Vector2 screenSpacePosition) =>
            Playfield.GetColumnByPosition(screenSpacePosition);

        protected override DrawableRuleset<ManiaHitObject> CreateDrawableRuleset(Ruleset ruleset, IBeatmap beatmap, IReadOnlyList<Mod> mods) =>
            drawableRuleset = new DrawableManiaEditorRuleset(ruleset, beatmap, mods);

        protected override ComposeBlueprintContainer CreateBlueprintContainer()
            => new ManiaBlueprintContainer(this);

        protected override BeatSnapGrid CreateBeatSnapGrid() => new ManiaBeatSnapGrid();

        protected override IReadOnlyList<HitObjectCompositionTool> CompositionTools => new HitObjectCompositionTool[]
        {
            new NoteCompositionTool(),
            new HoldNoteCompositionTool()
        };

        public override string ConvertSelectionToString()
            => string.Join(ObjectSeparator, EditorBeatmap.SelectedHitObjects.Cast<ManiaHitObject>().OrderBy(h => h.StartTime).Select(h => $"{h.StartTime}|{h.Column}"));

        public override bool HandleHitObjectSelection(HitObject hitObject, string objectInfo)
        {
            if (hitObject is not ManiaHitObject maniaHitObject)
                return false;

            double[] split = objectInfo.Split('|').Select(double.Parse).ToArray();
            if (split.Length != 2)
                return false;

            double timeValue = split[0];
            double columnValue = split[1];
            return Math.Abs(maniaHitObject.StartTime - timeValue) < 0.5
                   && Math.Abs(maniaHitObject.Column - columnValue) < 0.5;
        }
    }
}
