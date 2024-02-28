// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Mods;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.UI
{
    /// <summary>
    /// Display the specified mod at a fixed size.
    /// </summary>
    public partial class ModIcon : Container, IHasTooltip
    {
        public readonly BindableBool Selected = new BindableBool();

        private SpriteIcon modIcon = null!;
        private SpriteText modAcronym = null!;
        private Sprite background = null!;

        public static readonly Vector2 MOD_ICON_SIZE = new Vector2(80);

        public virtual LocalisableString TooltipText => showTooltip ? ((mod as Mod)?.IconTooltip ?? mod.Name) : string.Empty;

        private IMod mod;

        private readonly bool showTooltip;
        private readonly bool showExtendedInformation;

        public IMod Mod
        {
            get => mod;
            set
            {
                if (mod == value)
                    return;

                mod = value;

                if (IsLoaded)
                    updateMod(value);
            }
        }

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        private Color4 backgroundColour;

        private Sprite extendedBackground = null!;

        private OsuSpriteText extendedText = null!;

        private Container extendedContent = null!;

        private ModSettingChangeTracker? modSettingsChangeTracker;

        /// <summary>
        /// Construct a new instance.
        /// </summary>
        /// <param name="mod">The mod to be displayed</param>
        /// <param name="showTooltip">Whether a tooltip describing the mod should display on hover.</param>
        /// <param name="showExtendedInformation">Whether to display a mod's extended information, if available.</param>
        public ModIcon(IMod mod, bool showTooltip = true, bool showExtendedInformation = true)
        {
            // May expand due to expanded content, so autosize here.
            AutoSizeAxes = Axes.X;
            Height = MOD_ICON_SIZE.Y;

            this.mod = mod ?? throw new ArgumentNullException(nameof(mod));
            this.showTooltip = showTooltip;
            this.showExtendedInformation = showExtendedInformation;
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            Children = new Drawable[]
            {
                extendedContent = new Container
                {
                    Name = "extended content",
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Size = new Vector2(116, MOD_ICON_SIZE.Y),
                    X = MOD_ICON_SIZE.X - 22,
                    Children = new Drawable[]
                    {
                        extendedBackground = new Sprite
                        {
                            RelativeSizeAxes = Axes.Both,
                            FillMode = FillMode.Fit,
                            Texture = textures.Get("Icons/BeatmapDetails/mod-icon-extender"),
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        },
                        extendedText = new OsuSpriteText
                        {
                            Font = OsuFont.Default.With(size: 34f, weight: FontWeight.Bold),
                            UseFullGlyphHeight = false,
                            Text = mod.ExtendedIconInformation,
                            X = 6,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        },
                    }
                },
                new Container
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Name = "main content",
                    Size = MOD_ICON_SIZE,
                    Children = new Drawable[]
                    {
                        background = new Sprite
                        {
                            RelativeSizeAxes = Axes.Both,
                            FillMode = FillMode.Fit,
                            Texture = textures.Get("Icons/BeatmapDetails/mod-icon"),
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        },
                        modAcronym = new OsuSpriteText
                        {
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre,
                            Alpha = 0,
                            Font = OsuFont.Numeric.With(null, 22f),
                            UseFullGlyphHeight = false,
                            Text = mod.Acronym
                        },
                        modIcon = new SpriteIcon
                        {
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre,
                            Size = new Vector2(45),
                            Icon = FontAwesome.Solid.Question
                        },
                    }
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Selected.BindValueChanged(_ => updateColour());

            updateMod(mod);
        }

        private void updateMod(IMod value)
        {
            modSettingsChangeTracker?.Dispose();

            if (value is Mod actualMod)
            {
                modSettingsChangeTracker = new ModSettingChangeTracker(new[] { actualMod });
                modSettingsChangeTracker.SettingChanged = _ => updateExtendedInformation();
            }

            modAcronym.Text = value.Acronym;
            modIcon.Icon = value.Icon ?? FontAwesome.Solid.Question;

            if (value.Icon is null)
            {
                modIcon.FadeOut();
                modAcronym.FadeIn();
            }
            else
            {
                modIcon.FadeIn();
                modAcronym.FadeOut();
            }

            backgroundColour = colours.ForModType(value.Type);
            updateColour();

            updateExtendedInformation();
        }

        private void updateExtendedInformation()
        {
            bool showExtended = showExtendedInformation && !string.IsNullOrEmpty(mod.ExtendedIconInformation);

            extendedContent.Alpha = showExtended ? 1 : 0;
            extendedText.Text = mod.ExtendedIconInformation;
        }

        private void updateColour()
        {
            modAcronym.Colour = modIcon.Colour = OsuColour.Gray(84);

            extendedText.Colour = background.Colour = Selected.Value ? backgroundColour.Lighten(0.2f) : backgroundColour;
            extendedBackground.Colour = Selected.Value ? backgroundColour.Darken(2.4f) : backgroundColour.Darken(2.8f);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            modSettingsChangeTracker?.Dispose();
        }
    }
}
