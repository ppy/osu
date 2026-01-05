// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Screens.OnlinePlay.Match.Components;
using osu.Game.Users;

namespace osu.Game.Overlays.Dashboard.CurrentlyOnline
{
    public partial class OnlineUserListPanel : OnlineUserPanel
    {
        public OnlineUserListPanel(APIUser user)
            : base(user)
        {
            RelativeSizeAxes = Axes.X;
            Height = 40;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = new DelayedLoadUnloadWrapper(() => new GridContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                ColumnDimensions = new[]
                {
                    new Dimension(),
                    new Dimension(GridSizeMode.AutoSize)
                },
                RowDimensions = new[]
                {
                    new Dimension(GridSizeMode.AutoSize)
                },
                Content = new[]
                {
                    new Drawable[]
                    {
                        new UserListPanel(User),
                        new PurpleRoundedButton
                        {
                            Width = 100,
                            Text = "Spectate",
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Enabled = { BindTarget = CanSpectate },
                            Action = BeginSpectating
                        }
                    }
                }
            }, 40, 5000)
            {
                RelativeSizeAxes = Axes.Both,
            };
        }
    }
}
