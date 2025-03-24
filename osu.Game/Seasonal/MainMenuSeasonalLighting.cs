// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Timing;
using osu.Framework.Utils;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Seasonal
{
    public partial class MainMenuSeasonalLighting : CompositeDrawable
    {
        private IBindable<WorkingBeatmap> working = null!;

        private InterpolatingFramedClock? beatmapClock;

        private List<HitObject> hitObjects = null!;

        private RulesetInfo? osuRuleset;

        private int? lastObjectIndex;

        public MainMenuSeasonalLighting()
        {
            // match beatmap playfield
            RelativeChildSize = new Vector2(512, 384);

            RelativeSizeAxes = Axes.X;

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
        }

        [BackgroundDependencyLoader]
        private void load(IBindable<WorkingBeatmap> working, RulesetStore rulesets)
        {
            // operate in osu! ruleset to keep things simple for now.
            osuRuleset = rulesets.GetRuleset(0);

            this.working = working.GetBoundCopy();
            this.working.BindValueChanged(_ => Scheduler.AddOnce(updateBeatmap), true);
        }

        private void updateBeatmap()
        {
            lastObjectIndex = null;

            if (osuRuleset == null)
            {
                beatmapClock = new InterpolatingFramedClock(Clock);
                hitObjects = new List<HitObject>();
                return;
            }

            // Intentionally maintain separately so the lighting is not in audio clock space (it shouldn't rewind etc.)
            beatmapClock = new InterpolatingFramedClock(new FramedClock(working.Value.Track));

            hitObjects = working.Value
                                .GetPlayableBeatmap(osuRuleset)
                                .HitObjects
                                .SelectMany(h => h.NestedHitObjects.Prepend(h))
                                .OrderBy(h => h.StartTime)
                                .ToList();
        }

        protected override void Update()
        {
            base.Update();

            if (osuRuleset == null || beatmapClock == null)
                return;

            Height = DrawWidth / 16 * 10;

            beatmapClock.ProcessFrame();

            // intentionally slightly early since we are doing fades on the lighting.
            double time = beatmapClock.CurrentTime + 50;

            // handle seeks or OOB by skipping to current.
            if (lastObjectIndex == null || lastObjectIndex >= hitObjects.Count || (lastObjectIndex >= 0 && hitObjects[lastObjectIndex.Value].StartTime > time)
                || Math.Abs(beatmapClock.ElapsedFrameTime) > 500)
                lastObjectIndex = hitObjects.Count(h => h.StartTime < time) - 1;

            while (lastObjectIndex < hitObjects.Count - 1)
            {
                var h = hitObjects[lastObjectIndex.Value + 1];

                if (h.StartTime > time)
                    break;

                // Don't add lighting if the game is running too slow.
                if (Clock.ElapsedFrameTime < 20)
                    addLight(h);

                lastObjectIndex++;
            }
        }

        private void addLight(HitObject h)
        {
            var light = new Light
            {
                RelativePositionAxes = Axes.Both,
                Position = ((IHasPosition)h).Position
            };

            AddInternal(light);

            if (h.GetType().Name.Contains("Tick"))
            {
                light.Colour = SeasonalUIConfig.AMBIENT_COLOUR_1;
                light.Scale = new Vector2(0.5f);
                light
                    .FadeInFromZero(250)
                    .Then()
                    .FadeOutFromOne(1000, Easing.Out);

                light.MoveToOffset(new Vector2(RNG.Next(-20, 20), RNG.Next(-20, 20)), 1400, Easing.Out);
            }
            else
            {
                // default are green
                Color4 col = SeasonalUIConfig.PRIMARY_COLOUR_2;

                // whistles are red
                if (h.Samples.Any(s => s.Name == HitSampleInfo.HIT_WHISTLE))
                    col = SeasonalUIConfig.PRIMARY_COLOUR_1;
                // clap is third ambient (yellow) colour
                else if (h.Samples.Any(s => s.Name == HitSampleInfo.HIT_CLAP))
                    col = SeasonalUIConfig.AMBIENT_COLOUR_1;

                light.Colour = col;

                // finish results in larger lighting
                if (h.Samples.Any(s => s.Name == HitSampleInfo.HIT_FINISH))
                    light.Scale = new Vector2(3);

                light
                    .FadeInFromZero(150)
                    .Then()
                    .FadeOutFromOne(1000, Easing.In);
            }

            light.Expire();
        }

        private partial class Light : CompositeDrawable
        {
            private readonly Circle circle;

            public new Color4 Colour
            {
                set
                {
                    circle.Colour = value.Darken(0.8f);
                    circle.EdgeEffect = new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Glow,
                        Colour = value,
                        Radius = 80,
                    };
                }
            }

            public Light()
            {
                InternalChildren = new Drawable[]
                {
                    circle = new Circle
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Size = new Vector2(12),
                        Colour = SeasonalUIConfig.AMBIENT_COLOUR_1,
                        Blending = BlendingParameters.Additive,
                        EdgeEffect = new EdgeEffectParameters
                        {
                            Type = EdgeEffectType.Glow,
                            Colour = SeasonalUIConfig.AMBIENT_COLOUR_2,
                            Radius = 80,
                        }
                    }
                };

                Origin = Anchor.Centre;
                Alpha = 0.5f;
            }
        }
    }
}
