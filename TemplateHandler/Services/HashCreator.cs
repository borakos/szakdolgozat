using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TemplateHandler.Services {
    public static class HashCreator {
        private static Random random = new Random();
        private const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";


        public static string stringToSha256(string text, string salt) {
            try {
                string preSalt = salt.Substring((int)(salt.Length / 2), (int)(salt.Length / 2));
                string postSalt = salt.Substring(0, (int)(salt.Length / 2));
                SHA256 hash = SHA256.Create();
                byte[] bytes = hash.ComputeHash(Encoding.UTF8.GetBytes(preSalt + text + postSalt));
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++) {
                    sb.Append(bytes[i].ToString("x2"));
                }
                return sb.ToString();
            } catch (Exception ex) {
                throw;
            }
        }

        public static string createSalt() {
            try {
                StringBuilder sb = new StringBuilder();
                int length = chars.Length;
                for (int i = 0; i < 16; i++) {
                    sb.Append(chars[random.Next(length)]);
                }
                return sb.ToString();
            } catch(Exception ex) {
                throw;
            }
        }
    }
}
