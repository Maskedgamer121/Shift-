using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Cavrnus.SpatialConnector.Core.HistoryBuilder
{
    public class ChatGPTRequest : MonoBehaviour
    {
        public async Task<string> RequestChatGptResponse(string apiKey, string request)
        {
            var tcs = new TaskCompletionSource<string>();
            StartCoroutine(SendChatGPTMessage(apiKey, request, tcs));
            var res = await tcs.Task;

            GameObject.Destroy(gameObject);

            return res;
        }

        IEnumerator SendChatGPTMessage(string apiKey, string userMessage, TaskCompletionSource<string> tcs)
        {
            string url = "https://api.openai.com/v1/chat/completions";

            string jsonBody = JsonConvert.SerializeObject(new
            {
                model = "gpt-4o",
                messages = new[]
                {
                    new { role = "user", content = userMessage }
                }
            });

            UnityWebRequest request = new UnityWebRequest(url, "POST");
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonBody));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                ChatResponse response = JsonConvert.DeserializeObject<ChatResponse>(request.downloadHandler.text);
                var res = response.choices?[0]?.message?.content?.Trim() ?? "";

                tcs.SetResult(res);
            }
            else
            {
                Debug.LogError("Error: " + request.error);
                tcs.SetResult(request.error);
            }
        }

        public class ChatResponse
        {
            public string id { get; set; }
            public string @object { get; set; }
            public long created { get; set; }
            public string model { get; set; }
            public List<Choice> choices { get; set; }
            public Usage usage { get; set; }
            public string service_tier { get; set; }
            public string system_fingerprint { get; set; }
        }

        public class Choice
        {
            public int index { get; set; }
            public Message message { get; set; }
            public object logprobs { get; set; }
            public string finish_reason { get; set; }
        }

        public class Message
        {
            public string role { get; set; }
            public string content { get; set; }
            public object refusal { get; set; }
            public List<object> annotations { get; set; }
        }

        public class Usage
        {
            public int prompt_tokens { get; set; }
            public int completion_tokens { get; set; }
            public int total_tokens { get; set; }
            public PromptTokensDetails prompt_tokens_details { get; set; }
            public CompletionTokensDetails completion_tokens_details { get; set; }
        }

        public class PromptTokensDetails
        {
            public int cached_tokens { get; set; }
            public int audio_tokens { get; set; }
        }

        public class CompletionTokensDetails
        {
            public int reasoning_tokens { get; set; }
            public int audio_tokens { get; set; }
            public int accepted_prediction_tokens { get; set; }
            public int rejected_prediction_tokens { get; set; }
        }
    }
}