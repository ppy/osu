// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Utils;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osuTK;

namespace osu.Game.Tests.Visual.Gameplay
{
    [TestFixture]
    public class TestSceneStarCounter : OsuTestScene
    {
        public TestSceneStarCounter()
        {
            StarCounter stars = new StarCounter
            {
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre,
                Current = 5,
            };

            Add(stars);

            SpriteText starsLabel = new OsuSpriteText
            {
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre,
                Scale = new Vector2(2),
                Y = 50,
                Text = stars.Current.ToString("0.00"),
            };

            Add(starsLabel);

            AddRepeatStep(@"random value", delegate
            {
                stars.Current = RNG.NextSingle() * (stars.StarCount + 1);
                starsLabel.Text = stars.Current.ToString("0.00");
            }, 10);

            AddStep(@"Stop animation", delegate
            {
                stars.StopAnimation();
            });

            AddStep(@"Reset", delegate
            {
                stars.Current = 0;
                starsLabel.Text = stars.Current.ToString("0.00");
            });
        }
    }
}
