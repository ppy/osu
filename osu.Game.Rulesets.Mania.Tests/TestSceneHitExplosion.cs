// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.Mania.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Tests.Visual;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.Tests
{
    [TestFixture]
    public class TestSceneHitExplosion : OsuTestScene
    {
        private ScrollingTestContainer scrolling;

        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(DrawableNote),
            typeof(DrawableManiaHitObject),
        };

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Child = scrolling = new ScrollingTestContainer(ScrollingDirection.Down)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativePositionAxes = Axes.Y,
                Y = -0.25f,
                Size = new Vector2(Column.COLUMN_WIDTH, NotePiece.NOTE_HEIGHT),
            };

            int runcount = 0;

            AddRepeatStep("explode", () =>
            {
                runcount++;

                if (runcount % 15 > 12)
                    return;

                scrolling.AddRange(new Drawable[]
                {
                    new HitExplosion((runcount / 15) % 2 == 0 ? new Color4(94, 0, 57, 255) : new Color4(6, 84, 0, 255), runcount % 6 != 0)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    }
                });
            }, 100);
        }
    }
}
