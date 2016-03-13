using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

namespace WorldUtilities
{
    public static class ConfigurationLoader
    {
        public static string ConfigPath = Application.dataPath + "\\Internal\\Config\\";

		public static void SaveConfiguration(Dictionary<string, string> iconfig, string iID)
        {
            SaveConfigurationInternal(iconfig, ConfigPath + iID);
        }

		public static Dictionary<string, string> LoadConfiguration(string iID)
        {
            return LoadConfigurationInternal(ConfigPath + iID);
        }

		static void SaveConfigurationInternal(Dictionary<string, string> iconfig, string path)
        {
            string ttext = "";
            foreach (KeyValuePair<string, string> tentry in iconfig)
            {
                string tstr = tentry.Key.Replace(" ", "_") + " = " + tentry.Value.Replace(" ", "_");
                ttext = ttext + tstr + "\r\n";
            }

            byte[] tbytes = GetBytes(ttext);
            File.WriteAllBytes(path, tbytes);
        }

		static Dictionary<string, string> LoadConfigurationInternal(string path)
        {
            Dictionary<string, string> tconfig = new Dictionary<string,string>();
            byte[] tbytes = File.ReadAllBytes(path);
            string ttext = GetString(tbytes);
            string[] ttextarr = ttext.Split(new string[] {"\n", "\r\n"}, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < ttextarr.Length; i++)
            {
                string[] tarr = ttextarr[i].Split("=".ToCharArray());
                string tname = tarr[0].Replace(" ", "").Replace("_", " ");
                string tval = tarr[1].Replace(" ", "").Replace("_", " ");
                tconfig.Add(tname, tval);
            }
            return tconfig;
        }

        static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        static string GetString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }
    }
}