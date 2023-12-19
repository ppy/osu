// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.UI;

namespace osu.Game.Rulesets.Taiko.Tests.Skinning
{
    public partial class TestSceneDrawableJudgement : TaikoSkinnableTestScene
    {
        private readonly List<DrawablePool<DrawableTaikoJudgement>> pools;

        public TestSceneDrawableJudgement()
        {
            pools = new List<DrawablePool<DrawableTaikoJudgement>>();

            foreach (HitResult result in Enum.GetValues(typeof(HitResult)).OfType<HitResult>().Skip(1))
                showResult(result);
        }

        private void showResult(HitResult result)
        {
            AddStep("Show " + result.GetDescription(), () =>
            {
                int poolIndex = 0;

                SetContents(_ =>
                {
                    DrawablePool<DrawableTaikoJudgement> pool;

                    if (poolIndex >= pools.Count)
                        pools.Add(pool = new DrawablePool<DrawableTaikoJudgement>(1));
                    else
                    {
                        pool = pools[poolIndex];

                        // We need to make sure neither the pool nor the judgement get disposed when new content is set, and they both share the same parent.
                        ((Container)pool.Parent!).Clear(false);
                    }

                    var container = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Children = new Drawable[]
                        {
                            pool,
                            pool.Get(j => j.Apply(new JudgementResult(new HitObject
                            {
                                StartTime = Time.Current
                            }, new Judgement())
                            {
                                Type = result,
                            }, null)).With(j =>
                            {
                                j.Anchor = Anchor.Centre;
                                j.Origin = Anchor.Centre;
                            })
                        }
                    };

                    poolIndex++;
                    return container;
                });
            });
        }
    }
}
