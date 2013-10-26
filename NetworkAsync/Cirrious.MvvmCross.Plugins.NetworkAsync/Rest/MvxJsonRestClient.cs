// MvxJsonRestClient.cs
// (c) Copyright Cirrious Ltd. http://www.cirrious.com
// MvvmCross is licensed using Microsoft Public License (Ms-PL)
// Contributions and inspirations noted in readme.md and license.txt
// 
// Project Lead - Stuart Lodge, @slodge, me@slodge.com

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Cirrious.CrossCore;
using Cirrious.CrossCore.Platform;

namespace Cirrious.MvvmCross.Plugins.NetworkAsync.Rest
{
    public class MvxJsonRestClient
        : MvxRestClient
          , IMvxJsonRestClient
    {
        public Func<IMvxJsonConverter> JsonConverterProvider { get; set; }

        public void MakeRequestFor<T>(MvxRestRequest restRequest, Action<MvxDecodedRestResponse<T>> successAction,
                                      Action<Exception> errorAction)
        {
            MakeRequest(restRequest, (MvxStreamRestResponse streamResponse) =>
                {
                    using (var textReader = new StreamReader(streamResponse.Stream))
                    {
                        var text = textReader.ReadToEnd();
                        var result = JsonConverterProvider().DeserializeObject<T>(text);
                        var decodedResponse = new MvxDecodedRestResponse<T>
                            {
                                CookieCollection = streamResponse.CookieCollection,
                                Result = result,
                                StatusCode = streamResponse.StatusCode,
                                Tag = streamResponse.Tag
                            };
                        successAction(decodedResponse);
                    }
                }, errorAction);
        }

        public async Task<MvxDecodedRestResponse<T>> MakeRequestForAsync<T>(MvxRestRequest restRequest, CancellationToken ct)
        {
            MvxTextRestResponse response = await MakeRequestTextAsync(restRequest, ct);
            string text = response.Text;
            var result = JsonConverterProvider().DeserializeObject<T>(text);
            var decodedResponse = new MvxDecodedRestResponse<T>
            {
                CookieCollection = response.CookieCollection,
                Result = result,
                StatusCode = response.StatusCode,
                Tag = response.Tag
            };
            return decodedResponse;
        }

        public MvxJsonRestClient()
        {
            JsonConverterProvider = () => Mvx.Resolve<IMvxJsonConverter>();
        }
    }
}