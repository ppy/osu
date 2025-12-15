// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Input.Events;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Rooms;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Match.BeatmapSelect
{
    public partial class MatchmakingSelectPanelRandom : MatchmakingSelectPanel
    {
        public MatchmakingSelectPanelRandom(MultiplayerPlaylistItem item)
            : base(item)
        {
        }

        private CardContentRandom content = null!;
        private Drawable diceProxy = null!;
        private readonly List<APIUser> users = new List<APIUser>();

        private Sample? resultSample;
        private Sample? swooshSample;

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            resultSample = audio.Samples.Get(@"Multiplayer/Matchmaking/Selection/roulette-result");
            swooshSample = audio.Samples.Get(@"SongSelect/options-pop-out");

            Add(content = new CardContentRandom());

            AddInternal(diceProxy = content.Dice.CreateProxy());
        }

        public override void PresentAsChosenBeatmap(MatchmakingPlaylistItem playlistItem)
        {
            const double duration = 800;

            this.MoveTo(Vector2.Zero, 1000, Easing.OutExpo)
                .ScaleTo(1.5f, 1000, Easing.OutExpo);

            content.Dice.MoveToY(-200, duration * 0.55, new CubicBezierEasingFunction(0.33, 1, 0.8, 1))
                   .Then()
                   .Schedule(() => ChangeInternalChildDepth(diceProxy, float.MaxValue))
                   .MoveToY(-DrawHeight / 2, duration * 0.45, new CubicBezierEasingFunction(0.2, 0, 0.55, 0))
                   .Then()
                   .FadeOut()
                   .Expire();

            content.Dice.RotateTo(content.Dice.Rotation - 360 * 5, duration * 1.3f, Easing.Out);
            content.Label.FadeOut(200).Expire();

            swooshSample?.Play();

            Scheduler.AddDelayed(() =>
            {
                content.Expire();

                var flashLayer = new Box { RelativeSizeAxes = Axes.Both };

                AddRange(new Drawable[]
                {
                    new CardContentBeatmap(playlistItem.Beatmap, playlistItem.Mods),
                    flashLayer,
                });

                foreach (var user in users)
                    content.SelectionOverlay.AddUser(user);

                flashLayer.FadeOutFromOne(1000, Easing.In);

                ScaleContainer.ScaleTo(0.92f, 120, Easing.Out)
                              .Then()
                              .ScaleTo(1f, 600, Easing.OutElasticHalf);

                resultSample?.Play();
            }, duration);
        }

        public override void AddUser(APIUser user)
        {
            users.Add(user);
            content.SelectionOverlay.AddUser(user);
        }

        public override void RemoveUser(APIUser user)
        {
            users.Remove(user);
            content.SelectionOverlay.RemoveUser(user.Id);
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (AllowSelection && content is CardContentRandom randomContent)
                randomContent.RollDice();

            return base.OnClick(e);
        }
    }
}
