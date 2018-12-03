// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.Multi.Components;
using osu.Game.Screens.Select;
using osu.Game.Users;

namespace osu.Game.Screens.Multi.Screens.Match
{
    public class Match : MultiplayerScreen
    {
        private readonly Room room;
        private readonly Participants participants;

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

        public Match(Room room)
        {
            this.room = room;
            Header header;
            RoomSettingsOverlay settings;
            Info info;

            Children = new Drawable[]
            {
                header = new Header
                {
                    Depth = -1,
                },
                info = new Info
                {
                    Margin = new MarginPadding { Top = Header.HEIGHT },
                },
                participants = new Participants
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Top = Header.HEIGHT + Info.HEIGHT },
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Top = Header.HEIGHT },
                    Child = settings = new RoomSettingsOverlay(room)
                    {
                        RelativeSizeAxes = Axes.Both,
                        Height = 0.9f,
                    },
                },
            };

            header.OnRequestSelectBeatmap = () => Push(new MatchSongSelect());

            beatmapBind.BindTo(room.Beatmap);
            beatmapBind.BindValueChanged(b =>
            {
                header.BeatmapSet = b?.BeatmapSet;
                info.Beatmap = b;
            }, true);

            header.Tabs.Current.ValueChanged += t =>
            {
                if (t == MatchHeaderPage.Settings)
                    settings.Show();
                else
                    settings.Hide();
            };

            settings.StateChanged += s =>
            {
                if (s == Visibility.Hidden)
                    header.Tabs.Current.Value = MatchHeaderPage.Room;
            };

            nameBind.BindTo(room.Name);
            nameBind.BindValueChanged(n => info.Name = n, true);

            statusBind.BindTo(room.Status);
            statusBind.BindValueChanged(s => info.Status = s, true);

            availabilityBind.BindTo(room.Availability);
            availabilityBind.BindValueChanged(a => info.Availability = a, true);

            typeBind.BindTo(room.Type);
            typeBind.BindValueChanged(t => info.Type = t, true);

            maxParticipantsBind.BindTo(room.MaxParticipants);
            maxParticipantsBind.BindValueChanged(m => { participants.Max = m; }, true);

            participantsBind.BindTo(room.Participants);
            participantsBind.BindValueChanged(p => participants.Users = p, true);
        }
    }
}
