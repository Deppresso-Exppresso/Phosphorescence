﻿using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// Supplies an enumerator ButtonType, and ModSettings.
/// </summary>
namespace PhosphorescenceExtensions
{
    /// <summary>
    /// The mod settings that can be adjusted by a user, usually from the ModSelector.
    /// </summary>
    public class ModSettingsJSON
    {
        /// <summary>
        /// When disabled, there will be no indicators/letters for colors.
        /// </summary>
        public bool CruelMode { get; set; }

        /// <summary>
        /// Uses the appropriate event triggers for what this is set on.
        /// </summary>
        public bool VRMode { get; set; }

        /// <summary>
        /// How much additional time needs to be given due to stream delay?
        /// </summary>
        public int StreamDelay { get; set; }

        /// <summary>
        /// Gets the values from ModSettings.
        /// </summary>
        /// <param name="vrMode">Used for initalization only.</param>
        /// <param name="streamDelay">Determines the timer.</param>
        public static void Get(PhosphorescenceScript pho, out bool cruelMode, out bool vrMode, out int streamDelay)
        {
            // Default values.
            cruelMode = false;
            vrMode = false;
            streamDelay = 0;

            try
            {
                // Try loading settings.
                ModSettingsJSON settings = JsonConvert.DeserializeObject<ModSettingsJSON>(pho.ModSettings.Settings);

                // Do settings exist?
                if (settings != null)
                {
                    cruelMode = settings.CruelMode;
                    vrMode = settings.VRMode;
                    streamDelay = 15 * settings.StreamDelay;
                }
            }
            catch (JsonReaderException e)
            {
                // In the case of catastrophic failure and devastation.
                Debug.LogFormat("[Phosphorescence #{0}]: JSON error: \"{1}\", resorting to default values.", pho.init.moduleId, e.Message);
            }
        }
    }
}
