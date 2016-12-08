//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Configuration;
using osu.Game.Graphics;

namespace osu.Game.Overlays.Options
{
    public class EditorSection : OptionsSection
    {
        public override string Header => "Editor";
        public override FontAwesome Icon => FontAwesome.fa_pencil;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            content.Spacing = new Vector2(0, 5);
            Children = new Drawable[]
            {
                new CheckBoxOption
                {
                    LabelText = "Background video",
                    Bindable = config.GetBindable<bool>(OsuConfig.VideoEditor)
                },
                new CheckBoxOption
                {
                    LabelText = "Always use default skin",
                    Bindable = config.GetBindable<bool>(OsuConfig.EditorDefaultSkin)
                },
                new CheckBoxOption
                {
                    LabelText = "Snaking sliders",
                    Bindable = config.GetBindable<bool>(OsuConfig.EditorSnakingSliders)
                },
                new CheckBoxOption
                {
                    LabelText = "Hit animations",
                    Bindable = config.GetBindable<bool>(OsuConfig.EditorHitAnimations)
                },
                new CheckBoxOption
                {
                    LabelText = "Follow points",
                    Bindable = config.GetBindable<bool>(OsuConfig.EditorFollowPoints)
                },
                new CheckBoxOption
                {
                    LabelText = "Stacking",
                    Bindable = config.GetBindable<bool>(OsuConfig.EditorStacking)
                },
            };
        }
    }
}

