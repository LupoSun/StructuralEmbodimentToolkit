using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using Grasshopper.Kernel;



namespace StructuralEmbodiment.Core.Visualisation
{
    public class SDWebUISetting
    {
        public string ServerURL;
        public HttpClient Client;
        public bool isInitialised = false;
        
        public List<string> SDModels { get; set; }
        public List<string> CNModules { get; set; }
        public List<string> CNModels { get; set; }
        public List<string> LoRAs { get; set; }
        public List<string> VAEs { get; set; }
        public List<string> Samplers { get; set; }

        private static SDWebUISetting instance;
        public static SDWebUISetting Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SDWebUISetting();
                }
                return instance;
            }
        }
        public void SetSDWebUISetting(string serverURL)
        {
            ServerURL = serverURL;
        }
        public async void Initialise()
        {
            if (ServerURL != null)
            {
                this.Client = new HttpClient();

                try
                {
                    JObject decodedCNModules = (JObject)await Util.GetInfo(ServerURL, "/controlnet/module_list?alias_names=false", Client);
                    this.CNModules = decodedCNModules["module_list"].ToObject<List<string>>();
                    JObject decodedCNModels = (JObject)await Util.GetInfo(ServerURL, "/controlnet/model_list?update=true", Client);
                    this.CNModels = decodedCNModels["model_list"].ToObject<List<string>>();
                    JArray decodedSDModels = (JArray)await Util.GetInfo(ServerURL, "/sdapi/v1/sd-models", Client);
                    this.SDModels = Util.JArryEntryToList(decodedSDModels, "model_name");
                    JArray decodedLoRAs = (JArray)await Util.GetInfo(ServerURL, "/sdapi/v1/loras", Client);
                    this.LoRAs = Util.JArryEntryToList(decodedLoRAs, "name");
                    JArray decodedSamplers = (JArray)await Util.GetInfo(ServerURL, "/sdapi/v1/samplers", Client);
                    this.Samplers = Util.JArryEntryToList(decodedSamplers, "name");
                    JArray decodedVAEs = (JArray)await Util.GetInfo(ServerURL, "/sdapi/v1/sd-vae", Client);
                    this.VAEs = Util.JArryEntryToList(decodedVAEs, "model_name");

                }
                catch (Exception e)
                {
                    throw new Exception(e + "Failed to fatch data from the given server url");
                }

                this.isInitialised = true;
            } else
            {
                throw new Exception("Server URL is not set yet");
            }
        }
        public async void Refresh()
        {
            if (ServerURL != null)
            {
                try
                {
                    JObject decodedCNModules = (JObject)await Util.GetInfo(ServerURL, "/controlnet/module_list?alias_names=false", Client);
                    this.CNModules = decodedCNModules["module_list"].ToObject<List<string>>();
                    JObject decodedCNModels = (JObject)await Util.GetInfo(ServerURL, "/controlnet/model_list?update=true", Client);
                    this.CNModels = decodedCNModels["model_list"].ToObject<List<string>>();
                    JArray decodedSDModels = (JArray)await Util.GetInfo(ServerURL, "/sdapi/v1/sd-models", Client);
                    this.SDModels = Util.JArryEntryToList(decodedSDModels, "model_name");
                }
                catch (Exception e)
                {
                    throw new Exception(e + "Failed to fatch data from the given server url");
                }
            }
            else
            {
                throw new Exception("Server URL is not set yet");
            }
        }
        public void ReloadValueLists() {
            var componentNames = new List<string> { "ControlNet Models", "ControlNet Modules", "LoRA Models", "Samplers", "StableDiffusion Models", "Segmentation Colours" };
            var ghDoc = Grasshopper.Instances.ActiveCanvas.Document; // Get the active Grasshopper document
            if (ghDoc == null) return;
            foreach (IGH_DocumentObject docObj in ghDoc.Objects) // Iterate over all objects in the document
            {
                // Check if the object is a component and its name is in the list
                if (docObj is ISE_ValueList component)
                {
                    component.Refresh();
                    var valueList = component as Grasshopper.Kernel.Special.GH_ValueList;
                    // Expire the solution of the component to force it to recompute
                    Rhino.RhinoApp.WriteLine(valueList.Name + " refreshed");
                }
            }
        }
        public async Task<HttpResponseMessage> SDWebUIOptions(string sDModel) {
            string apiTail = "/sdapi/v1/options";
            var decodedOptions = await Util.GetInfo(this.ServerURL, apiTail, this.Client);
            decodedOptions["sd_model_checkpoint"] = sDModel;
            var payload = JObject.FromObject(new
            {
                sd_model_checkpoint = sDModel,
                alwayson_scripts = new { }
            });
            StringContent content = new StringContent(decodedOptions.ToString(), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await this.Client.PostAsync(this.ServerURL + apiTail, content);
            return response;
        }
        public override string ToString()
        {
            if (ServerURL == null) return "Server URL is not set yet";
            else return "Server URL: " + ServerURL;
        }
    }
}
