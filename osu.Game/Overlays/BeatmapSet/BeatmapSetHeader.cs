// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets;

namespace osu.Game.Overlays.BeatmapSet
{
    public class BeatmapSetHeader : OverlayHeader
    {
        public readonly Bindable<BeatmapSetInfo> BeatmapSet = new Bindable<BeatmapSetInfo>();
        public readonly Bindable<RulesetInfo> Ruleset = new Bindable<RulesetInfo>();

        public BeatmapRulesetSelector RulesetSelector;
        public BeatmapHeaderContent HeaderContent;

        public BeatmapSetHeader()
            : base(OverlayColourScheme.Blue)
        {
            BackgroundHeight = 0;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            BeatmapSet.BindValueChanged(beatmapSet => RulesetSelector.BeatmapSet = beatmapSet.NewValue, true);
        }

        protected override ScreenTitle CreateTitle() => new BeatmapSetTitle();

        protected override Drawable CreateTitleContent() => RulesetSelector = new BeatmapRulesetSelector(ColourScheme)
        {
            Current = Ruleset
        };

        protected override Drawable CreateContent() => HeaderContent = new BeatmapHeaderContent
        {
            BeatmapSet = { BindTarget = BeatmapSet },
            Ruleset = { BindTarget = Ruleset }
        };

        private class BeatmapSetTitle : ScreenTitle
        {
            public BeatmapSetTitle()
            {
                Title = "beatmap";
                Section = "info";
            }

            protected override Drawable CreateIcon() => new ScreenTitleTextureIcon(@"Icons/changelog");
        }
    }
}
