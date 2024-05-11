// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mania.Judgements;
using osu.Game.Rulesets.Mania.Skinning.Default;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.Objects;
using osuTK;

namespace osu.Game.Rulesets.Mania.Tests.Skinning
{
    [TestFixture]
    public partial class TestSceneHitExplosion : ManiaSkinnableTestScene
    {
        private readonly List<DrawablePool<PoolableHitExplosion>> hitExplosionPools = new List<DrawablePool<PoolableHitExplosion>>();

        public TestSceneHitExplosion()
        {
            int runCount = 0;

            AddRepeatStep("explode", () =>
            {
                runCount++;

                if (runCount % 15 > 12)
                    return;

                int poolIndex = 0;

                foreach (var c in CreatedDrawables.OfType<Container>())
                {
                    c.Add(hitExplosionPools[poolIndex].Get(e =>
                    {
                        e.Apply(new JudgementResult(new HitObject(), new ManiaJudgement()));

                        e.Anchor = Anchor.Centre;
                        e.Origin = Anchor.Centre;
                    }));

                    poolIndex++;
                }
            }, 100);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            SetContents(_ =>
            {
                var pool = new DrawablePool<PoolableHitExplosion>(5);
                hitExplosionPools.Add(pool);

                return new ColumnTestContainer(0, ManiaAction.Key1)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativePositionAxes = Axes.Y,
                    Y = -0.25f,
                    Size = new Vector2(Column.COLUMN_WIDTH, DefaultNotePiece.NOTE_HEIGHT),
                    Child = pool
                };
            });
        }
    }
}
