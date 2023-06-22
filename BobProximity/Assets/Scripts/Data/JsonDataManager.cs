using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using UnityEngine;

namespace Data
{
    public static class JsonDataManager
    {
        public static void SaveData<T>(T data , string relativeFilePath)
        {
            string absoluteFilePath = Path.Combine(Application.dataPath , relativeFilePath);
            var serializer = new DataContractJsonSerializer(typeof(T));
            using var stream = File.Open(absoluteFilePath , FileMode.Create);
            serializer.WriteObject(stream , data);
            Console.WriteLine("Data saved successfully.");
        }

        public static T LoadData<T>(string relativeFilePath)
        {
            string absoluteFilePath = Path.Combine(Application.dataPath , relativeFilePath);

            if(!File.Exists(absoluteFilePath))
            {
                Console.WriteLine("File does not exist.");
                return default;
            }

            var serializer = new DataContractJsonSerializer(typeof(T));

            using var stream = File.Open(absoluteFilePath , FileMode.Open);
            
            try
            {
                return (T)serializer.ReadObject(stream);
            }
            catch(SerializationException ex)
            {
                Console.WriteLine("Error deserializing data: " + ex.Message);
                return default;
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
        
        public static void DeleteFile(string relativeFilePath)
        {
            string absoluteFilePath = Path.Combine(Application.dataPath , relativeFilePath);

            if(File.Exists(absoluteFilePath))
            {
                File.Delete(absoluteFilePath);
                Debug.Log("File deleted successfully.");
            }
            else
            {
                Debug.LogWarning("File not found Bhanu :( " + absoluteFilePath);
            }
        }
    }
}