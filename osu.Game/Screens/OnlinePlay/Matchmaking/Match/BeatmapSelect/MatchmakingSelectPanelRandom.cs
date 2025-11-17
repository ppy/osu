// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Input.Events;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets.Mods;

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

        [BackgroundDependencyLoader]
        private void load()
        {
            Add(content = new CardContentRandom());

            AddInternal(diceProxy = content.Dice.CreateProxy());
        }

        public void RevealBeatmap(APIBeatmap beatmap, Mod[] mods)
        {
            const double duration = 800;

            content.Dice.MoveToY(-200, duration * 0.55, new PowEasingFunction(2.75, easeOut: true))
                   .Then()
                   .Schedule(() => ChangeInternalChildDepth(diceProxy, float.MaxValue))
                   .MoveToY(-DrawHeight / 2, duration * 0.45, new PowEasingFunction(2.2))
                   .Then()
                   .FadeOut()
                   .Expire();

            content.Dice.RotateTo(content.Dice.Rotation - 360 * 5, duration * 1.3f, Easing.Out);
            content.Label.FadeOut(200).Expire();

            Scheduler.AddDelayed(() =>
            {
                content.Expire();

                var flashLayer = new Box { RelativeSizeAxes = Axes.Both };

                AddRange(new Drawable[]
                {
                    new CardContentBeatmap(beatmap, mods),
                    flashLayer,
                });

                foreach (var user in users)
                    content.SelectionOverlay.AddUser(user);

                flashLayer.FadeOutFromOne(1000, Easing.In);
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

        private readonly struct PowEasingFunction(double exponent, bool easeOut = false) : IEasingFunction
        {
            public double ApplyEasing(double time)
            {
                if (easeOut)
                    time = 1 - time;

                double value = Math.Pow(time, exponent);

                return easeOut ? 1 - value : value;
            }
        }
    }
}
