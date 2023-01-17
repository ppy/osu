// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Users.Drawables;
using osuTK;

namespace osu.Game.Screens.Ranking.Expanded
{
    /// <summary>
    /// The content that appears in the middle section of the <see cref="ScorePanel"/>.
    /// </summary>
    public partial class ExpandedPanelTopContent : CompositeDrawable
    {
        private readonly APIUser user;

        private Sample appearanceSample;

        private readonly bool playAppearanceSound;

        /// <summary>
        /// Creates a new <see cref="ExpandedPanelTopContent"/>.
        /// </summary>
        /// <param name="user">The <see cref="APIUser"/> to display.</param>
        /// <param name="playAppearanceSound">Whether the appearance sample should play</param>
        public ExpandedPanelTopContent(APIUser user, bool playAppearanceSound = false)
        {
            this.user = user;
            this.playAppearanceSound = playAppearanceSound;
            Anchor = Anchor.TopCentre;
            Origin = Anchor.Centre;

            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            appearanceSample = audio.Samples.Get(@"Results/score-panel-top-appear");

            InternalChild = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Children = new Drawable[]
                {
                    new UpdateableAvatar(user)
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Size = new Vector2(80),
                        CornerRadius = 20,
                        CornerExponent = 2.5f,
                        Masking = true,
                    },
                    new OsuSpriteText
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Text = user.Username,
                        Font = OsuFont.Torus.With(size: 16, weight: FontWeight.SemiBold)
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (playAppearanceSound)
                appearanceSample?.Play();
        }
    }
}
