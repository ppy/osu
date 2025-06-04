// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Graphics;
using osuTK.Graphics;
using osuTK;
using osu.Framework.Input.Events;
using osu.Game.Overlays;
using osu.Game.Screens.OnlinePlay.Multiplayer.Participants;

namespace osu.Game.Screens.OnlinePlay.Multiplayer
{
    public partial class MultiplayerParticipantsSortTabControl : OverlaySortTabControl<ParticipantsSortMode>
    {
        public readonly Bindable<SortDirection> SortDirection = new Bindable<SortDirection>(Overlays.SortDirection.Descending);

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Current.BindValueChanged(_ => SortDirection.Value = Overlays.SortDirection.Descending);
        }

        protected override SortTabControl CreateControl() => new ParticipantsSortTabControlInternal
        {
            SortDirection = { BindTarget = SortDirection },
        };

        private partial class ParticipantsSortTabControlInternal : SortTabControl
        {
            protected override bool AddEnumEntriesAutomatically => true;

            public readonly Bindable<SortDirection> SortDirection = new Bindable<SortDirection>();

            protected override TabItem<ParticipantsSortMode> CreateTabItem(ParticipantsSortMode value) => new ParticipantsSortTabItem(value)
            {
                SortDirection = { BindTarget = SortDirection }
            };
        }

        private partial class ParticipantsSortTabItem : SortTabItem
        {
            public readonly Bindable<SortDirection> SortDirection = new Bindable<SortDirection>();

            public ParticipantsSortTabItem(ParticipantsSortMode value)
                : base(value)
            {
            }

            protected override TabButton CreateTabButton(ParticipantsSortMode value) => new ParticipantsTabButton(value)
            {
                Active = { BindTarget = Active },
                SortDirection = { BindTarget = SortDirection }
            };
        }

        public partial class ParticipantsTabButton : TabButton
        {
            public readonly Bindable<SortDirection> SortDirection = new Bindable<SortDirection>();

            protected override Color4 ContentColour
            {
                set
                {
                    base.ContentColour = value;
                    icon.Colour = value;
                }
            }

            private readonly SpriteIcon icon;

            public ParticipantsTabButton(ParticipantsSortMode value)
                : base(value)
            {
                Add(icon = new SpriteIcon
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    AlwaysPresent = true,
                    Alpha = 0,
                    Size = new Vector2(6),
                    Icon = FontAwesome.Solid.CaretDown,
                });
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                SortDirection.BindValueChanged(direction =>
                {
                    icon.ScaleTo(direction.NewValue == Overlays.SortDirection.Ascending && Active.Value ? new Vector2(1f, -1f) : Vector2.One, 300, Easing.OutQuint);
                }, true);
            }

            protected override void UpdateState()
            {
                base.UpdateState();
                icon.FadeTo(Active.Value || IsHovered ? 1 : 0, 200, Easing.OutQuint);
            }

            protected override bool OnClick(ClickEvent e)
            {
                if (Active.Value)
                    SortDirection.Value = SortDirection.Value == Overlays.SortDirection.Ascending ? Overlays.SortDirection.Descending : Overlays.SortDirection.Ascending;

                return base.OnClick(e);
            }
        }
    }
}
