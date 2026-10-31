// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Markdig.Extensions.CustomContainers;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers.Markdown;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Input.Events;
using osu.Game.Online;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Chat;
using osu.Game.Users.Drawables;

namespace osu.Game.Graphics.Containers.Markdown
{
    public partial class OsuMarkdownUserLink : OsuMarkdownLinkText, IHasCustomTooltip<APIUser?>
    {
        public APIUser? TooltipContent { get; private set; }

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private ILinkHandler? linkHandler { get; set; }

        private readonly int userId;
        private readonly CustomContainerInline customContainer;
        private GetUserRequest? request;

        public OsuMarkdownUserLink(CustomContainerInline customContainer, int userId)
            : base(string.Empty, customContainer)
        {
            this.userId = userId;
            this.customContainer = customContainer;
        }

        public ITooltip<APIUser?> GetCustomTooltip() => new UserCardTooltip();

        protected override void OnLinkPressed()
            => linkHandler?.HandleLink(new LinkDetails(LinkAction.OpenUserProfile, new APIUser { Id = userId }));

        protected override MarkdownTextFlowContainer CreateContent()
        {
            var textFlow = CreateTextFlow();
            textFlow.AddInlineText(customContainer);
            return textFlow;
        }

        protected override bool OnHover(HoverEvent e)
        {
            request = new GetUserRequest(userId);
            request.Success += response => Schedule(() => TooltipContent = response);
            request.Failure += _ => Schedule(() => TooltipContent = null);

            api.PerformAsync(request);

            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            request?.Cancel();
            TooltipContent = null;
            base.OnHoverLost(e);
        }
    }
}
