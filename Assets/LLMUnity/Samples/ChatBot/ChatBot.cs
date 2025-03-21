using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using LLMUnity;
using UnityEngine.UI;

namespace LLMUnitySamples
{
    public class ChatBot : MonoBehaviour
    {
        public Transform chatContainer;
        public Color playerColor = new Color32(81, 164, 81, 255);
        public Color aiColor = new Color32(29, 29, 73, 255);
        public Color fontColor = Color.white;
        public Font font;
        public int fontSize = 16;
        public int bubbleWidth = 600;
        public LLMCharacter llmCharacter;
        public float textPadding = 10f;
        public float bubbleSpacing = 10f;
        public Sprite sprite;
        public Button stopButton;
        public Button recordButton; // ?? ?????? ?????

        private List<Bubble> chatBubbles = new List<Bubble>();
        private BubbleUI playerUI, aiUI;
        private bool warmUpDone = false;
        private int lastBubbleOutsideFOV = -1;
        void Start()
        {
            if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            playerUI = new BubbleUI
            {
                sprite = sprite,
                font = font,
                fontSize = fontSize,
                fontColor = fontColor,
                bubbleColor = playerColor,
                bottomPosition = 0,
                leftPosition = 0,
                textPadding = textPadding,
                bubbleOffset = bubbleSpacing,
                bubbleWidth = bubbleWidth,
                bubbleHeight = -1
            };
            aiUI = playerUI;
            aiUI.bubbleColor = aiColor;
            aiUI.leftPosition = 1;

            //inputBubble = new InputBubble(chatContainer, playerUI, "InputBubble", "Loading...", 4);
            //inputBubble.AddSubmitListener(onInputFieldSubmit);
            //inputBubble.AddValueChangedListener(onValueChanged);
            //inputBubble.setInteractable(false);
            stopButton.gameObject.SetActive(true);
            ShowLoadedMessages();
            _ = llmCharacter.Warmup(WarmUpCallback);
        }

        Bubble AddBubble(string message, bool isPlayerMessage)
        {
            Bubble bubble = new Bubble(chatContainer, isPlayerMessage? playerUI: aiUI, isPlayerMessage? "PlayerBubble": "AIBubble", message);
            chatBubbles.Add(bubble);
            bubble.OnResize(UpdateBubblePositions);
            return bubble;
        }

        void ShowLoadedMessages()
        {
            for (int i=1; i<llmCharacter.chat.Count; i++) AddBubble(llmCharacter.chat[i].content, i%2==1);
        }

        //void onInputFieldSubmit(string newText)
        //{
        //    inputBubble.ActivateInputField();
        //    if (blockInput || newText.Trim() == "" || Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        //    {
        //        StartCoroutine(BlockInteraction());
        //        return;
        //    }
        //    blockInput = true;
        //    // replace vertical_tab
        //    string message = inputBubble.GetText().Replace("\v", "\n");

        //    AddBubble(message, true);
        //    Bubble aiBubble = AddBubble("...", false);
        //    Task chatTask = llmCharacter.Chat(message, aiBubble.SetText, AllowInput);
        //    inputBubble.SetText("");
        //}

        public void WarmUpCallback()
        {
            warmUpDone = true;
            //inputBubble.SetPlaceHolderText("Message me");
            //AllowInput();
        }

        //public void AllowInput()
        //{
        //    blockInput = false;
        //    inputBubble.ReActivateInputField();
        //}

        public void CancelRequests()
        {
            llmCharacter.CancelRequests();
            //AllowInput();
        }

        //IEnumerator<string> BlockInteraction()
        //{
        //    // prevent from change until next frame
        //    inputBubble.setInteractable(false);
        //    yield return null;
        //    inputBubble.setInteractable(true);
        //    // change the caret position to the end of the text
        //    inputBubble.MoveTextEnd();
        //}

        //void onValueChanged(string newText)
        //{
        //    // Get rid of newline character added when we press enter
        //    if (Input.GetKey(KeyCode.Return))
        //    {
        //        if (inputBubble.GetText().Trim() == "")
        //            inputBubble.SetText("");
        //    }
        //}

        public void UpdateBubblePositions()
        {
            //float y = inputBubble.GetSize().y + inputBubble.GetRectTransform().offsetMin.y + bubbleSpacing;
            float y = bubbleSpacing;
            float containerHeight = chatContainer.GetComponent<RectTransform>().rect.height;
            for (int i = chatBubbles.Count - 1; i >= 0; i--)
            {
                Bubble bubble = chatBubbles[i];
                RectTransform childRect = bubble.GetRectTransform();
                childRect.anchoredPosition = new Vector2(childRect.anchoredPosition.x, y);

                // last bubble outside the container
                if (y > containerHeight && lastBubbleOutsideFOV == -1)
                {
                    lastBubbleOutsideFOV = i;
                }
                y += bubble.GetSize().y + bubbleSpacing;
            }
        }

        void Update()
        {
            //if (!inputBubble.inputFocused() && warmUpDone)
            //{
            //    inputBubble.ActivateInputField();
            //    StartCoroutine(BlockInteraction());
            //}
            if (lastBubbleOutsideFOV != -1)
            {
                // destroy bubbles outside the container
                for (int i = 0; i <= lastBubbleOutsideFOV; i++)
                {
                    chatBubbles[i].Destroy();
                }
                chatBubbles.RemoveRange(0, lastBubbleOutsideFOV + 1);
                lastBubbleOutsideFOV = -1;
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                recordButton.onClick.Invoke();
            }
        }
        public void ProcessVoiceInput(string text)
        {
            AddBubble(text, true);
            string response = GenerateStaticResponse(text); // ??????? GenerateStaticResponse.
            if (response != null)
            {
                AddBubble(response, false); // ????? ????????? ???????.
            }
            else
            {
                Bubble aiBubble = AddBubble("...", false);
                Task chatTask = llmCharacter.Chat(text, aiBubble.SetText, null); // ??????? LLM ??????? ??????.
            }
        }

        public string GenerateStaticResponse(string input)
        {
            input = input.ToLower();
            if (input.Contains("who are you"))
                return "I'm just a miserable, grumpy duck stuck in this desert. Why do you care?";
            else if (input.Contains("what are you doing"))
                return "Trying not to die of thirst and killing enemies to restore my own health. But if your health gets low, I guess I should share a bit of mine... owner's order -_-";
            else if (input.Contains("how can i beat the enemies"))
                return "Maybe stop being useless for a start. Try not to die too quickly, alright?";
            else
                return null; // ????? null ??? ?? ??? ?????? ?? ??????? ???????.
        }

        public void ExitGame()
        {
            Debug.Log("Exit button clicked");
            Application.Quit();
        }
     

        bool onValidateWarning = true;
        void OnValidate()
        {
            if (onValidateWarning && !llmCharacter.remote && llmCharacter.llm != null && llmCharacter.llm.model == "")
            {
                Debug.LogWarning($"Please select a model in the {llmCharacter.llm.gameObject.name} GameObject!");
                onValidateWarning = false;
            }
        }
    }
}
