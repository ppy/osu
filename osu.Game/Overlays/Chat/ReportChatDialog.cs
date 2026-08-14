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
    public partial class ReportChatDialog : ReportDialog<ChatReportReason>
    {
        [Resolved]
        private ChannelManager channelManager { get; set; } = null!;

        private readonly Message message;

        public ReportChatDialog(Message message)
            : base(ReportStrings.UserTitle(message.Sender?.Username ?? @"Someone"), false)
        {
            this.message = message;

        }

        protected override APIRequest GetRequest(ChatReportReason reason, string comments) => new ChatReportRequest(message.Id, reason, comments);

        protected override bool IsCommentRequired(ChatReportReason reason) => reason == ChatReportReason.Other;
    }
}
