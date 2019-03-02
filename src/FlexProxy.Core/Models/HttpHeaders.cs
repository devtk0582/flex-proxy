using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlexProxy.Core.Models
{
    public class HttpHeaders
    {
        public const string Accept = HeaderNames.Accept;
        public const string AcceptEncoding = HeaderNames.AcceptEncoding;
        public const string Host = HeaderNames.Host;
        public const string IfModifiedSince = HeaderNames.IfModifiedSince;
        public const string Referer = HeaderNames.Referer;
        public const string Origin = "Origin";
        public const string Expect = HeaderNames.Expect;
        public const string Date = HeaderNames.Date;
        public const string Range = HeaderNames.Range;
        public const string UserAgent = HeaderNames.UserAgent;
        public const string Cookie = HeaderNames.Cookie;
        public const string ContentLength = HeaderNames.ContentLength;
        public const string ContentType = HeaderNames.ContentType;
        public const string Connection = HeaderNames.Connection;
        public const string KeepAlive = "Keep-Alive";
        public const string ProxyAuthenticate = HeaderNames.ProxyAuthenticate;
        public const string ProxyAuthorization = HeaderNames.ProxyAuthorization;
        public const string TE = HeaderNames.TE;
        public const string Trailer = HeaderNames.Trailer;
        public const string TransferEncoding = HeaderNames.TransferEncoding;
        public const string Upgrade = HeaderNames.Upgrade;
        public const string SetCookie = HeaderNames.SetCookie;
        public const string AccessControlAllowOrigin = "Access-Control-Allow-Origin";
        public const string Location = HeaderNames.Location;
        public const string WWWAuthenticate = HeaderNames.WWWAuthenticate;
        public const string ContentEncoding = HeaderNames.ContentEncoding;
    }
}
