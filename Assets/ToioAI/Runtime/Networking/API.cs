using System.Collections;
using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using ToioAI.Networking.Model;

namespace ToioAI.Networking
{
    public partial class API
    {
        public async UniTask<CubesLuaResponse> CubesLua(CubesLuaRequest req)
        {
            var path = "/api/toio/cubes/lua";
            return await PostJsonAsync<CubesLuaResponse>(path, req);
        }

        string BaseUrl()
        {
            return "http://localhost:3000";
        }

        async UniTask<T> PostJsonAsync<T>(string path, object data)
        {
            var json     = JsonUtility.ToJson(data);
            var postData = Encoding.UTF8.GetBytes(json);
            string url  = BaseUrl() + path;

            using var request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST)
            {
                uploadHandler   = new UploadHandlerRaw(postData),
                downloadHandler = new DownloadHandlerBuffer()
            };
            request.SetRequestHeader("Content-Type", "application/json");

            Debug.Log($"Sending request to {url} with data {json}");
            await request.SendWebRequest();
            Debug.Log($"Received response from {url} with data {request.downloadHandler.text}");
            return GetResponseAs<T>(request.downloadHandler.data);
        }

        T GetResponseAs<T>(byte[] bytes)
        {
            return JsonUtility.FromJson<T>(Encoding.UTF8.GetString(bytes));
        }
    }
}
