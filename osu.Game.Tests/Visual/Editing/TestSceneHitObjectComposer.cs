// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Edit;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit;
using osuTK;

namespace osu.Game.Tests.Visual.Editing
{
    [TestFixture]
    public class TestSceneHitObjectComposer : EditorClockTestScene
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            Beatmap.Value = CreateWorkingBeatmap(new Beatmap
            {
                HitObjects = new List<HitObject>
                {
                    new HitCircle { Position = new Vector2(256, 192), Scale = 0.5f },
                    new HitCircle { Position = new Vector2(344, 148), Scale = 0.5f },
                    new Slider
                    {
                        Position = new Vector2(128, 256),
                        Path = new SliderPath(PathType.Linear, new[]
                        {
                            Vector2.Zero,
                            new Vector2(216, 0),
                        }),
                        Scale = 0.5f,
                    }
                },
            });

            var editorBeatmap = new EditorBeatmap(Beatmap.Value.GetPlayableBeatmap(new OsuRuleset().RulesetInfo));

            var clock = new DecoupleableInterpolatingFramedClock { IsCoupled = false };
            Dependencies.CacheAs<IAdjustableClock>(clock);
            Dependencies.CacheAs<IFrameBasedClock>(clock);
            Dependencies.CacheAs(editorBeatmap);
            Dependencies.CacheAs<IBeatSnapProvider>(editorBeatmap);

            Child = new OsuHitObjectComposer(new OsuRuleset());
        }
    }
}
