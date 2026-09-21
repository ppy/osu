// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Net;

namespace osu.Game.Online.API
{
    public class APIException : InvalidOperationException
    {
        public HttpStatusCode? StatusCode { get; }

        public APIException(string message, Exception? innerException, HttpStatusCode? statusCode = null)
            : base(message, innerException)
        {
            StatusCode = statusCode;
        }
    }
}
