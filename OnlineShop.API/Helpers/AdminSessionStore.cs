using System.Collections.Concurrent;

namespace OnlineShop.API.Helpers
{
    public static class AdimSessionStore
    {
        private static readonly ConcurrentDictionary<string, bool> VerifiedAdmins = new();

        public static void Add(string userId)
        {
            VerifiedAdmins[userId] = true;
        }

        public static bool IsVerified(string userId)
        {
            return VerifiedAdmins.TryGetValue(userId, out var isVerified) && isVerified;
        }

        public static void Remove(string userId)
        {
            VerifiedAdmins.TryRemove(userId, out _);
        }
    }
}
