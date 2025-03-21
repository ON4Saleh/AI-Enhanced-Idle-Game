using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections.Generic;
using Newtonsoft.Json; 
[Serializable]
public class HuggingFaceRequest
{
    public string inputs;
}

[Serializable]
public class HuggingFaceResponse
{
    public string generated_text;
}

public class HuggingAPI : MonoBehaviour
{
    [SerializeField] private Text _responseText;
    [SerializeField] private Button _fetchResponseButton;

    private const string API_URL = "https://api-inference.huggingface.co/models/google/gemma-2-2b-it";
    private const string API_KEY = "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";

    private void Awake()
    {
        _fetchResponseButton.onClick.AddListener(OnButtonClick_FetchResponse);
    }

    private void OnDestroy()
    {
        _fetchResponseButton.onClick.RemoveListener(OnButtonClick_FetchResponse);
    }

    public async Task FetchAIResponse(string inputText)
    {
        try
        {
            _fetchResponseButton.interactable = false;
            //HuggingFaceRequest requestData = new HuggingFaceRequest { inputs = "tell me the advantages of zucchini." };
            HuggingFaceRequest requestData = new HuggingFaceRequest { inputs = inputText };
            string jsonPayload = JsonUtility.ToJson(requestData);

            using UnityWebRequest request = new UnityWebRequest(API_URL, "POST")
            {
                uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonPayload)),
                downloadHandler = new DownloadHandlerBuffer()
            };
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {API_KEY}");

            var operation = request.SendWebRequest();

            while (!operation.isDone)
                await Task.Yield();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error: {request.error}");
                _responseText.text = "Failed to get response";
                return;
            }

            string jsonResponse = request.downloadHandler.text;

            HuggingFaceResponse[] responses = JsonConvert.DeserializeObject<HuggingFaceResponse[]>(jsonResponse);
            _responseText.text = responses.Length > 0 ? responses[0].generated_text : "No response received";
        }
        catch (Exception e)
        {
            Debug.LogError($"Error fetching response: {e.Message}");
            _responseText.text = "Failed to fetch response";
        }
        finally
        {
            _fetchResponseButton.interactable = true;
            Debug.Log("AI Model Response: " + _responseText.text);

        }
    }

    //public void OnButtonClick_FetchResponse()
    //{
    //    Debug.Log($"_responseText is {(_responseText == null ? "null" : "valid")}");
    //    _ = FetchAIResponse();
    //}
    public void OnButtonClick_FetchResponse()
    {
        FindFirstObjectByType<VoiceRecorder>().SendToWhisper();
    }

}