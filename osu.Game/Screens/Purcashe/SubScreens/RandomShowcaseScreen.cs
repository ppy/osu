using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Events;
using osu.Framework.Screens;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Purcashe.Components;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Screens.Purcashe.SubScreens
{
    public class RandomShowcaseScreen : PurcasheBasicScreen
    {
        public List<RollResult> Results { get; set; }
        public bool IsCustom;

        private Container<ResultContainer> showcaseContainer;
        private int currentIndex = -1;
        private SampleChannel sampleNext;

        private ShowcasePurcasheProgressBar progress;

        private int maxIndex => Results.Count;

        public override float BackgroundParallaxAmount => 5f;
        public override bool AllowBackButton => false;

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            progress = new ShowcasePurcasheProgressBar
            {
                EndTime = maxIndex,
                CurrentTime = 0,
                RelativeSizeAxes = Axes.Both,
                FillColour = Color4.LightBlue,
                Anchor = Anchor.BottomLeft,
                Origin = Anchor.BottomLeft,
            };

            sampleNext = audio.Samples.Get("SongSelect/select-expand");
            InternalChildren = new Drawable[]
            {
                showcaseContainer = new Container<ResultContainer>
                {
                    RelativeSizeAxes = Axes.Both,
                }
            };

            if (IsCustom)
            {
                AddInternal(
                    new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 5,
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomLeft,
                        Child = progress
                    }.WithEffect(new GlowEffect
                    {
                        Colour = Color4.LightBlue,
                        BlurSigma = new Vector2(1),
                        Strength = 20,
                        PadExtent = true
                    }));
            }

            showNext();
        }

        private void showNext()
        {
            foreach (var rc in showcaseContainer)
            {
                rc.FadeOut(300).ScaleTo(0.8f, 300, Easing.OutQuint).Expire();
            }

            currentIndex++;

            if (currentIndex >= maxIndex)
            {
                this.Exit();
                return;
            }

            RollResult r = Results.ElementAt(currentIndex);

            sampleNext?.Play();
            progress.CurrentTime = 1 + currentIndex;

            showcaseContainer.Add(new ResultContainer
            {
                Result = r,
                RelativeSizeAxes = Axes.Both
            });
        }

        protected override bool OnClick(ClickEvent e)
        {
            showNext();
            return base.OnClick(e);
        }

        public override void OnEntering(IScreen last)
        {
            base.OnEntering(last);
            ApplyToBackground(b =>
            {
                b.FadeTo(0.2f, 250);
            });
        }

        public override bool OnExiting(IScreen next)
        {
            progress.CurrentTime = 0;
            this.FadeOut(300);

            return false;
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            switch (e.Key)
            {
                case Key.Space:
                    showNext();
                    break;
            }

            return base.OnKeyDown(e);
        }

        private class ResultContainer : Container
        {
            private FillFlowContainer fillFlow;
            private Container spriteContainer;
            public RollResult Result { get; set; }

            [BackgroundDependencyLoader]
            private void load(TextureStore textures)
            {
                Anchor = Origin = Anchor.Centre;

                InternalChildren = new Drawable[]
                {
                    fillFlow = new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        LayoutDuration = 300,
                        LayoutEasing = Easing.OutQuint,
                        Direction = FillDirection.Vertical,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Spacing = new Vector2(20),
                        Child = spriteContainer = new Container
                        {
                            Width = 540,
                            Height = 300,
                            Masking = true,
                            CornerRadius = 12.5f,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Alpha = 0,
                            EdgeEffect = new EdgeEffectParameters
                            {
                                Type = EdgeEffectType.Glow,
                                Radius = 10,
                                Colour = PurcasheColorProvider.GetColor(Result.Rank)
                            },
                            Children = new Drawable[]
                            {
                                new Sprite
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Texture = textures.Get($"{Result.TexturePath ?? "Online/avatar-guest"}"),
                                    FillMode = FillMode.Fill
                                },
                            }
                        }
                    },
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                var name = new GlowingSpriteText
                {
                    Text = Result.RollName,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Font = OsuFont.Numeric.With(size: 40, weight: FontWeight.Black),
                    Depth = 1,
                    GlowColour = PurcasheColorProvider.GetColor(Result.Rank)
                };

                //动画
                spriteContainer.ScaleTo(0.8f).Then()
                               .FadeIn(300).ScaleTo(1, 300, Easing.OutQuint);

                this.Delay(500).Schedule(() =>
                {
                    spriteContainer.FadeColour(Color4.White, 300);
                    fillFlow.Add(name);
                });
            }
        }
    }
}
