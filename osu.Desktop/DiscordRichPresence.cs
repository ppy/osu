// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Text;
using DiscordRPC;
using DiscordRPC.Message;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Logging;
using osu.Game.Online.API;
using osu.Game.Rulesets;
using osu.Game.Users;
using LogLevel = osu.Framework.Logging.LogLevel;
using User = osu.Game.Users.User;

namespace osu.Desktop
{
    internal class DiscordRichPresence : Component
    {
        private const string client_id = "367827983903490050";

        private DiscordRpcClient client;

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; }

        private IBindable<User> user;

        private readonly IBindable<UserStatus> status = new Bindable<UserStatus>();
        private readonly IBindable<UserActivity> activity = new Bindable<UserActivity>();

        private readonly RichPresence presence = new RichPresence
        {
            Assets = new Assets { LargeImageKey = "osu_logo_lazer", }
        };

        [BackgroundDependencyLoader]
        private void load(IAPIProvider provider)
        {
            client = new DiscordRpcClient(client_id)
            {
                SkipIdenticalPresence = false // handles better on discord IPC loss, see updateStatus call in onReady.
            };

            client.OnReady += onReady;

            // safety measure for now, until we performance test / improve backoff for failed connections.
            client.OnConnectionFailed += (_, __) => client.Deinitialize();

            client.OnError += (_, e) => Logger.Log($"An error occurred with Discord RPC Client: {e.Code} {e.Message}", LoggingTarget.Network);

            (user = provider.LocalUser.GetBoundCopy()).BindValueChanged(u =>
            {
                status.UnbindBindings();
                status.BindTo(u.NewValue.Status);

                activity.UnbindBindings();
                activity.BindTo(u.NewValue.Activity);
            }, true);

            ruleset.BindValueChanged(_ => updateStatus());
            status.BindValueChanged(_ => updateStatus());
            activity.BindValueChanged(_ => updateStatus());

            client.Initialize();
        }

        private void onReady(object _, ReadyMessage __)
        {
            Logger.Log("Discord RPC Client ready.", LoggingTarget.Network, LogLevel.Debug);
            updateStatus();
        }

        private void updateStatus()
        {
            if (!client.IsInitialized)
                return;

            if (status.Value is UserStatusOffline)
            {
                client.ClearPresence();
                return;
            }

            if (status.Value is UserStatusOnline && activity.Value != null)
            {
                presence.State = truncate(activity.Value.Status);
                presence.Details = truncate(getDetails(activity.Value));
            }
            else
            {
                presence.State = "Idle";
                presence.Details = string.Empty;
            }

            // update user information
            presence.Assets.LargeImageText = $"{user.Value.Username}" + (user.Value.Statistics?.Ranks.Global > 0 ? $" (rank #{user.Value.Statistics.Ranks.Global:N0})" : string.Empty);

            // update ruleset
            presence.Assets.SmallImageKey = ruleset.Value.ID <= 3 ? $"mode_{ruleset.Value.ID}" : "mode_custom";
            presence.Assets.SmallImageText = ruleset.Value.Name;

            client.SetPresence(presence);
        }

        private static readonly int ellipsis_length = Encoding.UTF8.GetByteCount(new[] { '…' });

        private string truncate(string str)
        {
            if (Encoding.UTF8.GetByteCount(str) <= 128)
                return str;

            ReadOnlyMemory<char> strMem = str.AsMemory();

            do
            {
                strMem = strMem[..^1];
            } while (Encoding.UTF8.GetByteCount(strMem.Span) + ellipsis_length > 128);

            return string.Create(strMem.Length + 1, strMem, (span, mem) =>
            {
                mem.Span.CopyTo(span);
                span[^1] = '…';
            });
        }

        private string getDetails(UserActivity activity)
        {
            switch (activity)
            {
                case UserActivity.SoloGame solo:
                    return solo.Beatmap.ToString();

                case UserActivity.Editing edit:
                    return edit.Beatmap.ToString();

                case UserActivity.InLobby lobby:
                    return lobby.Room.Name.Value;
            }

            return string.Empty;
        }

        protected override void Dispose(bool isDisposing)
        {
            client.Dispose();
            base.Dispose(isDisposing);
        }
    }
}
