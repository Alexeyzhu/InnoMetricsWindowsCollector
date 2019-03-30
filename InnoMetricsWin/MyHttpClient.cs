using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class MyHttpClient
    {

        static CookieContainer cookies = new CookieContainer();
        static HttpClientHandler handler = new HttpClientHandler();
        static HttpClient client = new HttpClient(handler);

        public static async Task<string> CreateUserAsync(User user)
        {
            handler.CookieContainer = cookies;
            HttpResponseMessage response = client.PostAsJsonAsync(
                "user", user).Result;
            //response.EnsureSuccessStatusCode();
            response = client.PostAsJsonAsync(
                "login", user).Result;
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException();
            }
            Uri uri = new Uri("https://innometric.guru");
            IEnumerable<Cookie> responseCookies = cookies.GetCookies(uri).Cast<Cookie>();
            foreach (Cookie cookie in responseCookies)
            {
                if (cookie.Name == "session")
                {
                    return cookie.Value;
                }
            }

            return "";
        }

        public static async void SendActivityAsync(Activity activity, string cookieValue)
        {
            handler.CookieContainer.Add(client.BaseAddress, new Cookie("session", cookieValue));
            ActivityRequest request = new ActivityRequest
            {
                Activity = activity
            };
            HttpResponseMessage response = client.PostAsJsonAsync(
                "activity", request).Result;
            //Console.WriteLine(response.StatusCode);
        }

        public static void SetUp()
        {
            // Update port # in the following line.
            client.BaseAddress = new Uri("https://innometric.guru:8120/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

    }
}
