// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Resources.Localisation.Web;
using osuTK.Graphics;

namespace osu.Game.Overlays.Profile.Header.Components
{
    public partial class LevelProgressBar : CompositeDrawable, IHasTooltip
    {
        public readonly Bindable<UserProfileData?> User = new Bindable<UserProfileData?>();

        public LocalisableString TooltipText { get; }

        private Bar levelProgressBar = null!;
        private OsuSpriteText levelProgressText = null!;

        public LevelProgressBar()
        {
            TooltipText = UsersStrings.ShowStatsLevelProgress;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            InternalChildren = new Drawable[]
            {
                new CircularContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    Child = levelProgressBar = new Bar
                    {
                        RelativeSizeAxes = Axes.Both,
                        BackgroundColour = Color4.Black,
                        Direction = BarDirection.LeftToRight,
                        AccentColour = colourProvider.Highlight1
                    }
                },
                levelProgressText = new OsuSpriteText
                {
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.TopRight,
                    Font = OsuFont.GetFont(size: 12, weight: FontWeight.Bold)
                }
            };

            User.BindValueChanged(user => updateProgress(user.NewValue?.User));
        }

        private void updateProgress(APIUser? user)
        {
            levelProgressBar.Length = user?.Statistics?.Level.Progress / 100f ?? 0;
            levelProgressText.Text = user?.Statistics?.Level.Progress.ToLocalisableString("0'%'") ?? default;
        }
    }
}
