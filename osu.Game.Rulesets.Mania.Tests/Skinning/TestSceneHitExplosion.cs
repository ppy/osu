// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Mania.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.Tests.Skinning
{
    [TestFixture]
    public class TestSceneHitExplosion : ManiaSkinnableTestScene
    {
        public TestSceneHitExplosion()
        {
            int runcount = 0;

            AddRepeatStep("explode", () =>
            {
                runcount++;

                if (runcount % 15 > 12)
                    return;

                CreatedDrawables.OfType<Container>().ForEach(c =>
                {
                    c.Add(new SkinnableDrawable(new ManiaSkinComponent(ManiaSkinComponents.HitExplosion, 0),
                        _ => new DefaultHitExplosion((runcount / 15) % 2 == 0 ? new Color4(94, 0, 57, 255) : new Color4(6, 84, 0, 255), runcount % 6 != 0)
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        }));
                });
            }, 100);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            SetContents(() => new ColumnTestContainer(0, ManiaAction.Key1)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativePositionAxes = Axes.Y,
                Y = -0.25f,
                Size = new Vector2(Column.COLUMN_WIDTH, DefaultNotePiece.NOTE_HEIGHT),
            });
        }
    }
}
