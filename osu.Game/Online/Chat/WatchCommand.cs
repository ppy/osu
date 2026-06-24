// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Screens;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Screens;
using osu.Game.Screens.Play;

namespace osu.Game.Online.Chat
{
    public partial class WatchCommand : Component
    {
        [Resolved]
        private IChannelPostTarget channelManager { get; set; } = null!;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private IPerformFromScreenRunner performer { get; set; } = null!;

        private readonly Channel? target;
        private readonly string username;

        public WatchCommand(Channel target, string username)
        {
            this.target = target;
            this.username = username;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            var request = new GetUserRequest(username);
            request.Success += user =>
            {
                performer.PerformFromScreen(s => s.Push(new SoloSpectatorScreen(user)));
                Expire();
            };
            request.Failure += e =>
            {
                target?.AddNewMessages(new ErrorMessage(
                    e.InnerException?.Message == @"NotFound"
                        ? $"User '{username}' was not found."
                        : $"Could not fetch user '{username}'."));
                Expire();
            };

            api.Queue(request);
        }
    }
}
