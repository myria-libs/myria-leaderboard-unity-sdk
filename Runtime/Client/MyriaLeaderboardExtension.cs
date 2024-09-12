using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using UnityEngine.Networking;
using MyriaLeaderboard.Requests;
using MyriaLeaderboard.MyriaLeaderboardEnums;

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto.Digests;


namespace MyriaLeaderboard.MyriaLeaderboardExtension {

    public class RsaEncryptionWithBouncyCastle
    {
        // Hàm đọc Public Key từ PEM và trả về đối tượng RSA
        public AsymmetricKeyParameter ReadPublicKey(string publicKeyPem)
        {
            PemReader pemReader = new PemReader(new System.IO.StringReader(publicKeyPem));
            AsymmetricKeyParameter publicKey = (AsymmetricKeyParameter)pemReader.ReadObject();
            return publicKey;
        }

        // Hàm mã hóa dữ liệu bằng Public Key
        public string EncryptWithPublicKey(string dataToEncrypt, string publicKeyPem)
        {
            // Bước 1: Đọc Public Key từ PEM
            var publicKey = ReadPublicKey(publicKeyPem);

            // Bước 2: Khởi tạo bộ mã hóa RSA
            var rsaEngine = new OaepEncoding(new RsaEngine(), new Sha256Digest());
            rsaEngine.Init(true, publicKey); // 'true' cho mã hóa

            // Bước 3: Chuyển đổi chuỗi thành byte[] và mã hóa
            byte[] dataBytes = Encoding.UTF8.GetBytes(dataToEncrypt);
            byte[] encryptedBytes = rsaEngine.ProcessBlock(dataBytes, 0, dataBytes.Length);

            return Convert.ToBase64String(encryptedBytes);
        }
    }

    public static class EOrderByExtensions
    {
        public static string OderByToStringValue(this EOrderBy orderBy)
        {
            switch (orderBy)
            {
                case EOrderBy.ASC:
                    return "ASC";
                case EOrderBy.DESC:
                    return "DESC";
                default:
                    return string.Empty;
            }
        }
    }

    public static class ESortingFieldExtensions
    {
        public static string SortingToStringValue(this ESortingField sortingField)
        {
            switch (sortingField)
            {
                case ESortingField.createdAt:
                    return "createdAt";
                case ESortingField.updatedAt:
                    return "updatedAt";
                case ESortingField.score:
                    return "score";
                case ESortingField.rank:
                    return "rank";
                case ESortingField.period:
                    return "period";
                case ESortingField.leaderboardId:
                    return "leaderboardId";
                case ESortingField.userId:
                    return "userId";
                default:
                    return "createdAt";
            }
        }
    }
    public static class EncryptExtentsions
    {
        // Hàm mã hóa
        public static string EncryptApiKey(string jsonData, string publicKeyPem)
        {
            RsaEncryptionWithBouncyCastle rsaData = new RsaEncryptionWithBouncyCastle();
            string encryptData = rsaData.EncryptWithPublicKey(jsonData, publicKeyPem.Replace("\\n", "\n"));
            return encryptData;
        }
        public static string EncryptHMACSHA256(string jsonData, string secretKey)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey)))
            {
                byte[] hashValue = hmac.ComputeHash(Encoding.UTF8.GetBytes(jsonData));
                return BitConverter.ToString(hashValue).Replace("-", "").ToLower();
            }
        }
    }
    
}