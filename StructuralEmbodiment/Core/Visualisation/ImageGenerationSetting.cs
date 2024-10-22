﻿using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

namespace StructuralEmbodimentToolkit.Core.Visualisation
{
    internal class ImageGenerationSetting
    {
        public string ServerURL { get; set; }
        public HttpClient Client { get; set; }
        public JObject Settings { get; set; }

        //public List<string> controlNetModules;
        //public List<string> controlNetModels;
        //public List<string> sdModels;

        public string prompt;
        public string nprompt;
        public int seed;
        public int batchSize;
        public int steps;
        public double cfgScale;
        public int width;
        public int height;
        public string sampler;


        public ImageGenerationSetting(SDWebUISetting sDWebUISetting)
        {
            this.ServerURL = sDWebUISetting.ServerURL;
            this.Client = sDWebUISetting.Client;
        }

        public void InitialiseSettings(string prompt, string nprompt, int seed, string sampler, int batchSize, int steps,double cfgScale, int width, int height)
        {
            this.prompt = prompt;
            this.nprompt = nprompt;
            this.seed = seed;
            this.sampler = sampler;
            this.batchSize = batchSize;
            this.steps = steps;
            this.cfgScale = cfgScale;
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
                cfg_scale = cfgScale,
                width,
                height,
                alwayson_scripts = new { }
            });
        }
        
        public override string ToString()
        {
            
            return "Server URL: " + this.ServerURL + "\n" + "\n" +
                "Prompt: " + this.prompt + "\n" + "\n" +
                "Negative Prompt: " + this.nprompt + "\n" + "\n" +
                "Random Seed: " + this.seed + "\n" + "\n" +
                "Sampler: " + this.sampler + "\n" + "\n" +
                "Batch Size: " + this.batchSize + "\n" + "\n" +
                "CFG Scale " + this.cfgScale + "\n" + "\n" +
                "Steps: " + this.steps + "\n" + "\n" +
                "Width: " + this.width + "\n" + "\n" +
                "Height: " + this.height + "\n" + "\n" +
                //"ControlNetModules: " +" \n "+ String.Join("\n", this.controlNetModules) + "\n" + "\n" +
                //"ControlNetModels: " + "\n" + String.Join("\n", this.controlNetModels) + "\n" + "\n" +
                //"SDModels: " + "\n" + String.Join("\n", this.sdModels) + "\n" + "\n" +
                "Payload" + "\n" + this.Settings.ToString();
            
        }



    }
}
