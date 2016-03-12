using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

namespace Avatar{
    public static class Utilities
    {
        public static string AvatarSavePath = Application.dataPath + "\\Internal\\Cache\\Avatar\\";
        public static string PlayerAvatarSavePath = Application.dataPath + "\\Save\\Avatar\\";

        public static void SavePlayerAvatar(UMADynamicAvatar iavatar, string iID)
        {
            SaveRecipe(iavatar, PlayerAvatarSavePath + iID);
        }

        public static void SaveCacheAvatar(UMADynamicAvatar iavatar, string iID)
        {
            SaveRecipe(iavatar, AvatarSavePath + iID);
        }

        public static void LoadPlayerAvatar(Transform iavatar, string iID)
        {
            LoadRecipe(iavatar, PlayerAvatarSavePath + iID);
        }

        public static void LoadCacheAvatar(Transform iavatar, string iID)
        {
            LoadRecipe(iavatar, AvatarSavePath + iID);
        }

        public static void SaveRecipe(UMADynamicAvatar iavatar, string path)
        {
            UMATextRecipe asset = ScriptableObject.CreateInstance<UMATextRecipe>();
            asset.Save(iavatar.umaData.umaRecipe, iavatar.context);
            byte[] tbytes = GetBytes(asset.recipeString);
            File.WriteAllBytes(path + ".bin", tbytes);
        }

        public static void LoadRecipe(Transform iavatar, string path)
        {
            byte[] tbytes = File.ReadAllBytes(path + ".bin");
            UMATextRecipe asset = ScriptableObject.CreateInstance<UMATextRecipe>();
            asset.recipeString = GetString(tbytes);
            UMADynamicAvatar avatar = iavatar.GetComponent<UMADynamicAvatar>();
            avatar.Load(asset);
            UnityEngine.Object.Destroy(asset);
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