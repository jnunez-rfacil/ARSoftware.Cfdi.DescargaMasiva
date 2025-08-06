using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using ARSoftware.Cfdi.DescargaMasiva.Constants;
using ARSoftware.Cfdi.DescargaMasiva.Exceptions;
using ARSoftware.Cfdi.DescargaMasiva.Helpers;
using ARSoftware.Cfdi.DescargaMasiva.Interfaces;
using ARSoftware.Cfdi.DescargaMasiva.Models;

namespace ARSoftware.Cfdi.DescargaMasiva.Services
{
    public sealed class SolicitaDescargaRecibidosService : ISolicitaDescargaRecibidosService
    {
        private readonly IHttpSoapClient _httpSoapClient;

        public SolicitaDescargaRecibidosService(IHttpSoapClient httpSoapClient)
        {
            _httpSoapClient = httpSoapClient;
        }

        public string GenerateSoapRequestEnvelopeXmlContent(SolicitaDescargaRecibidosRequest solicitud, X509Certificate2 certificate)
        {
            var xmlDocument = new XmlDocument();

            XmlElement envelopElement = xmlDocument.CreateElement(CfdiDescargaMasivaNamespaces.S11Prefix,
                "Envelope",
                CfdiDescargaMasivaNamespaces.S11NamespaceUrl);
            envelopElement.SetAttribute($"xmlns:{CfdiDescargaMasivaNamespaces.S11Prefix}", CfdiDescargaMasivaNamespaces.S11NamespaceUrl);
            envelopElement.SetAttribute($"xmlns:{CfdiDescargaMasivaNamespaces.DesPrefix}", CfdiDescargaMasivaNamespaces.DesNamespaceUrl);
            envelopElement.SetAttribute($"xmlns:{CfdiDescargaMasivaNamespaces.DsPrefix}", CfdiDescargaMasivaNamespaces.DsNamespaceUrl);
            xmlDocument.AppendChild(envelopElement);

            XmlElement headerElement = xmlDocument.CreateElement(CfdiDescargaMasivaNamespaces.S11Prefix,
                "Header",
                CfdiDescargaMasivaNamespaces.S11NamespaceUrl);
            envelopElement.AppendChild(headerElement);

            XmlElement bodyElement = xmlDocument.CreateElement(CfdiDescargaMasivaNamespaces.S11Prefix,
                "Body",
                CfdiDescargaMasivaNamespaces.S11NamespaceUrl);
            envelopElement.AppendChild(bodyElement);

            XmlElement solicitaDescargaElement = xmlDocument.CreateElement(CfdiDescargaMasivaNamespaces.DesPrefix,
                "SolicitaDescargaRecibidos",
                CfdiDescargaMasivaNamespaces.DesNamespaceUrl);
            bodyElement.AppendChild(solicitaDescargaElement);

            XmlElement solicitudElement = xmlDocument.CreateElement(CfdiDescargaMasivaNamespaces.DesPrefix,
                "solicitud",
                CfdiDescargaMasivaNamespaces.DesNamespaceUrl);

            // Los atributos deberan de ordenarse de la siguiente forma
            // 1. Complemento
            // 2. EstadoComprobante
            // 3. FechaInicial
            // 4. FechaFinal
            // 5. RfcEmisor
            // 6. RfcSolicitante
            // 7. TipoComprobante
            // 8. TipoSolicitud
            // 9. RfcReceptor
            // 10. RfcACuentaTerceros

            // Parámetro opcional
            if (solicitud.TieneComplemento)
                solicitudElement.SetAttribute("Complemento", solicitud.Complemento);

            // Parámetro opcional
            if (solicitud.TieneEstadoComprobante)
                solicitudElement.SetAttribute("EstadoComprobante", solicitud.EstadoComprobante.Name);

            // Parámetro requerido
            solicitudElement.SetAttribute("FechaInicial", solicitud.FechaInicial.ToSoapStartDateString());

            // Parámetro requerido
            solicitudElement.SetAttribute("FechaFinal", solicitud.FechaFinal.ToSoapEndDateString());

            // Parámetro opcional
            if(solicitud.TieneRfcEmisor)
                solicitudElement.SetAttribute("RfcEmisor", solicitud.RfcEmisor);

            // Parámetro opcional
            if (solicitud.TieneRfcSolicitante)
            {
                if (solicitud.RfcSolicitante != solicitud.RfcReceptor) throw new ArgumentException($"Si se define el parámetro {nameof          (solicitud.RfcSolicitante)} este debe de ser igual al parámetro {nameof(solicitud.RfcReceptor)}");

                solicitudElement.SetAttribute("RfcSolicitante", solicitud.RfcSolicitante);
            }

            // Parámetro opcional
            if (solicitud.TieneTipoComprobante)
            {
                solicitudElement.SetAttribute("TipoComprobante", solicitud.TipoComprobante.Name);
            }

            // Parámetro obligatorio
            solicitudElement.SetAttribute("TipoSolicitud", solicitud.TipoSolicitud.Name);

            // Parámetro obligatorio
            solicitudElement.SetAttribute("RfcReceptor", solicitud.RfcReceptor);

            // Parámetro opcional
            if (solicitud.TieneRfcACuentaTerceros)
                solicitudElement.SetAttribute("RfcACuentaTerceros", solicitud.RfcACuentaTerceros);

            XmlElement signatureElement = SignedXmlHelper.SignRequest(solicitudElement, certificate);
            solicitudElement.AppendChild(signatureElement);
            solicitaDescargaElement.AppendChild(solicitudElement);

            return xmlDocument.OuterXml;
        }

        public async Task<SoapRequestResult> SendSoapRequestAsync(string soapRequestContent,
                                                                  AccessToken accessToken,
                                                                  CancellationToken cancellationToken = default)
        {
            return await _httpSoapClient.SendRequestAsync(CfdiDescargaMasivaWebServiceUrls.SolicitaDescargaUrl,
                CfdiDescargaMasivaWebServiceUrls.SolicitaDescargaRecibidosSoapActionUrl,
                accessToken,
                soapRequestContent,
                cancellationToken);
        }

        public async Task<SolicitaDescargaRecibidosResult> SendSoapRequestAsync(SolicitaDescargaRecibidosRequest solicitaDescargaRecibidosRequest,
                                                                X509Certificate2 certificate,
                                                                CancellationToken cancellationToken = default)
        {
            string soapRequestContent = GenerateSoapRequestEnvelopeXmlContent(solicitaDescargaRecibidosRequest, certificate);

            SoapRequestResult soapRequestResult = await _httpSoapClient.SendRequestAsync(CfdiDescargaMasivaWebServiceUrls.SolicitaDescargaUrl,
                CfdiDescargaMasivaWebServiceUrls.SolicitaDescargaRecibidosSoapActionUrl,
                solicitaDescargaRecibidosRequest.AccessToken,
                soapRequestContent,
                cancellationToken);

            return GetSoapResponseResult(soapRequestResult);
        }

        public SolicitaDescargaRecibidosResult GetSoapResponseResult(SoapRequestResult soapRequestResult)
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(soapRequestResult.ResponseContent);

            XmlNode element = xmlDocument.GetElementsByTagName("SolicitaDescargaRecibidosResult")[0];
            if (element is null)
                throw new InvalidResponseContentException("Element SolicitaDescargaRecibidosResult is missing in response.",
                    soapRequestResult.ResponseContent);

            if (element.Attributes is null)
                throw new InvalidResponseContentException("Attributes property of Element SolicitaDescargaRecibidosResult is null.",
                    soapRequestResult.ResponseContent);

            string requestId = element.Attributes.GetNamedItem("IdSolicitud")?.Value ?? string.Empty;
            string requestStatusCode = element.Attributes.GetNamedItem("CodEstatus")?.Value ?? string.Empty;
            string requestStatusMessage = element.Attributes.GetNamedItem("Mensaje")?.Value ?? string.Empty;

            return SolicitaDescargaRecibidosResult.CreateInstance(requestId,
                requestStatusCode,
                requestStatusMessage,
                soapRequestResult.HttpStatusCode,
                soapRequestResult.ResponseContent);
        }
    }
}
