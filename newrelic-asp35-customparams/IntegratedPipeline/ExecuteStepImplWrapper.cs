using NewRelic.Agent.Api;
using NewRelic.Agent.Extensions.Logging;
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
        private string prefix = null;
        private string[] headerNames = null;

        public string readPrefix(IAgent agent)
        {
            string prefix = "";
            IReadOnlyDictionary<string, string> appSettings = agent.Configuration.GetAppSettings();
            
            if (appSettings.TryGetValue("prefix", out prefix))
            {
                prefix = prefix ?? "";
            }

            return prefix;
        }

        public string[] readConfiguredHeaderNames(IAgent agent)
        {
            string reqHeaders = null;
            string[] headerNamesList = new string[0];
            IReadOnlyDictionary<string, string> appSettings = agent.Configuration.GetAppSettings();

            if (appSettings.TryGetValue("requestHeaders", out reqHeaders))
            {
                headerNamesList = reqHeaders?.Split(',').Select(p => p.Trim()).ToArray<string>();
                agent.Logger.Log(Level.Info, "Custom Asp35 Extension: These HTTP headers will be read and added to NewRelic transaction: " + "[" + String.Join(",", headerNamesList) + "]");
            }
            return headerNamesList;
        }

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
            prefix = prefix ?? readPrefix(agent);
            headerNames = headerNames ?? readConfiguredHeaderNames(agent);

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
                transaction.AddCustomAttribute(prefix + "Url", requestUrl.AbsoluteUri);
            }

            foreach (var headerName in headerNames)
            {
                string headerValue = httpContext.Request.Headers?.Get(headerName);
                if (headerValue != null)
                {
                    transaction.AddCustomAttribute(prefix + headerName, headerValue);

                }
            }

            return Delegates.NoOp;
        }
    }

}
