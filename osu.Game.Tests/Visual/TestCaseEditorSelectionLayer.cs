// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Edit.Layers.Selection;
using osu.Game.Rulesets.Osu.Edit;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;

namespace osu.Game.Tests.Visual
{
    public class TestCaseEditorSelectionLayer : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[] { typeof(SelectionLayer) };

        [BackgroundDependencyLoader]
        private void load()
        {
            var playfield = new OsuEditPlayfield();

            Children = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Clock = new FramedClock(new StopwatchClock()),
                    Child = playfield
                },
                new SelectionLayer(playfield)
            };

            var hitCircle1 = new HitCircle { Position = new Vector2(256, 192), Scale = 0.5f };
            var hitCircle2 = new HitCircle { Position = new Vector2(344, 148), Scale = 0.5f };
            var slider = new Slider
            {
                ControlPoints = new List<Vector2>
                {
                    new Vector2(128, 256),
                    new Vector2(344, 256),
                },
                Distance = 400,
                Position = new Vector2(128, 256),
                Velocity = 1,
                TickDistance = 100,
                Scale = 0.5f,
            };

            hitCircle1.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());
            hitCircle2.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());
            slider.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

            playfield.Add(new DrawableHitCircle(hitCircle1));
            playfield.Add(new DrawableHitCircle(hitCircle2));
            playfield.Add(new DrawableSlider(slider));
        }
    }
}
