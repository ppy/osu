// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Online.Chat;
using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Game.Users;

namespace osu.Game.Graphics.Containers
{
    public class LinkFlowContainer : OsuTextFlowContainer
    {
        public LinkFlowContainer(Action<SpriteText> defaultCreationParameters = null)
            : base(defaultCreationParameters)
        {
        }

        [Resolved(CanBeNull = true)]
        private OsuGame game { get; set; }

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
                AddText(text[previousLinkEnd..link.Index]);

                string displayText = text.Substring(link.Index, link.Length);
                string linkArgument = link.Argument ?? link.Url;
                string tooltip = displayText == link.Url ? null : link.Url;

                AddLink(displayText, link.Action, linkArgument, tooltip);
                previousLinkEnd = link.Index + link.Length;
            }

            AddText(text.Substring(previousLinkEnd));
        }

        public void AddLink(string text, string url, Action<SpriteText> creationParameters = null) =>
            createLink(AddText(text, creationParameters), new LinkDetails(LinkAction.External, url), url);

        public void AddLink(string text, Action action, string tooltipText = null, Action<SpriteText> creationParameters = null)
            => createLink(AddText(text, creationParameters), new LinkDetails(LinkAction.Custom, null), tooltipText, action);

        public void AddLink(string text, LinkAction action, string argument, string tooltipText = null, Action<SpriteText> creationParameters = null)
            => createLink(AddText(text, creationParameters), new LinkDetails(action, argument), tooltipText);

        public void AddLink(IEnumerable<SpriteText> text, LinkAction action = LinkAction.External, string linkArgument = null, string tooltipText = null)
        {
            foreach (var t in text)
                AddArbitraryDrawable(t);

            createLink(text, new LinkDetails(action, linkArgument), tooltipText);
        }

        public void AddUserLink(User user, Action<SpriteText> creationParameters = null)
            => createLink(AddText(user.Username, creationParameters), new LinkDetails(LinkAction.OpenUserProfile, user.Id.ToString()), "view profile");

        private void createLink(IEnumerable<Drawable> drawables, LinkDetails link, string tooltipText, Action action = null)
        {
            AddInternal(new DrawableLinkCompiler(drawables.OfType<SpriteText>().ToList())
            {
                RelativeSizeAxes = Axes.Both,
                TooltipText = tooltipText,
                Action = () =>
                {
                    if (action != null)
                        action();
                    else
                        game?.HandleLink(link);
                },
            });
        }

        // We want the compilers to always be visible no matter where they are, so RelativeSizeAxes is used.
        // However due to https://github.com/ppy/osu-framework/issues/2073, it's possible for the compilers to be relative size in the flow's auto-size axes - an unsupported operation.
        // Since the compilers don't display any content and don't affect the layout, it's simplest to exclude them from the flow.
        public override IEnumerable<Drawable> FlowingChildren => base.FlowingChildren.Where(c => !(c is DrawableLinkCompiler));
    }
}
