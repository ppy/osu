// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Screens.OnlinePlay.Match.Components;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Overlays.Dashboard.CurrentlyOnline
{
    internal partial class OnlineUserGridPanel : OnlineUserPanel
    {
        public OnlineUserGridPanel(APIUser user)
            : base(user)
        {
            AutoSizeAxes = Axes.Y;
            Width = 290;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = new DelayedLoadWrapper(() => new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(2),
                Children = new Drawable[]
                {
                    new UserGridPanel(User)
                    {
                        RelativeSizeAxes = Axes.X,
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre
                    },
                    new PurpleRoundedButton
                    {
                        RelativeSizeAxes = Axes.X,
                        Text = "Spectate",
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Enabled = { BindTarget = CanSpectate },
                        Action = BeginSpectating
                    }
                }
            }, 0)
            {
                // These are approximate metrics - DLW will adopt the content's sizing mode after load.
                RelativeSizeAxes = Axes.X,
                Height = 160
            };
        }
    }
}
