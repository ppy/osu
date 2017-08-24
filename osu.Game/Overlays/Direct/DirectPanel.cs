// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transforms;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using OpenTK.Graphics;
using osu.Framework.Input;
using osu.Framework.MathUtils;

namespace osu.Game.Overlays.Direct
{
    public abstract class DirectPanel : Container
    {
        protected readonly BeatmapSetInfo SetInfo;

        protected Box BlackBackground;

        private const double hover_transition_time = 400;

        private Container content;

        protected override Container<Drawable> Content => content;

        protected DirectPanel(BeatmapSetInfo setInfo)
        {
            SetInfo = setInfo;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddInternal(content = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Shadow,
                    Offset = new Vector2(0f, 1f),
                    Radius = 2f,
                    Colour = Color4.Black.Opacity(0.25f),
                },
                Children = new[]
                {
                    // temporary blackness until the actual background loads.
                    BlackBackground = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Black,
                    },
                    CreateBackground(),
                }
            });
        }

        protected override bool OnHover(InputState state)
        {
            content.FadeEdgeEffectTo(1f, hover_transition_time, Easing.OutQuint);
            content.TransformTo(content.PopulateTransform(new TransformEdgeEffectRadius(), 14, hover_transition_time, Easing.OutQuint));
            content.MoveToY(-4, hover_transition_time, Easing.OutQuint);

            return base.OnHover(state);
        }

        protected override void OnHoverLost(InputState state)
        {
            content.FadeEdgeEffectTo(0.25f, hover_transition_time, Easing.OutQuint);
            content.TransformTo(content.PopulateTransform(new TransformEdgeEffectRadius(), 2, hover_transition_time, Easing.OutQuint));
            content.MoveToY(0, hover_transition_time, Easing.OutQuint);

            base.OnHoverLost(state);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            this.FadeInFromZero(200, Easing.Out);
        }

        protected List<DifficultyIcon> GetDifficultyIcons()
        {
            var icons = new List<DifficultyIcon>();

            foreach (var b in SetInfo.Beatmaps)
                icons.Add(new DifficultyIcon(b));

            return icons;
        }

        protected Drawable CreateBackground() => new DelayedLoadWrapper(new BeatmapSetCover(SetInfo)
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            RelativeSizeAxes = Axes.Both,
            FillMode = FillMode.Fill,
            OnLoadComplete = d =>
            {
                d.FadeInFromZero(400, Easing.Out);
                BlackBackground.Delay(400).FadeOut();
            },
        })
        {
            RelativeSizeAxes = Axes.Both,
            TimeBeforeLoad = 300
        };

        public class Statistic : FillFlowContainer
        {
            private readonly SpriteText text;

            private int value;

            public int Value
            {
                get { return value; }
                set
                {
                    this.value = value;
                    text.Text = Value.ToString(@"N0");
                }
            }

            public Statistic(FontAwesome icon, int value = 0)
            {
                Anchor = Anchor.TopRight;
                Origin = Anchor.TopRight;
                AutoSizeAxes = Axes.Both;
                Direction = FillDirection.Horizontal;
                Spacing = new Vector2(5f, 0f);

                Children = new Drawable[]
                {
                    text = new OsuSpriteText
                    {
                        Font = @"Exo2.0-SemiBoldItalic",
                    },
                    new SpriteIcon
                    {
                        Icon = icon,
                        Shadow = true,
                        Size = new Vector2(14),
                        Margin = new MarginPadding { Top = 1 },
                    },
                };

                Value = value;
            }
        }

        private class TransformEdgeEffectRadius : Transform<float, Container>
        {
            /// <summary>
            /// Current value of the transformed colour in linear colour space.
            /// </summary>
            private float valueAt(double time)
            {
                if (time < StartTime) return StartValue;
                if (time >= EndTime) return EndValue;

                return Interpolation.ValueAt(time, StartValue, EndValue, StartTime, EndTime, Easing);
            }

            public override string TargetMember => "EdgeEffect.Colour";

            protected override void Apply(Container c, double time)
            {
                EdgeEffectParameters e = c.EdgeEffect;
                e.Radius = valueAt(time);
                c.EdgeEffect = e;
            }

            protected override void ReadIntoStartValue(Container d) => StartValue = d.EdgeEffect.Radius;
        }
    }
}
