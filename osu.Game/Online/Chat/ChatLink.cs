// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics.Cursor;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays;
using System;
using System.Text.RegularExpressions;

namespace osu.Game.Online.Chat
{
    public class ChatLink : OsuLinkSpriteText, IHasTooltip
    {
        private APIAccess api;
        private BeatmapSetOverlay beatmapSetOverlay;
        private ChatOverlay chat;

        public override bool HandleInput => !string.IsNullOrEmpty(Url);

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

            int getId(string input)
            {
                var index = input.IndexOf('#');
                return int.Parse(index > 0 ? input.Remove(index) : input);
            }
        }

        public string TooltipText
        {
            get
            {
                if (Url == Text)
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

        [BackgroundDependencyLoader]
        private void load(APIAccess api, BeatmapSetOverlay beatmapSetOverlay, ChatOverlay chat)
        {
            this.api = api;
            this.beatmapSetOverlay = beatmapSetOverlay;
            this.chat = chat;
        }
    }
}
