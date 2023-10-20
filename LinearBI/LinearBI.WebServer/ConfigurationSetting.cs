namespace LinearBI.WebServer;

internal record ConfigurationSetting
{
	public string ReportsPath { get; set; }
	public string Port { get; set; }
	public string ServerAddress { get; set; }
	public string CompanyTitle { get; set; }
}