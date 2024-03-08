// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;
using osu.Game.Audio;
using osu.Game.Beatmaps.Formats;
using osu.Game.Extensions;
using osu.Game.IO;
using osu.Game.Screens.Play;
using osu.Game.Screens.Play.HUD;
using osu.Game.Screens.Play.HUD.HitErrorMeters;
using osu.Game.Skinning.Components;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Skinning
{
    public class ArgonSkin : Skin
    {
        public static SkinInfo CreateInfo() => new SkinInfo
        {
            ID = Skinning.SkinInfo.ARGON_SKIN,
            Name = "osu! \"argon\" (2022)",
            Creator = "team osu!",
            Protected = true,
            InstantiationInfo = typeof(ArgonSkin).GetInvariantInstantiationInfo()
        };

        protected readonly IStorageResourceProvider Resources;

        public ArgonSkin(IStorageResourceProvider resources)
            : this(CreateInfo(), resources)
        {
        }

        [UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
        public ArgonSkin(SkinInfo skin, IStorageResourceProvider resources)
            : base(
                skin,
                resources
            )
        {
            Resources = resources;

            Configuration.CustomComboColours = new List<Color4>
            {
                // Standard combo progression order is green - blue - red - yellow.
                // But for whatever reason, this starts from index 1, not 0.
                //
                // We've added two new combo colours in argon, so to ensure the initial rotation matches,
                // this same progression is in slots 1 - 4.

                // Orange
                new Color4(241, 116, 0, 255),
                // Green
                new Color4(0, 241, 53, 255),
                // Blue
                new Color4(0, 82, 241, 255),
                // Red
                new Color4(241, 0, 0, 255),
                // Yellow
                new Color4(232, 235, 0, 255),
                // Purple
                new Color4(92, 0, 241, 255),
            };
        }

        public override Texture? GetTexture(string componentName, WrapMode wrapModeS, WrapMode wrapModeT) => Textures?.Get(componentName, wrapModeS, wrapModeT);

        public override ISample? GetSample(ISampleInfo sampleInfo)
        {
            foreach (string lookup in sampleInfo.LookupNames)
            {
                var sample = Samples?.Get(lookup) ?? Resources.AudioManager?.Samples.Get(lookup);
                if (sample != null)
                    return sample;
            }

            return null;
        }

        public override Drawable? GetDrawableComponent(ISkinComponentLookup lookup)
        {
            // Temporary until default skin has a valid hit lighting.
            if ((lookup as SkinnableSprite.SpriteComponentLookup)?.LookupName == @"lighting") return Drawable.Empty();

            if (base.GetDrawableComponent(lookup) is Drawable c)
                return c;

            switch (lookup)
            {
                case SkinComponentsContainerLookup containerLookup:
                    // Only handle global level defaults for now.
                    if (containerLookup.Ruleset != null)
                        return null;

                    switch (containerLookup.Target)
                    {
                        case SkinComponentsContainerLookup.TargetArea.SongSelect:
                            var songSelectComponents = new DefaultSkinComponentsContainer(_ =>
                            {
                                // do stuff when we need to.
                            });

                            return songSelectComponents;

                        case SkinComponentsContainerLookup.TargetArea.MainHUDComponents:
                            var skinnableTargetWrapper = new DefaultSkinComponentsContainer(container =>
                            {
                                var health = container.OfType<ArgonHealthDisplay>().FirstOrDefault();
                                var healthLine = container.OfType<BoxElement>().FirstOrDefault();
                                var wedgePieces = container.OfType<ArgonWedgePiece>().ToArray();
                                var score = container.OfType<ArgonScoreCounter>().FirstOrDefault();
                                var accuracy = container.OfType<ArgonAccuracyCounter>().FirstOrDefault();
                                var performancePoints = container.OfType<ArgonPerformancePointsCounter>().FirstOrDefault();
                                var combo = container.OfType<ArgonComboCounter>().FirstOrDefault();
                                var songProgress = container.OfType<ArgonSongProgress>().FirstOrDefault();
                                var keyCounter = container.OfType<ArgonKeyCounterDisplay>().FirstOrDefault();

                                if (health != null)
                                {
                                    // elements default to beneath the health bar
                                    const float components_x_offset = 50;

                                    health.Anchor = Anchor.TopLeft;
                                    health.Origin = Anchor.TopLeft;
                                    health.UseRelativeSize.Value = false;
                                    health.Width = 300;
                                    health.BarHeight.Value = 30f;
                                    health.Position = new Vector2(components_x_offset, 20f);

                                    if (healthLine != null)
                                    {
                                        healthLine.Anchor = Anchor.TopLeft;
                                        healthLine.Origin = Anchor.CentreLeft;
                                        healthLine.Y = health.Y + ArgonHealthDisplay.MAIN_PATH_RADIUS;
                                        healthLine.Size = new Vector2(45, 3);
                                    }

                                    foreach (var wedgePiece in wedgePieces)
                                        wedgePiece.Position += new Vector2(-50, 15);

                                    if (score != null)
                                    {
                                        score.Origin = Anchor.TopRight;
                                        score.Position = new Vector2(components_x_offset + 200, wedgePieces.Last().Y + 30);
                                    }

                                    if (accuracy != null)
                                    {
                                        // +4 to vertically align the accuracy counter with the score counter.
                                        accuracy.Position = new Vector2(-20, 20);
                                        accuracy.Anchor = Anchor.TopRight;
                                        accuracy.Origin = Anchor.TopRight;
                                    }

                                    if (performancePoints != null && accuracy != null)
                                    {
                                        performancePoints.Position = new Vector2(accuracy.X, accuracy.Y + accuracy.DrawHeight + 10);
                                        performancePoints.Anchor = Anchor.TopRight;
                                        performancePoints.Origin = Anchor.TopRight;
                                    }

                                    var hitError = container.OfType<HitErrorMeter>().FirstOrDefault();

                                    if (hitError != null)
                                    {
                                        hitError.Anchor = Anchor.CentreLeft;
                                        hitError.Origin = Anchor.CentreLeft;
                                    }

                                    var hitError2 = container.OfType<HitErrorMeter>().LastOrDefault();

                                    if (hitError2 != null)
                                    {
                                        hitError2.Anchor = Anchor.CentreRight;
                                        hitError2.Scale = new Vector2(-1, 1);
                                        // origin flipped to match scale above.
                                        hitError2.Origin = Anchor.CentreLeft;
                                    }

                                    if (songProgress != null)
                                    {
                                        const float padding = 10;
                                        // Hard to find this at runtime, so taken from the most expanded state during replay.
                                        const float song_progress_offset_height = 36 + padding;

                                        songProgress.Position = new Vector2(0, -padding);
                                        songProgress.Scale = new Vector2(0.9f, 1);

                                        if (keyCounter != null && hitError != null)
                                        {
                                            keyCounter.Anchor = Anchor.BottomRight;
                                            keyCounter.Origin = Anchor.BottomRight;
                                            keyCounter.Position = new Vector2(-(hitError.Width + padding), -(padding * 2 + song_progress_offset_height));
                                        }

                                        if (combo != null && hitError != null)
                                        {
                                            combo.Anchor = Anchor.BottomLeft;
                                            combo.Origin = Anchor.BottomLeft;
                                            combo.Position = new Vector2((hitError.Width + padding), -(padding * 2 + song_progress_offset_height));
                                        }
                                    }
                                }
                            })
                            {
                                Children = new Drawable[]
                                {
                                    new ArgonWedgePiece
                                    {
                                        Size = new Vector2(380, 72),
                                    },
                                    new ArgonWedgePiece
                                    {
                                        Size = new Vector2(380, 72),
                                        Position = new Vector2(4, 5)
                                    },
                                    new ArgonScoreCounter
                                    {
                                        ShowLabel = { Value = false },
                                    },
                                    new ArgonHealthDisplay(),
                                    new BoxElement
                                    {
                                        CornerRadius = { Value = 0.5f }
                                    },
                                    new ArgonAccuracyCounter(),
                                    new ArgonPerformancePointsCounter
                                    {
                                        Scale = new Vector2(0.8f),
                                    },
                                    new ArgonComboCounter
                                    {
                                        Scale = new Vector2(1.3f)
                                    },
                                    new BarHitErrorMeter(),
                                    new BarHitErrorMeter(),
                                    new ArgonSongProgress(),
                                    new ArgonKeyCounterDisplay(),
                                }
                            };

                            return skinnableTargetWrapper;
                    }

                    return null;
            }

            return null;
        }

        public override IBindable<TValue>? GetConfig<TLookup, TValue>(TLookup lookup)
        {
            // todo: this code is pulled from LegacySkin and should not exist.
            // will likely change based on how databased storage of skin configuration goes.
            switch (lookup)
            {
                case GlobalSkinColours global:
                    switch (global)
                    {
                        case GlobalSkinColours.ComboColours:
                        {
                            LogLookupDebug(this, lookup, LookupDebugType.Hit);
                            return SkinUtils.As<TValue>(new Bindable<IReadOnlyList<Color4>?>(Configuration.ComboColours));
                        }
                    }

                    break;

                case SkinComboColourLookup comboColour:
                    LogLookupDebug(this, lookup, LookupDebugType.Hit);
                    return SkinUtils.As<TValue>(new Bindable<Color4>(getComboColour(Configuration, comboColour.ColourIndex)));
            }

            LogLookupDebug(this, lookup, LookupDebugType.Miss);
            return null;
        }

        private static Color4 getComboColour(IHasComboColours source, int colourIndex)
            => source.ComboColours![colourIndex % source.ComboColours.Count];
    }
}
