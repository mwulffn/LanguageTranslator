using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Text.RegularExpressions;

namespace uLanguageTranslator
{
    public class Language
    {
        public string Id { get; set; }
        public string InternationalName { get; set; }
        public string LocalName { get; set; }
        public string LocalId { get; set; }
        public string Culture { get; set; }

        public string CreatorsName { get; set; }
        public string Url { get; set; }

        private Dictionary<string, Dictionary<string, string>> Translations { get; set; }

        public string Path { get; set; }

        /// <summary>
        /// Returns an array of the areas currently available in this language
        /// </summary>
        public string[] Areas
        {
            get
            {
                return Translations.Keys.OrderBy(x => x).ToArray();
            }
        }

        /// <summary>
        /// Constructs an empty Language object that can be used to create a new language file.
        /// </summary>
        /// <param name="id">The two letter ISO-639-1 language code</param>
        /// <param name="internationalname">The language name in english</param>
        /// <param name="localname">The local name for this language (eg. 'deutsch' for german)</param>
        /// <param name="culture">The culture code for this language see CultureInfo in the .Net Framework</param>
        public Language(string id, string internationalname, string localname, string culture)
        {
            Id = id;
            InternationalName = internationalname;
            LocalName = localname;
            Culture = culture;
            LocalId = "";
            CreatorsName = "uTranslator";
            Url = "http://something.dk";

            Translations = new Dictionary<string, Dictionary<string, string>>();

            Path = "";
        }

        /// <summary>
        /// Synchronizes two languages, all entries in sourceLanguage will be compared to this language. Missing entries will be stamped with "TRANSLATE ME" and superflous entries will be stamped "REMOVE ME".
        /// </summary>
        /// <param name="sourceLanguage">the language to synchronize to</param>
        public void SynchronizeTranslations(Language sourceLanguage)
        {
            string[] sourceAreas = sourceLanguage.Areas;

            foreach (string sourceArea in sourceAreas)
            {
                string[] sourceKeys = sourceLanguage.GetTranslationKeys(sourceArea);

                if (sourceKeys.Length == 0)
                    continue;

                if (!Translations.ContainsKey(sourceArea))
                    Translations.Add(sourceArea, new Dictionary<string, string>());

                List<string> ownKeys = Translations[sourceArea].Keys.ToList();

                foreach (var key in sourceKeys)
                {
                    string phrase = sourceLanguage.GetRawPhrase(sourceArea, key);


                    if (!Translations[sourceArea].ContainsKey(key))
                        Translations[sourceArea].Add(key, string.Format("TRANSLATE ME: '{0}'", phrase));
                    else
                        ownKeys.Remove(key);

                }

                foreach (var extraKey in ownKeys)
                {
                    string phrase = GetRawPhrase(sourceArea, extraKey);

                    SetPhrase(sourceArea, extraKey, string.Format("REMOVE ME: {0}", phrase));
                }
            }

        }

        /// <summary>
        /// Returns all translation keys for a given area of the language
        /// </summary>
        /// <param name="area">the translation area</param>
        /// <returns>An array of the keys available in this language</returns>
        public string[] GetTranslationKeys(string area)
        {
            return Translations[area].Keys.OrderBy(x => x).ToArray();
        }

        public void SetPhrase(string area, string key, string phrase)
        {
            if (!Translations.ContainsKey(area))
                Translations[area] = new Dictionary<string, string>();

            if (Translations[area].ContainsKey(key))
                Translations[area][key] = phrase;
            else
                Translations[area].Add(key, phrase);

        }

        /// <summary>
        /// Returns a phrase from the language as selected by the area and key
        /// </summary>
        /// <param name="area">The area to use</param>
        /// <param name="key">The key to use</param>
        /// <returns>The translated phrase, if the phrase is not available "[key]" is returned</returns>
        public string GetPhrase(string area, string key)
        {
            return GetPhrase(area, key, new object[] { });
        }

        /// <summary>
        /// Returns a phrase from the language as selected by the area and key, and then formatted with string.Format using args
        /// </summary>
        /// <param name="area">The area to use</param>
        /// <param name="key">The key to use</param>
        /// <param name="args">The objects to be inserted into the phrase at runtime</param>
        /// <returns>The translated phrase, if the phrase is not available "[key]" is returned</returns>
        public string GetPhrase(string area, string key, params Object[] args)
        {
            if (!Translations.ContainsKey(area))
                return string.Format("[{0}]", key);

            if (!Translations[area].ContainsKey(key))
                return string.Format("[{0}]", key);

            return String.Format(Translations[area][key], args);
        }

        /// <summary>
        /// Get the raw non-formatted phrase from the language
        /// </summary>
        /// <param name="area">The area to use</param>
        /// <param name="key">The key to use</param>
        /// <returns>The raw unformatted phrase, if the phrase does not exists then an empty string is returned.</returns>
        public string GetRawPhrase(string area, string key)
        {
            if (!Translations.ContainsKey(area))
                return "";

            if (!Translations[area].ContainsKey(key))
                return "";

            return Translations[area][key];
        }

        /// <summary>
        /// Saves the language to an Umbraco compatible fileformat overwriting the file from which is was opened.
        /// </summary>
        public void Save()
        {
            if (String.IsNullOrEmpty(Path))
                throw new Exception("Can't save language to an empty path");

            Save(Path);
        }

        /// <summary>
        /// Saves the language to an Umbraco compatible fileformat to the path specified by path
        /// </summary>
        /// <param name="path">A path to a filename</param>
        public void Save(string path)
        {
            XElement root = new XElement("language");
            root.Add(new XAttribute("alias", Id));
            root.Add(new XAttribute("intName", InternationalName));
            root.Add(new XAttribute("localName", LocalName));
            root.Add(new XAttribute("lcid", LocalId));
            root.Add(new XAttribute("culture", Culture));

            XElement creator = new XElement("creator",
                                    new XElement("name", CreatorsName),
                                    new XElement("link", Url));

            root.Add(creator);

            //Save all areas sorted
            string[] areakeys = Translations.Keys.OrderBy(x => x).ToArray();

            foreach (var areakey in areakeys)
            {
                XElement area = new XElement("area", new XAttribute("alias", areakey));

                string[] translationkeys = Translations[areakey].Keys.OrderBy(x => x).ToArray();

                foreach (var translationkey in translationkeys)
                {
                    string translation = Translations[areakey][translationkey];

                    if (Regex.IsMatch(translation, "[<>\n]+"))
                        area.Add(new XElement("key", new XCData(translation), new XAttribute("alias", translationkey)));
                    else
                        area.Add(new XElement("key", translation, new XAttribute("alias", translationkey)));

                }

                root.Add(area);
            }

            XDocument doc = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"), root);

            doc.Save(path);
        }


        /// <summary>
        /// Loads a language from an Umbraco xml language file
        /// </summary>
        /// <param name="filename">A fully qualified path to an Umbraco xml language file</param>
        /// <returns>A fully instantiated language object with all phrases available, throws an exception if the load fails.</returns>
        public static Language Load(string filename)
        {
            try
            {
                XDocument doc = XDocument.Load(filename);

                XElement root = doc.Root;

                if (root.Name != "language")
                    throw new Exception("Invalid umbraco language file");


                string id = root.Attribute("alias").Value;
                string internationalName = root.Attribute("intName").Value;
                string localname = root.Attribute("localName") != null ? root.Attribute("localName").Value : internationalName;
                string culture = root.Attribute("culture").Value;

                Language language = new Language(id, internationalName, localname, culture);

                language.Path = filename;
                language.LocalId = root.Attribute("lcid") != null ? root.Attribute("lcid").Value : "";

                //Load creator information
                XElement creator = root.Descendants("creator").FirstOrDefault();

                if (creator != null)
                {
                    XElement name = creator.Descendants("name").FirstOrDefault();
                    XElement url = creator.Descendants("link").FirstOrDefault();

                    language.CreatorsName = name != null ? name.Value : "umbraco";
                    language.Url = url != null ? url.Value : "http://umbraco.org";
                }

                //Decode all ares
                IEnumerable<XElement> areas = doc.Descendants("area");

                foreach (var area in areas)
                {

                    if (area.Attribute("alias") == null)
                        throw new Exception("Alias is missing from area");

                    string areaAlias = area.Attribute("alias").Value;

                    foreach (var key in area.Descendants("key"))
                    {
                        string keyAlias = key.Attribute("alias").Value;
                        string value = key.Value;

                        //For backwards compatability convert %digit% til {digit}

                        value = Regex.Replace(value, @"\%(\d)\%", "{${1}}");

                        language.SetPhrase(areaAlias, keyAlias, value);
                    }

                }

                return language;

            }
            catch (Exception e)
            {

                throw new Exception(string.Format("The file {0} is not a valid language", filename), e);
            }
        }

    }
}
