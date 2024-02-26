using Newtonsoft.Json.Linq;
using Rhino;
using Rhino.Display;
using Rhino.Geometry;
using StructuralEmbodimentToolkit.Core.Formfinding;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace StructuralEmbodimentToolkit.Core
{
    public static class Util
    {
        /** Check if a line connects two points in a nested list of points
         */
        public static bool DoesLineConnectPoints(LineCurve line, List<List<Point3d>> pointLists, double tolerance)
        {
            var startPoint = line.PointAtStart;
            var endPoint = line.PointAtEnd;

            foreach (var pointList in pointLists)
            {
                bool startFound = pointList.Any(p => IsCloseEnough(startPoint, p, tolerance));
                bool endFound = pointList.Any(p => IsCloseEnough(endPoint, p, tolerance));

                if (startFound && endFound)
                {
                    return true;
                }
            }
            return false;
        }

        private static bool IsCloseEnough(Point3d point1, Point3d point2, double tolerance)
        {
            return point1.DistanceTo(point2) <= tolerance;
        }

        public static void RemoveClosestToCenter(List<Point3d> points)
        {
            if (points == null || points.Count == 0) return;
            if (points.Count % 2 != 0)
            {
                // Compute the centroid
                Point3d centroid = new Point3d(
                    points.Average(p => p.X),
                    points.Average(p => p.Y),
                    points.Average(p => p.Z));

                // Find the point closest to the centroid
                Point3d closestPoint = points.OrderBy(p => p.DistanceTo(centroid)).First();

                // Remove the closest point
                points.Remove(closestPoint);
            }
        }


        public static bool HaveSameDirection(Curve curve1, Curve curve2)
        {
            var curve1Start = curve1.PointAtStart;
            var curve1End = curve1.PointAtEnd;
            var curve2Start = curve2.PointAtStart;

            return curve1End.DistanceTo(curve2Start) > curve1Start.DistanceTo(curve2Start);
        }

        public static bool IsPointConnectedToMember(Point3d point, Member member, double tolerance)
        {
            return member.EdgeAsPoints.Any(memberPoint => point.DistanceTo(memberPoint) < tolerance);
        }

        public static Vector3d ComputePolylineTangentAt(Polyline polyline, int index)
        {
            Vector3d tangent;
            if (index == 0)
            {
                // First point - use the first segment's tangent
                tangent = polyline[1] - polyline[0];
            }
            else if (index == polyline.Count - 1)
            {
                // Last point - use the last segment's tangent
                tangent = polyline[index] - polyline[index - 1];
            }
            else
            {
                // Middle points - average the tangents of adjacent segments
                Vector3d tangent1 = polyline[index] - polyline[index - 1];
                Vector3d tangent2 = polyline[index + 1] - polyline[index];
                tangent = (tangent1 + tangent2) * 0.5;
            }

            tangent.Unitize();
            return tangent;
        }

        public static double ValueRemap(double value, Interval from, Interval to)
        {
            if (from.Length == 0) return double.NaN; // Prevent division by zero

            // Calculate the proportion of 'value' in the original interval
            double proportion = (value - from.T0) / from.Length;

            // Apply the proportion to the new interval
            return to.T0 + (proportion * to.Length);
        }

        public static bool AreMembersConnected(Member member1, Member member2, double tolerance)
        {
            // Check if the members are the same instance or have the same identity
            if (ReferenceEquals(member1, member2))
            {
                return false; // The same member cannot be considered 'connected' to itself
            }

            // Check if any end point of member1 is close to any end point of member2
            return member1.EdgeAsPoints[0].EpsilonEquals(member2.EdgeAsPoints[0], tolerance) ||
                   member1.EdgeAsPoints[0].EpsilonEquals(member2.EdgeAsPoints[1], tolerance) ||
                   member1.EdgeAsPoints[1].EpsilonEquals(member2.EdgeAsPoints[0], tolerance) ||
                   member1.EdgeAsPoints[1].EpsilonEquals(member2.EdgeAsPoints[1], tolerance);
        }

        /*
         * Compute the average point of a list of points
         */
        public static Point3d AveragePoint(List<Point3d> points)
        {
            if (points == null || !points.Any())
                return Point3d.Unset;

            Point3d sum = new Point3d(0, 0, 0);
            foreach (var pt in points)
            {
                sum += pt;
            }
            return sum / points.Count;
        }
        /*
         * Compute the average point of a list of points, this version automatically removes the duplicates with a given tolerance
         */
        public static Point3d AveragePoint(List<Point3d> points, double tolerance)
        {
            List<Point3d> uniquePoints = new List<Point3d>();
            foreach (Point3d point in points)
            {
                bool isUnique = true;
                foreach (Point3d uniquePoint in uniquePoints)
                {
                    if (point.DistanceTo(uniquePoint) <= tolerance)
                    {
                        isUnique = false;
                        break; // This point is not unique, no need to check further
                    }
                }
                if (isUnique)
                {
                    uniquePoints.Add(point);
                }
            }

            if (points == null || !points.Any())
                return Point3d.Unset;

            Point3d sum = new Point3d(0, 0, 0);
            foreach (var pt in uniquePoints)
            {
                sum += pt;
            }
            return sum / uniquePoints.Count;
        }

        public static bool IsMemberConnectingOutlines(Member member, List<Curve> deckOutlines, double tolerance)
        {
            if (member.EdgeAsPoints.Count < 2) return false; // Check for at least 2 points

            // Assuming the member's start and end points are the first and last in EdgeAsPoints
            Point3d startPoint = member.EdgeAsPoints.First();
            Point3d endPoint = member.EdgeAsPoints.Last();

            bool startOnOutline1 = IsPointOnCurve(startPoint, deckOutlines.First(), tolerance);
            bool endOnOutline2 = IsPointOnCurve(endPoint, deckOutlines.Last(), tolerance);
            bool startOnOutline2 = IsPointOnCurve(startPoint, deckOutlines.Last(), tolerance);
            bool endOnOutline1 = IsPointOnCurve(endPoint, deckOutlines.First(), tolerance);

            return (startOnOutline1 && endOnOutline2) || (startOnOutline2 && endOnOutline1);
        }

        public static bool IsPointOnCurve(Point3d point, Curve curve, double tolerance)
        {

            PolylineCurve polylineCurve = (PolylineCurve)curve;
            Polyline polyline = polylineCurve.ToPolyline();
            var pts = polyline.ToArray();
            return pts.Any(pt => pt.DistanceTo(point) < tolerance);

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
            if (width <= 0 || height <= 0)
            {
                viewCapture.Width = activeViewport.Size.Width;
                viewCapture.Height = activeViewport.Size.Height;
            }
            else
            {
                viewCapture.Width = width;
                viewCapture.Height = height;
            }


            viewCapture.ScaleScreenItems = false;
            viewCapture.DrawAxes = false;
            viewCapture.DrawGrid = false;
            viewCapture.DrawGridAxes = false;
            viewCapture.TransparentBackground = true;

            Bitmap capture = viewCapture.CaptureToBitmap(activeView);
            return capture;

        }
        public static Bitmap CaptureDepthView(int width = -1, int height = -1)
        {
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

            if (width <= 0 || height <= 0)
            {
                resizedZBuffer = sourceZBuffer;
            }
            else
            {
                targetWidth = width;
                targetHeight = height;
                resizedZBuffer = ResizeBitmap(sourceZBuffer, targetWidth, targetHeight);
            }

            return resizedZBuffer;
        }

        public static Bitmap ResizeBitmap(Bitmap sourceBitmap, int width, int height)
        {
            Bitmap resizedBitmap = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(resizedBitmap))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(sourceBitmap, 0, 0, width, height);
            }
            return resizedBitmap;
        }

        public static string AddLoRAsToPrompt(string prompt, List<string> loRAs, List<double> multipliers)
        {
            if (loRAs.Count > multipliers.Count)
            {
                while (loRAs.Count > multipliers.Count)
                {
                    multipliers.Add(1.0);
                }
            }

            for (int i = 0; i < loRAs.Count; i++)
            {
                string loRA = loRAs[i];
                double multiplier = multipliers[i];
                prompt += (" " + "<lora:" + loRA + ":" + multiplier.ToString() + ">");
            }

            return prompt;
        }

        public static Interval AdjustIntervalTo180(Interval angleRange)
        {
            double startAngle = NormaliseAndClampAngle(angleRange.T0);
            double endAngle = NormaliseAndClampAngle(angleRange.T1);

            // Adjust if start is greater than end
            if (startAngle > endAngle)
            {
                double temp = startAngle;
                startAngle = endAngle;
                endAngle = temp;
            }

            return new Interval(startAngle, endAngle);
        }

        private static double NormaliseAndClampAngle(double angle)
        {
            // Normalise angle to 0 to 360 range
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

        public static void RedrawView(Point3d cameraPt, Point3d targetPt, int lensLength)
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

        public static Vector3d ClosestWorldAxis(Vector3d vector)
        {
            // Normalize the input vector
            vector.Unitize();

            // Define unit vectors for the world axes and their opposites
            Vector3d[] axes = {Vector3d.XAxis,
                              -Vector3d.XAxis,
                               Vector3d.YAxis,
                              -Vector3d.YAxis,
                               Vector3d.ZAxis,
                              -Vector3d.ZAxis};

            double minAngle = double.MaxValue;
            Vector3d closestAxis = Vector3d.Unset;

            // Find the axis with the smallest angle to the vector
            foreach (Vector3d axis in axes)
            {
                double angle = Vector3d.VectorAngle(vector, axis);
                if (angle < minAngle)
                {
                    minAngle = angle;
                    closestAxis = axis;
                }
            }
            return closestAxis;
        }

        public static bool IsPointInCloud(List<Point3d> points, Point3d pointToCheck, double tolerance)
        {
            return points.Any(p => p.DistanceTo(pointToCheck) < tolerance);
        }


        public static List<List<LineCurve>> GroupConnectedLineCurves(List<LineCurve> lineCurves, double tolerance)
        {
            List<List<LineCurve>> groupedLines = new List<List<LineCurve>>();
            HashSet<LineCurve> remainingLines = new HashSet<LineCurve>(lineCurves);

            while (remainingLines.Any())
            {
                var currentLine = remainingLines.First();
                remainingLines.Remove(currentLine);

                List<LineCurve> connectedGroup = new List<LineCurve> { currentLine };
                bool added;

                do
                {
                    added = false;

                    foreach (var line in remainingLines.ToList())
                    {
                        if (AreLinesConnected(connectedGroup.Last(), line, tolerance, true))
                        {
                            connectedGroup.Add(line);
                            remainingLines.Remove(line);
                            added = true;
                        }
                        else if (AreLinesConnected(connectedGroup.First(), line, tolerance, false))
                        {
                            connectedGroup.Insert(0, line);
                            remainingLines.Remove(line);
                            added = true;
                        }
                    }
                } while (added);

                groupedLines.Add(connectedGroup);
            }

            return groupedLines;
        }

        private static bool AreLinesConnected(LineCurve line1, LineCurve line2, double tolerance, bool checkEndOfFirst)
        {
            Point3d pointToCheck = checkEndOfFirst ? line1.PointAtEnd : line1.PointAtStart;
            return pointToCheck.DistanceTo(line2.PointAtStart) <= tolerance || pointToCheck.DistanceTo(line2.PointAtEnd) <= tolerance;
        }

        public static void CreateSegDisplayMode(string displayModeName)
        {
            var existingMode = DisplayModeDescription.FindByName(displayModeName);
            if (existingMode == null)
            {

                var temporaryFileName = Path.GetTempFileName();
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var resourceName = assembly.GetManifestResourceNames().Single(n => n.EndsWith("SE_Seg.ini"));
                using (var resourceStream = assembly.GetManifestResourceStream(resourceName))
                {
                    using (var fileStream = new FileStream(temporaryFileName, FileMode.Create, FileAccess.Write))
                    {
                        resourceStream.CopyTo(fileStream);
                    }
                }
                // Import the display mode from the temporary file
                var newModeId = DisplayModeDescription.ImportFromFile(temporaryFileName);

                // Optionally, delete the temporary file after import
                File.Delete(temporaryFileName);
                var newMode = DisplayModeDescription.GetDisplayMode(newModeId);
                newMode.EnglishName = displayModeName;
                DisplayModeDescription.UpdateDisplayMode(newMode);

                RhinoApp.WriteLine(newModeId.ToString());
            }
        }
        public static void CreateLineDisplayMode(string displayModeName)
        {
            var existingMode = DisplayModeDescription.FindByName(displayModeName);
            if (existingMode == null)
            {

                var temporaryFileName = Path.GetTempFileName();
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var resourceName = assembly.GetManifestResourceNames().Single(n => n.EndsWith("SE_Line.ini"));
                using (var resourceStream = assembly.GetManifestResourceStream(resourceName))
                {
                    using (var fileStream = new FileStream(temporaryFileName, FileMode.Create, FileAccess.Write))
                    {
                        resourceStream.CopyTo(fileStream);
                    }
                }
                // Import the display mode from the temporary file
                var newModeId = DisplayModeDescription.ImportFromFile(temporaryFileName);

                // Optionally, delete the temporary file after import
                File.Delete(temporaryFileName);
                var newMode = DisplayModeDescription.GetDisplayMode(newModeId);
                newMode.EnglishName = displayModeName;
                DisplayModeDescription.UpdateDisplayMode(newMode);

                RhinoApp.WriteLine(newModeId.ToString());
            }
        }
        public static void SetDisplayModeToCurrentViewport(string displayModeName)
        {
            var existingMode = DisplayModeDescription.FindByName(displayModeName);
            if (existingMode != null)
            {
                RhinoDoc.ActiveDoc.Views.ActiveView.ActiveViewport.DisplayMode = existingMode;
            }
        }

        public static Bitmap ConvertToGrayscale(Bitmap original)
        {
            // Create a blank bitmap with the same dimensions as the original
            Bitmap grayscale = new Bitmap(original.Width, original.Height);

            // Iterate through the original bitmap's pixels
            for (int i = 0; i < original.Width; i++)
            {
                for (int j = 0; j < original.Height; j++)
                {
                    // Get the pixel from the original image
                    Color originalColor = original.GetPixel(i, j);

                    // Calculate the grayscale value
                    int grayScale = (int)((originalColor.R * 0.3) + (originalColor.G * 0.59) + (originalColor.B * 0.11));

                    // Create the grayscale version of the pixel
                    Color grayColor = Color.FromArgb(originalColor.A, grayScale, grayScale, grayScale);

                    // Set the pixel in the new image
                    grayscale.SetPixel(i, j, grayColor);
                }
            }

            return grayscale;
        }
        public static void CustomiseWebuiUserBat(string template, string outputFilePath, List<string> argsToAdd)
        {
            // Read the contents of the template file
            //string[] lines = File.ReadAllLines(templateFilePath);
            string[] lines = template.Split(new[] { "\r\n", "\r", "\n" }, System.StringSplitOptions.None);


            // Prepare the additional args as a single string
            string additionalArgs = string.Join(" ", argsToAdd.Select(arg => $"--{arg}"));

            // StringBuilder to store the modified content
            StringBuilder newContent = new StringBuilder();

            foreach (string line in lines)
            {
                // Check if the current line contains "COMMANDLINE_ARGS"
                if (line.StartsWith("set COMMANDLINE_ARGS="))
                {
                    // Append the additional args to the line
                    string modifiedLine = line + " " + additionalArgs;
                    newContent.AppendLine(modifiedLine);
                }
                else
                {
                    // If not the target line, add it unchanged
                    newContent.AppendLine(line);
                }
            }

            // Write the modified content to the output file
            File.WriteAllText(outputFilePath, newContent.ToString());
        }
    }
}
