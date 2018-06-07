// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using System.Linq;
using osu.Framework.Input.Bindings;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mania.UI.Components;
using osu.Game.Rulesets.UI.Scrolling;

namespace osu.Game.Rulesets.Mania.UI
{
    public class Column : ScrollingPlayfield, IKeyBindingHandler<ManiaAction>, IHasAccentColour
    {
        private const float hit_target_height = 10;
        private const float hit_target_bar_height = 2;

        private const float column_width = 45;
        private const float special_column_width = 70;

        private ManiaAction action;

        public ManiaAction Action
        {
            get => action;
            set
            {
                if (action == value)
                    return;
                action = value;

                background.Action = value;
                keyArea.Action = value;
            }
        }

        private readonly ColumnBackground background;
        private readonly ColumnKeyArea keyArea;

        private readonly Container hitTargetBar;

        internal readonly Container TopLevelContainer;
        private readonly Container explosionContainer;

        protected override Container<Drawable> Content => content;
        private readonly Container<Drawable> content;

        public Column(ScrollingDirection direction = ScrollingDirection.Up)
            : base(direction)
        {
            RelativeSizeAxes = Axes.Y;
            Width = column_width;

            Masking = true;
            CornerRadius = 5;

            background = new ColumnBackground(direction) { RelativeSizeAxes = Axes.Both };

            InternalChildren = new[]
            {
                // For input purposes, the background is added at the highest depth, but is then proxied back below all other elements
                background.CreateProxy(),
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
                        explosionContainer = new Container
                        {
                            Name = "Hit explosions",
                            RelativeSizeAxes = Axes.Both
                        }
                    }
                },
                keyArea = new ColumnKeyArea(direction)
                {
                    Anchor = direction == ScrollingDirection.Up ? Anchor.TopLeft : Anchor.BottomLeft,
                    Origin = direction == ScrollingDirection.Up ? Anchor.TopLeft : Anchor.BottomLeft,
                    RelativeSizeAxes = Axes.X,
                    Height = ManiaStage.HIT_TARGET_POSITION,
                },
                background,
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

                background.AccentColour = value;
                keyArea.AccentColour = value;

                hitTargetBar.EdgeEffect = new EdgeEffectParameters
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

        public bool OnPressed(ManiaAction action)
        {
            if (action != Action)
                return false;

            var hitObject = HitObjects.Objects.LastOrDefault(h => h.HitObject.StartTime > Time.Current) ?? HitObjects.Objects.FirstOrDefault();
            hitObject?.PlaySamples();

            return true;
        }

        public bool OnReleased(ManiaAction action) => false;
    }
}
