// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Online.API;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Users;

namespace osu.Game.Online.Chat
{
    public partial class NowPlayingCommand : Component
    {
        [Resolved]
        private IChannelPostTarget channelManager { get; set; } = null!;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private Bindable<WorkingBeatmap> currentBeatmap { get; set; } = null!;

        [Resolved]
        private Bindable<IReadOnlyList<Mod>> selectedMods { get; set; } = null!;

        [Resolved]
        private IBindable<RulesetInfo> currentRuleset { get; set; } = null!;

        private readonly Channel? target;

        /// <summary>
        /// Creates a new <see cref="NowPlayingCommand"/> to post the currently-playing beatmap to a parenting <see cref="IChannelPostTarget"/>.
        /// </summary>
        /// <param name="target">The target channel to post to. If <c>null</c>, the currently-selected channel will be posted to.</param>
        public NowPlayingCommand(Channel target)
        {
            this.target = target;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            string verb;

            int beatmapOnlineID;
            string beatmapDisplayTitle;

            switch (api.Activity.Value)
            {
                case UserActivity.InGame game:
                    verb = "playing";
                    beatmapOnlineID = game.BeatmapID;
                    beatmapDisplayTitle = game.BeatmapDisplayTitle;
                    break;

                case UserActivity.EditingBeatmap edit:
                    verb = "editing";
                    beatmapOnlineID = edit.BeatmapID;
                    beatmapDisplayTitle = edit.BeatmapDisplayTitle;
                    break;

                default:
                    verb = "listening to";
                    beatmapOnlineID = currentBeatmap.Value.BeatmapInfo.OnlineID;
                    beatmapDisplayTitle = currentBeatmap.Value.BeatmapInfo.GetDisplayTitle();
                    break;
            }

            string[] pieces =
            {
                "is",
                verb,
                getBeatmapPart(),
                getRulesetPart(),
                getModPart(),
            };

            channelManager.PostMessage(string.Join(' ', pieces.Where(p => !string.IsNullOrEmpty(p))), true, target);
            Expire();

            string getBeatmapPart()
            {
                return beatmapOnlineID > 0 ? $"[{api.WebsiteRootUrl}/b/{beatmapOnlineID} {beatmapDisplayTitle}]" : beatmapDisplayTitle;
            }

            string getRulesetPart()
            {
                if (api.Activity.Value is not UserActivity.InGame) return string.Empty;

                return $"<{currentRuleset.Value.Name}>";
            }

            string getModPart()
            {
                if (api.Activity.Value is not UserActivity.InGame) return string.Empty;

                if (selectedMods.Value.Count == 0)
                {
                    return string.Empty;
                }

                StringBuilder modsString = new StringBuilder();

                foreach (var mod in selectedMods.Value.Where(mod => mod.Type == ModType.DifficultyIncrease))
                {
                    modsString.Append($"+{mod.Acronym} ");
                }

                foreach (var mod in selectedMods.Value.Where(mod => mod.Type != ModType.DifficultyIncrease))
                {
                    modsString.Append($"-{mod.Acronym} ");
                }

                return modsString.ToString().Trim();
            }
        }
    }
}
