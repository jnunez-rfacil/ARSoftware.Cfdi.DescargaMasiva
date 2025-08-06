using System;
using System.Collections.Generic;
using System.Linq;
using ARSoftware.Cfdi.DescargaMasiva.Enumerations;

namespace ARSoftware.Cfdi.DescargaMasiva.Models
{
    /// <summary>
    ///     Peticion de solicitud.
    /// </summary>
    public sealed class SolicitaDescargaRecibidosRequest
    {
        private SolicitaDescargaRecibidosRequest(
                                 AccessToken accessToken,
                                 string complemento,
                                 EstadoComprobante estadoComprobante,
                                 DateTime fechaInicial,
                                 DateTime fechaFinal,
                                 string rfcEmisor,
                                 string rfcSolicitante,
                                 TipoComprobante tipoComprobante,
                                 TipoSolicitud tipoSolicitud,
                                 string rfcReceptor,
                                 string rfcACuentaTerceros
                                 )
        {
            AccessToken = accessToken;
            Complemento = complemento;
            EstadoComprobante = estadoComprobante;
            FechaInicial = fechaInicial;
            FechaFinal = fechaFinal;
            RfcEmisor = rfcEmisor;
            RfcSolicitante = rfcSolicitante;
            TipoComprobante = tipoComprobante;
            TipoSolicitud = tipoSolicitud;
            RfcReceptor = rfcReceptor;
            RfcACuentaTerceros = rfcACuentaTerceros;
        }

        /// <summary>
        ///     FechaInicial - Solo se buscarán CFDI, cuya fecha de emisión sea igual o mayor a la fecha inicial indicada en este
        ///     parámetro
        ///     Parámetro obligatorio. Este parámetro no debe declararse en caso de realizar una consulta por el folio fiscal
        ///     (UUID).
        /// </summary>
        public DateTime FechaInicial { get; }

        /// <summary>
        ///     Solo se buscarán CFDI, cuya fecha de emisión sea igual o menor a la fecha final indicada en este parámetro
        ///     Parámetro obligatorio. Este parámetro no debe declararse en caso de realizar una consulta por el folio fiscal
        ///     (UUID).
        /// </summary>
        public DateTime FechaFinal { get; }

        /// <summary>
        ///     TipoSolicitud - Define el tipo de descarga: [Metadata, CFDI]
        /// </summary>
        public TipoSolicitud TipoSolicitud { get; }

        /// <summary>
        ///     RfcEmisor - Contiene el RFC del emisor del cual se quiere consultar los CFDI.
        /// </summary>
        public string RfcEmisor { get; }

        /// <summary>
        ///     RfcReceptor - Contiene el RFC receptor del cual se quiere consultar los CFDIs
        /// </summary>
        public string RfcReceptor { get; }

        /// <summary>
        ///     RfcSolicitante - Contiene el RFC del que está realizando la solicitud de descarga.
        /// </summary>
        public string RfcSolicitante { get; }

        /// <summary>
        ///     Token de autorizacion.
        /// </summary>
        public AccessToken AccessToken { get; }

        /// <summary>
        ///     TipoComprobante - Define el tipo de comprobante: [Null, I = Ingreso, E = Egreso, T= Traslado, N = Nomina, P = Pago]
        ///     *Null es el valor predeterminado y en caso de no declararse, se obtendrán todos los comprobantes sin importar el
        ///     tipo comprobante.
        /// </summary>
        public TipoComprobante TipoComprobante { get; }

        /// <summary>
        ///     EstadoComprobante - Define el estado del comprobante: [Null, 0 = Cancelado, 1 = Vigente]
        ///     * Null es el valor predeterminado y en caso de no declararse, se obtendrán todos los comprobantes sin importar el
        ///     estado de los mismos.
        ///     Consideraciones:
        ///     • Para efectos de la información de la metadata, el listado incluirá los comprobantes vigentes y cancelados
        ///     • En el caso para la descarga de XML, solo incluirán los CFDI vigentes.Por lo que, el servicio no descargará XML
        ///     cancelados.
        /// </summary>
        public EstadoComprobante EstadoComprobante { get; }

        /// <summary>
        ///     RfcACuentaTerceros - Contiene el RFC del a cuenta a tercero del cual se quiere consultar los CFDIs
        /// </summary>
        public string RfcACuentaTerceros { get; }

        /// <summary>
        ///     Complemento - Define el complemento de CFDI a descargar:
        /// </summary>
        public string Complemento { get; }

        public bool TieneTipoComprobante => TipoComprobante != TipoComprobante.Null;
        public bool TieneEstadoComprobante => EstadoComprobante != EstadoComprobante.Null;
        public bool TieneComplemento => !string.IsNullOrWhiteSpace(Complemento);
        public bool TieneRfcEmisor => !string.IsNullOrWhiteSpace(RfcEmisor);
        public bool TieneRfcSolicitante => !string.IsNullOrWhiteSpace(RfcSolicitante);
        public bool TieneRfcACuentaTerceros => !string.IsNullOrWhiteSpace(RfcACuentaTerceros);

        public static SolicitaDescargaRecibidosRequest CreateInstance(
            AccessToken accessToken,
            string complemento,
            EstadoComprobante estadoComprobante,
            DateTime fechaInicial,
            DateTime fechaFinal,
            string rfcEmisor,
            string rfcSolicitante,
            TipoComprobante tipoComprobante,
            TipoSolicitud tipoSolicitud,
            string rfcReceptor,
            string rfcACuentaTerceros
            )
        {
            if (fechaInicial == DateTime.MinValue) throw new ArgumentException($"El valor de {nameof(fechaInicial)} es obligatorio.");
            
            if (fechaFinal == DateTime.MinValue) throw new ArgumentException($"El valor de {nameof(fechaFinal)} es obligatorio.");

            if (tipoSolicitud == null) throw new ArgumentException($"El valor de {nameof(tipoSolicitud)} es obligatorio.");

            if (string.IsNullOrWhiteSpace(rfcReceptor)) throw new ArgumentException($"El valor de {nameof(rfcReceptor)} es obligatorio.");

            return new SolicitaDescargaRecibidosRequest(
                accessToken,
                complemento,
                estadoComprobante,
                fechaInicial,
                fechaFinal,
                rfcEmisor,
                rfcSolicitante,
                tipoComprobante,
                tipoSolicitud,
                rfcReceptor,
                rfcACuentaTerceros);
        }
    }
}
