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

        public static void AddNewError(string vesselName, double errorLatitude, double errorLongitude)
        {
            var root = ConfigNode.Load(settingsConfigURL);

            var vesselData = root.GetNode("VesselsData");

           

            if (vesselData.GetNodes().All(x => x.GetValue("name") != vesselName))
            {
                var newVesselNode = vesselData.AddNode("Vessel");

                newVesselNode.AddValue("name", vesselName);
                newVesselNode.AddValue("errorLatitude", errorLatitude);
                newVesselNode.AddValue("errorLongitude",errorLongitude);

            }
            else
            {
                ConfigNode vesselNode = vesselData.GetNodes().Where(x => x.GetValue("name") == vesselName).ToArray()[0];
                double previousErrorLatitude = Convert.ToDouble(vesselNode.GetValue("errorLatitude"));

                vesselNode.SetValue("errorLatitude", previousErrorLatitude + errorLatitude);

                double previousErrorLongitude = Convert.ToDouble(vesselNode.GetValue("errorLongitude"));

                vesselNode.SetValue("errorLongitude", previousErrorLongitude + errorLongitude);
            }

            root.Save(settingsConfigURL);
        }

        public static LandingError GetError(string vesselName)
        {
            var root = ConfigNode.Load(settingsConfigURL);

            var vesselData = root.GetNode("VesselsData");
            ConfigNode vesselNode = vesselData.GetNodes().Where(x => x.GetValue("name") == vesselName).ToArray()[0];

            if (vesselNode == null)
            {
                Debug.Log($"GetError {vesselName} returns 0");
                return new LandingError(){LatitudeError = 0, LongitudeError = 0};
            }

            double errorLatitude = double.Parse(vesselNode.GetValue("errorLatitude"));

            double errorLongitude = double.Parse(vesselNode.GetValue("errorLongitude"));
            Debug.Log($"GetError {vesselName} returns not zero. ErrorLatitude {errorLatitude} ErrorLongitude {errorLongitude}");

            var newError = new LandingError
            {
                LatitudeError = errorLatitude, LongitudeError = errorLongitude
            };

            return newError;

        }

        
    }
}