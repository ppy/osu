// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Users;
using osu.Framework.Input;

namespace osu.Game.Overlays.BeatmapSet.Scores
{
    public class ClickableUsername : OsuHoverContainer
    {
        private readonly OsuSpriteText text;
        private UserProfileOverlay profile;

        private User user;
        public User User
        {
            get { return user; }
            set
            {
                if (user == value) return;
                user = value;

                text.Text = user.Username;
            }
        }

        public float TextSize
        {
            set
            {
                if (text.TextSize == value) return;
                text.TextSize = value;
            }
            get { return text.TextSize; }
        }

        public ClickableUsername()
        {
            AutoSizeAxes = Axes.Both;
            Child = text = new OsuSpriteText
            {
                Font = @"Exo2.0-BoldItalic",
            };
        }

        [BackgroundDependencyLoader(true)]
        private void load(UserProfileOverlay profile)
        {
            this.profile = profile;
        }

        protected override bool OnClick(InputState state)
        {
            profile?.ShowUser(user);
            return true;
        }
    }
}
