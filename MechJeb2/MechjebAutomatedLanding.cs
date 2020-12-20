using System;
using System.Linq;
using UnityEngine;

namespace MuMech
{
    [KSPAddon(KSPAddon.Startup.MainMenu, false)]
    public class MechjebAutomatedLanding : MonoBehaviour
    {
        public static string settingsConfigURL = "GameData/MechJeb2/AutomatedLandingData.cfg";

        //public static Dictionary<string, LandingError> LandingErrors = new Dictionary<string, LandingError>();

        void Awake()
        {
          
        }

        void Start()
        {
            var root = ConfigNode.Load(settingsConfigURL);
            // Create settings file if not present.
            if (ConfigNode.Load(settingsConfigURL) == null)
            {
                var node = new ConfigNode();
                node.AddNode("VesselsData");
                node.Save(settingsConfigURL);
            }
        }

        public static void AddNewError(string vesselName, Vector3d errorVector)
        {
            var root = ConfigNode.Load(settingsConfigURL);

            var vesselData = root.GetNode("VesselsData");

           

            if (vesselData.GetNodes().All(x => x.GetValue("name") != vesselName))
            {
                var newVesselNode = vesselData.AddNode("Vessel");

                newVesselNode.AddValue("name", vesselName);
                newVesselNode.AddValue("errorX", errorVector.x);
                newVesselNode.AddValue("errorY",errorVector.y);
                newVesselNode.AddValue("errorZ", errorVector.z);


            }
            else
            {
                ConfigNode vesselNode = vesselData.GetNodes().Where(x => x.GetValue("name") == vesselName).ToArray()[0];
                double previousErrorX = Convert.ToDouble(vesselNode.GetValue("errorX"));

                vesselNode.SetValue("errorX", previousErrorX + errorVector.x);

                double previousErrorY= Convert.ToDouble(vesselNode.GetValue("errorY"));

                vesselNode.SetValue("errorY", previousErrorY +  errorVector.y);

                double previousErrorZ = Convert.ToDouble(vesselNode.GetValue("errorZ"));

                vesselNode.SetValue("errorZ", previousErrorZ + errorVector.z);

            }

            root.Save(settingsConfigURL);
        }

        public static LandingError GetError(string vesselName)
        {
            var root = ConfigNode.Load(settingsConfigURL);

            var vesselData = root.GetNode("VesselsData");
            try
            {
                ConfigNode vesselNode = vesselData.GetNodes().Where(x => x.GetValue("name") == vesselName).ToArray()[0];

                if (vesselNode == null)
                {
                    Debug.Log($"GetError {vesselName} returns 0");
                    return new LandingError() { ErrorX = 0, ErrorY = 0, ErrorZ = 0 };
                }

                double errorX = double.Parse(vesselNode.GetValue("errorX"));

                double errorY = double.Parse(vesselNode.GetValue("errorY"));

                double errorz = double.Parse(vesselNode.GetValue("errorZ"));

                Debug.Log($"Getting Error {vesselName} returns {errorX},{errorY},{errorz}");
                var newError = new LandingError
                {
                    ErrorX = errorX,
                    ErrorY = errorY,
                    ErrorZ = errorz
                };

                return newError;
            }
            catch (Exception)
            {

                return new LandingError() { ErrorX = 0, ErrorY = 0, ErrorZ = 0 };
            }

        }

        
    }
}