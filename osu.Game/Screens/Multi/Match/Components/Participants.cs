// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Overlays.SearchableList;
using osu.Game.Screens.Multi.Components;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Screens.Multi.Match.Components
{
    public class Participants : CompositeDrawable
    {
        public readonly Bindable<IEnumerable<User>> Users = new Bindable<IEnumerable<User>>();
        public readonly Bindable<int?> MaxParticipants = new Bindable<int?>();

        public new MarginPadding Padding
        {
            get => base.Padding;
            set => base.Padding = value;
        }

        public Participants()
        {
            FillFlowContainer<UserPanel> usersFlow;
            ParticipantCount count;

            InternalChild = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding { Horizontal = SearchableListOverlay.WIDTH_PADDING },
                Children = new Drawable[]
                {
                    new ScrollContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding { Top = 10 },
                        Children = new Drawable[]
                        {
                            count = new ParticipantCount
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

            count.Participants.BindTo(Users);
            count.MaxParticipants.BindTo(MaxParticipants);

            Users.BindValueChanged(v =>
            {
                usersFlow.Children = v.Select(u => new UserPanel(u)
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Width = 300,
                    OnLoadComplete = d => d.FadeInFromZero(60),
                }).ToList();
            });
        }
    }
}
