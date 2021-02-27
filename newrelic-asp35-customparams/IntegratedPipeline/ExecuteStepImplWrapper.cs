using NewRelic.Agent.Api;
using NewRelic.Agent.Extensions.Providers.Wrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Custom.Providers.Wrapper.Asp35
{
    public class ExecuteStepImplWrapper : IWrapper
    {
        private const string AssemblyName = "System.Web";
        private const string TypeName = "System.Web.HttpApplication";
        private const string MethodName = "ExecuteStepImpl";
        public bool IsTransactionRequired => true;

        public CanWrapResponse CanWrap(InstrumentedMethodInfo instrumentedMethodInfo)
        {
            var method = instrumentedMethodInfo.Method;
            var canWrap = method.MatchesAny(
                assemblyNames: new[] { AssemblyName },
                typeNames: new[] { TypeName },
                methodNames: new[] { MethodName }
            );

            return new CanWrapResponse(canWrap);
        }

        public AfterWrappedMethodDelegate BeforeWrappedMethod(InstrumentedMethodCall instrumentedMethodCall,
            IAgent agent, ITransaction transaction)
        {
            if (!HttpRuntime.UsingIntegratedPipeline)
                return Delegates.NoOp;

            var httpApplication = (HttpApplication)instrumentedMethodCall.MethodCall.InvocationTarget;
            if (httpApplication == null)
                throw new NullReferenceException("httpApplication");

            var httpContext = httpApplication.Context;
            if (httpContext == null)
                throw new NullReferenceException("httpContext");

            var requestPath = RequestPathRetriever.TryGetRequestPath(httpContext.Request);

            var requestUrl = RequestUrlRetriever.TryGetRequestUrl(httpContext.Request, () => requestPath);
            if (requestUrl != null)
            {
                transaction.AddCustomAttribute("ism.Url", requestUrl.AbsoluteUri);
            }

            // We are using a variant of GetDelegateFor that doesn't pass down a return value to the local methods since we don't need the return.
            return Delegates.GetDelegateFor(
                onSuccess: OnSuccess
            );

            void OnSuccess()
            {
            }
        }
    }

}
