using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;

namespace Mvis.Plugin.RulesetPanel.Overlays
{
    public class SandboxCheckbox : CompositeDrawable
    {
        public readonly BindableBool Current = new BindableBool();

        private Sample sampleChecked;
        private Sample sampleUnchecked;

        public SandboxCheckbox(string label)
        {
            AutoSizeAxes = Axes.Both;
            AddInternal(new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(10, 0),
                Children = new Drawable[]
                {
                    new LocalBox
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Current = { BindTarget = Current }
                    },
                    new OsuSpriteText
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Colour = Color4.Black,
                        Font = OsuFont.GetFont(size: 20),
                        Text = label
                    }
                }
            });
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            sampleChecked = audio.Samples.Get(@"UI/check-on");
            sampleUnchecked = audio.Samples.Get(@"UI/check-off");
        }

        protected override bool OnClick(ClickEvent e)
        {
            Current.Toggle();

            if (Current.Value)
                sampleChecked?.Play();
            else
                sampleUnchecked?.Play();

            return true;
        }

        private class LocalBox : CompositeDrawable
        {
            public readonly BindableBool Current = new BindableBool();

            private readonly Container fill;

            public LocalBox()
            {
                Size = new Vector2(20);
                Masking = true;
                BorderColour = Color4.Black;
                BorderThickness = 3;
                CornerRadius = 3;
                InternalChildren = new Drawable[]
                {
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Masking = true,
                        BorderColour = Color4.Black,
                        BorderThickness = 3,
                        CornerRadius = 3,
                        Child = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0,
                            AlwaysPresent = true
                        }
                    },
                    fill = new Container
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Masking = true,
                        CornerRadius = 3,
                        Child = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Black
                        }
                    }
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                Current.BindValueChanged(enabled =>
                {
                    fill.ScaleTo(enabled.NewValue ? 0.5f : 0, 250, Easing.OutQuint);
                }, true);

                fill.FinishTransforms();
            }
        }
    }
}
