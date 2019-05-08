using Newtonsoft.Json;

namespace ConsoleApp1
{
    public class Activity
    {
        [JsonProperty("enc_key_h")]
        public string EncKey { get; set; }

        [JsonProperty("iv")]
        public string IV { get; set; }

        [JsonProperty("start_time")]
        public string StartTime { get; set; }

        [JsonProperty("executable_name")]
        public string ExecutableName { get; set; }

        [JsonProperty("end_time")]
        public string EndTime { get; set; }

        [JsonProperty("browser_url")]
        public string BrowserUrl { get; set; }

        [JsonProperty("browser_title")]
        public string BrowserTitle { get; set; }

        [JsonProperty("ip_address")]
        public string IpAddress { get; set; }

        [JsonProperty("mac_address")]
        public string MacAddress { get; set; }
    }
}
