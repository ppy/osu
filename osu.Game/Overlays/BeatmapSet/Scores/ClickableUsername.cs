// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Users;
using System.Collections.Generic;

namespace osu.Game.Overlays.BeatmapSet.Scores
{
    public class ClickableUsername : OsuHoverContainer
    {
        private readonly SpriteText text;
        protected override IEnumerable<Drawable> EffectTargets => new[] { text };

        private UserProfileOverlay profile;

        private User user;
        public User User
        {
            get { return user; }
            set
            {
                if (user == value) return;
                user = value;

                OnUserUpdate(user);
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
            Child = text = new SpriteText
            {
                Font = @"Exo2.0-BoldItalic",
            };
        }

        protected virtual void OnUserUpdate(User user)
        {
            text.Text = user.Username;
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
