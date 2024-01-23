// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Localisation.SkinComponents;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Skinning;

namespace osu.Game.Screens.Play.HUD
{
    public partial class ArgonScoreCounter : GameplayScoreCounter, ISerialisableDrawable
    {
        private ArgonScoreTextComponent scoreText = null!;

        protected override double RollingDuration => 250;

        [SettingSource("Wireframe opacity", "Controls the opacity of the wire frames behind the digits.")]
        public BindableFloat WireframeOpacity { get; } = new BindableFloat(0.25f)
        {
            Precision = 0.01f,
            MinValue = 0,
            MaxValue = 1,
        };

        [SettingSource(typeof(SkinnableComponentStrings), nameof(SkinnableComponentStrings.ShowLabel), nameof(SkinnableComponentStrings.ShowLabelDescription))]
        public Bindable<bool> ShowLabel { get; } = new BindableBool(true);

        public bool UsesFixedAnchor { get; set; }

        protected override LocalisableString FormatCount(long count) => count.ToLocalisableString();

        protected override IHasText CreateText() => scoreText = new ArgonScoreTextComponent(Anchor.TopRight, BeatmapsetsStrings.ShowScoreboardHeadersScore.ToUpper())
        {
            WireframeOpacity = { BindTarget = WireframeOpacity },
            ShowLabel = { BindTarget = ShowLabel },
        };

        public ArgonScoreCounter()
        {
            RequiredDisplayDigits.BindValueChanged(_ => updateWireframe());
        }

        public override long DisplayedCount
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
            scoreText.RequiredDisplayDigits.Value =
                Math.Max(RequiredDisplayDigits.Value, getDigitsRequiredForDisplayCount());
        }

        private int getDigitsRequiredForDisplayCount()
        {
            int digitsRequired = 1;
            long c = DisplayedCount;
            while ((c /= 10) > 0)
                digitsRequired++;
            return digitsRequired;
        }

        private partial class ArgonScoreTextComponent : ArgonCounterTextComponent
        {
            public ArgonScoreTextComponent(Anchor anchor, LocalisableString? label = null)
                : base(anchor, label)
            {
            }
        }
    }
}
