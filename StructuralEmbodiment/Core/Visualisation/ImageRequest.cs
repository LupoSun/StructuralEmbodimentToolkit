using Eto.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace StructuralEmbodimentToolkit.Core.Visualisation
{
    internal class ImageRequest
    {
        public string ServerUrl { get; set; }
        public JObject Payload { get; set; }
        public List<Bitmap> Images { get; set; }
        public HttpClient Client { get; set; }

        public ImageRequest(string url, JObject payload, HttpClient client)
        {
            this.ServerUrl = url;
            this.Payload = payload;
            this.Images = new List<Bitmap>();
            this.Client = client;
        }
        private async Task<HttpResponseMessage> SendRequestAsync(Enum genMode)
        {
            string apiTail = "";
            switch (genMode)
            {
                case GenerationMode.text2img:
                    apiTail = "/sdapi/v1/txt2img";
                    break;
                case GenerationMode.img2img:
                    break;
            }
            StringContent content = new StringContent(Payload.ToString(), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await Client.PostAsync(ServerUrl + apiTail, content);
            return response;
        }

        public async Task GenerateImage(Enum genMode)
        {
            HttpResponseMessage response = await SendRequestAsync(genMode);
            if (response.IsSuccessStatusCode)
            {
                //a = "Success";
                var jsonResponse = await response.Content.ReadAsStringAsync();
                dynamic r = JsonConvert.DeserializeObject(jsonResponse);
                //List<Image> images = new List<Image>();
                foreach (var i in r["images"])
                {
                    byte[] bytes = Convert.FromBase64String(i.ToString().Split(',')[0]);
                    using (MemoryStream ms = new MemoryStream(bytes))
                    {
                        Image image = Image.FromStream(ms);
                        Images.Add(new Bitmap(image));
                    }

                }
            }
            else throw new Exception("Request failed with status code: " + response.StatusCode);

        }

    }

}
