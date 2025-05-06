using System;
using System.Collections.Generic;
using MeetAndTalk;
using MeetAndTalk.Localization;
using MeetAndTalk.Settings;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

#if UNITY_EDITOR

public class AutoTranslate : MonoBehaviour
{
    public static void Translate(DialogueContainerSO _target)
    {
        bool isGoogle = false;
        string apiKey = Resources.Load<MeetAndTalkSettings>("MeetAndTalkSettings").DeeplApiKey;
        if (apiKey == "") { apiKey = Resources.Load<MeetAndTalkSettings>("MeetAndTalkSettings").DeeplApiKey; isGoogle = true; }

        if (string.IsNullOrEmpty(apiKey))
        {
            EditorUtility.DisplayDialog("API Key Missing", "DeepL API ot Google Translate Key is missing. Please set it in the settings.", "OK");
            return;
        }

        foreach (var dialogueData in _target.DialogueNodeDatas)
        {
            TranslateTextList(dialogueData.TextType, apiKey, isGoogle);
        }
        foreach (var dialogueChoiceData in _target.DialogueChoiceNodeDatas)
        {
            TranslateTextList(dialogueChoiceData.TextType, apiKey, isGoogle);
            TranslateNodePorts(dialogueChoiceData.DialogueNodePorts, apiKey, isGoogle);
        }
        foreach (var timerChoiceData in _target.TimerChoiceNodeDatas)
        {
            TranslateTextList(timerChoiceData.TextType, apiKey, isGoogle);
            TranslateNodePorts(timerChoiceData.DialogueNodePorts, apiKey, isGoogle);
        }
        foreach (var timerChoiceData in _target.AdvancedChoiceNodeDatas)
        {
            TranslateNodePorts(timerChoiceData.DialogueNodePorts, apiKey, isGoogle);
        }
        foreach (var timerChoiceData in _target.AdvancedTimeChoiceNodeDatas)
        {
            TranslateNodePorts(timerChoiceData.DialogueNodePorts, apiKey, isGoogle);
        }
    }

    private static void TranslateTextList(List<LanguageGeneric<string>> textList, string apiKey, bool isGoogle)
    {
        // Znajd� tekst angielski
        var englishText = textList.Find(t => t.languageEnum == LocalizationEnum.English)?.LanguageGenericType;
        if (string.IsNullOrEmpty(englishText))
        {
            Debug.LogWarning("No English text found for translation.");
            return;
        }

        foreach (var text in textList)
        {
            // Je�li pole w innym j�zyku jest ju� wype�nione, pomi� t�umaczenie
            if (text.languageEnum != LocalizationEnum.English && string.IsNullOrEmpty(text.LanguageGenericType))
            {
                if (isGoogle) text.LanguageGenericType = TranslateTextWithGoogle(englishText, text.languageEnum.ToString(), apiKey);
                else text.LanguageGenericType = TranslateTextWithDeepL(englishText, text.languageEnum.ToString(), apiKey);
            }
        }
    }
    private static void TranslateNodePorts(List<DialogueNodePort> nodePorts, string apiKey, bool isGoogle)
    {
        foreach (var port in nodePorts)
        {
            TranslateTextList(port.TextLanguage, apiKey, isGoogle);
        }
    }
    private static void TranslateNodePorts(List<AdvancedDialogueNodePort> nodePorts, string apiKey, bool isGoogle)
    {
        foreach (var port in nodePorts)
        {
            TranslateTextList(port.TextLanguage, apiKey, isGoogle);
        }
    }

    private static string TranslateTextWithDeepL(string text, string targetLanguage, string apiKey)
    {
        if (string.IsNullOrEmpty(text))
        {
            return null; // Zwraca null, je�li tekst jest pusty
        }

        string url = "https://api-free.deepl.com/v2/translate";

        WWWForm form = new WWWForm();
        form.AddField("auth_key", apiKey);
        form.AddField("text", text);
        form.AddField("target_lang", LocalizationManager.GetIsoLanguageCode(targetLanguage));

        using (var www = UnityWebRequest.Post(url, form))
        {
            www.SendWebRequest();

            while (!www.isDone) { }

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                return null;
            }
            else
            {
                var jsonResponse = JsonUtility.FromJson<DeepLResponse>(www.downloadHandler.text);
                return jsonResponse.translations[0].text;
            }
        }
    }

    private static string TranslateTextWithGoogle(string text, string targetLanguage, string apiKey)
    {
        if (string.IsNullOrEmpty(text))
        {
            return null; // Zwraca null, je�li tekst jest pusty
        }

        string url = $"https://translation.googleapis.com/language/translate/v2?key={apiKey}";

        // Tworzymy cia�o ��dania JSON
        var requestData = new
        {
            q = text,
            target = targetLanguage
        };

        string jsonData = JsonUtility.ToJson(requestData);

        using (var www = UnityWebRequest.PostWwwForm(url, UnityWebRequest.kHttpVerbPOST))
        {
            www.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonData));
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            www.SendWebRequest();

            while (!www.isDone) { }

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                return null;
            }
            else
            {
                var jsonResponse = JsonUtility.FromJson<GoogleTranslateResponse>(www.downloadHandler.text);
                return jsonResponse.data.translations[0].translatedText;
            }
        }
    }

    [Serializable]
    private class DeepLResponse
    {
        public List<Translation> translations;

        [Serializable]
        public class Translation
        {
            public string text;
        }
    }

    [System.Serializable]
    private class GoogleTranslateResponse
    {
        public TranslationData data;

        [System.Serializable]
        public class TranslationData
        {
            public List<Translation> translations;
        }

        [System.Serializable]
        public class Translation
        {
            public string translatedText;
        }
    }
}

#endif
