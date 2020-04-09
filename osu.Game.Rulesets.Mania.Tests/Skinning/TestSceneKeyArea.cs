// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Mania.Skinning;
using osu.Game.Rulesets.Mania.UI.Components;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Mania.Tests.Skinning
{
    public class TestSceneKeyArea : ManiaSkinnableTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(DefaultKeyArea),
            typeof(LegacyKeyArea)
        };

        [BackgroundDependencyLoader]
        private void load()
        {
            SetContents(() => new FillFlowContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Size = new Vector2(0.8f),
                Direction = FillDirection.Horizontal,
                Children = new Drawable[]
                {
                    new ColumnTestContainer(0, ManiaAction.Key1)
                    {
                        RelativeSizeAxes = Axes.Both,
                        Width = 0.5f,
                        Child = new SkinnableDrawable(new ManiaSkinComponent(ManiaSkinComponents.KeyArea, 0), _ => new DefaultKeyArea())
                        {
                            RelativeSizeAxes = Axes.Both
                        },
                    },
                    new ColumnTestContainer(1, ManiaAction.Key2)
                    {
                        RelativeSizeAxes = Axes.Both,
                        Width = 0.5f,
                        Child = new SkinnableDrawable(new ManiaSkinComponent(ManiaSkinComponents.KeyArea, 1), _ => new DefaultKeyArea())
                        {
                            RelativeSizeAxes = Axes.Both
                        },
                    },
                }
            });
        }
    }
}
