// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Framework.Utils;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;
using osuTK;

namespace osu.Game.Rulesets.Osu.Mods
{
    internal class OsuModZoomed : Mod, IUpdatableByPlayfield, IApplicableToScoreProcessor
    {
        public override string Name => "Zoomed";
        public override string Acronym => "ZM";
        public override IconUsage? Icon => FontAwesome.Solid.Glasses;
        public override ModType Type => ModType.Fun;
        public override LocalisableString Description => "Big brother is watching your cursor.";
        public override double ScoreMultiplier => 1;
        public override Type[] IncompatibleMods => new[] { typeof(OsuModBarrelRoll) };

        [SettingSource("Delay", "Delay in milliseconds for the view to catch up to the cursor")]
        public BindableInt MovementDelay { get; } = new BindableInt
        {
            MinValue = 0,
            MaxValue = 1000,
            Precision = 100,
        };

        [SettingSource("Initial zoom", "The starting zoom level")]
        public BindableDouble InitialZoom { get; } = new BindableDouble(1.5)
        {
            MinValue = 1,
            MaxValue = 2,
            Precision = 0.05
        };

        [SettingSource("Final zoom", "The starting zoom level")]
        public BindableDouble FinalZoom { get; } = new BindableDouble(1.8)
        {
            MinValue = 1,
            MaxValue = 2,
            Precision = 0.05
        };

        [SettingSource("Final zoom at combo", "The combo count at which point the zoom level stops increasing.", SettingControlType = typeof(SettingsSlider<int, OsuSliderBar<int>>))]
        public BindableInt FinalZoomCombo { get; } = new BindableInt
        {
            Default = 100,
            Value = 100,
            MinValue = 0,
            MaxValue = 500,
        };

        private readonly BindableInt currentCombo = new BindableInt();

        public void Update(Playfield playfield)
        {
            double zoom = InitialZoom.Value + (FinalZoom.Value - InitialZoom.Value) * Math.Min(1, ((float)currentCombo.Value / FinalZoomCombo.Value));
            Vector2 position = playfield.OriginPosition - playfield.Cursor.ActiveCursor.DrawPosition;

            playfield.Scale = new Vector2((float)Interpolation.DampContinuously(playfield.Scale.X, zoom, 200, Math.Abs(playfield.Clock.ElapsedFrameTime)));

            if (MovementDelay.Value == 0)
            {
                playfield.Position = position;
            }
            else
            {
                playfield.Position = new Vector2(
                    (float)Interpolation.DampContinuously(playfield.Position.X, position.X, MovementDelay.Value, Math.Abs(playfield.Clock.ElapsedFrameTime)),
                    (float)Interpolation.DampContinuously(playfield.Position.Y, position.Y, MovementDelay.Value, Math.Abs(playfield.Clock.ElapsedFrameTime))
                );
            }
        }

        public void ApplyToScoreProcessor(ScoreProcessor scoreProcessor)
        {
            currentCombo.BindTo(scoreProcessor.Combo);
        }

        public ScoreRank AdjustRank(ScoreRank rank, double accuracy) => rank;
    }
}
