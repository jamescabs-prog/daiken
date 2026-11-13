using System;
using System.Security.Cryptography;
using System.Text;

namespace Drafters.Services
{
    public static class Security
    {
        // SHA-256 hex (uppercase) - matches HASHBYTES('SHA2_256') in the seed data.
        // NOTE: fine for a school/demo project. For production use PBKDF2/bcrypt with a salt.
        public static string Hash(string text)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(text ?? ""));
                return BitConverter.ToString(bytes).Replace("-", "");
            }
        }
    }

    public static class Ui
    {
        // Bootstrap colour classes for a status badge
        public static string Badge(string status)
        {
            switch (status)
            {
                case "Pending Approval": return "warning text-dark";
                case "Ready for Release": return "info text-dark";
                case "Pending - Awaiting Stock": return "secondary";
                case "Completed": return "success";
                case "Rejected": return "danger";
                case "For Revision": return "danger";
                case "Cancelled": return "dark";
                case "Received": return "success";
                case "Pending": return "warning text-dark";
                default: return "light text-dark";
            }
        }
    }
}
