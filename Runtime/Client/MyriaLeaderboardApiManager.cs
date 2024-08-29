using System;
using MyriaLeaderboard.Requests;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Security.Cryptography;
using UnityEngine.Networking;


namespace MyriaLeaderboard
{
    public partial class MyriaLeaderboardAPIManager
    {
        private static string secretKey = MyriaLeaderboardConfig.current.xDeveloperApiKey; // Key phải có 16, 24 hoặc 32 ký tự cho AES-128, AES-192, hoặc AES-256
        public class Payload
        {
            public string data { get; set; }
        }
        // Hàm mã hóa
        public static string EncryptJson(string jsonData)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(secretKey.PadRight(32).Substring(0, 32));
                aes.GenerateIV(); // Khởi tạo một IV mới
                aes.Mode = CipherMode.CBC;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                {
                    ms.Write(aes.IV, 0, aes.IV.Length); // Lưu IV vào đầu stream

                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (System.IO.StreamWriter sw = new System.IO.StreamWriter(cs))
                    {
                        sw.Write(jsonData); // Ghi JSON vào stream để mã hóa
                    }

                    return Convert.ToBase64String(ms.ToArray()); // Trả về chuỗi mã hóa base64
                }
            }
        }

        public static void GetScoreList(MyriaLeaderboardGetScoreListRequest getRequests, Action<MyriaLeaderboardGetScoreListResponse> onComplete)
        {
            EndPointClass requestEndPoint = MyriaLeaderboardEndPoints.MyriaEndPointGetScoreByLeaderboardId;

            string tempEndpoint = requestEndPoint.endPoint;
            string endPoint = string.Format(requestEndPoint.endPoint, getRequests.leaderboardKey, getRequests.page, getRequests.limit, getRequests.sortingField, getRequests.orderBy);

            // Handle all paging
            if (getRequests.page == 0)
            {
                onComplete?.Invoke(MyriaLeaderboardResponseFactory.InputUnserializableError<MyriaLeaderboardGetScoreListResponse>());
                return;
            }
            tempEndpoint = requestEndPoint.endPoint + "";
            endPoint = string.Format(tempEndpoint, getRequests.leaderboardKey, getRequests.page, getRequests.limit, getRequests.sortingField, getRequests.orderBy);

            MyriaLeaderboardServerRequest.CallAPI(endPoint, requestEndPoint.httpMethod, null, (serverResponse) =>
            { MyriaLeaderboardResponse.Deserialize(onComplete, serverResponse); }, false, MyriaLeaderboardEnums.MyriaLeaderboardCallerRole.Base);
            //{
            //    MyriaLeaderboardGetScoreListResponse parseData = MyriaLeaderboardResponse.Deserialize<MyriaLeaderboardGetScoreListResponse>(serverResponse);
            //    onComplete?.Invoke(parseData);
            //}, false, MyriaLeaderboardEnums.MyriaLeaderboardCallerRole.Base);
        }
        public static void PostScores(MyriaLeaderboardPostScoreRequest getRequests, MyriaLeaderboardPostScoreParams data, Action<MyriaLeaderboardPostScoreResponse> onComplete)
        {
            if (data == null)
            {
                onComplete?.Invoke(MyriaLeaderboardResponseFactory.InputUnserializableError<MyriaLeaderboardPostScoreResponse>());
                return;
            }
            var json = MyriaLeaderboardJson.SerializeObject(data);
            // Debug.Log("json" + json);
            // string dataRaw = EncryptJson(json);
            // Payload dataPayload = new Payload {data = dataRaw};
            // var jsonData = MyriaLeaderboardJson.SerializeObject(dataPayload);
            EndPointClass requestEndPoint = MyriaLeaderboardEndPoints.MyriaEndPointPostScoreByLeaderboardId;
            string tempEndpoint = requestEndPoint.endPoint;
            string endPoint = string.Format(requestEndPoint.endPoint, getRequests.leaderboardKey);
            endPoint = string.Format(endPoint, getRequests.leaderboardKey);
            MyriaLeaderboardServerRequest.CallAPI(endPoint, requestEndPoint.httpMethod, json, (serverResponse) => { MyriaLeaderboardResponse.Deserialize(onComplete, serverResponse); }, false, MyriaLeaderboardEnums.MyriaLeaderboardCallerRole.Base);
        }

    }
}