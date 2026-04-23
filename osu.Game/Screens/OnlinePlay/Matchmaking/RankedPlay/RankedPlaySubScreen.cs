// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Globalization;
using Humanizer;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Components;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay
{
    public abstract partial class RankedPlaySubScreen : Container
    {
        public const float CENTERED_CARD_SCALE = 1.2f;

        public readonly Bindable<Visibility> CornerPieceVisibility = new Bindable<Visibility>(Visibility.Visible);
        protected readonly Bindable<Visibility> CountdownVisibility = new Bindable<Visibility>(Visibility.Visible);

        public virtual bool ShowBeatmapBackground => false;

        /// <summary>
        /// Whether a fullscreen overlay displaying the current stage (and any additional
        /// information like the currently picking player and/or the damage multiplier)
        /// should be displayed upon entering this screen.
        /// </summary>
        public virtual bool ShowStageOverlay => false;

        /// <summary>
        /// Heading text to be displayed indicating the purpose of the current stage.
        /// </summary>
        public abstract LocalisableString StageHeading { get; }

        /// <summary>
        /// Subtitle text to be displayed indicating the action a user should take in the current stage.
        /// </summary>
        protected LocalisableString StageCaption
        {
            get => StageDisplay.Caption;
            set => StageDisplay.Caption = value;
        }

        /// <summary>
        /// The colour scheme commonly used for components of this screen.
        /// </summary>
        protected virtual RankedPlayColourScheme ColourScheme => RankedPlayColourScheme.BLUE;

        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        protected MultiplayerClient Client => client;

        protected override Container<Drawable> Content { get; }

        /// <summary>
        /// Column in the centre of the screen whose width is calculated so its content don't overlap with the <see cref="RankedPlayCornerPiece"/>s
        /// </summary>
        protected readonly Container CenterColumn;

        protected readonly RankedPlayStageDisplay StageDisplay;

        protected RankedPlaySubScreen()
        {
            RelativeSizeAxes = Axes.Both;

            InternalChildren =
            [
                CenterColumn = new Container
                {
                    Name = "Center Column",
                    RelativeSizeAxes = Axes.Y,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                Content = new Container
                {
                    Name = "Content",
                    RelativeSizeAxes = Axes.Both,
                },
                StageDisplay = new RankedPlayStageDisplay(ColourScheme)
                {
                    Heading = StageHeading,
                    Margin = new MarginPadding { Top = 60 },
                    State = { BindTarget = CountdownVisibility }
                },
            ];
        }

        protected override void Update()
        {
            base.Update();

            CenterColumn.Width = DrawWidth - RankedPlayCornerPiece.WidthFor(DrawWidth) * 2;
        }

        public virtual void OnEntering(RankedPlaySubScreen? previous)
        {
        }

        public virtual void OnExiting(RankedPlaySubScreen? next)
        {
            Hide();
        }

        protected static string FormatRoundIndex(int roundNumber)
        {
            return roundNumber >= 10 ? roundNumber.Ordinalize(CultureInfo.InvariantCulture) : roundNumber.ToOrdinalWords(CultureInfo.InvariantCulture);
        }
    }
}
