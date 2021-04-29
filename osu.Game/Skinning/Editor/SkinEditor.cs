// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Cursor;

namespace osu.Game.Skinning.Editor
{
    public class SkinEditor : VisibilityContainer
    {
        private readonly Drawable target;
        private Container border;

        protected override bool StartHidden => true;

        public SkinEditor(Drawable target)
        {
            this.target = target;

            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            InternalChild = new OsuContextMenuContainer
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    border = new Container
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Masking = true,
                        BorderColour = colours.Yellow,
                        BorderThickness = 5,
                        CornerRadius = 5,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                AlwaysPresent = true,
                                Alpha = 0,
                                RelativeSizeAxes = Axes.Both,
                            },
                        }
                    },
                    new SkinBlueprintContainer(target),
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Show();
        }

        private const double transition_duration = 500;
        private const float visible_target_scale = 0.8f;

        protected override void PopIn()
        {
            if (IsLoaded)
            {
                target.ScaleTo(visible_target_scale, transition_duration, Easing.OutQuint);
                border.ScaleTo(visible_target_scale, transition_duration, Easing.OutQuint);
            }

            this.FadeIn(transition_duration);
        }

        protected override void PopOut()
        {
            if (IsLoaded)
            {
                target.ScaleTo(1, transition_duration, Easing.OutQuint);
                border.ScaleTo(1, transition_duration, Easing.OutQuint);
            }

            this.FadeOut(transition_duration, Easing.OutQuint);
        }
    }
}
