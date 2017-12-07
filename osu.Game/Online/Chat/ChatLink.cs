// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Input;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays;
using osu.Game.Overlays.Chat;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace osu.Game.Online.Chat
{
    public class ChatLink : OsuLinkSpriteText, IHasTooltip
    {
        public int LinkId = -1;

        private APIAccess api;
        private BeatmapSetOverlay beatmapSetOverlay;
        private ChatOverlay chat;

        private Color4 hoverColour;
        private Color4 urlColour;

        private readonly ChatHoverContainer content;

        /// <summary>
        /// Every other sprite in the containing ChatLine that represents the same link.
        /// </summary>
        protected IEnumerable<ChatLink> SameLinkSprites { get; private set; }

        protected override Container<Drawable> Content => content ?? base.Content;

        protected override void OnClick()
        {
            var url = Url;

            if (url.StartsWith("osu://"))
            {
                url = url.Substring(6);
                var args = url.Split('/');

                switch (args[0])
                {
                    case "chan":
                        var foundChannel = chat.AvailableChannels.Find(channel => channel.Name == args[1]);

                        if (foundChannel == null)
                            throw new ArgumentException($"Unknown channel name ({args[1]}).");
                        else
                            chat.OpenChannel(foundChannel);

                        break;
                    case "edit":
                        chat.Game?.LoadEditorTimestamp();
                        break;
                    case "b":
                        if (args.Length > 1 && int.TryParse(args[1], out int mapId))
                            beatmapSetOverlay.ShowBeatmap(mapId);

                        break;
                    case "s":
                    case "dl":
                        if (args.Length > 1 && int.TryParse(args[1], out int mapSetId))
                            beatmapSetOverlay.ShowBeatmapSet(mapSetId);

                        break;
                    case "spectate":
                        GetUserRequest req;
                        if (int.TryParse(args[1], out int userId))
                            req = new GetUserRequest(userId);
                        else
                            // Get by username instead
                            req = new GetUserRequest(args[1]);

                        req.Success += user =>
                        {
                            chat.Game?.LoadSpectatorScreen();
                        };
                        api.Queue(req);

                        break;
                    default:
                        throw new ArgumentException($"Unknown osu:// link at {nameof(OsuLinkSpriteText)} (https://osu.ppy.sh/{args[0]}).");
                }
            }
            else if (url.StartsWith("osump://"))
            {
                url = url.Substring(8);
                if (!int.TryParse(url.Split('/').ElementAtOrDefault(1), out int multiId))
                    return;

                chat.Game?.LoadMultiplayerLobby();
            }
            else if (url.StartsWith("http://") || url.StartsWith("https://") && url.IndexOf("osu.ppy.sh/") != -1)
            {
                var osuUrlIndex = url.IndexOf("osu.ppy.sh/");

                url = url.Substring(osuUrlIndex + 11);
                if (url.StartsWith("s/") || url.StartsWith("beatmapsets/") || url.StartsWith("d/"))
                {
                    var id = getIdFromUrl(url);
                    beatmapSetOverlay.ShowBeatmapSet(id);
                }
                else if (url.StartsWith("b/") || url.StartsWith("beatmaps/"))
                {
                    var id = getIdFromUrl(url);
                    beatmapSetOverlay.ShowBeatmap(id);
                }
                else
                    base.OnClick();
            }
            else
                base.OnClick();
        }

        private int getIdFromUrl(string url)
        {
            var lastSlashIndex = url.LastIndexOf('/');
            // Remove possible trailing slash
            if (lastSlashIndex == url.Length)
            {
                url = url.Remove(url.Length - 1);
                lastSlashIndex = url.LastIndexOf('/');
            }

            var lastQuestionMarkIndex = url.LastIndexOf('?');
            // Filter out possible queries like mode specifications (e.g. /b/252238?m=0)
            if (lastQuestionMarkIndex > lastSlashIndex)
                url = url.Remove(lastQuestionMarkIndex);

            return int.Parse(url.Substring(lastSlashIndex + 1));
        }

        public string TooltipText
        {
            get
            {
                if (LinkId == -1 || Url == Text)
                    return null;

                if (Url.StartsWith("osu://"))
                {
                    var args = Url.Substring(6).Split('/');

                    if (args.Length < 2)
                        return Url;

                    if (args[0] == "chan")
                        return "Switch to channel " + args[1];
                    if (args[0] == "edit")
                        return "Go to " + args[1].Remove(9).TrimEnd();
                }

                return Url;
            }
        }

        public ChatLink()
        {
            AddInternal(content = new ChatHoverContainer
            {
                AutoSizeAxes = Axes.Both,
            });

            OnLoadComplete = d =>
            {
                // All sprites in the same chatline that represent the same URL
                SameLinkSprites = (d.Parent as Container<Drawable>).Children.Where(child => (child as ChatLink)?.LinkId == LinkId && !d.Equals(child)).Cast<ChatLink>();
            };
        }

        protected override bool OnHover(InputState state)
        {
            var hoverResult = base.OnHover(state);

            if (!SameLinkSprites.Any(sprite => sprite.IsHovered))
                foreach (ChatLink sprite in SameLinkSprites)
                    sprite.TriggerOnHover(state);

            Content.FadeColour(hoverColour, 500, Easing.OutQuint);

            return hoverResult;
        }

        protected override void OnHoverLost(InputState state)
        {
            if (SameLinkSprites.Any(sprite => sprite.IsHovered))
            {
                // We have to do this so this sprite does not fade its colour back
                Content.FadeColour(hoverColour, 500, Easing.OutQuint);
                return;
            }

            Content.FadeColour(urlColour, 500, Easing.OutQuint);

            foreach (ChatLink sprite in SameLinkSprites)
                sprite.Content.FadeColour(urlColour, 500, Easing.OutQuint);

            base.OnHoverLost(state);
        }

        [BackgroundDependencyLoader]
        private void load(APIAccess api, BeatmapSetOverlay beatmapSetOverlay, ChatOverlay chat, OsuColour colours)
        {
            this.api = api;
            this.beatmapSetOverlay = beatmapSetOverlay;
            this.chat = chat;

            hoverColour = colours.Yellow;
            urlColour = colours.Blue;
            if (LinkId != -1)
                Content.Colour = urlColour;
        }

        private class ChatHoverContainer : OsuHoverContainer, IHasHoverSounds
        {
            public bool ShouldPlayHoverSound => ((ChatLink)Parent).SameLinkSprites.All(sprite => !sprite.IsHovered);
        }
    }
}
