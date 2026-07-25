using OpenTelemetry.Exporter;

namespace Events.Presentation.Settings
{
    /// <summary>
    /// Настройки для OLTP
    /// </summary>
    public class OtlpSettings
    {
        /// <summary>
        /// Адрес Jaeger
        /// </summary>
        public string Endpoint { get; set; } = string.Empty;

        /// <summary>
        /// Протокол предачиданных
        /// </summary>
        public string Protocol {  get; set; } = "HttpProtobuf";

        /// <summary>
        /// Интервал отправки
        /// </summary>
        public int ScheduledDelayMilliseconds { get; set; } = 2000;

        /// <summary>
        /// тайм-аут отправки
        /// </summary>
        public int ExporterTimeoutMilliseconds { get; set; } = 3000;
    }
}
