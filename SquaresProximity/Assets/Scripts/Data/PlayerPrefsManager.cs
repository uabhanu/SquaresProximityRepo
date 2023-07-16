using UnityEngine;

namespace Data
{
    public static class PlayerPrefsManager
    {
        private static T Deserialize<T>(string serializedData)
        {
            try
            {
                return (T)System.Convert.ChangeType(serializedData , typeof(T));
            }
            catch
            {
                return default;
            }
        }
        
        private static string Serialize<T>(T data)
        {
            return data.ToString();
        }
        
        public static void DeleteAll()
        {
            PlayerPrefs.DeleteAll();
        }

        public static void LoadData<T>(ref T[] dataArray , string[] keys)
        {
            for(int i = 0; i < dataArray.Length; i++)
            {
                if(PlayerPrefs.HasKey(keys[i]))
                {
                    string serializedData = PlayerPrefs.GetString(keys[i]);
                    dataArray[i] = Deserialize<T>(serializedData);
                }
                else
                {
                    dataArray[i] = default;
                }
            }
        }

        public static void SaveData<T>(T[] dataArray , string[] keys)
        {
            for(int i = 0; i < dataArray.Length; i++)
            {
                string serializedData = Serialize(dataArray[i]);
                PlayerPrefs.SetString(keys[i] , serializedData);
            }

            PlayerPrefs.Save();
        }
    }
}