using UnityEngine;

namespace Data
{
    public static class PlayerPrefsManager
    {
        public static void LoadData(string[] playerNamesArray , int[] playersTotalWinsArray)
        {
            for(int i = 0; i < playerNamesArray.Length; i++)
            {
                playerNamesArray[i] = PlayerPrefs.GetString("Player " + i + " Name");
                playersTotalWinsArray[i] = PlayerPrefs.GetInt("Player " + i + " Total Wins");
            }
        }

        public static void DeleteAll(string[] playerNamesArray , int[] playersTotalWinsArray)
        {
            PlayerPrefs.DeleteAll();
        
            for(int i = 0; i < playerNamesArray.Length; i++)
            {
                playerNamesArray[i] = "Total Wins ";
                playersTotalWinsArray[i] = 0;
            }
        }

        public static void SaveData(string[] playerNamesArray , int[] playersTotalWinsArray)
        {
            for(int i = 0; i < playerNamesArray.Length; i++)
            {
                PlayerPrefs.SetString("Player " + i + " Name", playerNamesArray[i]);
                PlayerPrefs.SetInt("Player " + i + " Total Wins", playersTotalWinsArray[i]);
            }

            PlayerPrefs.Save();
        }
    }
}