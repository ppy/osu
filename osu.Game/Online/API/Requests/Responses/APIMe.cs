// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace osu.Game.Online.API.Requests.Responses
{
    public class APIMe : APIUser
    {
        [JsonProperty("session_verification_method")]
        public SessionVerificationMethod? SessionVerificationMethod { get; set; }

        /// <summary>
        /// Comes from osu-web envvar `SCORE_PROCESSING_NOTICE_URL`
        /// </summary>
        [JsonProperty("score_processing_notice_url")]
        public string ScoreProcessingNoticeUrl { get; set; } = string.Empty;
    }

    public enum SessionVerificationMethod
    {
        [Description("Timed one-time password")]
        [EnumMember(Value = "totp")]
        TimedOneTimePassword,

        [Description("E-mail")]
        [EnumMember(Value = "mail")]
        EmailMessage,
    }
}
