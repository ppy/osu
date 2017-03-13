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
                    Bindable = config.GetBindable<bool>(OsuConfig.VideoEditor)
                },
                new OsuCheckbox
                {
                    LabelText = "Always use default skin",
                    Bindable = config.GetBindable<bool>(OsuConfig.EditorDefaultSkin)
                },
                new OsuCheckbox
                {
                    LabelText = "Snaking sliders",
                    Bindable = config.GetBindable<bool>(OsuConfig.EditorSnakingSliders)
                },
                new OsuCheckbox
                {
                    LabelText = "Hit animations",
                    Bindable = config.GetBindable<bool>(OsuConfig.EditorHitAnimations)
                },
                new OsuCheckbox
                {
                    LabelText = "Follow points",
                    Bindable = config.GetBindable<bool>(OsuConfig.EditorFollowPoints)
                },
                new OsuCheckbox
                {
                    LabelText = "Stacking",
                    Bindable = config.GetBindable<bool>(OsuConfig.EditorStacking)
                },
            };
        }
    }
}

