// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Framework.Input.States;
using osu.Framework.Logging;
using osu.Framework.Testing.Input;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests
{
    public partial class TestSceneHitMarker : OsuSkinnableTestScene
    {
        [Test]
        public void TestHitMarkers()
        {
            var markerContainers = new List<HitMarkerContainer>();

            AddStep("Create hit markers", () =>
            {
                markerContainers.Clear();
                SetContents(_ =>
                {
                    markerContainers.Add(new TestHitMarkerContainer(new HitObjectContainer())
                    {
                        HitMarkerEnabled = { Value = true },
                        AimMarkersEnabled = { Value = true },
                        AimLinesEnabled = { Value = true },
                        RelativeSizeAxes = Axes.Both
                    });

                    return new HitMarkerInputManager
                    {
                        RelativeSizeAxes = Axes.Both,
                        Size = new Vector2(0.95f),
                        Child = markerContainers[^1],
                    };
                });
            });

            AddUntilStep("Until skinnable expires", () =>
            {
                if (markerContainers.Count == 0)
                    return false;

                Logger.Log("How many: " + markerContainers.Count);

                foreach (var markerContainer in markerContainers)
                {
                    if (markerContainer.Children.Count != 0)
                        return false;
                }

                return true;
            });
        }

        private partial class HitMarkerInputManager : ManualInputManager
        {
            private double? startTime;

            public HitMarkerInputManager()
            {
                UseParentInput = false;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                MoveMouseTo(ToScreenSpace(DrawSize / 2));
            }

            protected override void Update()
            {
                base.Update();

                const float spin_angle = 4 * MathF.PI;

                startTime ??= Time.Current;

                float fraction = (float)((Time.Current - startTime) / 5_000);

                float angle = fraction * spin_angle;
                float radius = fraction * Math.Min(DrawSize.X, DrawSize.Y) / 2;

                Vector2 pos = radius * new Vector2(MathF.Cos(angle), MathF.Sin(angle)) + DrawSize / 2;
                MoveMouseTo(ToScreenSpace(pos));
            }
        }

        private partial class TestHitMarkerContainer : HitMarkerContainer
        {
            private double? lastClick;
            private double? startTime;
            private bool finishedDrawing;
            private bool leftOrRight;

            public TestHitMarkerContainer(HitObjectContainer hitObjectContainer)
                : base(hitObjectContainer)
            {
            }

            protected override void Update()
            {
                base.Update();

                if (finishedDrawing)
                    return;

                startTime ??= lastClick ??= Time.Current;

                if (startTime + 5_000 <= Time.Current)
                {
                    finishedDrawing = true;
                    HitMarkerEnabled.Value = AimMarkersEnabled.Value = AimLinesEnabled.Value = false;
                    return;
                }

                if (lastClick + 400 <= Time.Current)
                {
                    OnPressed(new KeyBindingPressEvent<OsuAction>(new InputState(), leftOrRight ? OsuAction.LeftButton : OsuAction.RightButton));
                    leftOrRight = !leftOrRight;
                    lastClick = Time.Current;
                }
            }
        }
    }
}
