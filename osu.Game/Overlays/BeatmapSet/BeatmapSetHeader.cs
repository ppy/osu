// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets;

namespace osu.Game.Overlays.BeatmapSet
{
    public class BeatmapSetHeader : OverlayHeader
    {
        public readonly Bindable<RulesetInfo> Ruleset = new Bindable<RulesetInfo>();

        public BeatmapRulesetSelector RulesetSelector { get; private set; }

        protected override ScreenTitle CreateTitle() => new BeatmapHeaderTitle();

        protected override Drawable CreateTitleContent() => RulesetSelector = new BeatmapRulesetSelector
        {
            Current = Ruleset
        };

        private class BeatmapHeaderTitle : ScreenTitle
        {
            public BeatmapHeaderTitle()
            {
                Title = @"beatmap";
                Section = @"info";
            }

            protected override Drawable CreateIcon() => new ScreenTitleTextureIcon(@"Icons/changelog");
        }
    }
}
