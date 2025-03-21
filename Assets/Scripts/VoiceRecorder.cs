using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Newtonsoft.Json;
using LLMUnitySamples;
using UnityEditor;

[Serializable]
public class WhisperResponse
{
    public string text;
}

public class VoiceRecorder : MonoBehaviour
{
    public AudioSource audioSource;
    public Text responseText;
    public MoveObjectsByCommand moveObjectsByCommand;
    private string micName;
    private bool isRecording = false;
    private AudioClip recordedClip;
    private string filePath;
    private enum RecordingState { Idle, Recording, Stopped }
    private RecordingState currentState = RecordingState.Idle;
    public Button recordSendButton;
    private const string API_URL = "https://api-inference.huggingface.co/models/openai/whisper-large-v3-turbo";
    private const string API_KEY = "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";
    private bool commandHandled = false;
    public VoiceCommandRecognizer voiceCommandRecognizer;
    void Start()
    {
        if (Microphone.devices.Length > 0)
        {
            micName = Microphone.devices[0];
            Debug.Log("Microphone found: " + micName);
        }
        else
        {
            Debug.LogError("No microphone found!");
        }

        filePath = Path.Combine(Application.persistentDataPath, "RecordedAudio.wav");
    }
    public void OnRecordSendButtonClick()
    {
        if (currentState == RecordingState.Idle)
        {
            StartRecording();
        }
        else
        {
            StopRecordingAndSend();
        }
    }
    public void StartRecording()
    {
        if (currentState == RecordingState.Idle)
        {
            isRecording = true;
            recordedClip = Microphone.Start(micName, false, 10, 44100);
            Debug.Log("Recording started...");
            currentState = RecordingState.Recording;
            if (recordSendButton != null && recordSendButton.GetComponentInChildren<Text>() != null)
                recordSendButton.GetComponentInChildren<Text>().text = "Send"; 
        }
    }

    public void StopRecordingAndSend()
    {
        if (currentState == RecordingState.Recording)
        {
            isRecording = false;
            Microphone.End(micName);
            audioSource.clip = recordedClip;

            Debug.Log($"Recording stopped. Samples: {recordedClip.samples}, Channels: {recordedClip.channels}");

            SaveRecording();
            currentState = RecordingState.Stopped;
            SendToWhisper();

            if (recordSendButton != null && recordSendButton.GetComponentInChildren<Text>() != null)
                recordSendButton.GetComponentInChildren<Text>().text = "Record"; 
            currentState = RecordingState.Idle;
        }
    }


    private void SaveRecording()
    {
        if (recordedClip != null)
        {
            byte[] wavData = ConvertAudioClipToWav(recordedClip);
            File.WriteAllBytes(filePath, wavData);
            Debug.Log("Saved at: " + filePath);
        }
        else
        {
            Debug.LogError("No recorded audio to save!");
        }
    }

    public void PlayRecording()
    {
        if (audioSource.clip != null)
        {
            audioSource.Play();
            Debug.Log("Playing recording...");
        }
    }

    public async void SendToWhisper()
    {
        if (currentState == RecordingState.Stopped && recordedClip != null)
        {
            if (recordedClip == null)
            {
                Debug.LogError("No recording available!");
                return;
            }

            SaveRecording();
            Debug.Log("Saved Recording at: " + filePath);

            string resultText = await SendAudioToWhisper(filePath);

            if (responseText != null)
                responseText.text = resultText;

            if (!string.IsNullOrEmpty(resultText))
            {
                if (voiceCommandRecognizer == null)
                {
                    voiceCommandRecognizer = FindFirstObjectByType<VoiceCommandRecognizer>(); 
                }

                if (voiceCommandRecognizer != null)
                {
                    voiceCommandRecognizer.ProcessVoiceCommand(resultText);
                }
            }
            else
            {
                Debug.LogError("Whisper API returned an empty or invalid response.");
            }
            recordedClip = null;
            currentState = RecordingState.Idle;
        }
        else
        {
            Debug.LogError("No recording available or incorrect state.");
        }
    }
    private byte[] ConvertAudioClipToWav(AudioClip clip)
    {
        int sampleCount = clip.samples * clip.channels;
        float[] samples = new float[sampleCount];
        clip.GetData(samples, 0);

        using MemoryStream stream = new MemoryStream();
        using BinaryWriter writer = new BinaryWriter(stream);

        writer.Write(System.Text.Encoding.UTF8.GetBytes("RIFF"));
        writer.Write(36 + samples.Length * 2);
        writer.Write(System.Text.Encoding.UTF8.GetBytes("WAVE"));

        writer.Write(System.Text.Encoding.UTF8.GetBytes("fmt "));
        writer.Write(16);
        writer.Write((short)1);
        writer.Write((short)clip.channels);
        writer.Write(clip.frequency);
        writer.Write(clip.frequency * clip.channels * 2);
        writer.Write((short)(clip.channels * 2));
        writer.Write((short)16);

        writer.Write(System.Text.Encoding.UTF8.GetBytes("data"));
        writer.Write(samples.Length * 2);

        foreach (var sample in samples)
        {
            short intSample = (short)(sample * short.MaxValue);
            writer.Write(intSample);
        }

        return stream.ToArray();
    }

    private async Task<string> SendAudioToWhisper(string audioFilePath)
    {
        Debug.Log("Sending file to Whisper API...");

        byte[] audioData = File.ReadAllBytes(audioFilePath);
        string resultText = "No response";

        using (UnityWebRequest request = new UnityWebRequest(API_URL, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(audioData);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "audio/wav");
            request.SetRequestHeader("Authorization", $"Bearer {API_KEY}");

            var operation = request.SendWebRequest();

            while (!operation.isDone)
                await Task.Yield();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;
                Debug.Log("Raw JSON Response: " + jsonResponse);

                try
                {
                    WhisperResponse response = JsonConvert.DeserializeObject<WhisperResponse>(jsonResponse);

                    if (response != null && !string.IsNullOrEmpty(response.text))
                    {
                        resultText = response.text.Trim();
                    }
                    else
                    {
                        Debug.LogError("Unexpected JSON structure: " + jsonResponse);
                        resultText = "Invalid response format.";
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("Failed to parse JSON response: " + e.Message);
                    resultText = "Error parsing response.";
                }
            }
            else
            {
                Debug.LogError($"Request failed: {request.error}\nResponse: {request.downloadHandler.text}");
                resultText = "Request failed.";
            }
        }

        return resultText;
    }
}
