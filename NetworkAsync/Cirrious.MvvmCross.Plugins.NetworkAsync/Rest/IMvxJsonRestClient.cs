// IMvxJsonRestClient.cs
// (c) Copyright Cirrious Ltd. http://www.cirrious.com
// MvvmCross is licensed using Microsoft Public License (Ms-PL)
// Contributions and inspirations noted in readme.md and license.txt
// 
// Project Lead - Stuart Lodge, @slodge, me@slodge.com

using System;
using System.Threading;
using System.Threading.Tasks;
using Cirrious.CrossCore.Platform;

namespace Cirrious.MvvmCross.Plugins.NetworkAsync.Rest
{
    public interface IMvxJsonRestClient
    {
        Func<IMvxJsonConverter> JsonConverterProvider { get; set; }

        void MakeRequestFor<T>(MvxRestRequest restRequest, Action<MvxDecodedRestResponse<T>> successAction,
                               Action<Exception> errorAction);

        Task<MvxDecodedRestResponse<T>> MakeRequestForAsync<T>(MvxRestRequest restRequest, CancellationToken ct);
        Task<MvxRestResponse> MakeRequestAsync(MvxRestRequest restRequest, CancellationToken ct);
    }
}