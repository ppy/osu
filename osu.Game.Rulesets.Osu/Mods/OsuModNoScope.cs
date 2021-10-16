// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.Mods;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterface;
using osu.Game.Configuration;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModNoScope : Mod, IUpdatableByPlayfield, IApplicableToScoreProcessor, IApplicableToDrawableRuleset<OsuHitObject>
    {
        /// <summary>
        /// Slightly higher than the cutoff for <see cref="Drawable.IsPresent"/>.
        /// </summary>
        private const float min_alpha = 0.0002f;

        private const float transition_duration = 100;

        public override string Name => "No Scope";
        public override string Acronym => "NS";
        public override ModType Type => ModType.Fun;
        public override IconUsage? Icon => FontAwesome.Solid.EyeSlash;
        public override string Description => "Where's the cursor?";
        public override double ScoreMultiplier => 1;

        private BindableNumber<int> currentCombo;

        private readonly BindableNumber<float> targetAlpha = new BindableFloat
        {
            MinValue = min_alpha,
            MaxValue = 1
        };

        private readonly BindableBool rest = new BindableBool();

        private readonly BindableBool restSpinner = new BindableBool();

        [SettingSource(
            "Hidden at combo",
            "The combo count at which the cursor becomes completely hidden",
            SettingControlType = typeof(SettingsSlider<int, HiddenComboSlider>)
        )]
        public BindableInt HiddenComboCount { get; } = new BindableInt
        {
            Default = 10,
            Value = 10,
            MinValue = 0,
            MaxValue = 50,
        };

        [SettingSource("Show on rest", "Show cursor during breaks and spinners.")]
        public BindableBool ShowCursorDuringBreaks { get; } = new BindableBool();

        public OsuModNoScope()
        {
            HiddenComboCount.BindValueChanged(combo =>
            {
                if (combo.NewValue == 0)
                    ShowCursorDuringBreaks.Value = false;
            });

            ShowCursorDuringBreaks.BindValueChanged(show =>
            {
                if (show.NewValue && HiddenComboCount.Value == 0)
                    HiddenComboCount.Value = 1;
            });
        }

        public ScoreRank AdjustRank(ScoreRank rank, double accuracy) => rank;

        public class RestInfo
        {
            public double StartTime { get; set; }
            public bool IsSpinner { get; set; }

            public RestInfo(double startTime, bool isSpinner)
            {
                StartTime = startTime;
                IsSpinner = isSpinner;
            }
        }

        public void ApplyToDrawableRuleset(DrawableRuleset<OsuHitObject> drawableRuleset)
        {
            Beatmap<OsuHitObject> beatmap = drawableRuleset.Beatmap;
            GameplayCursorContainer cursor = drawableRuleset.Cursor;
            var restCollection = new List<RestInfo>();

            if (HiddenComboCount.Value == 0) return;
            if (!ShowCursorDuringBreaks.Value) return;

            foreach (var hitObject in beatmap.HitObjects)
            {
                if (hitObject is Spinner)
                {
                    addRest(hitObject.StartTime, true);
                }
            }

            foreach (var breakInfo in beatmap.Breaks)
            {
                if (breakInfo.HasEffect)
                {
                    addRest(breakInfo.StartTime, false);
                }
            }

            restCollection.Sort((info, restInfo) => info.StartTime.CompareTo(restInfo.StartTime));

            cursor.OnLoadComplete += _ =>
            {
                foreach (var restInfo in restCollection)
                {
                    showCursor(restInfo);
                }
            };

            void addRest(double startTime, bool spinner) => restCollection.Add(new RestInfo(startTime - transition_duration, spinner));

            void showCursor(RestInfo restInfo)
            {
                using (cursor.BeginAbsoluteSequence(restInfo.StartTime))
                {
                    cursor.TransformBindableTo(targetAlpha, 1, transition_duration, Easing.OutCubic)
                          .TransformBindableTo(rest, true);

                    if (restInfo.IsSpinner)
                    {
                        cursor.TransformBindableTo(restSpinner, true);
                    }
                }
            }
        }

        public virtual void Update(Playfield playfield)
        {
            playfield.Cursor.Alpha = (float)Interpolation.Lerp(playfield.Cursor.Alpha, targetAlpha.Value, Math.Clamp(playfield.Time.Elapsed / transition_duration, 0, 1));
        }

        public void ApplyToScoreProcessor(ScoreProcessor scoreProcessor)
        {
            int lastComboBeforeRest = 0;

            if (HiddenComboCount.Value == 0) return;

            currentCombo = scoreProcessor.Combo.GetBoundCopy();
            currentCombo.BindValueChanged(combo =>
            {
                if (combo.NewValue == 0)
                {
                    lastComboBeforeRest = 0;
                }

                if (rest.Value)
                {
                    // handle combo increase after spinner
                    lastComboBeforeRest = restSpinner.Value ? combo.NewValue : combo.NewValue - 1;
                    rest.Value = restSpinner.Value = false;
                }

                targetAlpha.Value = Math.Max(min_alpha, 1 - (float)(combo.NewValue - lastComboBeforeRest) / HiddenComboCount.Value);
            }, true);
        }
    }

    public class HiddenComboSlider : OsuSliderBar<int>
    {
        public override LocalisableString TooltipText => Current.Value == 0 ? "always hidden" : base.TooltipText;
    }
}
