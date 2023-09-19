// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Screens.Edit.Setup;

namespace osu.Game.Rulesets.Mania.Edit.Setup
{
    public partial class ManiaSetupSection : RulesetSetupSection
    {
        private LabelledSwitchButton specialStyle;

        public ManiaSetupSection()
            : base(new ManiaRuleset().RulesetInfo)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                specialStyle = new LabelledSwitchButton
                {
                    Label = "Use special (N+1) style",
                    Description = "Changes one column to act as a classic \"scratch\" or \"special\" column, which can be moved around by the user's skin (to the left/right/centre). Generally used in 6K (5+1) or 8K (7+1) configurations.",
                    Current = { Value = Beatmap.BeatmapInfo.SpecialStyle }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            specialStyle.Current.BindValueChanged(_ => updateBeatmap());
        }

        private void updateBeatmap()
        {
            Beatmap.BeatmapInfo.SpecialStyle = specialStyle.Current.Value;
            Beatmap.SaveState();
        }
    }
}
