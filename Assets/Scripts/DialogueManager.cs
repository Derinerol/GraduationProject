
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("Dialogue UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text npcNameText;
    [SerializeField] private TMP_Text dialogueText;

    [Header("Dynamic Choice Buttons")]
    [SerializeField] private Transform choiceContainer;
    [SerializeField] private Button choiceButtonPrefab;

    [Header("Close Button")]
    [SerializeField] private Button closeButton;

    private readonly List<Button> spawnedChoiceButtons =
        new List<Button>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        dialoguePanel.SetActive(false);

        closeButton.onClick.AddListener(CloseDialogue);
    }

    // New dynamic dialogue method.
    public void ShowDialogue(
        string npcName,
        string openingLine,
        List<DialogueChoice> choices)
    {
        ClearChoiceButtons();

        npcNameText.text = npcName;
        dialogueText.text = openingLine;

        foreach (DialogueChoice choice in choices)
        {
            Button button = Instantiate(
                choiceButtonPrefab,
                choiceContainer
            );

            TMP_Text buttonText =
                button.GetComponentInChildren<TMP_Text>(true);

            if (buttonText != null)
            {
                buttonText.text = choice.Text;
            }
            else
            {
                Debug.LogError(
                    "Dialogue choice prefab has no TMP text component."
                );
            }

            button.onClick.AddListener(() =>
            {
                choice.OnSelected?.Invoke();
                ShowResponse(choice.Response);
            });

            spawnedChoiceButtons.Add(button);
        }

        closeButton.gameObject.SetActive(true);
        dialoguePanel.SetActive(true);
    }

    // Compatibility method for existing Interactable.cs calls.
    public void ShowDialogue(
        string npcName,
        string openingLine,
        string choice1,
        string response1,
        string choice2,
        string response2,
        bool showChoice2 = true,
        Action onChoice1Selected = null,
        Action onChoice2Selected = null,
        string choice3 = "",
        string response3 = "",
        bool showChoice3 = false,
        Action onChoice3Selected = null)
    {
        List<DialogueChoice> choices =
            new List<DialogueChoice>();

        choices.Add(new DialogueChoice(
            choice1,
            response1,
            onChoice1Selected
        ));

        if (showChoice2)
        {
            choices.Add(new DialogueChoice(
                choice2,
                response2,
                onChoice2Selected
            ));
        }

        if (showChoice3)
        {
            choices.Add(new DialogueChoice(
                choice3,
                response3,
                onChoice3Selected
            ));
        }

        ShowDialogue(npcName, openingLine, choices);
    }

    private void ShowResponse(string response)
    {
        dialogueText.text = response;
    }

    private void ClearChoiceButtons()
    {
        foreach (Button button in spawnedChoiceButtons)
        {
            if (button != null)
            {
                Destroy(button.gameObject);
            }
        }

        spawnedChoiceButtons.Clear();
    }

    public void CloseDialogue()
    {
        dialoguePanel.SetActive(false);
        ClearChoiceButtons();
    }

    public bool IsDialogueOpen()
    {
        return dialoguePanel.activeSelf;
    }
}

// Represents one available question or dialogue action.
public class DialogueChoice
{
    public string Text { get; }
    public string Response { get; }
    public Action OnSelected { get; }

    public DialogueChoice(
        string text,
        string response,
        Action onSelected = null)
    {
        Text = text;
        Response = response;
        OnSelected = onSelected;
    }
}
