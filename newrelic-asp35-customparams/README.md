[![New Relic Experimental header](https://github.com/newrelic/opensource-website/raw/master/src/images/categories/Experimental.png)](https://opensource.newrelic.com/oss-category/#new-relic-experimental)

# Asp .Net - Adding Custom Transaction Parameters

A custom extension for the New Relic .Net Framework agent to add custom transaction parameters from configured ASP .NET Http Request query parameters, headers and cookies.

## Installation

1. Drop the extension dll in the newrelic agent's Program Files "extensions" folder.

```cmd
   copy Custom.Providers.Wrapper.Asp35.dll C:\Program Files\New Relic\.NET Agent\netframework\Extensions
```

2. Drop the extension xml in the newrelic agent ProgramData "extensions" folder.

```cmd
   copy Custom.Providers.Wrapper.Asp35.xml C:\ProgramData\New Relic\.NET Agent\netframework\Extensions
```

***
**Note: The XML file must be dropped into ProgramData's extension folder whereas DLL file must be dropped into Program Files's extension folder**
***

3. Edit the newrelic agent's configuration file (`newrelic.config`) and add the following properties as applicable to the `appSettings` element:

```xml
  # To collect full request Url
  <add key="requestProperties" value="Url"/>
  # To collect custom request headers:
  <add key="requestHeaders" value="Origin" />
  # To collect custom request parameters:
  <add key="requestParams" value="city, country" />
  #To collect custom cookies
  <add key="requestCookies" value="SSOUSER, SSOSESSIONID" />
  # To set a prefix for the collected attributes
  # Leave blank or set to "blank" to have no prefix.
  # Default: ''
  <add key="prefix" value="request." />
```

An Example snippet of newrelic.config with the above configuration looks like this

```xml
<?xml version="1.0"?>
<!-- Copyright (c) 2008-2017 New Relic, Inc.  All rights reserved. -->
<!-- For more information see: https://newrelic.com/docs/dotnet/dotnet-agent-configuration -->
<configuration xmlns="urn:newrelic-config" agentEnabled="true">
  <service licenseKey="???" />
  <application>
    <name>My Application</name>
  </application>
  <appSettings>
	<add key="prefix" value="request." />
    <add key="requestProperties" value="Url"/>
    <add key="requestHeaders" value="Origin"/>
    <add key="requestParams" value="city, country"/>
	<add key="requestCookies" value="SSOUSER, SSOSESSIONID"/>
  </appSettings>
  <log level="info" />
...
```
4. Restart your application after adding the extension files and configurations.
3. Check your [results](#results)!

## Results

The instrumentation will add the extracted request headers, parameters and cookies as custom transaction parameters, which are found in these places:

- APM Transaction Traces (both distributed traces and classic) in the "Attributes" section
- Transaction events in Insights

## Support

New Relic has open-sourced this project. This project is provided AS-IS WITHOUT WARRANTY OR DEDICATED SUPPORT. Issues and contributions should be reported to the project here on GitHub.
We encourage you to bring your experiences and questions to the [Explorers Hub](https://discuss.newrelic.com) where our community members collaborate on solutions and new ideas.


## Contributing

We encourage your contributions to improve [Project Name]! Keep in mind when you submit your pull request, you'll need to sign the CLA via the click-through using CLA-Assistant. You only have to sign the CLA one time per project. If you have any questions, or to execute our corporate CLA, required if your contribution is on behalf of a company, please drop us an email at opensource@newrelic.com.

**A note about vulnerabilities**

As noted in our [security policy](../../security/policy), New Relic is committed to the privacy and security of our customers and their data. We believe that providing coordinated disclosure by security researchers and engaging with the security community are important means to achieve our security goals.

If you believe you have found a security vulnerability in this project or any of New Relic's products or websites, we welcome and greatly appreciate you reporting it to New Relic through [HackerOne](https://hackerone.com/newrelic).

## License

[newrelic-dotnet-customparams] is licensed under the [Apache 2.0](http://apache.org/licenses/LICENSE-2.0.txt) License.

