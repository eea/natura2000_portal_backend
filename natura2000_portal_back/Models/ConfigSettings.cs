namespace natura2000_portal_back.Models
{
    public class fme_service_config
    {
        public string server_url { get; set; } = string.Empty;
        public string repository { get; set; } = string.Empty;
        public string workspace { get; set; } = string.Empty;
    }

    public class ConfigSettings
    {
        public string Environment { get; set; } = string.Empty;
        public string SACComputationDestDatasetFolder { get; set; } = string.Empty;
        public string fme_security_token { get; set; } = string.Empty;
        public fme_service_config fme_service_sac_computation { get; set; }
        public string fme_service_results_download { get; set; } = string.Empty;
    }
}