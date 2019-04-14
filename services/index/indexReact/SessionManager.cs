using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace indexReact
{
    public static class SessionManager
    {
        public static readonly Dictionary<string, byte[]> Store = new Dictionary<string, byte[]>();
        private static readonly RNGCryptoServiceProvider Rng;

        static SessionManager()
        {
            Rng = new RNGCryptoServiceProvider();
        }

        public static string CreateSession(string login)
        {
            var salt = new byte[10];
            Rng.GetBytes(salt);
            var loginBytes = Encoding.UTF8.GetBytes(login);
            var loginWithSalt = ConcatArrays(loginBytes, salt);
            using (var sha512 = SHA512.Create())
            {
                var sid = Convert.ToBase64String(sha512.ComputeHash(loginWithSalt));
                Store[login] = salt;

                return sid;
            }
        }

        public static bool ValidateSession(string login, string sid)
        {
            if (login == null || sid == null)
                return false;

            if (!Store.TryGetValue(login, out var salt))
                return false;

            var loginBytes = Encoding.UTF8.GetBytes(login);
            var loginWithSalt = ConcatArrays(loginBytes, salt);
            using (var sha512 = SHA512.Create())
            {
                var computedSid = Convert.ToBase64String(sha512.ComputeHash(loginWithSalt));

                return sid == computedSid;
            }
        }

        public static void Remove(string login, string sid)
        {
            if (ValidateSession(login, sid))
                Store.Remove(login);
        }

        private static T[] ConcatArrays<T>(T[] f, T[] s)
        {
            var r = new T[f.Length + s.Length];
            f.CopyTo(r, 0);
            s.CopyTo(r, f.Length);

            return r;
        }
    }
}