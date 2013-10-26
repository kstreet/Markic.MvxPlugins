using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkAsync.SampleApp.Core.Services
{
    public interface ISimpleRestService
    {
        // Pass in the <T>ype of thing you are expecting back
        // verb usually like GET, POST, PUT

        // TODO: Asyncify
        void MakeRequest<T>(string requestUrl, string verb, Action<T> successAction, Action<Exception> errorAction);
    }
}
