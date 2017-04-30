// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using OpenTK;

namespace osu.Game.Overlays.Options.Sections
{
    public class EditorSection : OptionsSection
    {
        public override string Header => "Editor";
        public override FontAwesome Icon => FontAwesome.fa_pencil;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            FlowContent.Spacing = new Vector2(0, 5);
            Children = new Drawable[]
            {
                new OsuCheckbox
                {
                    LabelText = "Background video",
                    Bindable = config.GetBindable<bool>(OsuConfig.VideoEditor),
                    TooltipText = "Show background video in the editor."
                },
                new OsuCheckbox
                {
                    LabelText = "Always use default skin",
                    Bindable = config.GetBindable<bool>(OsuConfig.EditorDefaultSkin),
                    TooltipText = "Override any custom skin while editing. The default skin is recommended when editing to check overlaps etc."
                },
                new OsuCheckbox
                {
                    LabelText = "Snaking sliders",
                    Bindable = config.GetBindable<bool>(OsuConfig.EditorSnakingSliders),
                    TooltipText = "Sliders gradually snake out (and in?) from their starting point in the editor."
                },
                new OsuCheckbox
                {
                    LabelText = "Hit animations",
                    Bindable = config.GetBindable<bool>(OsuConfig.EditorHitAnimations),
                    TooltipText = "Hitobjects appear hit instead of fading out."
                },
                new OsuCheckbox
                {
                    LabelText = "Follow points",
                    Bindable = config.GetBindable<bool>(OsuConfig.EditorFollowPoints),
                    TooltipText = "Display follow points in the editor."
                },
                new OsuCheckbox
                {
                    LabelText = "Stacking",
                    Bindable = config.GetBindable<bool>(OsuConfig.EditorStacking),
                    TooltipText = "Display hitobjects stacked in the editor."
                },
            };
        }
    }
}

