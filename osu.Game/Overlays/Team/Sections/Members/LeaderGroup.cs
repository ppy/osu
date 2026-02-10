// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Overlays.Team.Sections.Members
{
    public partial class LeaderGroup : CompositeDrawable
    {
        public APIUser? User
        {
            set => cardContainer.Child = value != null ? new UserGridPanel(value) { RelativeSizeAxes = Axes.X } : Empty();
        }

        private Container cardContainer = null!;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Masking = true;
            CornerRadius = 10;
            InternalChildren = new Drawable[]
            {
                new Box
                {
                    Colour = colours.Orange1,
                    RelativeSizeAxes = Axes.Both,
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Padding = new MarginPadding(5),
                    Spacing = new Vector2(0, 5),
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Font = OsuFont.GetFont(size: 14, weight: FontWeight.SemiBold),
                            Colour = colours.Orange4,
                            Shadow = false,
                            Text = TeamsStrings.ShowMembersOwner,
                            Padding = new MarginPadding { Left = 10 },
                        },
                        cardContainer = new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                        },
                    },
                },
            };
        }
    }
}
