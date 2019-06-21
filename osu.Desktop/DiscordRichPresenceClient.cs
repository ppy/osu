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
using static osu.Game.Users.UserActivity;
using User = osu.Game.Users.User;

namespace osu.Desktop
{
    internal class DiscordRichPresenceClient : Component
    {
        private const string client_id = "563024054391537674";

        private Bindable<User> user;

        private readonly DiscordRpcClient client = new DiscordRpcClient(client_id);

        [BackgroundDependencyLoader]
        private void load(IAPIProvider provider)
        {
            user = provider.LocalUser.GetBoundCopy();

            user.ValueChanged += usr =>
            {
                usr.NewValue.Activity.ValueChanged += activity => updateStatus(user.Value.Status.Value, activity.NewValue);
                usr.NewValue.Status.ValueChanged += status => updateStatus(status.NewValue, user.Value.Activity.Value);
            };

            user.TriggerChange();

            enableLogging();

            client.Initialize();
        }

        private void enableLogging()
        {
            client.OnReady += (_, __) => Logger.Log("Discord RPC Client ready.", LoggingTarget.Network, LogLevel.Debug);
            client.OnError += (_, e) => Logger.Log($"An error occurred with Discord RPC Client : {e.Message}", LoggingTarget.Network, LogLevel.Debug);
            client.OnConnectionFailed += (_, e) => Logger.Log("Discord RPC Client failed to initialize : is discord running ?", LoggingTarget.Network, LogLevel.Debug);
            client.OnPresenceUpdate += (_, __) => Logger.Log("Updated Discord Rich Presence", LoggingTarget.Network, LogLevel.Debug);
        }

        private void updateStatus(UserStatus st, UserActivity a)
        {
            var presence = defaultPresence(st is UserStatusOnline ? a?.Status ?? st.Message : st.Message); //display the current user activity if the user status is online & user activity != null, else display the current user online status

            if (!(st is UserStatusOnline)) //don't update the presence any further if the current user status is DND / Offline & simply return with the default presence
            {
                client.SetPresence(presence);
                return;
            }

            switch (a)
            {
                case SoloGame game:
                    presence.State = $"{game.Beatmap.Metadata.Artist} - {game.Beatmap.Metadata.Title} [{game.Beatmap.Version}]";
                    setPresenceGamemode(game.Ruleset, presence);
                    break;

                case Editing editing:
                    presence.State = $"{editing.Beatmap.Metadata.Artist} - {editing.Beatmap.Metadata.Title} " + (!string.IsNullOrEmpty(editing.Beatmap.Version) ? $"[{editing.Beatmap.Version}]" : "");
                    presence.Assets.SmallImageKey = "edit";
                    presence.Assets.SmallImageText = "editing";
                    break;
            }

            client.SetPresence(presence);
        }

        private void setPresenceGamemode(RulesetInfo ruleset, RichPresence presence)
        {
            if (ruleset.ID != null && ruleset.ID <= 3) //legacy rulesets use an ID between 0 and 3
                presence.Assets.SmallImageKey = ruleset.ShortName;
            else
                presence.Assets.SmallImageKey = "unknown"; //not a legacy ruleset so let's display the unknown ruleset icon.

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

        protected override void Dispose(bool isDisposing)
        {
            client.Dispose();
            base.Dispose(isDisposing);
        }
    }
}
