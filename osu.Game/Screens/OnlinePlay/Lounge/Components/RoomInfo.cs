// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Screens.OnlinePlay.Components;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Lounge.Components
{
    public class RoomInfo : OnlinePlayComposite
    {
        private readonly List<Drawable> statusElements = new List<Drawable>();
        private readonly OsuTextFlowContainer roomName;

        public RoomInfo()
        {
            AutoSizeAxes = Axes.Y;

            RoomStatusInfo statusInfo;
            ModeTypeInfo typeInfo;
            ParticipantInfo participantInfo;

            InternalChild = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 4),
                Children = new Drawable[]
                {
                    new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Children = new Drawable[]
                        {
                            new FillFlowContainer
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Vertical,
                                Children = new Drawable[]
                                {
                                    roomName = new OsuTextFlowContainer(t => t.Font = OsuFont.GetFont(size: 30))
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                    },
                                    statusInfo = new RoomStatusInfo(),
                                }
                            },
                            typeInfo = new ModeTypeInfo
                            {
                                Anchor = Anchor.BottomRight,
                                Origin = Anchor.BottomRight
                            }
                        }
                    },
                    participantInfo = new ParticipantInfo(),
                }
            };

            statusElements.AddRange(new Drawable[] { statusInfo, typeInfo, participantInfo });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (RoomID.Value == null)
                statusElements.ForEach(e => e.FadeOut());

            RoomID.BindValueChanged(id =>
            {
                if (id.NewValue == null)
                    statusElements.ForEach(e => e.FadeOut(100));
                else
                    statusElements.ForEach(e => e.FadeIn(100));
            }, true);

            RoomName.BindValueChanged(name =>
            {
                roomName.Text = name.NewValue ?? "No room selected";
            }, true);
        }
    }
}
