// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Game.Online.Chat;
using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Online;
using osu.Game.Users;
using osu.Game.Localisation;

namespace osu.Game.Graphics.Containers
{
    public partial class LinkFlowContainer : OsuTextFlowContainer
    {
        public LinkFlowContainer(Action<SpriteText> defaultCreationParameters = null)
            : base(defaultCreationParameters)
        {
        }

        [Resolved(CanBeNull = true)]
        private ILinkHandler linkHandler { get; set; }

        [Resolved]
        private GameHost host { get; set; }

        public void AddLinks(string text, List<Link> links)
        {
            if (string.IsNullOrEmpty(text) || links == null)
                return;

            if (links.Count == 0)
            {
                AddText(text);
                return;
            }

            int previousLinkEnd = 0;

            foreach (var link in links)
            {
                string displayText = text.Substring(link.Index, link.Length);

                if (previousLinkEnd > link.Index)
                {
                    Logger.Log($@"Link ""{link.Url}"" with text ""{displayText}"" overlaps previous link, ignoring.");
                    continue;
                }

                AddText(text[previousLinkEnd..link.Index]);

                object linkArgument = link.Argument;
                string tooltip = displayText == link.Url ? null : link.Url;

                AddLink(displayText, link.Action, linkArgument, tooltip);
                previousLinkEnd = link.Index + link.Length;
            }

            AddText(text.Substring(previousLinkEnd));
        }

        public void AddLink(LocalisableString text, string url, Action<SpriteText> creationParameters = null) =>
            createLink(CreateChunkFor(text, true, CreateSpriteText, creationParameters), new LinkDetails(LinkAction.External, url), url);

        public void AddLink(LocalisableString text, Action action, string tooltipText = null, Action<SpriteText> creationParameters = null)
            => createLink(CreateChunkFor(text, true, CreateSpriteText, creationParameters), new LinkDetails(LinkAction.Custom, string.Empty), tooltipText, action);

        public void AddLink(LocalisableString text, LinkAction action, object argument, string tooltipText = null, Action<SpriteText> creationParameters = null)
            => createLink(CreateChunkFor(text, true, CreateSpriteText, creationParameters), new LinkDetails(action, argument), tooltipText);

        public void AddLink(IEnumerable<SpriteText> text, LinkAction action, object linkArgument, string tooltipText = null)
        {
            createLink(new TextPartManual(text), new LinkDetails(action, linkArgument), tooltipText);
        }

        public void AddUserLink(IUser user, Action<SpriteText> creationParameters = null)
            => createLink(CreateChunkFor(user.Username, true, CreateSpriteText, creationParameters), new LinkDetails(LinkAction.OpenUserProfile, user), ContextMenuStrings.ViewProfile);

        private void createLink(ITextPart textPart, LinkDetails link, LocalisableString tooltipText, Action action = null)
        {
            Action onClickAction = () =>
            {
                if (action != null)
                    action();
                else if (linkHandler != null)
                    linkHandler.HandleLink(link);
                // fallback to handle cases where OsuGame is not available, ie. tournament client.
                else if (link.Action == LinkAction.External)
                    host.OpenUrlExternally(link.Argument.ToString());
            };

            AddPart(new TextLink(textPart, tooltipText, onClickAction));
        }

        private class TextLink : TextPart
        {
            private readonly ITextPart innerPart;
            private readonly LocalisableString tooltipText;
            private readonly Action action;

            public TextLink(ITextPart innerPart, LocalisableString tooltipText, Action action)
            {
                this.innerPart = innerPart;
                this.tooltipText = tooltipText;
                this.action = action;
            }

            protected override IEnumerable<Drawable> CreateDrawablesFor(TextFlowContainer textFlowContainer)
            {
                var linkFlowContainer = (LinkFlowContainer)textFlowContainer;

                innerPart.RecreateDrawablesFor(linkFlowContainer);
                var drawables = innerPart.Drawables.ToList();

                drawables.Add(linkFlowContainer.CreateLinkCompiler(innerPart).With(c =>
                {
                    c.RelativeSizeAxes = Axes.Both;
                    c.TooltipText = tooltipText;
                    c.Action = action;
                }));

                return drawables;
            }
        }

        protected virtual DrawableLinkCompiler CreateLinkCompiler(ITextPart textPart) => new DrawableLinkCompiler(textPart);

        // We want the compilers to always be visible no matter where they are, so RelativeSizeAxes is used.
        // However due to https://github.com/ppy/osu-framework/issues/2073, it's possible for the compilers to be relative size in the flow's auto-size axes - an unsupported operation.
        // Since the compilers don't display any content and don't affect the layout, it's simplest to exclude them from the flow.
        public override IEnumerable<Drawable> FlowingChildren => base.FlowingChildren.Where(c => !(c is DrawableLinkCompiler));
    }
}
