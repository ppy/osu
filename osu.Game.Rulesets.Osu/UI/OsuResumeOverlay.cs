// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Rulesets.Osu.UI.Cursor;
using osu.Game.Screens.Play;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.UI
{
    public partial class OsuResumeOverlay : ResumeOverlay
    {
        private Container cursorScaleContainer;
        private OsuClickToResumeCursor clickToResumeCursor;

        private OsuCursorContainer localCursorContainer;
        private IBindable<float> localCursorScale;

        public override CursorContainer LocalCursor => State.Value == Visibility.Visible ? localCursorContainer : null;

        protected override LocalisableString Message => "Click the orange cursor to resume";

        [BackgroundDependencyLoader]
        private void load()
        {
            Add(cursorScaleContainer = new Container
            {
                Child = clickToResumeCursor = new OsuClickToResumeCursor { ResumeRequested = Resume }
            });
        }

        protected override void PopIn()
        {
            base.PopIn();

            GameplayCursor.ActiveCursor.Hide();
            cursorScaleContainer.Position = ToLocalSpace(GameplayCursor.ActiveCursor.ScreenSpaceDrawQuad.Centre);
            clickToResumeCursor.Appear();

            if (localCursorContainer == null)
            {
                Add(localCursorContainer = new OsuCursorContainer());

                localCursorScale = new BindableFloat();
                localCursorScale.BindTo(localCursorContainer.CursorScale);
                localCursorScale.BindValueChanged(scale => cursorScaleContainer.Scale = new Vector2(scale.NewValue), true);
            }
        }

        protected override void PopOut()
        {
            base.PopOut();

            localCursorContainer?.Expire();
            localCursorContainer = null;
            GameplayCursor?.ActiveCursor.Show();
        }

        protected override bool OnHover(HoverEvent e) => true;

        public partial class OsuClickToResumeCursor : OsuCursor, IKeyBindingHandler<OsuAction>
        {
            public override bool HandlePositionalInput => true;

            public Action ResumeRequested;

            public OsuClickToResumeCursor()
            {
                RelativePositionAxes = Axes.Both;
            }

            protected override bool OnHover(HoverEvent e)
            {
                updateColour();
                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                updateColour();
                base.OnHoverLost(e);
            }

            public bool OnPressed(KeyBindingPressEvent<OsuAction> e)
            {
                switch (e.Action)
                {
                    case OsuAction.LeftButton:
                    case OsuAction.RightButton:
                        if (!IsHovered) return false;

                        this.ScaleTo(2, TRANSITION_TIME, Easing.OutQuint);

                        ResumeRequested?.Invoke();
                        return true;
                }

                return false;
            }

            public void OnReleased(KeyBindingReleaseEvent<OsuAction> e)
            {
            }

            public void Appear() => Schedule(() =>
            {
                updateColour();
                this.ScaleTo(4).Then().ScaleTo(1, 1000, Easing.OutQuint);
            });

            private void updateColour()
            {
                this.FadeColour(IsHovered ? Color4.White : Color4.Orange, 400, Easing.OutQuint);
            }
        }
    }
}
