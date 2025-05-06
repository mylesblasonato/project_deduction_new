using System.Collections;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
// FIX: Better JSON handling

namespace Game.Prototype_Code
{
    public class AIChatClient : MonoBehaviour
    {
        private string aiServerUrl = "http://127.0.0.1:5000/generate";
        public TMP_InputField inputField;  // Input field for the prompt
        public TMP_Text responseLabel;     // Text label to display the AI response

        public void SendRequest()
        {
            string userPrompt = inputField.text;  // Get the prompt text from the input field

            // FIX: Ensure AI understands it's supposed to generate a response
            string formattedPrompt = "User: " + userPrompt + "\nAI:";

            StartCoroutine(GetAIResponse(formattedPrompt)); // Start the coroutine
        }

        public IEnumerator GetAIResponse(string prompt)
        {
            // Ensure the AI gets the right format
            string jsonData = JsonConvert.SerializeObject(new { prompt = prompt });

            Debug.Log($"📤 Sending JSON: {jsonData}"); // Debugging

            byte[] postData = Encoding.UTF8.GetBytes(jsonData);

            using (UnityWebRequest request = new UnityWebRequest(aiServerUrl, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(postData);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                yield return request.SendWebRequest();

                Debug.Log($"📩 Server Response: {request.downloadHandler.text}");

                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError("❌ Network Error: " + request.error);
                }
                else if (string.IsNullOrEmpty(request.downloadHandler.text))
                {
                    Debug.LogError("❌ Empty response from AI Server");
                }
                else
                {
                    try
                    {
                        JObject jsonResponse = JObject.Parse(request.downloadHandler.text);

                        // Extracting the correct response field
                        string aiResponse = jsonResponse["response"]?.ToString();

                        if (!string.IsNullOrEmpty(aiResponse))
                        {
                            Debug.Log("✅ AI Response: " + aiResponse);
                            responseLabel.text = "AI: " + aiResponse;
                        }
                        else
                        {
                            Debug.LogError("⚠ AI response is missing or empty!");
                        }
                    }
                    catch (JsonReaderException e)
                    {
                        Debug.LogError($"❌ JSON Parsing Error: {e.Message}");
                    }
                }
            }
        }
    }
}