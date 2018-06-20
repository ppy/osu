// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Colour;
using osu.Game.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using System;
using System.Linq;
using osu.Framework.Input.Bindings;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.UI.Scrolling;

namespace osu.Game.Rulesets.Mania.UI
{
    public class Column : ScrollingPlayfield, IKeyBindingHandler<ManiaAction>, IHasAccentColour
    {
        private const float key_icon_size = 10;
        private const float key_icon_corner_radius = 3;
        private const float key_icon_border_radius = 2;

        private const float hit_target_height = 10;
        private const float hit_target_bar_height = 2;

        private const float column_width = 45;
        private const float special_column_width = 70;

        public ManiaAction Action;

        private readonly Box background;
        private readonly Box backgroundOverlay;
        private readonly Container hitTargetBar;
        private readonly Container keyIcon;

        internal readonly Container TopLevelContainer;
        private readonly Container explosionContainer;

        protected override Container<Drawable> Content => content;
        private readonly Container<Drawable> content;

        public Column()
            : base(ScrollingDirection.Up)
        {
            RelativeSizeAxes = Axes.Y;
            Width = column_width;

            Masking = true;
            CornerRadius = 5;

            InternalChildren = new Drawable[]
            {
                background = new Box
                {
                    Name = "Background",
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0.3f
                },
                backgroundOverlay = new Box
                {
                    Name = "Background Gradient Overlay",
                    RelativeSizeAxes = Axes.Both,
                    Height = 0.5f,
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    Blending = BlendingMode.Additive,
                    Alpha = 0
                },
                new Container
                {
                    Name = "Hit target + hit objects",
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Top = ManiaStage.HIT_TARGET_POSITION },
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            Name = "Hit target",
                            RelativeSizeAxes = Axes.X,
                            Height = hit_target_height,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    Name = "Background",
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = Color4.Black
                                },
                                hitTargetBar = new Container
                                {
                                    Name = "Bar",
                                    RelativeSizeAxes = Axes.X,
                                    Height = hit_target_bar_height,
                                    Masking = true,
                                    Children = new[]
                                    {
                                        new Box
                                        {
                                            RelativeSizeAxes = Axes.Both
                                        }
                                    }
                                }
                            }
                        },
                        content = new Container
                        {
                            Name = "Hit objects",
                            RelativeSizeAxes = Axes.Both,
                        },
                        // For column lighting, we need to capture input events before the notes
                        new InputTarget
                        {
                            Pressed = onPressed,
                            Released = onReleased
                        },
                        explosionContainer = new Container
                        {
                            Name = "Hit explosions",
                            RelativeSizeAxes = Axes.Both
                        }
                    }
                },
                new Container
                {
                    Name = "Key",
                    RelativeSizeAxes = Axes.X,
                    Height = ManiaStage.HIT_TARGET_POSITION,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Name = "Key gradient",
                            RelativeSizeAxes = Axes.Both,
                            Colour = ColourInfo.GradientVertical(Color4.Black, Color4.Black.Opacity(0)),
                            Alpha = 0.5f
                        },
                        keyIcon = new Container
                        {
                            Name = "Key icon",
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = new Vector2(key_icon_size),
                            Masking = true,
                            CornerRadius = key_icon_corner_radius,
                            BorderThickness = 2,
                            BorderColour = Color4.White, // Not true
                            Children = new[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Alpha = 0,
                                    AlwaysPresent = true
                                }
                            }
                        }
                    }
                },
                TopLevelContainer = new Container { RelativeSizeAxes = Axes.Both }
            };

            TopLevelContainer.Add(explosionContainer.CreateProxy());
        }

        public override Axes RelativeSizeAxes => Axes.Y;

        private bool isSpecial;
        public bool IsSpecial
        {
            get { return isSpecial; }
            set
            {
                if (isSpecial == value)
                    return;
                isSpecial = value;

                Width = isSpecial ? special_column_width : column_width;
            }
        }

        private Color4 accentColour;
        public Color4 AccentColour
        {
            get { return accentColour; }
            set
            {
                if (accentColour == value)
                    return;
                accentColour = value;

                background.Colour = accentColour;
                backgroundOverlay.Colour = ColourInfo.GradientVertical(accentColour.Opacity(0.6f), accentColour.Opacity(0));

                hitTargetBar.EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Glow,
                    Radius = 5,
                    Colour = accentColour.Opacity(0.5f),
                };

                keyIcon.EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Glow,
                    Radius = 5,
                    Colour = accentColour.Opacity(0.5f),
                };
            }
        }

        /// <summary>
        /// Adds a DrawableHitObject to this Playfield.
        /// </summary>
        /// <param name="hitObject">The DrawableHitObject to add.</param>
        public override void Add(DrawableHitObject hitObject)
        {
            hitObject.AccentColour = AccentColour;
            hitObject.OnJudgement += OnJudgement;

            HitObjects.Add(hitObject);
        }

        internal void OnJudgement(DrawableHitObject judgedObject, Judgement judgement)
        {
            if (!judgement.IsHit || !judgedObject.DisplayJudgement)
                return;

            explosionContainer.Add(new HitExplosion(judgedObject));
        }

        private bool onPressed(ManiaAction action)
        {
            if (action == Action)
            {
                backgroundOverlay.FadeTo(1, 50, Easing.OutQuint).Then().FadeTo(0.5f, 250, Easing.OutQuint);
                keyIcon.ScaleTo(1.4f, 50, Easing.OutQuint).Then().ScaleTo(1.3f, 250, Easing.OutQuint);
            }

            return false;
        }

        private bool onReleased(ManiaAction action)
        {
            if (action == Action)
            {
                backgroundOverlay.FadeTo(0, 250, Easing.OutQuint);
                keyIcon.ScaleTo(1f, 125, Easing.OutQuint);
            }

            return false;
        }

        /// <summary>
        /// This is a simple container which delegates various input events that have to be captured before the notes.
        /// </summary>
        private class InputTarget : Container, IKeyBindingHandler<ManiaAction>
        {
            public Func<ManiaAction, bool> Pressed;
            public Func<ManiaAction, bool> Released;

            public InputTarget()
            {
                RelativeSizeAxes = Axes.Both;
                AlwaysPresent = true;
                Alpha = 0;
            }

            public bool OnPressed(ManiaAction action) => Pressed?.Invoke(action) ?? false;
            public bool OnReleased(ManiaAction action) => Released?.Invoke(action) ?? false;
        }

        public bool OnPressed(ManiaAction action)
        {
            if (action != Action)
                return false;

            var nextObject =
                HitObjects.AliveObjects.FirstOrDefault(h => h.HitObject.StartTime > Time.Current) ??
                // fallback to non-alive objects to find next off-screen object
                HitObjects.Objects.FirstOrDefault(h => h.HitObject.StartTime > Time.Current) ??
                HitObjects.Objects.LastOrDefault();

            nextObject?.PlaySamples();

            return true;
        }

        public bool OnReleased(ManiaAction action) => false;
    }
}
