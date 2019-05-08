using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Text;

namespace ConsoleApp1
{
    public class MyHttpClient
    {

        static CookieContainer cookies = new CookieContainer();
        static HttpClientHandler handler = new HttpClientHandler();
        static HttpClient client = new HttpClient(handler);

        public static async Task<string[]> CreateUserAsync(User user)
        {
            handler.CookieContainer = cookies;
            HttpResponseMessage response = client.PostAsJsonAsync(
                "user", user).Result;
            response = client.PostAsJsonAsync(
                "login", user).Result;
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException();
            }
            Uri uri = new Uri("http://10.90.137.225");
            IEnumerable<Cookie> responseCookies = cookies.GetCookies(uri).Cast<Cookie>();
            string[] stringArray = new string[2];
            foreach (Cookie cookie in responseCookies)
            {
                if (cookie.Name == "session")
                {
                    stringArray[0] = cookie.Value;
                }

                if (cookie.Name == "public_key")
                {
                    byte[] b = Convert.FromBase64String(cookie.Value);

                    string strOriginal = System.Text.Encoding.UTF8.GetString(b);
                    stringArray[1] = strOriginal;
                }
            }

            return stringArray;
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
            client.BaseAddress = new Uri("http://10.90.137.225:8120");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

    }
}
