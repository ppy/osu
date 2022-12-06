// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osuTK;

namespace osu.Game.Overlays.FirstRunSetup
{
    public partial class ProgressRoundedButton : RoundedButton
    {
        public new Action? Action;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        private ProgressBar progressBar = null!;

        private LoadingSpinner loading = null!;

        private SpriteIcon tick = null!;

        public ProgressRoundedButton()
        {
            base.Action = () =>
            {
                loading.Show();
                Enabled.Value = false;

                Action?.Invoke();
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            AddRange(new Drawable[]
            {
                progressBar = new ProgressBar(false)
                {
                    RelativeSizeAxes = Axes.Both,
                    Blending = BlendingParameters.Additive,
                    FillColour = BackgroundColour,
                    Alpha = 0.5f,
                    Depth = float.MinValue
                },
                new Container
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    Margin = new MarginPadding(15),
                    Size = new Vector2(20),
                    Children = new Drawable[]
                    {
                        loading = new LoadingSpinner
                        {
                            RelativeSizeAxes = Axes.Both,
                            Size = Vector2.One,
                        },
                        tick = new SpriteIcon
                        {
                            Icon = FontAwesome.Solid.Check,
                            RelativeSizeAxes = Axes.Both,
                            Size = Vector2.One,
                            Alpha = 0,
                        }
                    }
                },
            });
        }

        public void Complete()
        {
            loading.Hide();
            tick.FadeIn(500, Easing.OutQuint);

            this.TransformTo(nameof(BackgroundColour), colours.Green, 500, Easing.OutQuint);
            progressBar.FillColour = colours.Green;

            this.TransformBindableTo(progressBar.Current, 1, 500, Easing.OutQuint);
        }

        public void Abort()
        {
            loading.Hide();
            Enabled.Value = true;
            this.TransformBindableTo(progressBar.Current, 0, 500, Easing.OutQuint);
        }

        public void SetProgress(double progress, bool animated)
        {
            this.TransformBindableTo(progressBar.Current, progress, animated ? 500 : 0, Easing.OutQuint);
        }
    }
}
