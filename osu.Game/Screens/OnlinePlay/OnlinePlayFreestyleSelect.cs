// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using Humanizer;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Logging;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets;
using osu.Game.Screens.Select;
using osu.Game.Users;

namespace osu.Game.Screens.OnlinePlay
{
    public abstract partial class OnlinePlayFreestyleSelect : SongSelect, IOnlinePlaySubScreen, IHandlePresentBeatmap
    {
        public string ShortTitle => "style selection";

        public override string Title => ShortTitle.Humanize();

        public override bool AllowEditing => false;

        protected override UserActivity InitialActivity => new UserActivity.InLobby(room);

        private readonly Room room;
        private readonly PlaylistItem item;

        protected OnlinePlayFreestyleSelect(Room room, PlaylistItem item)
        {
            this.room = room;
            this.item = item;

            Padding = new MarginPadding { Horizontal = HORIZONTAL_OVERFLOW_PADDING };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            LeftArea.Padding = new MarginPadding { Top = Header.HEIGHT };
        }

        protected override bool OnStart()
        {
            FilterCriteria criteria = FilterControl.CreateCriteria();

            // Beatmaps with too different of a duration are filtered away; this is just a final safety.
            if (!criteria.Length.IsInRange(Beatmap.Value.BeatmapInfo.Length))
            {
                Logger.Log("The selected beatmap's duration differs too much from the host's selection.", level: LogLevel.Error);
                return false;
            }

            // Beatmaps without a valid online ID are filtered away; this is just a final safety.
            if (Beatmap.Value.BeatmapInfo.OnlineID < 0)
            {
                Logger.Log("The selected beatmap is not available online.", level: LogLevel.Error);
                return false;
            }

            // Beatmaps from different sets are filtered away; this is just a final safety.
            if (Beatmap.Value.BeatmapSetInfo.OnlineID != criteria.BeatmapSetId)
            {
                Logger.Log("The selected beatmap is from a different beatmap set.", level: LogLevel.Error);
                return false;
            }

            if (Ruleset.Value.OnlineID < 0)
            {
                Logger.Log("The selected ruleset is not available online.", level: LogLevel.Error);
                return false;
            }

            return true;
        }

        protected override FilterControl CreateFilterControl() => new DifficultySelectFilterControl(item);

        protected override IEnumerable<(FooterButton button, OverlayContainer? overlay)> CreateSongSelectFooterButtons()
        {
            // Required to create the drawable components.
            base.CreateSongSelectFooterButtons();
            return Enumerable.Empty<(FooterButton, OverlayContainer?)>();
        }

        protected override BeatmapDetailArea CreateBeatmapDetailArea() => new PlayBeatmapDetailArea();

        public void PresentBeatmap(WorkingBeatmap beatmap, RulesetInfo ruleset)
        {
            // This screen cannot present beatmaps.
        }

        private partial class DifficultySelectFilterControl : FilterControl
        {
            private readonly PlaylistItem item;
            private double itemLength;
            private int beatmapSetId;

            public DifficultySelectFilterControl(PlaylistItem item)
            {
                this.item = item;
            }

            [BackgroundDependencyLoader]
            private void load(RealmAccess realm)
            {
                realm.Run(r =>
                {
                    int beatmapId = item.Beatmap.OnlineID;
                    BeatmapInfo? beatmap = r.All<BeatmapInfo>().FirstOrDefault(b => b.OnlineID == beatmapId);

                    itemLength = beatmap?.Length ?? 0;
                    beatmapSetId = beatmap?.BeatmapSet?.OnlineID ?? 0;
                });
            }

            public override FilterCriteria CreateCriteria()
            {
                var criteria = base.CreateCriteria();

                // Must be from the same set as the playlist item.
                criteria.BeatmapSetId = beatmapSetId;
                criteria.HasOnlineID = true;

                // Must be within 30s of the playlist item.
                criteria.Length.Min = itemLength - 30000;
                criteria.Length.Max = itemLength + 30000;
                criteria.Length.IsLowerInclusive = true;
                criteria.Length.IsUpperInclusive = true;

                return criteria;
            }
        }
    }
}
