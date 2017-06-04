// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Configuration;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.Containers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Game.Screens.Menu
{
    public class KiaiEffectContainer : BeatSyncedContainer
    {
        public override bool HandleInput => false;

        private readonly Bindable<WorkingBeatmap> beatmap = new Bindable<WorkingBeatmap>();

        private const double triangle_fade_in_time = 80;
        private const float triangle_max_alpha = 0.6f;
        private const float triangle_velocity = 2;

        private Color4 triangleColour;

        public KiaiEffectContainer()
        {
            EarlyActivationMilliseconds = triangle_fade_in_time;

            RelativeSizeAxes = Axes.Both;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
        }

        [BackgroundDependencyLoader]
        private void load(OsuGameBase game, OsuColour colours)
        {
            beatmap.BindTo(game.Beatmap);
            triangleColour = colours.Blue.Opacity(triangle_max_alpha);
        }

        private bool wasKiai;

        protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, TrackAmplitudes amplitudes)
        {
            if (beatIndex < 0)
                return;

            if (effectPoint.KiaiMode && !wasKiai)
            {
                Triangles left = new effectTriangles(triangleColour)
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomCentre,
                    Rotation = 45f,
                    Position = new Vector2(-DrawWidth, DrawHeight) / 4f
                };
                Triangles right = new effectTriangles(triangleColour)
                {
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomCentre,
                    Rotation = -45f,
                    Position = DrawSize / 4f
                };

                Add(new[]
                {
                    left,
                    right
                });

                DoEffect(left, timingPoint);
                DoEffect(right, timingPoint);
            }

            wasKiai = effectPoint.KiaiMode;
        }

        public void DoEffect(Triangles triangles, TimingControlPoint control)
        {
            triangles.MoveTo(Vector2.Zero, control.BeatLength / triangle_velocity, EasingTypes.OutQuad);
            triangles.FadeInFromZero(triangle_fade_in_time);
            
            // Lasts for two bars
            using (triangles.BeginDelayedSequence(control.Time, false))
                triangles.FadeOut(control.Time, EasingTypes.Out);
        }

        private class effectTriangles : Triangles
        {
            protected override bool CreateNewTriangles => true;
            protected override bool ExpireOffScreenTriangles => false;
            
            public effectTriangles(Color4 colour)
            {
                AlwaysPresent = true;
                Origin = Anchor.Centre;
                RelativeSizeAxes = Axes.Both;
                Size = new Vector2(1f, 0.5f);
                Velocity = triangle_velocity;
                Colour = colour;
                ColourLight = Color4.White;
                ColourDark = Color4.Transparent;

                TriangleScale = 1.8f;
            }
        }
    }
}
