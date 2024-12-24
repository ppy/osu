// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using Humanizer;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets;
using osu.Game.Screens.Select;
using osu.Game.Users;

namespace osu.Game.Screens.OnlinePlay.Multiplayer
{
    public partial class MultiplayerMatchStyleSelect : SongSelect, IOnlinePlaySubScreen
    {
        public string ShortTitle => "style selection";

        public override string Title => ShortTitle.Humanize();

        public override bool AllowEditing => false;

        protected override UserActivity InitialActivity => new UserActivity.InLobby(room);

        private readonly Room room;
        private readonly PlaylistItem item;
        private readonly Action<BeatmapInfo, RulesetInfo> onSelect;

        public MultiplayerMatchStyleSelect(Room room, PlaylistItem item, Action<BeatmapInfo, RulesetInfo> onSelect)
        {
            this.room = room;
            this.item = item;
            this.onSelect = onSelect;

            Padding = new MarginPadding { Horizontal = HORIZONTAL_OVERFLOW_PADDING };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            LeftArea.Padding = new MarginPadding { Top = Header.HEIGHT };
        }

        protected override FilterControl CreateFilterControl() => new DifficultySelectFilterControl(item);

        protected override IEnumerable<(FooterButton, OverlayContainer?)> CreateSongSelectFooterButtons()
        {
            // Required to create the drawable components.
            base.CreateSongSelectFooterButtons();
            return Enumerable.Empty<(FooterButton, OverlayContainer?)>();
        }

        protected override BeatmapDetailArea CreateBeatmapDetailArea() => new PlayBeatmapDetailArea();

        protected override bool OnStart()
        {
            onSelect(Beatmap.Value.BeatmapInfo, Ruleset.Value);
            this.Exit();
            return true;
        }

        private partial class DifficultySelectFilterControl : FilterControl
        {
            private readonly PlaylistItem item;
            private double itemLength;

            public DifficultySelectFilterControl(PlaylistItem item)
            {
                this.item = item;
            }

            [BackgroundDependencyLoader]
            private void load(RealmAccess realm)
            {
                int beatmapId = item.Beatmap.OnlineID;
                itemLength = realm.Run(r => r.All<BeatmapInfo>().FirstOrDefault(b => b.OnlineID == beatmapId)?.Length ?? 0);
            }

            public override FilterCriteria CreateCriteria()
            {
                var criteria = base.CreateCriteria();

                // Must be from the same set as the playlist item.
                criteria.BeatmapSetId = item.BeatmapSetId;

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
