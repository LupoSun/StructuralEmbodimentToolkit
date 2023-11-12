using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using Rhino;
using Rhino.Display;
using System.Windows.Forms;

namespace StructuralEmbodiment.Core.Visualisation
{
    internal static class Util
    {
        /*
         * Obsolete
         */
        public static async Task<JObject> GetInfoJOBJ(string serverUrl, string goal, HttpClient client)
        {
            string url = serverUrl + goal;
            HttpResponseMessage response;
            try { response = await client.GetAsync(url); }
            catch (HttpRequestException e)
            {
                throw new Exception("HTTP request failed for getting information from " + goal, e);
            }

            if (response.IsSuccessStatusCode)
            {
                string jsonResponse = await response.Content.ReadAsStringAsync();
                dynamic decodedResponse = JsonConvert.SerializeObject(jsonResponse);
                return decodedResponse;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Get ControlNet modules failed with status code {response.StatusCode}: {response.ReasonPhrase}\n{errorContent}");
            }
        }

        public static async Task<JToken> GetInfo(string serverUrl, string goal, HttpClient client)
        {
            string url = serverUrl + goal;
            HttpResponseMessage response;
            try { response = await client.GetAsync(url); }
            catch (HttpRequestException e)
            {
                throw new Exception("HTTP request failed for getting information from " + goal, e);
            }

            if (response.IsSuccessStatusCode)
            {
                string jsonResponse = await response.Content.ReadAsStringAsync();
                JToken data = JToken.Parse(jsonResponse);
                //dynamic decodedResponse = JsonConvert.SerializeObject(jsonResponse);
                return data;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Get ControlNet modules failed with status code {response.StatusCode}: {response.ReasonPhrase}\n{errorContent}");
            }
        }

        public static List<string> JArryEntryToList(JArray jsonArray, string entryName)
        {
            // Use LINQ to retrieve all the model names from the JArray
            List<string> listOfEntry = jsonArray
                .Select(entry => (string)entry[entryName])
                .ToList();

            return listOfEntry;
        }

        public static string ReadImage(Image imageToRead)
        {
            using (Image image = imageToRead)
            using (MemoryStream ms = new MemoryStream())
            {
                // Save to memory stream in PNG format
                image.Save(ms, ImageFormat.Png);
                // Convert the memory stream to a byte array
                byte[] imageBytes = ms.ToArray();
                // Convert the byte array to a Base64 string
                return Convert.ToBase64String(imageBytes);
            }
        }

        public static JObject BuildPayload(params object[] settings)
        {
            var payload = JObject.FromObject(settings);
            return payload;
        }

        public static JObject AddControlNet(JObject payload, object controlNetSettings)
        {
            bool keyExists = ((JObject)payload["alwayson_scripts"]).ContainsKey("controlnet");
            if (keyExists)
            {
                ((JArray)payload["alwayson_scripts"]["controlnet"]["args"]).Add(JToken.FromObject(controlNetSettings));
            }
            else
            {
                payload["alwayson_scripts"] = JToken.FromObject(new
                {
                    controlnet = new
                    {
                        args = new JArray { JToken.FromObject(controlNetSettings) }
                    }
                });
            }
            return payload;
        }

        public static Bitmap CaptureView(int Height=-1, int Width = -1)
        {
            //Settng up viewport
            RhinoDoc activeDoc = RhinoDoc.ActiveDoc;
            RhinoView activeView = activeDoc.Views.ActiveView;
            RhinoViewport activeViewport = activeView.ActiveViewport;

            //Setting up view capture
            var viewCapture = new ViewCapture();
            if (Width <= 0)
            {
                viewCapture.Width = activeViewport.Size.Width;
            }
            else { viewCapture.Width = Width; }

            if (Height <= 0)
            {
                viewCapture.Height = activeViewport.Size.Height;
            }
            else { viewCapture.Height = Height; }

            viewCapture.ScaleScreenItems = false;
            viewCapture.DrawAxes = false;
            viewCapture.DrawGrid = false ;
            viewCapture.DrawGridAxes = false ;
            viewCapture.TransparentBackground = true;

            var bitmap = viewCapture.CaptureToBitmap(activeView);
            return bitmap;




        }
    }
}
