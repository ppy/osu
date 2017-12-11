// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace osu.Game.Online.Chat
{
    public class ChatLink : OsuLinkSpriteText, IHasTooltip
    {
        /// <summary>
        /// Identifier unique to every link in a message.
        /// <para>A value of -1 means that this <see cref="ChatLink"/> instance does not contain a link.</para>
        /// </summary>
        public int LinkId = -1;

        private APIAccess api;
        private BeatmapSetOverlay beatmapSetOverlay;
        private ChatOverlay chat;

        private Color4 hoverColour;
        private Color4 urlColour;

        private readonly HoverClickSounds hoverClickSounds;

        /// <summary>
        /// Every other sprite in the containing ChatLine that represents the same link.
        /// </summary>
        protected IEnumerable<ChatLink> SameLinkSprites { get; private set; }

        public override bool HandleInput => LinkId != -1;

        protected override bool OnClick(InputState state)
        {
            hoverClickSounds.TriggerOnClick(state);
            return base.OnClick(state);
        }

        protected override void OnLinkClicked()
        {
            var urlMatch = Regex.Matches(Url, @"^(?<protocol>osu(?:mp)?|https?):\/\/(?<content>.*)")[0];
            if (urlMatch.Success)
            {
                var args = urlMatch.Groups["content"].Value.Split('/');

                switch (urlMatch.Groups["protocol"].Value)
                {
                    case "osu":
                        if (args.Length == 1)
                        {
                            base.OnLinkClicked();
                            break;
                        }

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
                                    return;

                                req.Success += user =>
                                {
                                    chat.Game?.LoadSpectatorScreen();
                                };
                                api.Queue(req);

                                break;
                            default:
                                throw new ArgumentException($"Unknown osu:// link at {nameof(ChatLink)} ({urlMatch.Groups["content"].Value}).");
                        }

                        break;
                    case "osump":
                        if (args.Length > 1 && int.TryParse(args[1], out int multiId))
                            chat.Game?.LoadMultiplayerLobby(multiId);

                        break;
                    case "http":
                    case "https":
                        if (args[0] == "osu.ppy.sh" && args.Length > 2)
                        {
                            switch (args[1])
                            {
                                case "b":
                                case "beatmaps":
                                    beatmapSetOverlay.ShowBeatmap(getId(args[2]));
                                    break;
                                case "s":
                                case "beatmapsets":
                                case "d":
                                    beatmapSetOverlay.ShowBeatmapSet(getId(args[2]));
                                    break;
                                default:
                                    base.OnLinkClicked();
                                    break;
                            }
                        }
                        else
                            base.OnLinkClicked();
                        break;
                    default:
                        base.OnLinkClicked();
                        break;
                }
            }
            else
                base.OnLinkClicked();
        }

        private int getId(string input)
        {
            var index = input.IndexOf('#');
            return int.Parse(index > 0 ? input.Remove(index) : input);
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
            hoverClickSounds = new HoverClickSounds();

            OnLoadComplete = d =>
            {
                // All sprites in the same chatline that represent the same URL
                SameLinkSprites = ((Container<Drawable>)d.Parent).Children.Where(child => (child as ChatLink)?.LinkId == LinkId && !d.Equals(child)).Cast<ChatLink>();
            };
        }

        protected override bool OnHover(InputState state)
        {
            if (!SameLinkSprites.Any(sprite => sprite.IsHovered))
            {
                hoverClickSounds.TriggerOnHover(state);

                foreach (ChatLink sprite in SameLinkSprites)
                    sprite.TriggerOnHover(state);
            }

            Content.FadeColour(hoverColour, 500, Easing.OutQuint);

            return true;
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
            // Should be ok, inexpensive operation
            LoadComponentAsync(hoverClickSounds);

            this.api = api;
            this.beatmapSetOverlay = beatmapSetOverlay;
            this.chat = chat;

            hoverColour = colours.Yellow;
            urlColour = colours.Blue;
            if (LinkId != -1)
                Content.Colour = urlColour;
        }
    }
}
