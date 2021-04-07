// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
    public class TaikoLegacyPlayfieldBackgroundRight : BeatSyncedContainer
    {
        private Sprite kiai;

        private bool kiaiDisplayed;

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

        protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, ChannelAmplitudes amplitudes)
        {
            base.OnNewBeat(beatIndex, timingPoint, effectPoint, amplitudes);

            if (effectPoint.KiaiMode != kiaiDisplayed)
            {
                kiaiDisplayed = effectPoint.KiaiMode;

                kiai.ClearTransforms();
                kiai.FadeTo(kiaiDisplayed ? 1 : 0, 200);
            }
        }
    }
}
