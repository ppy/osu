// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation.SkinComponents;
using osu.Game.Skinning;
using osuTK;
using Vortice.DXGI;

namespace osu.Game.Screens.Play.HUD
{
    public partial class BPMCounter : RollingCounter<double>, ISerialisableDrawable
    {
        protected override double RollingDuration => 375;

        [SettingSource(typeof(SkinnableComponentStrings), nameof(SkinnableComponentStrings.Font), nameof(SkinnableComponentStrings.FontDescription))]
        public Bindable<Typeface> Font { get; } = new Bindable<Typeface>(Typeface.Venera);

        [SettingSource(typeof(SkinnableComponentStrings), nameof(SkinnableComponentStrings.Colour), nameof(SkinnableComponentStrings.ColourDescription))]
        public BindableColour4 TextColour { get; } = new BindableColour4(Color4Extensions.FromHex(@"ddffff"));

        [SettingSource(typeof(SkinnableComponentStrings), nameof(SkinnableComponentStrings.ShowLabel), nameof(SkinnableComponentStrings.ShowLabelDescription))]
        public Bindable<bool> ShowLabel { get; } = new BindableBool(true);

        [Resolved]
        private IBindable<WorkingBeatmap> beatmap { get; set; } = null!;

        [Resolved]
        private IGameplayClock gameplayClock { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            Current.Value = DisplayedCount = 0;
            TextColour.BindValueChanged(c => Colour = TextColour.Value, true);
        }

        protected override void Update()
        {
            base.Update();

            // We want to check Rate every update to cover windup/down
            Current.Value = beatmap.Value.Beatmap.ControlPointInfo.TimingPointAt(gameplayClock.CurrentTime).BPM * gameplayClock.GetTrueGameplayRate();
        }

        protected override OsuSpriteText CreateSpriteText()
            => base.CreateSpriteText().With(s => s.Font = s.Font.With(size: 20f, fixedWidth: true));

        protected override LocalisableString FormatCount(double count)
        {
            return $@"{count:0}";
        }

        protected override IHasText CreateText() => new TextComponent()
        {
            ShowLabel = { BindTarget = ShowLabel },
            Font = { BindTarget = Font },
        };

        private partial class TextComponent : CompositeDrawable, IHasText
        {
            public LocalisableString Text
            {
                get => text.Text;
                set => text.Text = value;
            }
            public Bindable<bool> ShowLabel { get; } = new BindableBool();
            public Bindable<Typeface> Font { get; } = new Bindable<Typeface>();

            private readonly OsuSpriteText text;
            private readonly OsuSpriteText label;

            public TextComponent()
            {
                AutoSizeAxes = Axes.Both;

                InternalChild = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Spacing = new Vector2(2),
                    Children = new Drawable[]
                    {
                        text = new OsuSpriteText
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            Font = OsuFont.Numeric.With(size: 16, fixedWidth: true)
                        },
                        label = new OsuSpriteText
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            Font = OsuFont.Numeric.With(size: 8),
                            Text = @"BPM",
                            Padding = new MarginPadding { Bottom = 2f }, // align baseline better
                        }
                    }
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                ShowLabel.BindValueChanged(s =>
                {
                    label.Alpha = s.NewValue ? 1 : 0;
                }, true);

                Font.BindValueChanged(typeface =>
                {
                    // We only have bold weight for venera, so let's force that.
                    FontWeight fontWeight = typeface.NewValue == Typeface.Venera ? FontWeight.Bold : FontWeight.Regular;

                    FontUsage f = OsuFont.GetFont(typeface.NewValue, weight: fontWeight);

                    // Fixed width looks better on venera only in my opinion.
                    text.Font = f.With(size: 16, fixedWidth: typeface.NewValue == Typeface.Venera);
                    label.Font = f.With(size: 8);
                }, true);
            }
        }

        public bool UsesFixedAnchor { get; set; }
    }
}
