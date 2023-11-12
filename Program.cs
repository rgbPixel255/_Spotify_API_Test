using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using System.Data.SQLite;

namespace _Spotify_API_Test
{
    internal class Program
    {
        static async Task Main()
        {
            string clientId = string.Empty;
            string clientSecret = string.Empty;
            string connectionString = @"URI=file:spotify-api-test.sqlite";
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = "SELECT client_id FROM Credentials";
                    clientId = command.ExecuteScalar().ToString();

                    command.CommandText = "SELECT client_secret FROM Credentials";
                    clientSecret = command.ExecuteScalar().ToString();
                }
            }

            string responseType = "code";
            string redirectUri = "http://localhost:3000/callback";
            string scopes = "user-read-private user-read-email user-follow-read";
            string authorizationUrl = $"https://accounts.spotify.com/authorize?client_id={clientId}&response_type={responseType}&redirect_uri={redirectUri}&scope={scopes}";

            System.Diagnostics.Process.Start("xdg-open", $"\"{authorizationUrl}\"");

            string code = string.Empty;;
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (var command = new SQLiteCommand(connection))
                {
                    while(code == string.Empty)
                    {
                        command.CommandText = "SELECT code FROM AuthorizationCodes";
                        var result = command.ExecuteScalar();
                        if (result != null) code = result.ToString();
                        else System.Threading.Thread.Sleep(100); //Avoid infinite loop
                    }
                }

            }

            // # HTTP REQUESTS
            var client = new HttpClient();
            var connectionValues = new Dictionary<string, string>
            { 
                { "grant_type", "authorization_code" },
                { "code", code },
                { "redirect_uri", redirectUri },
                { "client_id", clientId },
                { "client_secret", clientSecret }
            };

            // # POST REQUEST TO GET ACCESS_TOKEN
            var content = new FormUrlEncodedContent(connectionValues);
            var postResponse = await client.PostAsync("https://accounts.spotify.com/api/token", content);
            var postResponseString = await postResponse.Content.ReadAsStringAsync();
            Console.WriteLine($"###POST Status Code{postResponse.StatusCode}");

            // # DESERIALIZE THE JSON FILE
            JObject json = JObject.Parse(postResponseString);
            string accessToken = (string)json["access_token"];
            Console.WriteLine($"###ACCESS TOKEN: {accessToken}");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            // # GET FOLLOWED ARTISTS
            var getResponse = await client.GetAsync("https://api.spotify.com/v1/me/following?type=artist");
            var getResponseString = await getResponse.Content.ReadAsStringAsync();
            Console.WriteLine($"----- FOLLOWED ARTISTS -----\n{getResponseString}");
        }
    }
}