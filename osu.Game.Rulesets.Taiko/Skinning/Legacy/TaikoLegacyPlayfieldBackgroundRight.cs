// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics.Containers;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Taiko.Skinning.Legacy
{
    public partial class TaikoLegacyPlayfieldBackgroundRight : BeatSyncedContainer
    {
        private Sprite kiai = null!;

        private bool isKiaiActive;

        [BackgroundDependencyLoader]
        private void load(ISkinSource skin)
        {
            RelativeSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                new Sprite
                {
                    Texture = skin.GetTexture("taiko-bar-right"),
                    RelativeSizeAxes = Axes.Both,
                    Size = Vector2.One,
                },
                kiai = new Sprite
                {
                    Texture = skin.GetTexture("taiko-bar-right-glow"),
                    RelativeSizeAxes = Axes.Both,
                    Size = Vector2.One,
                    Alpha = 0,
                }
            };
        }

        protected override void Update()
        {
            base.Update();

            if (isKiaiActive)
                kiai.Alpha = (float)Math.Min(1, kiai.Alpha + Math.Abs(Clock.ElapsedFrameTime) / 200f);
            else
                kiai.Alpha = (float)Math.Max(0, kiai.Alpha - Math.Abs(Clock.ElapsedFrameTime) / 200f);
        }

        protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, ChannelAmplitudes amplitudes)
        {
            isKiaiActive = effectPoint.KiaiMode;
        }
    }
}
