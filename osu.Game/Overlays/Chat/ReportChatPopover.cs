// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.Chat;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.Chat
{
    public partial class ReportChatPopover : ReportPopover<ChatReportReason>
    {
        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private ChannelManager channelManager { get; set; } = null!;

        private readonly Message message;

        public ReportChatPopover(Message message)
            : base(ReportStrings.UserTitle(message.Sender?.Username ?? @"Someone"))
        {
            this.message = message;
            Action = report;
        }

        protected override bool IsCommentRequired(ChatReportReason reason) => reason == ChatReportReason.Other;

        private void report(ChatReportReason reason, string comments)
        {
            var request = new ChatReportRequest(message.Id, reason, comments);

            request.Success += () => channelManager.CurrentChannel.Value.AddNewMessages(new InfoMessage(UsersStrings.ReportThanks.ToString()));

            api.Queue(request);
        }
    }
}
