using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using static System.Net.Mime.MediaTypeNames;

namespace LocalizationUtilities
{
    public static class LocalizationStringUtility
    {
        /// <summary>        
        /// If true, will log the generated card's key and current DefaultText
        /// to the BepInEx log.
        /// </summary>
        private static bool LogCardInfo { get; set; }

        /// <summary>
        /// The mod's directory.  EG: Path.GetDirectoryName(Info.Location);
        /// </summary>
        private static string ModPath { get; set; }

        private static ManualLogSource Logger { get; set; }

        private static SHA1 Sha1 = SHA1.Create();

        private static Dictionary<string, string> Localization;

        /// <summary>
        /// Initializes the Utility settings.
        /// </summary>
        /// <param name="logCardInfo"></param>
        /// <param name="dllPath"></param>
        /// <param name="logger"></param>
        public static void Init(bool logCardInfo, string dllPath, ManualLogSource logger)
        {
            LogCardInfo = logCardInfo;
            ModPath = Path.GetDirectoryName(dllPath);
            Logger = logger;
        }

        /// <summary>
        /// Generates and sets a localization key based on a hash of the DefaultText.
        /// If there is a Localization file loaded, the DefaultText will be set to that key.
        /// </summary>
        /// <param name="localizedString"></param>
        public static void SetLocalizationInfo(this ref LocalizedString localizedString)
        {
            if(Localization == null) Localization = GetLocalizationLookup();
            if (String.IsNullOrEmpty(localizedString.DefaultText)) return;


            //----Set LocalizationKey
            const string prefix = "T-";
            string key = prefix + Convert.ToBase64String(Sha1.ComputeHash(UTF8Encoding.UTF8.GetBytes(localizedString.DefaultText)));

            localizedString.LocalizationKey = key;


            //----Set localized text if available.
            //If there is a Localization lookup loaded, replace the text.
            if(Localization.TryGetValue(key, out string localizedText))
            {
                localizedString.DefaultText = localizedText;
            }

            //Log the keys and current values.  Useful for creating SimpEn.csv entries for the dynamically created cards
            //  in this mod.
            if (LogCardInfo)
            {
                Logger.LogInfo($"{localizedString.LocalizationKey}|{localizedString.DefaultText}");
            }
        }
        
        /// <summary>
        /// Creates a localization lookup from ./Localization/SimpEn.csv if the game's language is English.
        /// </summary>
        /// <returns></returns>
        private static Dictionary<string,string> GetLocalizationLookup()
        {

            try
            {
                if (LocalizationManager.Instance.Languages[LocalizationManager.CurrentLanguage].LanguageName != "English")
                {
                    return new();
                }

                //Check for localization file
                string translationFile = Path.Combine(ModPath, "Localization/SimpEn.csv");

                if (!File.Exists(translationFile))
                {
                    return new();
                }

                Dictionary<string, List<string>> translations = CSVParser.LoadFromPath(translationFile);

                Dictionary<string, string> translationLookup = new();

                int lineIndex = 0;
                foreach (KeyValuePair<string, List<string>> translation in translations)
                {
                    lineIndex++;

                    string key = translation.Key;
                    if(translation.Value.Count <=0)
                    {
                        throw new ApplicationException($"translation key missing values.  line: {lineIndex} Key: '{translation.Key}'");
                    }
                    string value = translation.Value[0];

                    //Overwrite if there are multiple entries
                    translationLookup[key] = value;
                }

                return translationLookup;
            }
            catch (Exception ex)
            {
                if(Logger is not null)
                {
                    Logger.LogError($"Error loading SimpEn.csv: {ex}");
                }

                return new();
            }
        }

    }
}
