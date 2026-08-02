// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Play.HUD;

namespace osu.Game.Screens.OnlinePlay.Multiplayer
{
    public partial class MultiplayerUserModDisplay : CompositeDrawable
    {
        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        private ModDisplay modDisplay = null!;

        public MultiplayerUserModDisplay()
        {
            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = modDisplay = new ModDisplay();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            client.RoomUpdated += onRoomUpdated;
            onRoomUpdated();
        }

        private void onRoomUpdated() => Scheduler.AddOnce(() =>
        {
            if (client.Room == null || client.LocalUser == null)
                return;

            MultiplayerPlaylistItem currentItem = client.Room.CurrentPlaylistItem;
            Ruleset ruleset = rulesets.GetRuleset(client.LocalUser.RulesetId ?? currentItem.RulesetID)!.CreateInstance();
            Mod[] userMods = client.LocalUser.Mods.Select(m => m.ToMod(ruleset)).ToArray();

            if (!userMods.SequenceEqual(modDisplay.Current.Value))
                modDisplay.Current.Value = userMods;
        });

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (client.IsNotNull())
                client.RoomUpdated -= onRoomUpdated;
        }
    }
}
