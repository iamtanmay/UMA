using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

namespace WorldUtilities
{
	public static class WorldUtilities
    {
        public static string ConfigPath = Application.dataPath + "\\Internal\\Config\\";

		public static void SaveConfiguration(string[] ikeys, string[] ivals, string iID, bool debugmode)
		{
			Dictionary<string, string> tdic = new Dictionary<string, string> ();
			for (int i=0; i<ikeys.Length; i++) {
				tdic.Add(ikeys[i], ivals[i]);
			}
			SaveConfiguration (tdic, iID, debugmode);
		}

		public static void SaveConfiguration(List<string> ikeys, List<string> ivals, string iID, bool debugmode)
		{
			SaveConfiguration (ikeys.ToArray (), ivals.ToArray (), iID, debugmode);
		}

		public static void SaveConfiguration(Dictionary<string, string> iconfig, string iID, bool debugmode)
        {
			SaveConfigurationInternal(iconfig, ConfigPath + iID, debugmode);
        }

		public static Dictionary<string, string> LoadConfiguration(string iID, bool debugmode)
        {
			return LoadConfigurationInternal(ConfigPath + iID, debugmode);
        }

		static void SaveConfigurationInternal(Dictionary<string, string> iconfig, string path, bool debugmode)
        {
            string ttext = "";
            foreach (KeyValuePair<string, string> tentry in iconfig)
            {
                string tstr = tentry.Key.Replace(" ", "_") + " = " + tentry.Value.Replace(" ", "_");
                ttext = ttext + tstr + "\r\n";
            }

			if (debugmode)
				Debug.Log ("Saving configuration to " + path);

            byte[] tbytes = GetBytes(ttext);
            File.WriteAllBytes(path, tbytes);
        }

		static Dictionary<string, string> LoadConfigurationInternal(string path, bool debugmode)
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