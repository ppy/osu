// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Chat;

namespace osu.Game.Overlays.Team.Sections.Info
{
    public partial class LeaderValueDisplay : CompositeDrawable
    {
        private readonly OsuSpriteText title;
        private readonly Container container;

        private APIUser? user;

        public LocalisableString Title
        {
            set => title.Text = value;
        }

        public APIUser? User
        {
            get => user;
            set
            {
                if (user == value)
                    return;

                user = value;
                Scheduler.AddOnce(updateLink);
            }
        }

        public LeaderValueDisplay()
        {
            AutoSizeAxes = Axes.Both;
            InternalChild = new FillFlowContainer
            {
                Direction = FillDirection.Vertical,
                AutoSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    title = new OsuSpriteText
                    {
                        Font = OsuFont.GetFont(size: 12),
                    },
                    container = new Container
                    {
                        AutoSizeAxes = Axes.Both,
                    },
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            title.Colour = colourProvider.Content1;
        }

        private void updateLink()
        {
            container.Child = new LinkFlowContainer(sprite => sprite.Font = OsuFont.GetFont(size: 12, weight: FontWeight.Light))
                .With(container =>
                {
                    if (user == null)
                        return;

                    container.AddLink(user.Username, LinkAction.OpenUserProfile, user);
                });
        }
    }
}
