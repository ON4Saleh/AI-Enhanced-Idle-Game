using UnityEngine;
using System.Threading.Tasks; 
using LLMUnitySamples;

public class VoiceCommandRecognizer : MonoBehaviour
{
    public VoiceRecorder voiceRecorder;
    public MoveObjectsByCommand moveObjectsByCommand;
    public ChatBot chatBot;

    public async void ProcessVoiceCommand(string text) 
    {
        bool commandHandled = await moveObjectsByCommand.onInputFieldSubmit(text, null); 

        if (!commandHandled && chatBot != null)
        {
            chatBot.ProcessVoiceInput(text);
        }
        else if (chatBot == null)
        {
            Debug.LogError("ChatBot script not found!");
        }
    }

    public void OnRecordSendButtonClick()
    {
        voiceRecorder.OnRecordSendButtonClick();
    }
}