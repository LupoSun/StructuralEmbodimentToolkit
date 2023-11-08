using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

namespace StructuralEmbodiment.Core.Visualisation
{
    internal class ImageSettings
    {
        private string url { get; set; }
        private HttpClient client { get; set; }
        public JObject settings;

        public List<string> controlNetModules;
        public List<string> controlNetModels;
        public List<string> sdModels;

        public string prompt;
        public string nprompt;
        public int seed;
        public int batchSize;
        public int steps;
        public int width;
        public int height;
        public string sampler;


        private ImageSettings(string url)
        {
            this.url = url;
            client = new HttpClient();
        }

        public void InitialiseSettings(string prompt, string nprompt, int seed, string sampler, int batchSize, int steps, int width, int height)
        {
            settings = JObject.FromObject(new
            {
                prompt,
                negative_prompt = nprompt,
                seed,
                sampler_name = sampler,
                batch_size = batchSize,
                steps,
                width,
                height
            });
        }

        public async Task<ImageSettings> InitialiseAsyncAttribute()
        {
            JObject decodedCNModules = (JObject)await Util.GetInfo(url, "/controlnet/module_list?alias_names=false", client);
            controlNetModules = decodedCNModules["module_list"].ToObject<List<string>>();
            JObject decodedCNModels = (JObject)await Util.GetInfo(url, "/controlnet/model_list?update=true", client);
            controlNetModels = decodedCNModels["model_list"].ToObject<List<string>>();
            JArray decodedSDModels = (JArray)await Util.GetInfo(url, "/sdapi/v1/sd-models", client);
            sdModels = Util.JArryEntryToList(decodedSDModels, "model_name");
            return this;
        }

        public static async Task<ImageSettings> CreateImageSettingsObject(string url)
        {
            var imageSettingsObject = new ImageSettings(url);
            return await imageSettingsObject.InitialiseAsyncAttribute();
        }



    }
}
