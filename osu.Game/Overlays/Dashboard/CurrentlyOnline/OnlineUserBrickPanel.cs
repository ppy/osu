// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Overlays.Dashboard.CurrentlyOnline
{
    public class OnlineUserBrickPanel : OnlineUserPanel
    {
        public OnlineUserBrickPanel(APIUser user)
            : base(user)
        {
            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = new DelayedLoadWrapper(() => new BrickPanelWithSpectateButton(User)
            {
                CanSpectate = { BindTarget = CanSpectate },
                OnSpectate = BeginSpectating
            }, 0)
            {
                // These are approximate metrics - DLW will adopt the content's sizing mode after load.
                Size = new Vector2(50, 20)
            };
        }

        private partial class BrickPanelWithSpectateButton : UserBrickPanel
        {
            public readonly IBindable<bool> CanSpectate = new Bindable<bool>();
            public required Action OnSpectate { get; init; }

            private IconButton icon = null!;

            public BrickPanelWithSpectateButton(APIUser user)
                : base(user)
            {
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                icon.IconColour = colours.Yellow;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                CanSpectate.BindValueChanged(e =>
                {
                    if (e.NewValue)
                        icon.FadeIn(50);
                    else
                        icon.FadeOut(50);
                }, true);
            }

            protected override Drawable CreateLayout()
            {
                var flow = (FillFlowContainer)base.CreateLayout();

                flow.Add(icon = new IconButton
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Icon = FontAwesome.Solid.Eye,
                    Size = new Vector2(20, 13),
                    Alpha = 0,
                    AlwaysPresent = true,
                    Enabled = { BindTarget = CanSpectate },
                    Action = OnSpectate
                });

                return flow;
            }
        }
    }
}
