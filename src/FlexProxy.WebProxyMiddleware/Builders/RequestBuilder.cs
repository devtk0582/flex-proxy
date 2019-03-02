using FlexProxy.Core;
using FlexProxy.Core.Extensions;
using FlexProxy.Core.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace FlexProxy.WebProxyMiddleware.Builders
{
    public interface IRequestBuilder
    {
        void CopyHeaders(HttpContext context, HttpWebRequest remoteRequest, HostMappingOptions mapping);
        void CopyCookies(HttpContext context, HttpWebRequest remoteRequest, HostMappingOptions mapping, List<TransientCookie> transientCookies, IEnumerable<string> internalCookies = null);
    }

    public class RequestBuilder : IRequestBuilder
    {
        public void CopyHeaders(HttpContext context, HttpWebRequest remoteRequest, HostMappingOptions mapping)
        {
            var originalRequest = context.Request;

            remoteRequest.Method = originalRequest.Method;
            remoteRequest.ContentType = originalRequest.ContentType;
            remoteRequest.AllowAutoRedirect = false;
            remoteRequest.KeepAlive = false;
            remoteRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            foreach (var headerKey in originalRequest.Headers.Keys)
            {
                if (remoteRequest.Headers[headerKey] != null)
                {
                    continue;
                }

                switch (headerKey)
                {
                    case HttpHeaders.UserAgent:
                        remoteRequest.UserAgent = originalRequest.Headers[HttpHeaders.UserAgent];
                        break;
                    case HttpHeaders.Accept:
                        remoteRequest.Accept = originalRequest.Headers[headerKey];
                        break;
                    case HttpHeaders.Host:
                        remoteRequest.Host = new HostString(mapping.DownstreamHost).ToUriComponent();
                        break;
                    case HttpHeaders.IfModifiedSince:
                        DateTime modifiedSinceDate;

                        if (DateTime.TryParse(originalRequest.Headers[headerKey], out modifiedSinceDate))
                        {
                            remoteRequest.IfModifiedSince = modifiedSinceDate;
                        }
                        break;
                    case HttpHeaders.Referer:
                        string refererHeaderValue = originalRequest.Headers[headerKey];

                        if (Uri.IsWellFormedUriString(refererHeaderValue, UriKind.Absolute))
                        {
                            remoteRequest.Referer = new UriBuilder(refererHeaderValue).ReplaceHost(mapping.ServingHost, mapping.DownstreamHost, mapping.DownstreamScheme);
                        }
                        else
                        {
                            remoteRequest.Referer = refererHeaderValue.Replace(mapping.ServingHost, mapping.DownstreamHost);
                        }
                        break;
                    case HttpHeaders.Origin:
                        string originHeaderValue = originalRequest.Headers[headerKey];

                        if (Uri.IsWellFormedUriString(originHeaderValue, UriKind.Absolute))
                        {
                            remoteRequest.Headers.Add(headerKey, new UriBuilder(originHeaderValue).ReplaceHost(mapping.ServingHost, mapping.DownstreamHost, mapping.DownstreamScheme));
                        }
                        else
                        {
                            remoteRequest.Headers.Add(headerKey, originHeaderValue.Replace(mapping.ServingHost, mapping.DownstreamHost));
                        }
                        break;
                    case HttpHeaders.Expect:
                        remoteRequest.Expect = originalRequest.Headers[headerKey];
                        break;
                    case HttpHeaders.Date:
                        DateTime requestDate;

                        if (DateTime.TryParse(originalRequest.Headers[headerKey], out requestDate))
                        {
                            remoteRequest.Date = requestDate;
                        }
                        break;
                    case HttpHeaders.Range:
                        var ranges = ParseRangeHeaderValue(originalRequest.Headers[headerKey]);

                        foreach (var range in ranges)
                        {
                            if (range.Start.HasValue && range.End.HasValue)
                            {
                                remoteRequest.AddRange(range.Start.Value, range.End.Value);
                            }
                            else if (range.Start.HasValue)
                            {
                                remoteRequest.AddRange(range.Start.Value);
                            }
                            else
                            {
                                remoteRequest.AddRange(-range.End.Value);
                            }
                        }
                        break;
                    case HttpHeaders.Cookie:
                    case HttpHeaders.ContentLength:
                    case HttpHeaders.ContentType:
                    case HttpHeaders.Connection:
                    case HttpHeaders.KeepAlive:
                    case HttpHeaders.ProxyAuthorization:
                    case HttpHeaders.TE:
                    case HttpHeaders.Upgrade:
                    case HttpHeaders.AcceptEncoding:
                        break;
                    default:
                        remoteRequest.Headers.Add(headerKey, originalRequest.Headers[headerKey]);
                        break;
                }
            }
        }

        public void CopyCookies(HttpContext context, HttpWebRequest remoteRequest, HostMappingOptions mapping, List<TransientCookie> transientCookies, IEnumerable<string> internalCookies = null)
        {
            var originalRequest = context.Request;

            if (originalRequest.Cookies == null || originalRequest.Cookies.Count == 0)
            {
                return;
            }

            var cookies = originalRequest.Cookies;

            if (remoteRequest.CookieContainer == null)
                remoteRequest.CookieContainer = new CookieContainer();

            var cookiesToEncode = context.Items[Constants.HTTP_CONTEXT_COOKIES_TO_ENCODE_KEY] as List<string>;
            var cookiesToAdd = transientCookies?.Where(cookie => cookie.Action == TransientCookieAction.Add);
            var cookiesToUpdate = transientCookies?.Where(cookie => cookie.Action == TransientCookieAction.Update);
            var cookiesToRemove = transientCookies?.Where(cookie => cookie.Action == TransientCookieAction.Delete);

            foreach (var cookie in cookies)
            {
                if (internalCookies.Contains(cookie.Key))
                {
                    continue;
                }

                if (cookiesToRemove != null
                    &&
                    cookiesToRemove.Any(transientCookie => transientCookie.Name.Equals(cookie.Key)))
                {
                    continue;
                }

                if (cookiesToUpdate != null
                    &&
                    cookiesToUpdate.Any(transientCookie => transientCookie.Name.Equals(cookie.Key)))
                {
                    var cookieToUpdate = cookiesToUpdate.FirstOrDefault(transientCookie => transientCookie.Name.Equals(cookie.Key));

                    AddCookie(remoteRequest, cookieToUpdate.Name, cookieToUpdate.Value);

                    continue;
                }

                UpdateCookieForRemoteRequest(context, remoteRequest, cookie, mapping, cookiesToEncode);
            }

            if (cookiesToAdd != null)
            {
                foreach (var cookie in cookiesToAdd)
                {
                    AddCookie(remoteRequest, cookie.Name, cookie.Value);
                }
            }
        }

        private void UpdateCookieForRemoteRequest(HttpContext context, HttpWebRequest remoteRequest, KeyValuePair<string, string> originalCookie, HostMappingOptions mapping, List<string> cookiesToEncode)
        {
            var cookieKey = originalCookie.Key.Replace(mapping.ServingHost, mapping.DownstreamHost);
            var cookieValue = originalCookie.Value.Replace(mapping.ServingHost, mapping.DownstreamHost);

            if (cookiesToEncode != null && cookiesToEncode.Contains(originalCookie.Key))
            {
                cookieValue = WebUtility.UrlEncode(cookieValue);
            }

            AddCookie(remoteRequest, cookieKey, cookieValue);
        }

        private void AddCookie(HttpWebRequest remoteRequest, string key, string value)
        {
            var cookie = new Cookie(key, value)
            {
                Domain = remoteRequest.Host
            };

            remoteRequest.CookieContainer.Add(cookie);
        }

        private List<RangeHeader> ParseRangeHeaderValue(string value)
        {
            List<RangeHeader> ranges = new List<RangeHeader>();

            if (!Regex.IsMatch(value, @"^bytes=\d*-\d*(,\d*-\d*)*$"))
            {
                return ranges;
            }

            string[] rangePairs = value.Substring(6).Split(',');

            foreach (string rangePair in rangePairs)
            {
                string[] rangeValues = rangePair.Trim().Split('-');

                var rangeStart = rangeValues[0];
                var rangeEnd = rangeValues[1];

                if (!string.IsNullOrEmpty(rangeStart) && !string.IsNullOrEmpty(rangeEnd))
                {
                    ranges.Add(new RangeHeader(Convert.ToInt64(rangeStart), Convert.ToInt64(rangeEnd)));
                }
                else if (!string.IsNullOrEmpty(rangeStart))
                {
                    ranges.Add(new RangeHeader(Convert.ToInt64(rangeStart), null));
                }
                else
                {
                    ranges.Add(new RangeHeader(null, Convert.ToInt64(rangeEnd)));
                }
            }

            return ranges;
        }
    }
}
