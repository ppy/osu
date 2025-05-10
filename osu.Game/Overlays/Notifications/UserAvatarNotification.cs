// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Users.Drawables;

namespace osu.Game.Overlays.Notifications
{
    public partial class UserAvatarNotification : Notification
    {
        private LocalisableString text;

        public override LocalisableString Text
        {
            get => text;
            set
            {
                text = value;
                if (textDrawable != null)
                    textDrawable.Text = text;
            }
        }

        private TextFlowContainer? textDrawable;

        private readonly APIUser user;

        public UserAvatarNotification(APIUser user, LocalisableString text)
        {
            this.user = user;
            Text = text;
        }

        protected override IconUsage CloseButtonIcon => FontAwesome.Solid.Times;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, OverlayColourProvider colourProvider)
        {
            Light.Colour = colours.Orange2;

            Content.Add(textDrawable = new OsuTextFlowContainer(t => t.Font = t.Font.With(size: 14, weight: FontWeight.Medium))
            {
                AutoSizeAxes = Axes.Y,
                RelativeSizeAxes = Axes.X,
                Text = text
            });

            IconContent.Masking = true;
            IconContent.CornerRadius = CORNER_RADIUS;

            IconContent.AddRange(new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background5,
                },
            });

            LoadComponentAsync(new DrawableAvatar(user)
            {
                FillMode = FillMode.Fill,
            }, IconContent.Add);
        }
    }
}
