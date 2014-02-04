using System;

namespace vizzopWeb.Models
{
    [Serializable]
    public class AnalyticsDatatables
    {

        public int ID { get; set; }
        public Converser converser { get; set; }
        public string Url { get; set; }
        public int PageViews { get; set; }
        public int UniqueIPs { get; set; }
        public string Referrer { get; set; }
        public string Lang { get; set; }
        public string UserAgent { get; set; }
        public string IP { get; set; }
        public string DurationRange { get; set; }
        public DateTime TimeStamp_First { get; set; }
        public DateTime TimeStamp_Last { get; set; }
        public int MilliSecondsSpent { get; set; }
        public bool ReturningVisitor { get; set; }
        public string Ubication { get; set; }
        public string Headers { get; set; }

        /*
         * Definiciones de Analytics -> Primarias
Paginas Vistas
Pag Vistas Unicas
         * Promedio de Tiempo
         * Accesos
         * Porcentaje de Rebote
         * Porcentaje de salidas
         * Valor de página
         * * Definiciones de Analytics -> Secundarias
fuente
medio
palabra clave
campaña
pagina de destino
--
idioma
continente, pais, ciudad etc
--
navegador, version navegador
SO
Colores
Resolucion
Flash Compatible
--
nombre del host
valor definido por usuario
         */
    }
}