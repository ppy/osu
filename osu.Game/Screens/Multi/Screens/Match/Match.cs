// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.Select;
using osu.Game.Users;

namespace osu.Game.Screens.Multi.Screens.Match
{
    public class Match : MultiplayerScreen
    {
        private readonly Room room;
        private readonly Participants participants;
        private readonly Info info;
        private readonly Settings settings;

        private readonly Bindable<string> nameBind = new Bindable<string>();
        private readonly Bindable<RoomStatus> statusBind = new Bindable<RoomStatus>();
        private readonly Bindable<RoomAvailability> availabilityBind = new Bindable<RoomAvailability>();
        private readonly Bindable<GameType> typeBind = new Bindable<GameType>();
        private readonly Bindable<BeatmapInfo> beatmapBind = new Bindable<BeatmapInfo>();
        private readonly Bindable<int?> maxParticipantsBind = new Bindable<int?>();
        private readonly Bindable<IEnumerable<User>> participantsBind = new Bindable<IEnumerable<User>>();

        protected override Container<Drawable> TransitionContent => participants;

        public override string Type => "room";
        public override string Title => room.Name.Value;

        private MatchHeaderPage currentPage = MatchHeaderPage.Room;

        public Match(Room room)
        {
            this.room = room;
            Header header;

            Children = new Drawable[]
            {
                header = new Header(),
                info = new Info
                {
                    Margin = new MarginPadding { Top = Header.HEIGHT },
                },
                participants = new Participants
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Top = Header.HEIGHT + Info.HEIGHT },
                },
                settings = new Settings
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Top = Header.HEIGHT },
                    Alpha = 0,
                },
            };

            header.OnRequestSelectBeatmap = () => Push(new MatchSongSelect());

            beatmapBind.BindTo(room.Beatmap);
            beatmapBind.BindValueChanged(b =>
            {
                header.BeatmapSet = b?.BeatmapSet;
                info.Beatmap = b;
            }, true);

            nameBind.BindTo(room.Name);
            settings.RoomName.BindTo(nameBind);
            nameBind.BindValueChanged(n =>
            {
                info.Name = n;
                settings.RoomName.Value = n;
            }, true);

            statusBind.BindTo(room.Status);
            statusBind.BindValueChanged(s => info.Status = s, true);

            availabilityBind.BindTo(room.Availability);
            settings.RoomAvailability.BindTo(availabilityBind);
            availabilityBind.BindValueChanged(a =>
            {
                info.Availability = a;
                settings.RoomAvailability.Value = a;
            }, true);

            typeBind.BindTo(room.Type);
            settings.GameType.BindTo(typeBind);
            typeBind.BindValueChanged(t =>
            {
                info.Type = t;
                settings.GameType.Value = t;
            }, true);

            maxParticipantsBind.BindTo(room.MaxParticipants);
            settings.MaxParticipants.BindTo(maxParticipantsBind);
            maxParticipantsBind.BindValueChanged(m => {
                participants.Max = m;
                settings.MaxParticipants.Value = m;
            }, true);

            participantsBind.BindTo(room.Participants);
            participantsBind.BindValueChanged(p => participants.Users = p, true);

            header.Tabs.Current.BindValueChanged(onPageChanged);
        }

        private void onPageChanged(MatchHeaderPage page)
        {
            if (page == currentPage) return;

            // probably need to make left/right move logic generic in some kind of PageContainer to go with PageTabControl, but we have only 2 pages for now /shrug
            switch (page)
            {
                case MatchHeaderPage.Settings:
                    info
                        .MoveToX(300, 500f, Easing.OutExpo)
                        .FadeOutFromOne(200);
                    participants
                        .MoveToX(300, 500f, Easing.OutExpo)
                        .FadeOutFromOne(200);
                    settings
                        .MoveToX(0, 500f, Easing.OutExpo)
                        .FadeInFromZero(200);

                    break;

                case MatchHeaderPage.Room:
                    info
                        .MoveToX(0, 500f, Easing.OutExpo)
                        .FadeInFromZero(200);
                    participants
                        .MoveToX(0, 500f, Easing.OutExpo)
                        .FadeInFromZero(200);
                    settings
                        .MoveToX(-300, 500f, Easing.OutExpo)
                        .FadeOutFromOne(200);

                    break;
            }

            currentPage = page;
        }
    }
}
