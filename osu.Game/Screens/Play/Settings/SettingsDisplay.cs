// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Screens.Play.Settings
{
    public class SettingsDisplay : FillFlowContainer
    {
        private bool isVisible;
        public bool IsVisible
        {
            set
            {
                isVisible = value;

                if (isVisible)
                    Show();
                else
                    Hide();
            }
            get { return isVisible; }
        }

        public SettingsDisplay()
        {
            Direction = FillDirection.Vertical;
            Spacing = new Vector2(0, 20);

            Add(new CollectionSettings());
            Add(new DiscussionSettings());
            Add(new PlaybackSettings());
        }
    }
}
