// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Containers;
using osu.Game.Overlays.SearchableList;
using osu.Game.Screens.Multi.Components;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Screens.Multi.Match.Components
{
    public class Participants : MultiplayerComposite
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            FillFlowContainer<UserPanel> usersFlow;

            InternalChild = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding { Horizontal = SearchableListOverlay.WIDTH_PADDING },
                Children = new Drawable[]
                {
                    new OsuScrollContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding { Top = 10 },
                        Children = new Drawable[]
                        {
                            new ParticipantCountDisplay
                            {
                                Anchor = Anchor.TopRight,
                                Origin = Anchor.TopRight,
                            },
                            usersFlow = new FillFlowContainer<UserPanel>
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Spacing = new Vector2(5),
                                Padding = new MarginPadding { Top = 40 },
                                LayoutDuration = 200,
                                LayoutEasing = Easing.OutQuint,
                            },
                        },
                    },
                },
            };

            Participants.BindValueChanged(participants =>
            {
                usersFlow.Children = participants.NewValue.Select(u =>
                {
                    var panel = new UserPanel(u)
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Width = 300,
                    };

                    panel.OnLoadComplete += d => d.FadeInFromZero(60);

                    return panel;
                }).ToList();
            }, true);
        }
    }
}
