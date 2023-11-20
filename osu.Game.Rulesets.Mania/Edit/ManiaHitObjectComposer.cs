// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.Mods;
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

        // 123|0,456|1,789|2 ...
        private static readonly Regex selection_regex = new Regex(@"^\d+\|\d+(,\d+\|\d+)*$");

        public override string ConvertSelectionToString()
            => string.Join(',', EditorBeatmap.SelectedHitObjects.Cast<ManiaHitObject>().OrderBy(h => h.StartTime).Select(h => $"{h.StartTime}|{h.Column}"));

        public override void SelectFromTimestamp(double timestamp, string objectDescription)
        {
            if (!selection_regex.IsMatch(objectDescription))
                return;

            List<ManiaHitObject> remainingHitObjects = EditorBeatmap.HitObjects.Cast<ManiaHitObject>().Where(h => h.StartTime >= timestamp).ToList();
            string[] splitDescription = objectDescription.Split(',').ToArray();

            for (int i = 0; i < splitDescription.Length; i++)
            {
                string[] split = splitDescription[i].Split('|').ToArray();
                if (split.Length != 2)
                    continue;

                if (!double.TryParse(split[0], out double time) || !int.TryParse(split[1], out int column))
                    continue;

                ManiaHitObject current = remainingHitObjects.FirstOrDefault(h => h.StartTime == time && h.Column == column);

                if (current == null)
                    continue;

                EditorBeatmap.SelectedHitObjects.Add(current);

                if (i < splitDescription.Length - 1)
                    remainingHitObjects = remainingHitObjects.Where(h => h != current && h.StartTime >= current.StartTime).ToList();
            }
        }
    }
}
