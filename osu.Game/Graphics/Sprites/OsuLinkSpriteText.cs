// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Containers;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays;
using osu.Game.Screens.Edit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Game.Graphics.Sprites
{
    public class OsuLinkSpriteText : OsuSpriteText
    {
        private ChatOverlay chat;

        private readonly OsuHoverContainer content;

        private APIAccess api;
        private BeatmapSetOverlay beatmapSetOverlay;

        public override bool HandleInput => content.Action != null;

        protected override Container<Drawable> Content => content ?? (Container<Drawable>)this;

        protected override IEnumerable<Drawable> FlowingChildren => Children;

        private string url;

        public string Url
        {
            get
            {
                return url;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    url = value;

                    content.Action = onClickAction;

                    // For inheriting classes
                    if (Content is OsuHoverContainer hover)
                        hover.Action = onClickAction;
                }
            }
        }

        public OsuLinkSpriteText()
        {
            AddInternal(content = new OsuHoverContainer
            {
                AutoSizeAxes = Axes.Both,
            });
        }

        public ColourInfo TextColour
        {
            get { return Content.Colour; }
            set { Content.Colour = value; }
        }

        [BackgroundDependencyLoader]
        private void load(APIAccess api, BeatmapSetOverlay beatmapSetOverlay, ChatOverlay chat)
        {
            this.api = api;
            this.beatmapSetOverlay = beatmapSetOverlay;
            this.chat = chat;
        }

        private void onClickAction()
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
                            TextColour = Color4.White;
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
            else if (url.StartsWith("http://") || url.StartsWith("https://"))
            {
                var osuUrlIndex = url.IndexOf("osu.ppy.sh/");
                if (osuUrlIndex == -1)
                {
                    Process.Start(url);
                    return;
                }

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
                    Process.Start($"https://osu.ppy.sh/{url}");
            }
            else
                Process.Start(url);
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
    }
}
