using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StructuralEmbodiment.Core.Visualisation
{
    public class SDWebUISetting
    {
        public string ServerURL;

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
        public void SetSDWebUISetting(string serverURL )
        {
            ServerURL = serverURL;
        }
        public override string ToString()
        {
            return "Server URL: " + ServerURL;
        }
    }
}
