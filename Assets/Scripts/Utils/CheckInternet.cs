using System;
using System.Collections;
using System.Collections.Generic;
using PimDeWitte.UnityMainThreadDispatcher;
using UnityEngine;
using UnityEngine.Networking;

public class CheckInternet : MonoBehaviour
{
    private string url = "https://www.google.com";
    private float timeout = 3f;

    private static CheckInternet instance = null;
    private static bool isRunning = false;
    static Action<bool> SaveActions;

    public static void CheckInternetConnection(Action<bool> OnComplete)
    {
        if (isRunning)
        {
            SaveActions += OnComplete;
            return;
        }
        isRunning = true;
        SaveActions = OnComplete;

        if (instance == null)
        {
            GameObject tempObj = new GameObject("CheckInternet");
            instance = tempObj.AddComponent<CheckInternet>();
            DontDestroyOnLoad(tempObj);
        }
        instance.RunCheck(OnComplete);
    }

    private void RunCheck(Action<bool> OnComplete)
    {
        StartCoroutine(RunCheckInternetConnection(OnComplete));
    }

    private IEnumerator RunCheckInternetConnection(Action<bool> OnComplete)
    {
        UnityWebRequest request = UnityWebRequest.Get(url);
        float timer = 0;

        // Set the download handler to discard the response data
        request.downloadHandler = new DownloadHandlerBuffer();

        // Send the request asynchronously
        UnityEngine.AsyncOperation operation = request.SendWebRequest();

        // Wait until the request is done or the timeout is reached
        while (!operation.isDone && timer < timeout)
        {
            yield return null; // Wait for the next frame
            timer += Globals.frameTime;//  (Time.deltaTime - (float)pauseTimeToRemove);
        }

        // Check if the request timed out
        if (timer >= timeout)
        {
            Debug.Log("Internet connection timed out.");
            SaveActions?.Invoke(false);
        }
        else
        {
            // Check if the request was successful
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Internet is working.");
                SaveActions?.Invoke(true);
            }
            else
            {
                Debug.Log("Internet is not working.");
                SaveActions?.Invoke(false);
            }
        }
        isRunning = false;

        // Clean up the request
        request.Dispose();
    }
}
