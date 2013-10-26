// MvxRestClient.cs
// (c) Copyright Cirrious Ltd. http://www.cirrious.com
// MvvmCross is licensed using Microsoft Public License (Ms-PL)
// Contributions and inspirations noted in readme.md and license.txt
// 
// Project Lead - Stuart Lodge, @slodge, me@slodge.com

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Cirrious.CrossCore;
using Cirrious.CrossCore.Exceptions;

namespace Cirrious.MvvmCross.Plugins.NetworkAsync.Rest
{
    public class MvxRestClient : IMvxRestClient
    {
        protected static void TryCatch(Action toTry, Action<Exception> errorAction)
        {
            try
            {
                toTry();
            }
            catch (Exception exception)
            {
                errorAction(exception);
            }
        }

        protected Dictionary<string, object> Options { set; private get; }

        public MvxRestClient()
        {
            Options = new Dictionary<string, object>
                {
                    {MvxKnownOptions.ForceWindowsPhoneToUseCompression, "true"}
                };
        }

        public void ClearSetting(string key)
        {
            try
            {
                Options.Remove(key);
            }
            catch (KeyNotFoundException)
            {
                // ignored - not a problem
            }
        }

        public void SetSetting(string key, object value)
        {
            Options[key] = value;
        }

        public void MakeRequest(MvxRestRequest restRequest, Action<MvxStreamRestResponse> successAction,
                                Action<Exception> errorAction)
        {
            TryCatch(() =>
                {
                    var httpRequest = BuildHttpRequest(restRequest);

                    Action processResponse = () => ProcessResponse(restRequest, httpRequest, successAction, errorAction);
                    if (restRequest.NeedsRequestStream)
                    {
                        ProcessRequestThen(restRequest, httpRequest, processResponse, errorAction);
                    }
                    else
                    {
                        processResponse();
                    }
                }, errorAction);
        }

        public void MakeRequest(MvxRestRequest restRequest, Action<MvxRestResponse> successAction,
                                Action<Exception> errorAction)
        {
            TryCatch(() =>
                {
                    var httpRequest = BuildHttpRequest(restRequest);

                    Action processResponse = () => ProcessResponse(restRequest, httpRequest, successAction, errorAction);
                    if (restRequest.NeedsRequestStream)
                    {
                        ProcessRequestThen(restRequest, httpRequest, processResponse, errorAction);
                    }
                    else
                    {
                        processResponse();
                    }
                }, errorAction);
        }

        public async Task<MvxRestResponse> MakeRequestAsync(MvxRestRequest restRequest, CancellationToken ct)
        {
            var httpRequest = BuildHttpRequest(restRequest);
            await AddContentIfNeededAsync(httpRequest, restRequest, ct);
            return await ProcessResponseAsync(restRequest, httpRequest, ct);
        }

        public async Task<MvxTextRestResponse> MakeRequestTextAsync(MvxRestRequest restRequest, CancellationToken ct)
        {
            HttpWebRequest httpRequest = BuildHttpRequest(restRequest);
            await AddContentIfNeededAsync(httpRequest, restRequest, ct);
            return await ProcessResponseTextAsync(restRequest, httpRequest, ct).ConfigureAwait(false);
        }

        private async Task AddContentIfNeededAsync(HttpWebRequest httpRequest, MvxRestRequest restRequest, CancellationToken ct)
        {
            if (restRequest.NeedsRequestStream)
            {
                var task = Task.Factory.FromAsync(
                    (cb, o) => ((HttpWebRequest)o).BeginGetRequestStream(cb, o),
                    res => (Stream)((HttpWebRequest)res.AsyncState).EndGetRequestStream(res),
                    httpRequest,
                    hr => hr.Abort(),
                    ct);
                using (Stream stream = await task.ConfigureAwait(false))
                {
                    restRequest.ProcessRequestStream(stream);
                    stream.Flush();
                }
            }
        }

        protected virtual HttpWebRequest BuildHttpRequest(MvxRestRequest restRequest)
        {
            var httpRequest = CreateHttpWebRequest(restRequest);
            SetMethod(restRequest, httpRequest);
            SetContentType(restRequest, httpRequest);
            SetUserAgent(restRequest, httpRequest);
            SetAccept(restRequest, httpRequest);
            SetCookieContainer(restRequest, httpRequest);
            SetCredentials(restRequest, httpRequest);
            SetCustomHeaders(restRequest, httpRequest);
            SetPlatformSpecificProperties(restRequest, httpRequest);
            return httpRequest;
        }

        private static void SetCustomHeaders(MvxRestRequest restRequest, HttpWebRequest httpRequest)
        {
            if (restRequest.Headers != null)
            {
                foreach (var kvp in restRequest.Headers)
                {
                    httpRequest.Headers[kvp.Key] = kvp.Value;
                }
            }
        }

        protected virtual void SetCredentials(MvxRestRequest restRequest, HttpWebRequest httpRequest)
        {
            if (restRequest.Credentials != null)
            {
                httpRequest.Credentials = restRequest.Credentials;
            }
        }

        protected virtual void SetCookieContainer(MvxRestRequest restRequest, HttpWebRequest httpRequest)
        {
            // note that we don't call
            //   httpRequest.SupportsCookieContainer
            // here - this is because Android complained about this...
            try
            {
                if (restRequest.CookieContainer != null)
                {
                    httpRequest.CookieContainer = restRequest.CookieContainer;
                }
            }
            catch (Exception exception)
            {
                Mvx.Warning("Error masked during Rest call - cookie creation - {0}", exception.ToLongString());
            }
        }

        protected virtual void SetAccept(MvxRestRequest restRequest, HttpWebRequest httpRequest)
        {
            if (!string.IsNullOrEmpty(restRequest.Accept))
            {
                httpRequest.Accept = restRequest.Accept;
            }
        }

        protected virtual void SetUserAgent(MvxRestRequest restRequest, HttpWebRequest httpRequest)
        {
            if (!string.IsNullOrEmpty(restRequest.UserAgent))
            {
                httpRequest.Headers["user-agent"] = restRequest.UserAgent;
            }
        }

        protected virtual void SetContentType(MvxRestRequest restRequest, HttpWebRequest httpRequest)
        {
            if (!string.IsNullOrEmpty(restRequest.ContentType))
            {
                httpRequest.ContentType = restRequest.ContentType;
            }
        }

        protected virtual void SetMethod(MvxRestRequest restRequest, HttpWebRequest httpRequest)
        {
            httpRequest.Method = restRequest.Verb;
        }

        protected virtual HttpWebRequest CreateHttpWebRequest(MvxRestRequest restRequest)
        {
            return (HttpWebRequest) WebRequest.Create(restRequest.Uri);
        }

        protected virtual void SetPlatformSpecificProperties(MvxRestRequest restRequest, HttpWebRequest httpRequest)
        {
            // do nothing by default
        }

        protected virtual async Task<MvxRestResponse> ProcessResponseAsync(MvxRestRequest restRequest, HttpWebRequest httpRequest, CancellationToken ct)
        {
            var task = Task.Factory.FromAsync(
                (cb, o) => ((HttpWebRequest)o).BeginGetResponse(cb, o), 
                res => (HttpWebResponse)((HttpWebRequest)res.AsyncState).EndGetResponse(res), 
                httpRequest,
                hr => hr.Abort(),
                ct);
            var response = await task.ConfigureAwait(false);
            var code = response.StatusCode;

            var restResponse = new MvxRestResponse
            {
                CookieCollection = response.Cookies,
                Tag = restRequest.Tag,
                StatusCode = code
            };
            return restResponse;
        }

        protected virtual void ProcessResponse(
            MvxRestRequest restRequest,
            HttpWebRequest httpRequest,
            Action<MvxRestResponse> successAction,
            Action<Exception> errorAction)
        {
            httpRequest.BeginGetResponse(result =>
                                         TryCatch(() =>
                                             {
                                                 var response = (HttpWebResponse) httpRequest.EndGetResponse(result);

                                                 var code = response.StatusCode;

                                                 var restResponse = new MvxRestResponse
                                                     {
                                                         CookieCollection = response.Cookies,
                                                         Tag = restRequest.Tag,
                                                         StatusCode = code
                                                     };
                                                 successAction(restResponse);
                                             }, errorAction)
                                         , null);
        }

        protected virtual async Task<MvxTextRestResponse> ProcessResponseTextAsync(MvxRestRequest restRequest, HttpWebRequest httpRequest, CancellationToken ct)
        {
            
            var task = Task.Factory.FromAsync(
                (cb, o) => ((HttpWebRequest)o).BeginGetResponse(cb, o),
                res => (HttpWebResponse)((HttpWebRequest)res.AsyncState).EndGetResponse(res),
                httpRequest,
                hr => hr.Abort(),
                ct);

            try
            {
                var response = await task.ConfigureAwait(false);
                var code = response.StatusCode;

                using (var responseStream = response.GetResponseStream())
                using (var textReader = new StreamReader(responseStream))
                {
                    var restResponse = new MvxTextRestResponse
                    {
                        CookieCollection = response.Cookies,
                        Tag = restRequest.Tag,
                        StatusCode = code,
                        Text = textReader.ReadToEnd()
                    };
                    return restResponse;
                };
            }
            catch (Exception ex)
            {
                Mvx.Trace(Cirrious.CrossCore.Platform.MvxTraceLevel.Error, ex.Message);
                throw;
            }
        }

        protected virtual void ProcessResponse(
            MvxRestRequest restRequest,
            HttpWebRequest httpRequest,
            Action<MvxStreamRestResponse> successAction,
            Action<Exception> errorAction)
        {
            httpRequest.BeginGetResponse(result =>
                                         TryCatch(() =>
                                             {
                                                 var response = (HttpWebResponse) httpRequest.EndGetResponse(result);

                                                 var code = response.StatusCode;

                                                 using (var responseStream = response.GetResponseStream())
                                                 {
                                                     var restResponse = new MvxStreamRestResponse
                                                         {
                                                             CookieCollection = response.Cookies,
                                                             Stream = responseStream,
                                                             Tag = restRequest.Tag,
                                                             StatusCode = code
                                                         };
                                                     successAction(restResponse);
                                                 }
                                             }, errorAction)
                                         , null);
        }

        protected virtual void ProcessRequestThen(
            MvxRestRequest restRequest,
            HttpWebRequest httpRequest,
            Action continueAction,
            Action<Exception> errorAction)
        {
            httpRequest.BeginGetRequestStream(result =>
                                              TryCatch(() =>
                                                  {
                                                      using (var stream = httpRequest.EndGetRequestStream(result))
                                                      {
                                                          restRequest.ProcessRequestStream(stream);
                                                          stream.Flush();
                                                      }

                                                      continueAction();
                                                  }, errorAction)
                                              , null);
        }
    }
}