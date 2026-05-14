namespace VictoriaIdentityProvider.Application.DTOs
{
    public record ClientMetadataDto
    {
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;

        public string? Location { get; set; }
    }
}
