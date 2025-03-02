using System;
using System.Net;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.CloudSave;
using System.Collections.Generic;
using Unity.VisualScripting;

class AuthInitialization : MonoBehaviour
{
    public static AuthInitialization instance;
    public User user ;
    public CloudSaveManager cloudSaveManager;
    public GameObject wheelInspactorPanel,registerPanel,loginPanel;
    public InputField usernameField;
    public InputField passwordField;
    public InputField usernameFieldLogin;
    public InputField passwordFieldLogin;
    public Text errorText;
    public Button loginButton;
    public string username;
    public string password;


    async void Awake()
    {
 
        try
        {
            await UnityServices.InitializeAsync();
            if(instance == null){
                instance = this;
                DontDestroyOnLoad(gameObject);
            }else{
                Destroy(gameObject);
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }

       
    }
    async void Start()
    {
        await SignInCachedUserAsync();
    }

    // Setup authentication event handlers if desired
    void SetupEvents()
    {
        AuthenticationService.Instance.SignedIn += () =>
        {
            // Shows how to get a playerID
            Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");

            // Shows how to get an access token
            Debug.Log($"Access Token: {AuthenticationService.Instance.AccessToken}");

        };

        AuthenticationService.Instance.SignInFailed += (err) =>
        {
            Debug.LogError(err);
        };

        AuthenticationService.Instance.SignedOut += () =>
        {
            Debug.Log("Player signed out.");
        };

        AuthenticationService.Instance.Expired += () =>
          {
              Debug.Log("Player session could not be refreshed and expired.");
          };
    }

    public async Task SignInCachedUserAsync()
    {
        // Check if a cached player already exists by checking if the session token exists
        if (!AuthenticationService.Instance.SessionTokenExists)
        {
            // if not, then do nothing
            print("No cached player found");
            loginPanel.SetActive(true);
            return;
        }

        // Sign in Anonymously
        // This call will sign in the cached player.
        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Sign in anonymously succeeded!");

            // Shows how to get the playerID
            Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");
            user = await cloudSaveManager.LoadUserData();
            try{
                FindAnyObjectByType<StartGame>().UpdateTextUI();
            }catch(Exception e){
                print(e.Message);
            }
            registerPanel.SetActive(false); 
            loginPanel.SetActive(false);
        }
        catch (AuthenticationException ex)
        {
            // Compare error code to AuthenticationErrorCodes
            // Notify the player with the proper error message
            if(ex.ErrorCode  == AuthenticationErrorCodes.ClientInvalidProfile){

            }
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message
            
            Debug.LogException(ex);
        }
    }

    public async void RegisterUser()
    {
        
        username = usernameField.text;
        password = passwordField.text;
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
           errorText.text = "Please enter username and password.";
            return;
        }

        try
        {
            // Treat username as email for Unity Authentication
            registerPanel.SetActive(false);
            wheelInspactorPanel.SetActive(true);
            await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(username, password);
           
            user = new User(
                AuthenticationService.Instance.PlayerId,
                username,
                1000,
                10

            );
            await cloudSaveManager.SaveUserData(user);
            user = await cloudSaveManager.LoadUserData();
            try{
                FindAnyObjectByType<StartGame>().UpdateTextUI();
            }catch(Exception e){
                print(e.Message);
            }
            wheelInspactorPanel.SetActive(false);
            Debug.Log("User registered with username (email): " + username);
          
        //   if(Application.internetReachability == NetworkReachability.NotReachable){
        //     errorText.text = "No internet connection";
        //     return;
        //   }
            
           
           
        }catch(Exception e){
            print(e.Message);
            registerPanel.SetActive(true);
            errorText.text = "Error: " + e.Message;
        }
        // catch (AuthenticationException e)
        // {
        //     errorText.text = "Error: " + e.Message;
        //     Debug.LogError("Sign-up failed: " + e.Message);

        // }
    }

      public async void loginUser()
    {   
        
        username = usernameFieldLogin.text;
        password = passwordFieldLogin.text;
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
           errorText.text = "Please enter username and password.";
            return;
        }

        try
        {
            // Treat username as email for Unity Authentication
            loginPanel.SetActive(false);
            wheelInspactorPanel.SetActive(true);
            await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(username, password);
            user = await cloudSaveManager.LoadUserData();
            try{
                FindAnyObjectByType<StartGame>().UpdateTextUI();
            }catch(Exception e){
                print(e.Message);
            }
            wheelInspactorPanel.SetActive(false);
          
            Debug.Log("User login with username (email): " + username);
          
        //   if(Application.internetReachability == NetworkReachability.NotReachable){
        //     errorText.text = "No internet connection";
        //     return;
        //   }
            
           
           
        }catch(Exception e){
            print(e.Message);
            loginPanel.SetActive(true);
            wheelInspactorPanel.SetActive(false);
            errorText.text = "Error: " + e.Message;
        }
        // catch (AuthenticationException e)
        // {
        //     errorText.text = "Error: " + e.Message;
        //     Debug.LogError("Sign-up failed: " + e.Message);

        // }
    }


     public  void LogoutUser()
    {

        AuthenticationService.Instance.SignOut(true);

        Debug.Log("User logged out.");
        loginPanel.SetActive(true);
    }

     public static async void SaveCoins(int coins)
    {
        try
        {
            Dictionary<string, object> data = new Dictionary<string, object>
            {
                { "coin", coins }
            };
            await CloudSaveService.Instance.Data.ForceSaveAsync(data);
            Debug.Log($"Coins saved for {AuthenticationService.Instance.PlayerId}: {coins}");
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to save coins: " + e.Message);
        }
    }


    public static async Task<int> LoadCoins()
    {
        var playerData = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { "userCoins" });
        if (playerData.TryGetValue("coin", out var keyName))
        {
            Debug.Log($"keyName: {keyName.Value.GetAs<string>()}");
            return keyName.Value.GetAs<int>();
        }
        return 0;
    }


}