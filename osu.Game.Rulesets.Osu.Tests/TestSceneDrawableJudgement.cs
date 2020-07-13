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
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Tests
{
    public class TestSceneDrawableJudgement : OsuSkinnableTestScene
    {
        public TestSceneDrawableJudgement()
        {
            var pools = new List<DrawablePool<DrawableOsuJudgement>>();

            foreach (HitResult result in Enum.GetValues(typeof(HitResult)).OfType<HitResult>().Skip(1))
            {
                AddStep("Show " + result.GetDescription(), () =>
                {
                    int poolIndex = 0;

                    SetContents(() =>
                    {
                        DrawablePool<DrawableOsuJudgement> pool;

                        if (poolIndex >= pools.Count)
                            pools.Add(pool = new DrawablePool<DrawableOsuJudgement>(1));
                        else
                        {
                            pool = pools[poolIndex];

                            // We need to make sure neither the pool nor the judgement get disposed when new content is set, and they both share the same parent.
                            ((Container)pool.Parent).Clear(false);
                        }

                        var container = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                pool,
                                pool.Get(j => j.Apply(new JudgementResult(new HitObject(), new Judgement()) { Type = result }, null)).With(j =>
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
}
