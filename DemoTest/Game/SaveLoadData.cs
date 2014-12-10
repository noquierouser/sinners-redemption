using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.GamerServices;

namespace DemoTest
{
    public class SaveLoadData
    {
        #region Fields

        StorageDevice device;
        string containerName = "DemoTest";
        string optionsFile = "config.cfg";
        string playerFile = "saveData.sav";

        [Serializable]
        public struct OptionsData
        {
            public int soundVolume;
            public int musicVolume;
        }

        [Serializable]
        public struct PlayerData
        {
            public int hitPoints;
            public int maxHitPoints;
            public int str;
            public int dex;
            public int vit;
            public Vector2 position;
            public int levelIndex;
        }

        #endregion

        #region Initiate Save & Load

        public void InitiateSaveOptions()
        {
            if (!Guide.IsVisible)
            {
                device = null;
                StorageDevice.BeginShowSelector(PlayerIndex.One, this.SaveOptions, null);                
            }
        }

        public void InitiateLoadOptions()
        {
            if (!Guide.IsVisible)
            {
                device = null;
                StorageDevice.BeginShowSelector(PlayerIndex.One, this.LoadOptions, null);
            }
        }

        public void InitiateSavePlayer()
        {
            if (!Guide.IsVisible)
            {
                device = null;
                StorageDevice.BeginShowSelector(PlayerIndex.One, this.SavePlayerData, null);
            }
        }

        public void InitiateLoadPlayer()
        {
            if (!Guide.IsVisible)
            {
                device = null;
                StorageDevice.BeginShowSelector(PlayerIndex.One, this.LoadPlayerData, null);
            }
        }

        #endregion

        #region Save & Load Methods

        void SaveOptions(IAsyncResult result)
        {
            device = StorageDevice.EndShowSelector(result);

            if (device != null && device.IsConnected)
            {
                OptionsData options = new OptionsData()
                {
                    soundVolume = Global.sound,
                    musicVolume = Global.music,
                };
                IAsyncResult r = device.BeginOpenContainer(containerName, null, null);
                result.AsyncWaitHandle.WaitOne();
                StorageContainer container = device.EndOpenContainer(r);
                if (container.FileExists(optionsFile))
                    container.DeleteFile(optionsFile);
                Stream stream = container.CreateFile(optionsFile);
                XmlSerializer serializer = new XmlSerializer(typeof(OptionsData));
                serializer.Serialize(stream, options);
                stream.Close();
                container.Dispose();
                result.AsyncWaitHandle.Close();
            }
        }

        void LoadOptions(IAsyncResult result)
        {
            device = StorageDevice.EndShowSelector(result);
            IAsyncResult r = device.BeginOpenContainer(containerName, null, null);
            result.AsyncWaitHandle.WaitOne();
            StorageContainer container = device.EndOpenContainer(r);
            result.AsyncWaitHandle.Close();
            if (container.FileExists(optionsFile))
            {
                Stream stream = container.OpenFile(optionsFile, FileMode.Open);
                XmlSerializer serializer = new XmlSerializer(typeof(OptionsData));
                OptionsData options = (OptionsData)serializer.Deserialize(stream);
                stream.Close();
                container.Dispose();
                Global.sound = options.soundVolume;
                Global.music = options.musicVolume;
            }
        }

        void SavePlayerData(IAsyncResult result)
        {
            device = StorageDevice.EndShowSelector(result);

            if (device != null && device.IsConnected)
            {
                PlayerData data = new PlayerData()
                {
                    hitPoints = Global.hp,
                    maxHitPoints = Global.maxHp,
                    str = Global.str,
                    dex = Global.dex,
                    vit = Global.vit,
                    position = Global.position,
                    levelIndex = Global.levelIndex,
                };
                IAsyncResult r = device.BeginOpenContainer(containerName, null, null);
                result.AsyncWaitHandle.WaitOne();
                StorageContainer container = device.EndOpenContainer(r);
                if (container.FileExists(playerFile))
                    container.DeleteFile(playerFile);
                Stream stream = container.CreateFile(playerFile);
                XmlSerializer serializer = new XmlSerializer(typeof(PlayerData));
                serializer.Serialize(stream, data);
                stream.Close();
                container.Dispose();
                result.AsyncWaitHandle.Close();
                Global.saveExists = true;
            }
        }

        void LoadPlayerData(IAsyncResult result)
        {
            device = StorageDevice.EndShowSelector(result);
            IAsyncResult r = device.BeginOpenContainer(containerName, null, null);
            result.AsyncWaitHandle.WaitOne();
            StorageContainer container = device.EndOpenContainer(r);
            result.AsyncWaitHandle.Close();
            if (container.FileExists(playerFile))
            {
                Stream stream = container.OpenFile(playerFile, FileMode.Open);
                XmlSerializer serializer = new XmlSerializer(typeof(PlayerData));
                PlayerData data = (PlayerData)serializer.Deserialize(stream);
                stream.Close();
                container.Dispose();
                Global.hp = data.hitPoints;
                Global.maxHp = data.maxHitPoints;
                Global.str = data.str;
                Global.dex = data.dex;
                Global.vit = data.vit;
                Global.position = data.position;
                Global.levelIndex = data.levelIndex;
                Global.saveExists = true;
            }
            else
                Global.saveExists = false;
        }

        #endregion
    }
}
