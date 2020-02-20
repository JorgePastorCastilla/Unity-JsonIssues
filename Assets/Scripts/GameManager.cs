using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private const string httpServer = "http://localhost:51747";
    // cached references
    public Text idText;
    public InputField emailInputField;
    public InputField passwordInputField;
    public InputField confirmPasswordInputField;
    public InputField firstNameInputField;
    public InputField lastNameInputField;
    public InputField nickNameInputField;
    public InputField cityInputField;
    public InputField dateOfBirthInputField;
    public Text dateJoinedText;
    public Text debugConsoleText;

    public PlayerSerializable player;
    private string token;

    // Start is called before the first frame update
    void Start()
    {
        emailInputField.text = "a1@b.c";
        passwordInputField.text = "Secret_20";
        confirmPasswordInputField.text = "Secret_20";
        firstNameInputField.text = "Edy";
        lastNameInputField.text = "On3";
        nickNameInputField.text = "edyone";
        cityInputField.text = "Palma";
    }

    public void RegisterButtonClickedAPI()
    {
        StartCoroutine(RegisterNewPlayer());
    }

    private IEnumerator RegisterNewPlayer()
    {
        if (passwordInputField.text == confirmPasswordInputField.text)
        {
            if (!string.IsNullOrEmpty(emailInputField.text))
            {
                yield return RegisterAspNetUser();
                yield return GetAuthenticationToken();
                yield return GetAspNetUserId();
                yield return InsertPlayer();
                yield return GetPlayerDateJoined();
                GetPlayerAge();
            }
        }
    }


    private IEnumerator RegisterAspNetUser()
    {
        AspNetUserModel aspPlayer = new AspNetUserModel();
        aspPlayer.Email = emailInputField.text;
        aspPlayer.Password = passwordInputField.text;
        aspPlayer.ConfirmPassword = confirmPasswordInputField.text;

        using (UnityWebRequest httpClient = new UnityWebRequest(httpServer + "/api/Account/Register", "POST"))
        {
            string bodyJson = JsonUtility.ToJson(aspPlayer);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(bodyJson);
            httpClient.uploadHandler = new UploadHandlerRaw(bodyRaw);
            httpClient.SetRequestHeader("Content-type", "application/json");

            yield return httpClient.SendWebRequest();

            if (httpClient.isNetworkError || httpClient.isHttpError)
            {
                throw new Exception("RegisterNewPlayer > RegisterAspNetUser: " + httpClient.error);
            }
            else
            {
                Debug.Log("RegisterNewPlayer > RegisterAspNetUser: " + httpClient.responseCode);
            }
        }

    }

    private IEnumerator GetAuthenticationToken()
    {
        WWWForm data = new WWWForm();
        data.AddField("grant_type", "password");
        data.AddField("username", emailInputField.text);
        data.AddField("password", passwordInputField.text);
        // The Content-Type header will be set to application/x-www-form-urlencoded by default.
        using (UnityWebRequest httpClient = UnityWebRequest.Post(httpServer + "/Token", data))
        {
            yield return httpClient.SendWebRequest();  // Return control to LoginButtonClickedAPI since web request returns

            if (httpClient.isNetworkError || httpClient.isHttpError)
            {
                throw new System.Exception("RegisterNewPlayer > GetAuthenticationToken: " + httpClient.error);
            }
            else
            {
                string jsonResponse = httpClient.downloadHandler.text;
                AuthToken authToken = JsonUtility.FromJson<AuthToken>(jsonResponse);
                token = authToken.access_token;
            }
        }
    }

    private IEnumerator GetAspNetUserId()
    {
        using (UnityWebRequest httpClient = new UnityWebRequest(httpServer + "/api/Account/UserId", "GET"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes("Nothing");
            httpClient.uploadHandler = new UploadHandlerRaw(bodyRaw);

            httpClient.downloadHandler = new DownloadHandlerBuffer();

            httpClient.SetRequestHeader("Accept", "application/json");
            httpClient.SetRequestHeader("Authorization", "bearer " + token);

            yield return httpClient.SendWebRequest();

            if (httpClient.isNetworkError || httpClient.isHttpError)
            {
                throw new Exception("RegisterNewPlayer > GetAspNetUserId: " + httpClient.error);
            }
            else
            {
                player.Id = httpClient.downloadHandler.text.Replace("\"", "");
                idText.text = player.Id;
            }
        }
    }

    private IEnumerator InsertPlayer()
    {
        player.Email = emailInputField.text;
        player.FirstName = firstNameInputField.text;
        player.LastName = lastNameInputField.text;
        player.NickName = nickNameInputField.text;
        player.City = cityInputField.text;
        player.DateOfBirth = dateOfBirthInputField.text;
        player.BlobUri = "https://spdvistorage.blob.core.windows.net/clickycrates-blobs/custom/Annotation%202019-11-18%20232554.png";

        using (UnityWebRequest httpClient = new UnityWebRequest(httpServer + "/api/Player/InsertNewPlayer", "POST"))
        {
            //string playerData = JsonUtility.ToJson(new PlayerInfo(player));
            string playerData = JsonUtility.ToJson(player);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(playerData);
            httpClient.uploadHandler = new UploadHandlerRaw(bodyRaw);
            httpClient.downloadHandler = new DownloadHandlerBuffer();
            httpClient.SetRequestHeader("Content-type", "application/json");
            httpClient.SetRequestHeader("Authorization", "bearer " + token);

            yield return httpClient.SendWebRequest();

            if (httpClient.isNetworkError || httpClient.isHttpError)
            {
                throw new Exception("RegisterNewPlayer > InsertPlayer: " + httpClient.error);
            }
            else
            {
                Debug.Log("RegisterNewPlayer > InsertPlayer: " + httpClient.responseCode);
            }
        }
    }

    private void GetPlayerAge()
    {
        DateTime birthday = DateTime.Parse(player.DateOfBirth);
        DateTime today = DateTime.Now;
        TimeSpan difference = today - birthday;
        debugConsoleText.text += "\nPlayer is " + difference.Days / 365 + " years and " + difference.Days%365 + " days old";
    }

    private IEnumerator GetPlayerDateJoined()
    {
        using (UnityWebRequest httpClient = new UnityWebRequest(httpServer + "/api/Player/GetPlayerDateJoined", "GET"))
        {
            httpClient.downloadHandler = new DownloadHandlerBuffer();

            httpClient.SetRequestHeader("Accept", "application/json");
            httpClient.SetRequestHeader("Authorization", "bearer " + token);

            yield return httpClient.SendWebRequest();

            if (httpClient.isNetworkError || httpClient.isHttpError)
            {
                throw new Exception("RegisterNewPlayer > GetPlayerDateJoined: " + httpClient.error);
            }
            else
            {
                player.DateJoined = httpClient.downloadHandler.text.Replace("\"", "");
                dateJoinedText.text = player.DateJoined;
            }
        }
    }

}