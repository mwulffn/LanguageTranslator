using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Text.RegularExpressions;

namespace uLanguageTranslator
{


    public static class LanguageRepository
    {
        private static string Folder { get; set; }

        /// <summary>
        /// All the languages indexed by language-code.
        /// </summary>
        private static Dictionary<string, Language> Languages { get; set; }


        /// <summary>
        /// Returns a language that corresponds to the two letter language code
        /// </summary>
        /// <param name="languagecode">The ISO 639-1 language code </param>
        /// <returns>A instantiated language object or null if it does not exist in Umbraco</returns>
        public static Language GetLanguage(string languagecode)
        {
            if (!Languages.ContainsKey(languagecode))
            {
                if (!LoadLanguage(languagecode))
                    return null;
            }

            return Languages[languagecode];
        }

        /// <summary>
        /// Returns the physical path of a language file in Umbraco
        /// </summary>
        /// <param name="languagecode">The ISO 639-1 language code </param>
        /// <returns>An absolute path to the language file</returns>
        public static string GetPathForLanguage(string languagecode)
        {
            if (Folder == "")
                throw new Exception("The repository must initialized before use, call LanguageRepository.Init(string folder) first");

            return Path.Combine(Folder, languagecode + ".xml");
        }

        private static bool LoadLanguage(string languagecode)
        {
            string path = GetPathForLanguage(languagecode);

            if (!File.Exists(path))
                return false;

            try
            {
                Language language = Language.Load(path);

                Languages[language.Id] = language;

                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }

        /// <summary>
        /// Returns a list of all the language codes that have a language file in Umbraco
        /// </summary>
        /// <returns>List of language codes</returns>
        public static string[] ListLanguages()
        {
            if (Folder == "")
                throw new Exception("The repository must initialized before use, call LanguageRepository.Init(string folder) first");

            string[] files = Directory.GetFiles(Folder, "*.xml");

            return files.Select(x => Path.GetFileNameWithoutExtension(x)).ToArray();
        }

        /// <summary>
        /// Init function must be called with Umbraco root folder path
        /// </summary>
        /// <param name="folder">Umbraco root folder path</param>

        public static void Init(string folder)
        {
            if (!Directory.Exists(folder))
                throw new Exception(string.Format("Can't find the folder: '{0}'", folder));

            Folder = folder;

            Languages = new Dictionary<string, Language>();
        }







    }
}
