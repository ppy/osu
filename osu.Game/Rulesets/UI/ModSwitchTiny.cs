// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Utils;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osu.Game.Rulesets.Mods;
using osuTK.Graphics;

namespace osu.Game.Rulesets.UI
{
    public partial class ModSwitchTiny : CompositeDrawable
    {
        public BindableBool Active { get; } = new BindableBool();

        public const float DEFAULT_HEIGHT = 30;
        private const float width = 73;

        protected readonly IMod Mod;
        private readonly bool showExtendedInformation;

        private readonly Box background;
        private readonly OsuSpriteText acronymText;

        private Color4 activeForegroundColour;
        private Color4 inactiveForegroundColour;

        private Color4 activeBackgroundColour;
        private Color4 inactiveBackgroundColour;

        private readonly CircularContainer extendedContent;
        private readonly Box extendedBackground;
        private readonly OsuSpriteText extendedText;
        private ModSettingChangeTracker? modSettingsChangeTracker;

        public ModSwitchTiny(IMod mod, bool showExtendedInformation = false)
        {
            Mod = mod;
            this.showExtendedInformation = showExtendedInformation;
            AutoSizeAxes = Axes.X;
            Height = DEFAULT_HEIGHT;

            InternalChildren = new Drawable[]
            {
                extendedContent = new CircularContainer
                {
                    Name = "extended content",
                    Width = 100 + DEFAULT_HEIGHT / 2,
                    RelativeSizeAxes = Axes.Y,
                    Masking = true,
                    X = width,
                    Margin = new MarginPadding { Left = -DEFAULT_HEIGHT },
                    Children = new Drawable[]
                    {
                        extendedBackground = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                        },
                        extendedText = new OsuSpriteText
                        {
                            Margin = new MarginPadding { Left = 3 * DEFAULT_HEIGHT / 4 },
                            Font = OsuFont.Default.With(size: 30f, weight: FontWeight.Bold),
                            UseFullGlyphHeight = false,
                            Text = mod.ExtendedIconInformation,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        },
                    }
                },
                new CircularContainer
                {
                    Width = width,
                    RelativeSizeAxes = Axes.Y,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        background = new Box
                        {
                            RelativeSizeAxes = Axes.Both
                        },
                        acronymText = new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Shadow = false,
                            Font = OsuFont.Numeric.With(size: 24, weight: FontWeight.Black),
                            Text = mod.Acronym,
                            Margin = new MarginPadding
                            {
                                Top = 4
                            }
                        },
                    },
                }
            };
        }

        [BackgroundDependencyLoader(true)]
        private void load(OsuColour colours, OverlayColourProvider? colourProvider)
        {
            inactiveBackgroundColour = colourProvider?.Background5 ?? colours.Gray3;
            activeBackgroundColour = colours.ForModType(Mod.Type);

            inactiveForegroundColour = colourProvider?.Background2 ?? colours.Gray5;
            activeForegroundColour = Interpolation.ValueAt<Colour4>(0.1f, Colour4.Black, activeForegroundColour, 0, 1);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Active.BindValueChanged(_ => updateState(), true);
            FinishTransforms(true);

            if (Mod is Mod actualMod)
            {
                modSettingsChangeTracker = new ModSettingChangeTracker(new[] { actualMod });
                modSettingsChangeTracker.SettingChanged = _ => updateExtendedInformation();
            }

            updateExtendedInformation();
        }

        private void updateExtendedInformation()
        {
            bool showExtended = showExtendedInformation && !string.IsNullOrEmpty(Mod.ExtendedIconInformation);

            extendedContent.Alpha = showExtended ? 1 : 0;
            extendedText.Text = Mod.ExtendedIconInformation;
        }

        private void updateState()
        {
            acronymText.FadeColour(Active.Value ? activeForegroundColour : inactiveForegroundColour, 200, Easing.OutQuint);
            background.FadeColour(Active.Value ? activeBackgroundColour : inactiveBackgroundColour, 200, Easing.OutQuint);

            extendedText.Colour = Active.Value ? activeBackgroundColour.Lighten(0.2f) : inactiveBackgroundColour;
            extendedBackground.Colour = Active.Value ? activeBackgroundColour.Darken(2.4f) : inactiveBackgroundColour.Darken(2.8f);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            modSettingsChangeTracker?.Dispose();
        }
    }
}
