// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics.Cursor;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using System;

namespace osu.Game.Online.Chat
{
    public class ChatLink : OsuSpriteLink, IHasTooltip
    {
        private BeatmapSetOverlay beatmapSetOverlay;
        private ChatOverlay chat;
        private OsuGame game;

        /// <summary>
        /// The type of action executed on clicking this link.
        /// </summary>
        public LinkAction LinkAction { get; set; }

        /// <summary>
        /// The argument necessary for the action specified by <see cref="LinkAction"/> to execute.
        /// <para>Usually a part of the URL.</para>
        /// </summary>
        public string LinkArgument { get; set; }

        protected override void OnLinkClicked()
        {
            switch (LinkAction)
            {
                case LinkAction.OpenBeatmap:
                    // todo: implement this when overlay.ShowBeatmap(id) exists
                    break;
                case LinkAction.OpenBeatmapSet:
                    if (int.TryParse(LinkArgument, out int setId))
                        beatmapSetOverlay.ShowBeatmapSet(setId);
                    break;
                case LinkAction.OpenChannel:
                    chat.OpenChannel(chat.AvailableChannels.Find(c => c.Name == LinkArgument));
                    break;
                case LinkAction.OpenEditorTimestamp:
                    game?.LoadEditorTimestamp();
                    break;
                case LinkAction.JoinMultiplayerMatch:
                    if (int.TryParse(LinkArgument, out int matchId))
                        game?.JoinMultiplayerMatch(matchId);
                    break;
                case LinkAction.Spectate:
                    // todo: implement this when spectating exists
                    break;
                case LinkAction.External:
                    base.OnLinkClicked();
                    break;
                default:
                    throw new NotImplementedException($"This {nameof(Chat.LinkAction)} ({LinkAction.ToString()}) is missing an associated action.");
            }
        }

        public string TooltipText
        {
            get
            {
                if (Url == Text)
                    return null;

                switch (LinkAction)
                {
                    case LinkAction.OpenChannel:
                        return "Switch to channel " + LinkArgument;
                    case LinkAction.OpenEditorTimestamp:
                        return "Go to " + LinkArgument;
                    default:
                        return Url;
                }
            }
        }

        [BackgroundDependencyLoader(true)]
        private void load(BeatmapSetOverlay beatmapSetOverlay, ChatOverlay chat, OsuGame game)
        {
            this.beatmapSetOverlay = beatmapSetOverlay;
            this.chat = chat;
            // this will be null in tests
            this.game = game;
        }
    }
}
