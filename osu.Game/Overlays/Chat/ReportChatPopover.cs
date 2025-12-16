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

            request.Success += () =>
            {
                string thanksMessage;

                switch (channelManager.CurrentChannel.Value.Type)
                {
                    case ChannelType.PM:
                        thanksMessage = """
                                  Chat moderators have been alerted. You have reported a private message so they will not be able to read history to maintain your privacy. Please make sure to include as much details as you can.
                                  You can submit a second report with more details if required, or contact abuse@ppy.sh if a user is being extremely offensive.
                                  You can also block a user via the block button on their user profile, or by right-clicking on their name in the chat and selecting "Block".
                                  """;
                        break;

                    default:
                        thanksMessage = @"Chat moderators have been alerted. Thanks for your help.";
                        break;
                }

                channelManager.CurrentChannel.Value.AddNewMessages(new InfoMessage(thanksMessage));
            };

            api.Queue(request);
        }
    }
}
