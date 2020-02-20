using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class JsonTests : MonoBehaviour
{
    private const string httpServer = "https://localhost:44392";

    public InputField jsonInputField1;
    public InputField jsonInputField2;
    public Text debugConsoleText;

    public void ReceiveJsonString()
    {
        UnityWebRequest httpClient = new UnityWebRequest(httpServer + "/api/Values/GetString", "GET");
        httpClient.downloadHandler = new DownloadHandlerBuffer();
        //httpClient.SetRequestHeader("Content-Type", "application/json");
        httpClient.SetRequestHeader("Accept", "application/json");
        httpClient.SendWebRequest();
        while(!httpClient.isDone)
        {
            Task.Delay(1);
        }
        if (httpClient.isHttpError || httpClient.isNetworkError)
        {
            throw new Exception("JsonTests > ReceiveJsonString: " + httpClient.error);
        }

        string jsonResponse = httpClient.downloadHandler.text;


        //string response = JsonUtility.FromJson<string>(jsonResponse);
        // ArgumentException: JSON must represent an object type.
        // To avoid this error we must define a class with only 1 string.
        // This is not very elegant.

        // Better & quicker solution:
        string response = jsonResponse.Replace("\"", "");
        
        debugConsoleText.text += "\nJsonTests > ReceiveJsonString: \n\t" + jsonResponse;
        debugConsoleText.text += "\nJsonTests > ReceiveJsonString: \n\t" + response;
    }

    public void ReceiveJsonListOfString()
    {
        UnityWebRequest httpClient = new UnityWebRequest(httpServer + "/api/Values/GetListOfString", "GET");
        httpClient.downloadHandler = new DownloadHandlerBuffer();
        //httpClient.SetRequestHeader("Content-Type", "application/json");
        httpClient.SetRequestHeader("Accept", "application/json");
        httpClient.SendWebRequest();
        while (!httpClient.isDone)
        {
            Task.Delay(1);
        }
        if (httpClient.isHttpError || httpClient.isNetworkError)
        {
            throw new Exception("ReceiveJsonListString: " + httpClient.error);
        }

        string jsonResponse = httpClient.downloadHandler.text;
        // {["Hello json","Happy St. Vals day <3"]}

        //List<string> listOfString = JsonUtility.FromJson<List<string>>(jsonResponse);
        // ArgumentException: JSON must represent an object type.

        //string response = "{" + jsonResponse + "}";
        //List<string> listOfString = JsonUtility.FromJson<List<string>>(response);
        // ArgumentException: JSON parse error: Missing a name for object member.
        // {"myStrings":["Hello json","Happy St. Vals day <3"]}

        //string response = "{\"myStrings\":" + jsonResponse + "}";
        //List<string> listOfString = JsonUtility.FromJson<List<string>>(response);
        // listOfString is empty; Count = 0


        // Solution:
        string response = "{\"myStrings\":" + jsonResponse + "}";
        ListOfStringModel listOfString =
            JsonUtility.FromJson<ListOfStringModel>(response);  // Deserialize object

        debugConsoleText.text += "\nJsonTests > ReceiveJsonListOfString: ";
        foreach (string str in listOfString.myStrings)
        {
            debugConsoleText.text += "\n\t" + str;
        }
        
    }

    public void SendJsonString()
    {
        UnityWebRequest httpClient = new UnityWebRequest(httpServer + "/api/Values/PostString", "POST");
        //httpClient.downloadHandler = new DownloadHandlerBuffer();
        string stringToSend = jsonInputField1.text;
        
        //
        //string jsonString = JsonUtility.ToJson(stringToSend); // Serialize object
        //byte[] data = Encoding.UTF8.GetBytes(jsonString);
        // JsonUtility.ToJson serializes a single string as an empty json object. Bad.
        
        // To avoid that, when sending a string (only a string) do:

        byte[] data = Encoding.UTF8.GetBytes("\"" + stringToSend + "\"");
        httpClient.uploadHandler = new UploadHandlerRaw(data);
        
        httpClient.SetRequestHeader("Content-Type", "application/json");
        //httpClient.SetRequestHeader("Accept", "application/json");
        httpClient.SendWebRequest();
        while (!httpClient.isDone)
        {
            Task.Delay(1);
        }
        if (httpClient.isHttpError || httpClient.isNetworkError)
        {
            throw new Exception("JsonTests > SendJsonString: " + httpClient.error);
        }

        debugConsoleText.text += "\nJsonTests > SendJsonString: " + httpClient.responseCode;
    }

    public void SendJsonListOfString()
    {
        UnityWebRequest httpClient = new UnityWebRequest(httpServer + "/api/Values/PostListOfString", "POST");
        //httpClient.downloadHandler = new DownloadHandlerBuffer();

        ListOfStringModel listOfString = new ListOfStringModel();
        listOfString.myStrings.Add(jsonInputField1.text);
        listOfString.myStrings.Add(jsonInputField2.text);

        
        string jsonString = JsonUtility.ToJson(listOfString); // Serialize object
        byte[] data = Encoding.UTF8.GetBytes(jsonString);

        httpClient.uploadHandler = new UploadHandlerRaw(data);

        httpClient.SetRequestHeader("Content-Type", "application/json");
        //httpClient.SetRequestHeader("Accept", "application/json");
        httpClient.SendWebRequest(); // ERROR: API receives null. ???
        while (!httpClient.isDone)
        {
            Task.Delay(1);
        }
        if (httpClient.isHttpError || httpClient.isNetworkError)
        {
            throw new Exception("JsonTests > SendJsonString: " + httpClient.error);
        }

        debugConsoleText.text += "\nJsonTests > SendJsonString: " + httpClient.responseCode;
    }
}
