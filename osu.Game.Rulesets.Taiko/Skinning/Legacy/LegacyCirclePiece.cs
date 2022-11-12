// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Animations;
using osu.Framework.Graphics.Primitives;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Screens.Play;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Taiko.Skinning.Legacy
{
    public class LegacyCirclePiece : BeatSyncedContainer, IHasAccentColour
    {
        private Drawable backgroundLayer = null!;
        private Drawable? foregroundLayer;

        private Bindable<int> currentCombo { get; } = new BindableInt();

        private bool enableAnimations;
        private int animationFrame;

        // required for editor blueprints (not sure why these circle pieces are zero size).
        public override Quad ScreenSpaceDrawQuad => backgroundLayer.ScreenSpaceDrawQuad;

        public LegacyCirclePiece()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [Resolved]
        private GameplayState? gameplayState { get; set; }

        [BackgroundDependencyLoader]
        private void load(ISkinSource skin, DrawableHitObject drawableHitObject)
        {
            Drawable? getDrawableFor(string lookup)
            {
                const string normal_hit = "taikohit";
                const string big_hit = "taikobig";

                string prefix = ((drawableHitObject.HitObject as TaikoStrongableHitObject)?.IsStrong ?? false) ? big_hit : normal_hit;

                return skin.GetAnimation($"{prefix}{lookup}", true, false) ??
                       // fallback to regular size if "big" version doesn't exist.
                       skin.GetAnimation($"{normal_hit}{lookup}", true, false);
            }

            // backgroundLayer is guaranteed to exist due to the pre-check in TaikoLegacySkinTransformer.
            AddInternal(backgroundLayer = new LegacyKiaiFlashingDrawable(() => getDrawableFor("circle")));

            foregroundLayer = getDrawableFor("circleoverlay");
            if (foregroundLayer != null)
                AddInternal(foregroundLayer);

            foreach (var c in InternalChildren)
            {
                (c as IFramedAnimation)?.Stop();

                c.Anchor = Anchor.Centre;
                c.Origin = Anchor.Centre;
            }

            if (gameplayState != null)
                currentCombo.BindTo(gameplayState.ScoreProcessor.Combo);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            updateAccentColour();
        }

        protected override void Update()
        {
            base.Update();

            // Not all skins (including the default osu-stable) have similar sizes for "hitcircle" and "hitcircleoverlay".
            // This ensures they are scaled relative to each other but also match the expected DrawableHit size.
            foreach (var c in InternalChildren)
                c.Scale = new Vector2(DrawHeight / 128);

            if (currentCombo.Value >= 150)
            {
                enableAnimations = true;
                Divisor = 4;
            }
            else if (currentCombo.Value >= 50)
            {
                enableAnimations = true;
                Divisor = 2;
            }
            else
            {
                enableAnimations = false;
                (foregroundLayer as IFramedAnimation)?.GotoFrame(0);
                Divisor = 2;
            }
        }

        private Color4 accentColour;

        public Color4 AccentColour
        {
            get => accentColour;
            set
            {
                if (value == accentColour)
                    return;

                accentColour = value;
                if (IsLoaded)
                    updateAccentColour();
            }
        }

        private void updateAccentColour()
        {
            backgroundLayer.Colour = LegacyColourCompatibility.DisallowZeroAlpha(accentColour);
        }

        protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, ChannelAmplitudes amplitudes)
        {
            if (!enableAnimations)
                return;

            animationFrame = beatIndex % 4 == 0 || (beatIndex - 1) % 4 == 0 ? 1 : 0;

            (foregroundLayer as IFramedAnimation)?.GotoFrame(animationFrame);
        }
    }
}
