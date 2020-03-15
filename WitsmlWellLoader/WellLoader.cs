using System;
using System.Xml;
using System.Net;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace WitsmlWellLoader
{
    class WellLoader
    {
        static void Main(string[] args)
        {
            // Bunch of stuff to get the secrets from secrets.json, ignore

            var services = new ServiceCollection();
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddUserSecrets<ServerCreds>();
            IConfiguration configuration = builder.Build();

            var serverCreds = configuration.GetSection("ServerCreds");
            services.Configure<ServerCreds>(serverCreds);


            // get request header
            var _action = "http://www.witsml.org/action/120/Store.WMLS_GetFromStore";


            XmlDocument soapEnvelopeXML = CreateSoapEnvelope();
            HttpWebRequest webRequest = CreateWebRequest(serverCreds.GetSection("url").Value, _action, serverCreds.GetSection("username").Value, serverCreds.GetSection("password").Value);

            InsertSoapEnvelopeIntoWebRequest(soapEnvelopeXML, webRequest);

            // begin async call to web request.
            IAsyncResult asyncResult = webRequest.BeginGetResponse(null, null);

            // suspend this thread until call is complete. 
            asyncResult.AsyncWaitHandle.WaitOne();

            // get the response from the completed web request.
            string soapResult;
            using (WebResponse webResponse = webRequest.EndGetResponse(asyncResult))
            {
                using (StreamReader rd = new StreamReader(webResponse.GetResponseStream()))
                {
                    soapResult = rd.ReadToEnd();
                }
                Console.Write(soapResult);
            }

            XmlDocument resultDoc = new XmlDocument();
            resultDoc.LoadXml(soapResult);

        }

        private static XmlDocument CreateSoapEnvelope()
        {
            XmlDocument soapEnvelopeXml = new XmlDocument();
            soapEnvelopeXml.LoadXml(@"<?xml version=""1.0"" encoding=""utf-8""?>
                    <soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:soapenc=""http://schemas.xmlsoap.org/soap/encoding/"" xmlns:tns=""http://www.witsml.org/wsdl/120"" xmlns:types=""http://www.witsml.org/wsdl/120/encodedTypes"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
                    <soap:Body soap:encodingStyle=""http://schemas.xmlsoap.org/soap/encoding/"">
                    <q1:WMLS_GetFromStore xmlns:q1=""http://www.witsml.org/message/120"">
                    <WMLtypeIn xsi:type=""xsd:string"">well</WMLtypeIn>
                    <QueryIn xsi:type=""xsd:string"">&lt;wells xmlns=""http://www.witsml.org/schemas/1series"" version=""1.4.1.1""&gt;
                        &lt;well uid=""""&gt;
                        &lt;name /&gt;
                        &lt;/well&gt;
                        &lt;/wells&gt;</QueryIn>
                        <OptionsIn xsi:type=""xsd:string"">returnElements=requested;maxReturnNodes=100;requestLatestValues=10</OptionsIn>
                        </q1:WMLS_GetFromStore>
                        </soap:Body>
                      </soap:Envelope>");
            return soapEnvelopeXml;
        }

        private static HttpWebRequest CreateWebRequest(string url, string action, string username, string password)
        {

            string authInfo = username + ":" + password;
            authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Headers.Add("Authorization", "Basic " + authInfo);
            webRequest.Headers.Add("SOAPAction", action);
            webRequest.ContentType = "text/xml";
            webRequest.Method = "POST";
            return webRequest;
        }


        private static void InsertSoapEnvelopeIntoWebRequest(XmlDocument soapEnvelopeXml, HttpWebRequest webRequest)
        {
            using (Stream stream = webRequest.GetRequestStream())
            {
                soapEnvelopeXml.Save(stream);
            }
        }

    }

}
