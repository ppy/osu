// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Mania.Skinning;
using osu.Game.Rulesets.Mania.UI.Components;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Mania.Tests.Skinning
{
    public class TestSceneStageBackground : ManiaSkinnableTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => base.RequiredTypes.Concat(new[]
        {
            typeof(DefaultStageBackground),
            typeof(LegacyStageBackground),
        }).ToList();

        [BackgroundDependencyLoader]
        private void load()
        {
            SetContents(() => new SkinnableDrawable(new ManiaSkinComponent(ManiaSkinComponents.StageBackground), _ => new DefaultStageBackground())
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Width = 0.5f,
            });
        }
    }
}
