// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Utils
{
    public static class SupportedExtensions
    {
        public static readonly string[] VIDEO_EXTENSIONS = [@".mp4", @".mov", @".avi", @".flv", @".mpg", @".wmv", @".m4v"];
        public static readonly string[] AUDIO_EXTENSIONS = [@".mp3", @".ogg", @".wav"];
        public static readonly string[] IMAGE_EXTENSIONS = [@".jpg", @".jpeg", @".png"];

        public static readonly string[] ALL_EXTENSIONS =
        [
            ..VIDEO_EXTENSIONS,
            ..AUDIO_EXTENSIONS,
            ..IMAGE_EXTENSIONS
        ];
    }
}
