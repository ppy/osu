// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using DiscordRPC;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Logging;
using osu.Game.Online.API;
using osu.Game.Rulesets;
using osu.Game.Users;
using User = osu.Game.Users.User;

namespace osu.Desktop
{
    internal class DiscordRichPresenceClient : Component
    {
        private const string client_id = "559391129716391967";

        private Bindable<User> user;

        private readonly DiscordRpcClient client = new DiscordRpcClient(client_id);

        [BackgroundDependencyLoader]
        private void load(IAPIProvider provider)
        {
            user = provider.LocalUser.GetBoundCopy();

            user.ValueChanged += usr =>
            {
                usr.OldValue.Status.ValueChanged -= updateStatus;
                usr.NewValue.Status.ValueChanged += updateStatus;
            };

            user.Value.Status.ValueChanged += updateStatus;

            client.OnReady += (_, __) => Logger.Log("Discord RPC Client ready.", LoggingTarget.Network, LogLevel.Debug);
            client.OnError += (_, e) => Logger.Log($"An error occurred with Discord RPC Client : {e.Message}", LoggingTarget.Network, LogLevel.Debug);
            client.OnConnectionFailed += (_, e) => Logger.Log("Discord RPC Client failed to initialize : is discord running ?", LoggingTarget.Network, LogLevel.Debug);
            client.OnPresenceUpdate += (_, __) => Logger.Log("Updated Discord Rich Presence", LoggingTarget.Network, LogLevel.Debug);

            client.Initialize();
        }

        private void updateStatus(ValueChangedEvent<UserStatus> e)
        {
            var presence = defaultPresence(e.NewValue.Message);

            switch (e.NewValue)
            {
                case UserStatusSoloGame game:
                    presence.State = $"{game.Beatmap.Metadata.Artist} - {game.Beatmap.Metadata.Title} [{game.Beatmap.Version}]";
                    setPresenceGamemode(game.Ruleset, presence);
                    break;

                case UserStatusEditing editing:
                    presence.State = $"{editing.Beatmap.Metadata.Artist} - {editing.Beatmap.Metadata.Title}" + (editing.Beatmap.Version != null ? $"[{editing.Beatmap.Version}]" : "");
                    break;
            }

            client.SetPresence(presence);
        }

        private void setPresenceGamemode(RulesetInfo ruleset, RichPresence presence)
        {
            switch (ruleset.ID)
            {
                case 0:
                    presence.Assets.SmallImageKey = "osu";
                    break;

                case 1:
                    presence.Assets.SmallImageKey = "taiko";
                    break;

                case 2:
                    presence.Assets.SmallImageKey = "fruits";
                    break;

                case 3:
                    presence.Assets.SmallImageKey = "mania";
                    break;
            }

            presence.Assets.SmallImageText = ruleset.ShortName;
        }

        private RichPresence defaultPresence(string status) => new RichPresence
        {
            Details = status,
            Assets = new Assets
            {
                LargeImageKey = "lazer",
                LargeImageText = user.Value.Username
            }
        };

        protected override void Update()
        {
            if (client.IsInitialized)
                client?.Invoke();

            base.Update();
        }
    }
}
