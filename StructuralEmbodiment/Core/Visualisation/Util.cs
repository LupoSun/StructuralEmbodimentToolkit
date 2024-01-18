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
using Rhino.Geometry;
using Rhino.Geometry.Intersect;

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

        public static Bitmap CaptureView(int width = -1, int height = -1)
        {
            //Settng up viewport
            RhinoDoc activeDoc = RhinoDoc.ActiveDoc;
            RhinoView activeView = activeDoc.Views.ActiveView;
            RhinoViewport activeViewport = activeView.ActiveViewport;

            //Setting up view capture
            var viewCapture = new ViewCapture();
            if (width <= 0 || height <=0)
            {
                viewCapture.Width = activeViewport.Size.Width;
                viewCapture.Height = activeViewport.Size.Height;
            }
            else { 
                viewCapture.Width = width;
                viewCapture.Height = height;
            }


            viewCapture.ScaleScreenItems = false;
            viewCapture.DrawAxes = false;
            viewCapture.DrawGrid = false ;
            viewCapture.DrawGridAxes = false ;
            viewCapture.TransparentBackground = true;

            Bitmap capture = viewCapture.CaptureToBitmap(activeView);
            return capture;

        }
        public static Bitmap CaptureDepthView(int width = -1, int height = -1) {
            //Settng up viewport
            RhinoDoc activeDoc = RhinoDoc.ActiveDoc;
            RhinoView activeView = activeDoc.Views.ActiveView;
            RhinoViewport activeViewport = activeView.ActiveViewport;

            //Setting up zbuffer capture
            ZBufferCapture zBufferCapture = new ZBufferCapture(activeViewport);
            zBufferCapture.ShowCurves(true);
            zBufferCapture.ShowPoints(false);
            zBufferCapture.ShowIsocurves(false);
            zBufferCapture.ShowLights(false);

            Bitmap sourceZBuffer = zBufferCapture.GrayscaleDib();

            int targetWidth;
            int targetHeight;
            Bitmap resizedZBuffer;

            if (width <= 0 || height <=0)
            {
                resizedZBuffer = sourceZBuffer;
            }
            else { 
                targetWidth = width;
                targetHeight = height;
                resizedZBuffer = ResizeBitmap(sourceZBuffer, targetWidth, targetHeight);
            }

            return resizedZBuffer;
        }

        public static Bitmap ResizeBitmap (Bitmap sourceBitmap,int width, int height)
        {
            Bitmap resizedBitmap = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(resizedBitmap))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(sourceBitmap, 0, 0, width, height);
            }
            return resizedBitmap;
        }

        public static Interval AdjustIntervalTo180(Interval angleRange)
        {
            double startAngle = NormalizeAndClampAngle(angleRange.T0);
            double endAngle = NormalizeAndClampAngle(angleRange.T1);

            // Adjust if start is greater than end
            if (startAngle > endAngle)
            {
                double temp = startAngle;
                startAngle = endAngle;
                endAngle = temp;
            }

            return new Interval(startAngle, endAngle);
        }

        private static double NormalizeAndClampAngle(double angle)
        {
            // Normalize angle to 0 to 360 range
            angle = angle % 360;
            if (angle < 0)
                angle += 360;

            // Clamp angle to 0 to 180 range
            return angle > 180 ? 180 : angle;
        }

        public static Interval DegreesToRadiansInterval(Interval angleRangeDegrees)
        {
            double startRadians = angleRangeDegrees.T0 * Math.PI / 180.0;
            double endRadians = angleRangeDegrees.T1 * Math.PI / 180.0;
            return new Interval(startRadians, endRadians);
        }

        public static double DegreesToRadians(double degrees)
        {
            return degrees * (Math.PI / 180);
        }

        public static void RedrawView(Point3d cameraPt, Point3d targetPt,int lensLength)
        {
            RhinoDoc doc = RhinoDoc.ActiveDoc;
            RhinoView view = doc.Views.ActiveView;
            if (view != null)
            {
                view.ActiveViewport.SetCameraLocation(cameraPt, true);
                view.ActiveViewport.SetCameraDirection(targetPt - cameraPt, true);
                view.ActiveViewport.Camera35mmLensLength = lensLength;
                //NEEDS MORE INVESTIGATION
                //view.ActiveViewport.SetCameraTarget(targetPt, true);
                view.Redraw();
            }
        }

        public static Point3d SampleRandomPointOnBreps(List<Brep> breps, Random rnd)
        {
            if (breps == null || breps.Count == 0)
            {
                return Point3d.Unset;
            }

            // Select a random Brep from the list
            int brepIndex = rnd.Next(breps.Count);
            Brep selectedBrep = breps[brepIndex];

            if (selectedBrep.Faces.Count == 0)
            {
                return Point3d.Unset;
            }

            // Select a random face from the selected Brep
            int faceIndex = rnd.Next(selectedBrep.Faces.Count);
            BrepFace face = selectedBrep.Faces[faceIndex];

            // Get domain of the surface in U and V direction
            Interval domainU = face.Domain(0);
            Interval domainV = face.Domain(1);

            // Generate random parameters within the domain
            double u = domainU.ParameterAt(rnd.NextDouble());
            double v = domainV.ParameterAt(rnd.NextDouble());

            // Evaluate the surface at these parameters
            Point3d pt = face.PointAt(u, v);

            return pt;
        }

         
    }
}
