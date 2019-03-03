// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Multiplayer;
using osuTK.Graphics;

namespace osu.Game.Screens.Multi.Match.Components
{
    public class MatchTabControl : PageTabControl<MatchPage>
    {
        [Resolved(typeof(Room), nameof(Room.RoomID))]
        private Bindable<int?> roomId { get; set; }

        public MatchTabControl()
        {
            AddItem(new RoomMatchPage());
            AddItem(new SettingsMatchPage());
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            roomId.BindValueChanged(id =>
            {
                if (id.NewValue.HasValue)
                {
                    Items.ForEach(t => t.Enabled.Value = !(t is SettingsMatchPage));
                    Current.Value = new RoomMatchPage();
                }
                else
                {
                    Items.ForEach(t => t.Enabled.Value = t is SettingsMatchPage);
                    Current.Value = new SettingsMatchPage();
                }
            }, true);
        }

        protected override TabItem<MatchPage> CreateTabItem(MatchPage value) => new TabItem(value);

        private class TabItem : PageTabItem
        {
            private readonly IBindable<bool> enabled = new BindableBool();

            public TabItem(MatchPage value)
                : base(value)
            {
                enabled.BindTo(value.Enabled);
                enabled.BindValueChanged(enabled => Colour = enabled.NewValue ? Color4.White : Color4.Gray, true);
            }

            protected override bool OnClick(ClickEvent e)
            {
                if (!enabled.Value)
                    return true;

                return base.OnClick(e);
            }
        }
    }
}
