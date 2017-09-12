// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Timing;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using OpenTK;
using osu.Game.Rulesets.Osu;
using osu.Framework.Allocation;
using osu.Game.Rulesets;

namespace osu.Desktop.Tests.Visual
{
    internal class TestCaseHitObjects : OsuTestCase
    {
        private FramedClock framedClock;

        private bool auto;

        [BackgroundDependencyLoader]
        private void load(RulesetStore rulesets)
        {
            var rateAdjustClock = new StopwatchClock(true);
            framedClock = new FramedClock(rateAdjustClock);

            AddStep(@"circles", () => loadHitobjects(HitObjectType.Circle));
            AddStep(@"slider", () => loadHitobjects(HitObjectType.Slider));
            AddStep(@"spinner", () => loadHitobjects(HitObjectType.Spinner));

            AddToggleStep("Auto", state => { auto = state; loadHitobjects(mode); });
            AddSliderStep("Playback speed", 0.0, 2.0, 0.5, v => rateAdjustClock.Rate = v);

            framedClock.ProcessFrame();

            var clockAdjustContainer = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Clock = framedClock,
                Children = new[]
                {
                    playfieldContainer = new OsuInputManager(rulesets.GetRuleset(0)) { RelativeSizeAxes = Axes.Both },
                    approachContainer = new Container { RelativeSizeAxes = Axes.Both }
                }
            };

            Add(clockAdjustContainer);
        }

        private HitObjectType mode = HitObjectType.Slider;

        private Container playfieldContainer;
        private Container approachContainer;

        private void loadHitobjects(HitObjectType mode)
        {
            this.mode = mode;

            switch (mode)
            {
                case HitObjectType.Circle:
                    const int count = 10;

                    for (int i = 0; i < count; i++)
                    {
                        var h = new HitCircle
                        {
                            StartTime = framedClock.CurrentTime + 600 + i * 80,
                            Position = new Vector2((i - count / 2) * 14),
                        };

                        add(new DrawableHitCircle(h));
                    }
                    break;
                case HitObjectType.Slider:
                    add(new DrawableSlider(new Slider
                    {
                        StartTime = framedClock.CurrentTime + 600,
                        ControlPoints = new List<Vector2>
                        {
                            new Vector2(-200, 0),
                            new Vector2(400, 0),
                        },
                        Distance = 400,
                        Position = new Vector2(-200, 0),
                        Velocity = 1,
                        TickDistance = 100,
                    }));
                    break;
                case HitObjectType.Spinner:
                    add(new DrawableSpinner(new Spinner
                    {
                        StartTime = framedClock.CurrentTime + 600,
                        EndTime = framedClock.CurrentTime + 1600,
                        Position = new Vector2(0, 0),
                    }));
                    break;
            }
        }

        private int depth;

        private void add(DrawableOsuHitObject h)
        {
            h.Anchor = Anchor.Centre;
            h.Depth = depth++;

            if (auto)
                h.State = ArmedState.Hit;

            playfieldContainer.Add(h);
            var proxyable = h as IDrawableHitObjectWithProxiedApproach;
            if (proxyable != null)
                approachContainer.Add(proxyable.ProxiedLayer.CreateProxy());
        }

        private enum HitObjectType
        {
            Circle,
            Slider,
            Spinner
        }
    }
}
