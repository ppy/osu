// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Cursor;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Edit.Screens.Compose;

namespace osu.Game.Rulesets.Mania.Edit
{
    public class ManiaEditRulesetContainer : ManiaRulesetContainer
    {
        public BindableBeatDivisor BeatDivisor;

        public ManiaEditRulesetContainer(Ruleset ruleset, WorkingBeatmap beatmap, BindableBeatDivisor beatDivisor)
            : base(ruleset, beatmap)
        {
            BeatDivisor = beatDivisor;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            BeatDivisor.ValueChanged += OnBeatSnapDivisorChange;
            OnBeatSnapDivisorChange(BeatDivisor.Value);
        }

        public void OnBeatSnapDivisorChange(int newDivisor)
        {
        }

        protected override Playfield CreatePlayfield() => new ManiaEditPlayfield(Beatmap.Stages);

        protected override Vector2 PlayfieldArea => Vector2.One;

        protected override CursorContainer CreateCursor() => null;
    }
}
