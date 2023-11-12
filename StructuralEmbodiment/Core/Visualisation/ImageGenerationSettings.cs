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
    internal class ImageGenerationSettings
    {
        public string ServerUrl { get; set; }
        public HttpClient Client { get; set; }
        public JObject Settings { get; set; }

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


        private ImageGenerationSettings(string url)
        {
            this.ServerUrl = url;
            Client = new HttpClient();
        }

        public void InitialiseSettings(string prompt, string nprompt, int seed, string sampler, int batchSize, int steps, int width, int height)
        {
            this.prompt = prompt;
            this.nprompt = nprompt;
            this.seed = seed;
            this.sampler = sampler;
            this.batchSize = batchSize;
            this.steps = steps;
            this.width = width;
            this.height = height;
            this.sampler = sampler;
            Settings = JObject.FromObject(new
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

        public async Task<ImageGenerationSettings> InitialiseAsyncAttribute()
        {
            try
            {
                JObject decodedCNModules = (JObject)await Util.GetInfo(ServerUrl, "/controlnet/module_list?alias_names=false", Client);
                controlNetModules = decodedCNModules["module_list"].ToObject<List<string>>();
                JObject decodedCNModels = (JObject)await Util.GetInfo(ServerUrl, "/controlnet/model_list?update=true", Client);
                controlNetModels = decodedCNModels["model_list"].ToObject<List<string>>();
                JArray decodedSDModels = (JArray)await Util.GetInfo(ServerUrl, "/sdapi/v1/sd-models", Client);
                sdModels = Util.JArryEntryToList(decodedSDModels, "model_name");
            }
            catch (Exception e)
            {
                throw new Exception("Failed to fatch data from the given server url");
            }
              
            return this;
        }

        public static async Task<ImageGenerationSettings> CreateImageSettingsObject(string url)
        {
            var imageSettingsObject = new ImageGenerationSettings(url);
            return await imageSettingsObject.InitialiseAsyncAttribute();
        }

        public override string ToString()
        {
            return "ServerURL: " + this.ServerUrl + "\n" + "\n" +
                "ControlNetModules: " + String.Join("\n", this.controlNetModules) + "\n" + "\n" +
                "ControlNetModels: " + String.Join("\n", this.controlNetModels) + "\n" + "\n" +
                "SDModels: " + String.Join("\n", this.sdModels) + "\n" + "\n" +
                "Prompt: " + this.prompt + "\n" + "\n" +
                "NegativePrompt: " + this.nprompt + "\n" + "\n" +
                "RandomSeed: " + this.seed + "\n" + "\n" +
                "Sampler: " + this.sampler + "\n" + "\n" +
                "BatchSize: " + this.batchSize + "\n" + "\n" +
                "Steps: " + this.steps + "\n" + "\n" +
                "Width: " + this.width + "\n" + "\n" +
                "Height: " + this.height + "\n";
        }



    }
}
