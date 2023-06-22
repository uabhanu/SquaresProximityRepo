using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using UnityEngine;

namespace Data
{
    public static class JsonDataManager
    {
        public static void SaveData<T>(T data , string filePath)
        {
            var serializer = new DataContractJsonSerializer(typeof(T));

            using(var stream = File.Open(filePath , FileMode.Create))
            {
                serializer.WriteObject(stream , data);
            }
        }

        public static T LoadData<T>(string filePath)
        {
            var serializer = new DataContractJsonSerializer(typeof(T));
        
            using(var stream = File.Open(filePath , FileMode.Open))
            {
                return (T)serializer.ReadObject(stream);
            }
        }

        public static string SerializeData<T>(T data)
        {
            var serializer = new DataContractJsonSerializer(typeof(T));
        
            using (var memoryStream = new MemoryStream())
            {
                serializer.WriteObject(memoryStream , data);
                return Encoding.UTF8.GetString(memoryStream.ToArray());
            }
        }

        public static T DeserializeData<T>(string jsonData)
        {
            var serializer = new DataContractJsonSerializer(typeof(T));
        
            using(var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(jsonData)))
            {
                return (T)serializer.ReadObject(memoryStream);
            }
        }
        
        public static void DeleteFile(string filePath)
        {
            if(File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            else
            {
                Debug.LogError("File not found Bhanu :( : " + filePath);
            }
        }
    }
}