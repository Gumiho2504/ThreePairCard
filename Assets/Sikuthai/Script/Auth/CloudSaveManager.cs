using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using Unity.Services.Core;
using UnityEngine;

public class CloudSaveManager : MonoBehaviour
{
    private  void Start()
    {
        //await InitializeUnityServices();
    }

    private async Task InitializeUnityServices()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            await UnityServices.InitializeAsync();
        }

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Signed in Anonymously: " + AuthenticationService.Instance.PlayerId);
        }
    }

    /// <summary>
    /// Saves user data to Unity Cloud Save.
    /// </summary>
    public async Task SaveUserData(User user)
    {
        try
        {
            var dataToSave = new Dictionary<string, object>
            {
                { "user_id", user.id },
                { "user_name", user.name },
                { "user_coin", user.coin },
                { "user_gem", user.gem }
            };

            await CloudSaveService.Instance.Data.ForceSaveAsync(dataToSave);
            Debug.Log("User data saved successfully!");
        }
        catch (Exception e)
        {
            Debug.LogError("Error saving user data: " + e.Message);
        }
    }

    
    public async Task<User> LoadUserData()
    {
        try
        {
            var savedData = await CloudSaveService.Instance.Data.LoadAsync(new HashSet<string>
            {
                "user_id",
                "user_name",
                "user_coin",
                "user_gem"
            });

            string id = savedData.ContainsKey("user_id") ? savedData["user_id"].ToString() : "default_id";
            string name = savedData.ContainsKey("user_name") ? savedData["user_name"].ToString() : "Guest";
            int coin = savedData.ContainsKey("user_coin") ? Convert.ToInt32(savedData["user_coin"]) : 0;
            int gem = savedData.ContainsKey("user_gem") ? Convert.ToInt32(savedData["user_gem"]) : 0;

            Debug.Log($"User Data Loaded - ID: {id}, Name: {name}, Coins: {coin}, Gems: {gem}");

            return new User(id, name, coin, gem);
        }
        catch (Exception e)
        {
            Debug.LogError("Error loading user data: " + e.Message);
            return new User("default_id", "Guest", 0, 0); // Default data
        }
    }

     public  async void SaveCoins(int coins)
    {
        try
        {
            Dictionary<string, object> data = new Dictionary<string, object>
            {
                { "user_coin", coins }
            };
            await CloudSaveService.Instance.Data.ForceSaveAsync(data);
            Debug.Log($"Coins saved for {AuthenticationService.Instance.PlayerId}: {coins}");
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to save coins: " + e.Message);
        }
    }
}


