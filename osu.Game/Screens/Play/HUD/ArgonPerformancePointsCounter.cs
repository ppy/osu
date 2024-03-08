// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;
using osu.Game.Localisation.SkinComponents;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Screens.Play.HUD
{
    public partial class ArgonPerformancePointsCounter : PerformancePointsCounter
    {
        private ArgonCounterTextComponent text = null!;

        protected override double RollingDuration => 250;

        private const float alpha_when_invalid = 0.3f;

        [SettingSource("Wireframe opacity", "Controls the opacity of the wireframes behind the digits.")]
        public BindableFloat WireframeOpacity { get; } = new BindableFloat(0.25f)
        {
            Precision = 0.01f,
            MinValue = 0,
            MaxValue = 1,
        };

        [SettingSource(typeof(SkinnableComponentStrings), nameof(SkinnableComponentStrings.ShowLabel), nameof(SkinnableComponentStrings.ShowLabelDescription))]
        public Bindable<bool> ShowLabel { get; } = new BindableBool(true);

        public override bool IsValid
        {
            get => base.IsValid;
            set
            {
                if (value == IsValid)
                    return;

                base.IsValid = value;
                text.FadeTo(value ? 1 : alpha_when_invalid, 1000, Easing.OutQuint);
            }
        }

        public override int DisplayedCount
        {
            get => base.DisplayedCount;
            set
            {
                base.DisplayedCount = value;
                updateWireframe();
            }
        }

        private void updateWireframe()
        {
            int digitsRequiredForDisplayCount = Math.Max(3, getDigitsRequiredForDisplayCount());

            if (digitsRequiredForDisplayCount != text.WireframeTemplate.Length)
                text.WireframeTemplate = new string('#', digitsRequiredForDisplayCount);
        }

        private int getDigitsRequiredForDisplayCount()
        {
            int digitsRequired = 1;
            long c = DisplayedCount;
            while ((c /= 10) > 0)
                digitsRequired++;
            return digitsRequired;
        }

        protected override IHasText CreateText() => text = new ArgonCounterTextComponent(Anchor.TopRight, BeatmapsetsStrings.ShowScoreboardHeaderspp.ToUpper())
        {
            WireframeOpacity = { BindTarget = WireframeOpacity },
            ShowLabel = { BindTarget = ShowLabel },
        };
    }
}
