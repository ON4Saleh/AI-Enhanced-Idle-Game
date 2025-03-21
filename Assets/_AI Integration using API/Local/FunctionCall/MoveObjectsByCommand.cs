using System.Collections.Generic;
using System.Reflection;
using LLMUnity;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Threading.Tasks;

public class MoveObjectsByCommand : MonoBehaviour
{
    public LLMCharacter llmCharacter;
    public RectTransform blueSquare;
    public RectTransform redSquare;
    public PlayerSkills playerSkills;
    public RectTransform moveableObject;
    public GameObject targetEnemy;

    void Start()
    {
        InvokeRepeating("AutoAttack", 0, 1f);
    }

    private void AutoAttack()
    {
        if (targetEnemy != null)
        {
            Debug.Log("Attacking enemy: " + targetEnemy.name);
            PAttack attackScript = moveableObject.GetComponent<PAttack>();
            if (attackScript != null)
            {
                attackScript.StartAttacking();
            }
        }
    }

    string[] GetFunctionNames<T>()
    {
        List<string> functionNames = new List<string>();
        foreach (var function in typeof(T).GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly))
            functionNames.Add(function.Name);
        return functionNames.ToArray();
    }

    string ConstructDirectionPrompt(string message)
    {
        string prompt = "From the input, which direction is mentioned? Choose from the following options:\n\n";
        prompt += "Input:" + message + "\n\n";
        prompt += "Choices:\n";
        foreach (string functionName in GetFunctionNames<DirectionFunctions>())
            prompt += $"- {functionName}\n";
        prompt += "\nAnswer directly with the choice, focusing only on direction";
        return prompt;
    }

    string ConstructColorPrompt(string message)
    {
        string prompt = "From the input, which color is mentioned? Choose from the following options:\n\n";
        prompt += "Input:" + message + "\n\n";
        prompt += "Choices:\n";
        foreach (string functionName in GetFunctionNames<ColorFunctions>())
            prompt += $"- {functionName}\n";
        prompt += "\nAnswer directly with the choice, focusing only on color";
        return prompt;
    }

    public async Task<bool> onInputFieldSubmit(string message, Action callback = null)
    {
        if (playerSkills == null)
        {
            playerSkills = FindFirstObjectByType<PlayerSkills>();
        }

        bool commandProcessed = await ProcessCommand(message);

        string getDirection = await llmCharacter.Chat(ConstructDirectionPrompt(message));

        Vector3 direction = (Vector3)typeof(DirectionFunctions).GetMethod(getDirection).Invoke(null, null);

        Debug.Log($"Direction function called: {getDirection}, returned: {direction}");

        if (callback != null)
        {
            if (commandProcessed)
            {
                callback.Invoke();
            }
        }

        if (moveableObject != null)
        {
            moveableObject.anchoredPosition += (Vector2)direction * 100f;
        }

        return commandProcessed;
    }
    private async Task<bool> ProcessCommand(string command)
    {
        if (command.Contains("Stop") || command.Contains("Cancel"))
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                PAttack attackScript = playerObject.GetComponent<PAttack>();
                if (attackScript != null)
                {
                    attackScript.StopAttacking();
                    return true;
                }
                else
                {
                    Debug.LogError("PAttack script not found on Player!");
                    return false;
                }
            }
            else
            {
                Debug.LogError("Player object not found with tag 'Player'!");
                return false;
            }
        }
        else if (command.Contains("Shield"))
        {
            if (playerSkills != null)
            {
                playerSkills.UseShieldSkill();
                return true;
            }
            else
            {
                Debug.LogError("PlayerSkills script not found!");
                return false;
            }
        }
        else if (command.Contains("Target first enemy"))
        {
            targetEnemy = GameObject.FindGameObjectWithTag("Enemy");
            if (targetEnemy != null)
            {
                moveableObject.anchoredPosition += Vector2.up * 100f;
                return true;
            }
            else
            {
                Debug.LogError("Enemy object not found with tag 'Enemy'!");
                return false;
            }

        }
        else if (command.Contains("Target second enemy"))
        {
            targetEnemy = GameObject.FindGameObjectWithTag("Enemy2");
            if (targetEnemy != null)
            {
                moveableObject.anchoredPosition += Vector2.down * 100f;
                return true;
            }
            else
            {
                Debug.LogError("Enemy object not found with tag 'Enemy2'!");
                return false;
            }
        }
        else if (command.Contains("blue") || command.Contains("red"))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void CancelRequests()
    {
        llmCharacter.CancelRequests();
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