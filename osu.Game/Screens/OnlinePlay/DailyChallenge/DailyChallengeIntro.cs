// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Screens;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Rooms;

namespace osu.Game.Screens.OnlinePlay.DailyChallenge
{
    public partial class DailyChallengeIntro : OsuScreen
    {
        private readonly Room room;

        public DailyChallengeIntro(Room room)
        {
            this.room = room;

            ValidForResume = false;
        }

        public override void OnEntering(ScreenTransitionEvent e)
        {
            base.OnEntering(e);

            InternalChildren = new Drawable[]
            {
                new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Text = "wangs"
                }
            };

            Scheduler.AddDelayed(() =>
            {
                this.Push(new DailyChallenge(room));
            }, 2000);
        }
    }
}
