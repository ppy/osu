// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets;
using osu.Game.Users;
using JetBrains.Annotations;

namespace osu.Game.Overlays.Profile.Sections
{
    public abstract class ProfileSubsection : FillFlowContainer
    {
        protected readonly Bindable<User> User = new Bindable<User>();

        protected RulesetStore Rulesets { get; private set; }

        protected OsuSpriteText Missing { get; private set; }

        private readonly string headerText;
        private readonly string missingText;
        private readonly CounterVisibilityState counterVisibilityState;

        private ProfileSubsectionHeader header;

        protected ProfileSubsection(Bindable<User> user, string headerText = "", string missingText = "", CounterVisibilityState counterVisibilityState = CounterVisibilityState.AlwaysHidden)
        {
            this.headerText = headerText;
            this.missingText = missingText;
            this.counterVisibilityState = counterVisibilityState;
            User.BindTo(user);
        }

        [BackgroundDependencyLoader]
        private void load(RulesetStore rulesets)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Direction = FillDirection.Vertical;

            Children = new[]
            {
                header = new ProfileSubsectionHeader(headerText, counterVisibilityState)
                {
                    Alpha = string.IsNullOrEmpty(headerText) ? 0 : 1
                },
                CreateContent(),
                Missing = new OsuSpriteText
                {
                    Font = OsuFont.GetFont(size: 15),
                    Text = missingText,
                    Alpha = 0,
                },
            };

            Rulesets = rulesets;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            User.BindValueChanged(OnUserChanged, true);
        }

        [NotNull]
        protected abstract Drawable CreateContent();

        protected virtual void OnUserChanged(ValueChangedEvent<User> e)
        {
        }

        protected void SetCount(int value) => header.Current.Value = value;
    }
}
