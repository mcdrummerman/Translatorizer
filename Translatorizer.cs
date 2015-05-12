using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;
using Newtonsoft.Json;
using TranslatorizerConsole.Enums;
using TranslatorizerConsole.TranslatableLanguage;

namespace TranslatorizerConsole
{
    static class Translatorizer
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// <note>
        /// The required parameters are in the format "[path to the resx file to be translated] [comma seperated lsit of language codes]"
        /// </note>
        /// <example usage>
        /// Translatorizer.exe C:\path_to_language_file\Language.resx es,zh-CN
        /// </example>
        [STAThread]
        private static void Main(string[] args)
        {
            Trace.Listeners.Add(new ConsoleTraceListener());
            Trace.WriteLine("Starting Resx Translations.");
            Trace.WriteLine("Current Directory: " + Environment.CurrentDirectory);

            // !! Make sure to add parameters to the command line arguments in the Debug section of the project's properties window
            // link: https://msdn.microsoft.com/en-us/library/vstudio/1ktzfy9w%28v=vs.100%29.aspx
            if (args.Length < 2)
            {
                Trace.WriteLine("File path and language list is required. Less than two parameters were supplied.");
                return;
            }

            var type = string.Empty;
            TranlsationInputFileType inputType;
            if (args.Length == 3)
            {
                type = args[2];
            }

            // will default to Resx if fails
            Enum.TryParse(value: type, ignoreCase: true, result: out inputType);

            var languageCodes = args[1].Split(',');
            var languages = Language.TranslatableCollection.Where(available =>
                languageCodes.Any(code => string.Equals(code, available.Value.ToString(CultureInfo.InvariantCulture), StringComparison.CurrentCultureIgnoreCase)));

            var filePath = Path.GetDirectoryName(args[0]);
            var resourceFileName = Path.GetFileName(args[0]);
            var fullFilePath = Path.Combine(filePath, resourceFileName);

            if (!File.Exists(fullFilePath))
            {
                Trace.WriteLine("No file found to translate from.");
                return;
            }

            foreach (var lang in languages)
            {
                var translatedFile = Path.Combine(filePath, "Language." + lang.Value + ".resx");
                try
                {
                    switch (inputType)
                    {
                        case TranlsationInputFileType.Resx:
                            TranslateSingleFileFromResx(lang, fullFilePath, translatedFile, false);
                            break;
                        case TranlsationInputFileType.Json:
                            TranslateSingleFileFromJson(lang, fullFilePath, translatedFile);
                            break;
                    }
                }
                catch (Exception e)
                {
                    Trace.Write(e.Message);
                    return;
                }
            }
            Trace.Write("Done.");
        }

        private static void TranslateSingleFileFromResx(Language desiredLanguage, string filePath, string fileSavePath,
            bool includeBlankResources)
        {
            // Open the input file.
            var reader = new ResXResourceReader(filePath);

            try
            {
                // Get the enumerator.  If this throws an ArguementException
                // it means the file is not a .RESX file.
                var enumerator = reader.GetEnumerator();
            }
            catch (ArgumentException ex)
            {
                Trace.WriteLine("WARNING: could not parse " + filePath);
                Trace.WriteLine("         " + ex.Message);
                return;
            }

            // Allocate the list for this instance.
            var textResourcesList = new SortedDictionary<object, object>();

            // Run through the file looking for only true text related
            // properties and only those with values set.
            foreach (DictionaryEntry dic in reader)
            {
                // Only consider this entry if the value is something.
                if (null != dic.Value)
                {
                    // Is this a System.String.
                    if ("System.String" == dic.Value.GetType().ToString())
                    {
                        var keyString = dic.Key.ToString();

                        // Make sure the key name does not start with the
                        // "$" or ">>" meta characters and is not an empty
                        // string (or we're explicitly including empty strings).
                        if ((!keyString.StartsWith(">>")) &&
                            (!keyString.StartsWith("$")) &&
                            (includeBlankResources || "" != dic.Value.ToString()))
                        {
                            // We've got a winner.
                            textResourcesList.Add(dic.Key, dic.Value);
                        }

                        // Special case the Windows Form "$this.Text" or
                        // I don't get the form titles.
                        if (string.CompareOrdinal(keyString, "$this.Text") == 0)
                        {
                            textResourcesList.Add(dic.Key, dic.Value);
                        }
                    }
                }
            }

            WriteToResx(textResourcesList, desiredLanguage, filePath, fileSavePath);
        }

        private static void TranslateSingleFileFromJson(Language desiredLanguage, string filePath, string fileSavePath)
        {
            // Open the input file.
            var json = File.ReadAllText(filePath);

            IDictionary<object, object> dictionary = null;

            try
            {
                dictionary = JsonConvert.DeserializeObject<IDictionary<object, object>>(json);
            }
            catch (ArgumentException ex)
            {
                Trace.WriteLine("WARNING: could not parse " + filePath);
                Trace.WriteLine("         " + ex.Message);
                return;
            }

            WriteToResx(dictionary, desiredLanguage, filePath, fileSavePath);
        }

        private static void WriteToResx(IDictionary<object, object> entries, Language desiredLanguage, string filePath, string fileSavePath)
        {
            // It's entirely possible that there are no text strings in the
            // .ResX file.
            if (entries.Count > 0)
            {
                if (fileSavePath != null)
                {
                    Dictionary<string, string> existingEntries = null;
                    try
                    {
                        existingEntries = new ResXResourceReader(fileSavePath)
                            .Cast<DictionaryEntry>()
                            .ToDictionary(k => k.Key.ToString(), v => v.Value.ToString());
                    }
                    catch (FileNotFoundException)
                    {
                        Trace.WriteLine(fileSavePath + " not found. New file will be created.");
                    }

                    // Create the new file.
                    var writer = new ResXResourceWriter(fileSavePath);

                    foreach (var languageEntry in entries)
                    {
                        var key = languageEntry.Key.ToString();

                        if (existingEntries == null ||
                            // ReSharper disable SimplifyLinqExpression
                            !existingEntries.Any(entry =>
                                entry.Key.ToString(CultureInfo.InvariantCulture) == languageEntry.Key.ToString()))
                        // ReSharper restore SimplifyLinqExpression
                        {
                            var translate = desiredLanguage != Language.English;

                            var value = translate
                                ? Translator.Translate(desiredLanguage, languageEntry.Value.ToString())
                                : languageEntry.Value.ToString();

                            // does not exist try to translate it and add to the file
                            writer.AddResource(languageEntry.Key.ToString(), value);
                        }
                        else
                        {
                            // already exists,  we simply add the value back to the file
                            writer.AddResource(key, existingEntries[key]);
                        }
                    }

                    writer.Generate();
                    writer.Close();
                }
            }
            else
            {
                Trace.WriteLine("WARNING: No text resources found in " + filePath);
            }
        }
    }
}
