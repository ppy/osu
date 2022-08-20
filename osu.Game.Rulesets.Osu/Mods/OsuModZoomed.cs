// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Utils;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;
using osuTK;

namespace osu.Game.Rulesets.Osu.Mods
{
    internal class OsuModZoomed : Mod, IUpdatableByPlayfield, IApplicableToScoreProcessor, IApplicableToDrawableRuleset<OsuHitObject>
    {
        public override string Name => "Zoomed";
        public override string Acronym => "ZM";
        public override IconUsage? Icon => FontAwesome.Solid.Glasses;
        public override ModType Type => ModType.Fun;
        public override string Description => "Big brother is watching your cursor.";
        public override double ScoreMultiplier => 1;

        private const int last_zoom_combo = 200;
        private const int zoom_every_combo_amount = 100;
        private const int apply_zoom_duration = 1000;
        private const double default_zoom = 1.8;
        private const double zoom_with_combo_by = 0.1;

        [SettingSource("Delay", "Delay in milliseconds for the view to catch up to the cursor")]
        public BindableInt MovementDelay { get; } = new BindableInt
        {
            MinValue = 0,
            MaxValue = 1000,
            Precision = 100,
        };

        [SettingSource("Initial zoom", "The starting zoom level")]
        public BindableDouble InitialZoom { get; } = new BindableDouble(default_zoom)
        {
            MinValue = 1.5,
            MaxValue = 2,
            Precision = 0.05
        };

        [SettingSource("Increase zoom with combo", "Whether zoom should increase as combo increases")]
        public BindableBool ComboBasedZoom { get; } = new BindableBool(true);

        private int cameraDelay => MovementDelay.Value;

        private double baseZoom => InitialZoom.Value;

        private double currentZoom;

        private readonly BindableInt currentCombo = new BindableInt();

        private IFrameStableClock? gameplayClock;

        public void ApplyToDrawableRuleset(DrawableRuleset<OsuHitObject> drawableRuleset)
        {
            currentZoom = baseZoom;
            gameplayClock = drawableRuleset.FrameStableClock;
        }

        public void Update(Playfield playfield)
        {
            Debug.Assert(gameplayClock != null);

            double currentScale = Interpolation.ValueAt(Math.Min(Math.Abs(gameplayClock.ElapsedFrameTime), apply_zoom_duration), playfield.Scale.X, currentZoom, 0, apply_zoom_duration, Easing.Out);

            playfield.Scale = new Vector2((float)currentScale);

            moveDrawablesFollowingCursor(playfield, gameplayClock);
        }

        private Vector2 getDrawablePositionForCursorPosition(Playfield playfield, Drawable drawable)
        {
            var position = playfield.Cursor.ActiveCursor.DrawPosition;
            return Vector2.Clamp(playfield.OriginPosition - position, -playfield.OriginPosition, playfield.OriginPosition);
        }

        private void moveDrawablesFollowingCursor(Playfield playfield, IFrameStableClock gameplayClock)
        {
            var trackingPosition = getDrawablePositionForCursorPosition(playfield, playfield);

            if (cameraDelay == 0)
                playfield.Position = trackingPosition;
            else
            {
                playfield.Position = Interpolation.ValueAt(
                    Math.Min(Math.Abs(gameplayClock.ElapsedFrameTime), cameraDelay), playfield.Position, trackingPosition, 0, cameraDelay, Easing.Out);
            }
        }

        public void ApplyToScoreProcessor(ScoreProcessor scoreProcessor)
        {
            if (ComboBasedZoom.Value)
            {
                scoreProcessor.Combo.BindTo(currentCombo);
                currentCombo.ValueChanged += e => currentZoom = getZoomForCombo(e.NewValue);
            }
        }

        private double getZoomForCombo(int combo)
        {
            double setCombo = Math.Min(combo, last_zoom_combo);
            return baseZoom + zoom_with_combo_by * Math.Floor(setCombo / zoom_every_combo_amount);
        }

        public ScoreRank AdjustRank(ScoreRank rank, double accuracy)
        {
            switch (rank)
            {
                case ScoreRank.X:
                    return ScoreRank.XH;

                case ScoreRank.S:
                    return ScoreRank.SH;

                default:
                    return rank;
            }
        }
    }
}
