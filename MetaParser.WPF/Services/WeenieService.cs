using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MetaParser.WPF.Services
{
    public class WeenieService
    {
        private static readonly string LIFESTONED_URL = @"https://lifestoned.net/Weenie/";

        public class Weenie
        {
            public int WeenieClassId { get; set; }
            public string Name { get; set; }
            public object Description { get; set; }
            public int WeenieType { get; set; }
            public int ItemType { get; set; }
            public object LastModified { get; set; }
            public object ModifiedBy { get; set; }
            public bool IsDone { get; set; }
            public bool HasSandboxChange { get; set; }
        }

        public class WeenieDetails
        {
            public int WeenieClassId { get; set; }
            public int WeenieTypeId { get; set; }
            public List<KeyValuePair<int, int>> BoolStats { get; set; }
            public List<KeyValuePair<int, int>> IntStats { get; set; }
            public List<KeyValuePair<int, uint>> DidStats { get; set; }
            public List<KeyValuePair<int, int>> IidStats { get; set; }
            public List<KeyValuePair<int, float>> FloatStats { get; set; }
            public List<KeyValuePair<int, long>> Int64Stats { get; set; }
            public List<KeyValuePair<int, string>> StringStats { get; set; }

            public List<KeyValuePair<int, WeeniePosition>> Positions { get; set; }

            public string Name { get; set; }
        }

        public class WeeniePosition
        {
            public WeenieFrame Frame { get; set; }
            public uint LandCellId { get; set; }
        }

        public class WeenieFrame
        {
            public object Position { get; set; }
            public object Rotations { get; set; }
        }

        public async IAsyncEnumerable<Weenie> GetWeeniesAsync(string partialNameSearch)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(LIFESTONED_URL);

            var request = new HttpRequestMessage(HttpMethod.Post, "WeenieFinder");
            var search = new { PartialName = partialNameSearch };
            request.Content = new StringContent(JsonSerializer.Serialize(search), Encoding.UTF8, "application/json");

            var response = await client.SendAsync(request).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                await foreach (var weenie in JsonSerializer.DeserializeAsyncEnumerable<Weenie>(stream).ConfigureAwait(false))
                {
                    yield return weenie;
                }
            }
            else
                throw new Exception("Unable to get weenies");
        }

        public async IAsyncEnumerable<Weenie> GetWeeniesAsync(int weenieType, string partialNameSearch = null)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(LIFESTONED_URL);

            var request = new HttpRequestMessage(HttpMethod.Post, "WeenieFinder");
            var search = new { WeenieType = weenieType, PartialName = partialNameSearch };
            request.Content = new StringContent(JsonSerializer.Serialize(search), Encoding.UTF8, "application/json");

            var response = await client.SendAsync(request).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                await foreach (var weenie in JsonSerializer.DeserializeAsyncEnumerable<Weenie>(stream).ConfigureAwait(false))
                {
                    yield return weenie;
                }
            }
            else
                throw new Exception("Unable to get weenies");
        }

        public async Task<WeenieDetails> GetWeenieDetailsAsync(int weenieId)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(LIFESTONED_URL);

            var request = new HttpRequestMessage(HttpMethod.Get, $"Get?id={weenieId}");

            var response = await client.SendAsync(request).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var details = JsonSerializer.Deserialize<WeenieDetails>(json);
                return details;
            }
            else
                throw new Exception("Unable to get weenie details");
        }
    }
}
