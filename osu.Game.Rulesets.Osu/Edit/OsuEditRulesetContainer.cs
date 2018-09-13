// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics.Cursor;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.UI;
using OpenTK;

namespace osu.Game.Rulesets.Osu.Edit
{
    public class OsuEditRulesetContainer : OsuRulesetContainer, IEditRulesetContainer
    {
        public OsuEditRulesetContainer(Ruleset ruleset, WorkingBeatmap beatmap)
            : base(ruleset, beatmap)
        {
        }

        protected override Vector2 PlayfieldArea => Vector2.One;

        protected override CursorContainer CreateCursor() => null;

        public void AddObject(HitObject obj)
        {
            var osuObject = (OsuHitObject)obj;

            var insertionIndex = Beatmap.HitObjects.IndexOf(osuObject);
            if (insertionIndex < 0)
                insertionIndex = ~insertionIndex;

            Beatmap.HitObjects.Insert(insertionIndex, osuObject);

            IBeatmapProcessor processor = new OsuBeatmapProcessor(Beatmap);

            processor.PreProcess();
            obj.ApplyDefaults(Beatmap.ControlPointInfo, Beatmap.BeatmapInfo.BaseDifficulty);
            processor.PostProcess();

            var drawableObject = GetVisualRepresentation(osuObject);

            Playfield.Add(drawableObject);
            Playfield.PostProcess();
        }
    }
}
