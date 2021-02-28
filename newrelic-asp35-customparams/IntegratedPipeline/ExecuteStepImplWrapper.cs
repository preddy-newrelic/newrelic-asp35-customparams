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

        private static string prefix = null;
        private static string[] headerNames = null;
        private static string[] paramNames = null;
        private static string[] cookieNames = null;

        public bool IsTransactionRequired => true;

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

        public string[] readConfiguredParamNames(IAgent agent)
        {
            string reqParams = null;
            string[] paramNamesList = new string[0];
            IReadOnlyDictionary<string, string> appSettings = agent.Configuration.GetAppSettings();

            if (appSettings.TryGetValue("requestParams", out reqParams))
            {
                paramNamesList = reqParams?.Split(',').Select(p => p.Trim()).ToArray<string>();
                agent.Logger.Log(Level.Info, "Custom Asp35 Extension: These HTTP params will be read and added to NewRelic transaction: " + "[" + String.Join(",", paramNamesList) + "]");
            }
            return paramNamesList;
        }

        public string[] readConfiguredCookieNames(IAgent agent)
        {
            string reqCookies = null;
            string[] cookieNamesList = new string[0];
            IReadOnlyDictionary<string, string> appSettings = agent.Configuration.GetAppSettings();

            if (appSettings.TryGetValue("requestCookies", out reqCookies))
            {
                cookieNamesList = reqCookies?.Split(',').Select(p => p.Trim()).ToArray<string>();
                agent.Logger.Log(Level.Info, "Custom Asp35 Extension: These HTTP cookies will be read and added to NewRelic transaction: " + "[" + String.Join(",", cookieNamesList) + "]");
            }
            return cookieNamesList;
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
            paramNames = paramNames ?? readConfiguredParamNames(agent);
            cookieNames = cookieNames ?? readConfiguredCookieNames(agent);

            if (!HttpRuntime.UsingIntegratedPipeline)
                return Delegates.NoOp;

            var httpApplication = (HttpApplication)instrumentedMethodCall.MethodCall.InvocationTarget;
            if (httpApplication == null)
                throw new NullReferenceException("httpApplication");

            var httpContext = httpApplication.Context;
            if (httpContext == null)
                throw new NullReferenceException("httpContext");

            if (httpContext.CurrentNotification != RequestNotification.MapRequestHandler)
            {
                return Delegates.NoOp;
            }

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

            foreach (var paramName in paramNames)
            {
                string paramValue = httpContext.Request.Params?.Get(paramName);
                if (paramValue != null)
                {
                    transaction.AddCustomAttribute(prefix + paramName, paramValue);

                }
            }

            foreach (var cookieName in cookieNames)
            {
                HttpCookie cookieValue = httpContext.Request.Cookies?.Get(cookieName);
                if (cookieValue != null)
                {
                    transaction.AddCustomAttribute(prefix + cookieName, cookieValue.Value);

                }
            }

            return Delegates.NoOp;
        }
    }

}
