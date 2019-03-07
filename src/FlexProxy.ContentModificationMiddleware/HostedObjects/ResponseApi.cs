using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlexProxy.ContentModificationMiddleware.HostedObjects
{
    public interface IResponseApi
    {
        string GetUrl();
        string GetContentType();
        int GetStatusCode();
        string GetHeader(string name);
        void AddHeader(string name, string value);
        void UpdateHeader(string name, string value);
        void ReplaceHeader(string name, string oldValue, string newValue);
        void AddCookie(string key, string value);
        void AddCookie(string key, string value, string path, double? expireHours, string domain, bool httpOnly, bool secure);
        void DeleteCookie(string key);
    }

    public class ResponseApi : IResponseApi
    {
        private IHttpContextAccessor _httpContextAccessor;

        public ResponseApi(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetUrl()
        {
            return UriHelper.GetEncodedUrl(_httpContextAccessor.HttpContext.Request);
        }

        public string GetContentType()
        {
            return _httpContextAccessor.HttpContext.Response.ContentType;
        }

        public int GetStatusCode()
        {
            return _httpContextAccessor.HttpContext.Response.StatusCode;
        }

        public string GetHeader(string name)
        {
            return _httpContextAccessor.HttpContext.Response.Headers[name];
        }

        public void AddHeader(string name, string value)
        {
            var httpResponse = _httpContextAccessor.HttpContext.Response;

            if (!httpResponse.Headers.ContainsKey(name))
            {
                httpResponse.Headers.Add(name, value);
            }
        }

        public void UpdateHeader(string name, string value)
        {
            var httpResponse = _httpContextAccessor.HttpContext.Response;

            if (httpResponse.Headers.ContainsKey(name))
            {
                httpResponse.Headers[name] = value;
            }
        }

        public void ReplaceHeader(string name, string oldValue, string newValue)
        {
            var httpResponse = _httpContextAccessor.HttpContext.Response;

            if (!httpResponse.Headers.ContainsKey(name))
            {
                return;
            }

            var values = httpResponse.Headers[name];

            if (values.Count == 0)
            {
                return;
            }

            var updatedValues = values.Select(value => value.Replace(oldValue, newValue)).ToArray();

            httpResponse.Headers[name] = new StringValues(updatedValues);
        }

        public void AddCookie(string key, string value)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value))
            {
                return;
            }

            var httpResponse = _httpContextAccessor.HttpContext.Response;

            httpResponse.Cookies.Append(key, value);
        }

        public void AddCookie(string key, string value, string path, double? expireHours, string domain, bool httpOnly = false, bool secure = false)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value))
            {
                return;
            }

            var httpRequest = _httpContextAccessor.HttpContext.Request;
            var httpResponse = _httpContextAccessor.HttpContext.Response;

            var expires = expireHours > 0 ? new DateTimeOffset?(DateTime.Now.AddHours(expireHours.Value)) : null;

            var options = new CookieOptions
            {
                Path = string.IsNullOrEmpty(path) ? "/" : path,
                Expires = expires,
                Domain = string.IsNullOrEmpty(domain) ? httpRequest.Host.Value : domain,
                HttpOnly = httpOnly,
                Secure = secure
            };

            httpResponse.Cookies.Append(key, value, options);
        }

        public void DeleteCookie(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            var httpRequest = _httpContextAccessor.HttpContext.Request;
            var httpResponse = _httpContextAccessor.HttpContext.Response;

            if (httpRequest.Cookies[key] != null)
            {
                httpResponse.Cookies.Delete(key);
            }
        }
    }
}
