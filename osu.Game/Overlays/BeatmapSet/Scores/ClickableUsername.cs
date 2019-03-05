// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Users;

namespace osu.Game.Overlays.BeatmapSet.Scores
{
    public class ClickableUsername : OsuHoverContainer
    {
        private readonly OsuSpriteText text;
        private UserProfileOverlay profile;

        private User user;

        public User User
        {
            get => user;
            set
            {
                if (user == value) return;

                user = value;

                text.Text = user.Username;
            }
        }

        public float TextSize
        {
            get => text.Font.Size;
            set => text.Font = text.Font.With(size: value);
        }

        public ClickableUsername()
        {
            AutoSizeAxes = Axes.Both;
            Child = text = new OsuSpriteText { Font = OsuFont.GetFont(weight: FontWeight.Bold, italics: true) };
        }

        [BackgroundDependencyLoader(true)]
        private void load(UserProfileOverlay profile)
        {
            this.profile = profile;
        }

        protected override bool OnClick(ClickEvent e)
        {
            profile?.ShowUser(user);
            return true;
        }
    }
}
