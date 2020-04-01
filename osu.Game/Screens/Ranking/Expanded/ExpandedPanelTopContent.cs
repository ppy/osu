// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Users;
using osu.Game.Users.Drawables;
using osuTK;

namespace osu.Game.Screens.Ranking.Expanded
{
    /// <summary>
    /// The content that appears in the middle section of the <see cref="ScorePanel"/>.
    /// </summary>
    public class ExpandedPanelTopContent : CompositeDrawable
    {
        private readonly User user;

        /// <summary>
        /// Creates a new <see cref="ExpandedPanelTopContent"/>.
        /// </summary>
        /// <param name="user">The <see cref="User"/> to display.</param>
        public ExpandedPanelTopContent(User user)
        {
            this.user = user;
            Anchor = Anchor.TopCentre;
            Origin = Anchor.Centre;

            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Children = new Drawable[]
                {
                    new UpdateableAvatar(user)
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Size = new Vector2(80),
                        CornerRadius = 20,
                        CornerExponent = 2.5f,
                        Masking = true,
                    },
                    new OsuSpriteText
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Text = user.Username,
                        Font = OsuFont.Torus.With(size: 16, weight: FontWeight.SemiBold)
                    }
                }
            };
        }
    }
}
