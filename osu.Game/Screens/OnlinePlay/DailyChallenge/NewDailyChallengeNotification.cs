// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Screens;
using osu.Game.Beatmaps.Drawables.Cards;
using osu.Game.Configuration;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Rooms;
using osu.Game.Overlays.Notifications;
using osu.Game.Screens.Menu;
using osu.Game.Localisation;

namespace osu.Game.Screens.OnlinePlay.DailyChallenge
{
    public partial class NewDailyChallengeNotification : SimpleNotification
    {
        private readonly Room room;

        private BeatmapCardNano card = null!;

        public NewDailyChallengeNotification(Room room)
        {
            this.room = room;
        }

        [BackgroundDependencyLoader]
        private void load(OsuGame? game, SessionStatics statics)
        {
            Text = DailyChallengeStrings.ChallengeLiveNotification;
            Content.Add(card = new BeatmapCardNano((APIBeatmapSet)room.Playlist.Single().Beatmap.BeatmapSet!));
            Activated = () =>
            {
                if (statics.Get<bool>(Static.DailyChallengeIntroPlayed))
                    game?.PerformFromScreen(s => s.Push(new DailyChallenge(room)), [typeof(MainMenu)]);
                else
                    game?.PerformFromScreen(s => s.Push(new DailyChallengeIntro(room)), [typeof(MainMenu)]);

                return true;
            };
        }

        protected override void Update()
        {
            base.Update();
            card.Width = Content.DrawWidth;
        }
    }
}
