using Firebase.Database;
using Firebase.Database.Query;
using SlopeGuard.Models;
using System;
using System.Threading.Tasks;

namespace SlopeGuard.Services
{
    public class FirebaseService
    {
        private readonly FirebaseClient _firebaseClient;

        public FirebaseService()
        {
            var baseUrl = "https://slopeguard-8c766-default-rtdb.europe-west1.firebasedatabase.app/";

            _firebaseClient = new FirebaseClient(
                baseUrl,
                new FirebaseOptions
                {
                    // each time the client needs a token, it'll pull the latest value
                    AuthTokenAsyncFactory = () => Task.FromResult(AppConfig.FirebaseKey ?? string.Empty)
                });
        }

        /// <summary>
        /// Writes the latest skier snapshot under /sessions/{sessionId}/skier_data
        /// </summary>
        public async Task WriteSkierData(string sessionId, SkierData skierData)
        {
            await _firebaseClient
                .Child("sessions")
                .Child(sessionId)
                .Child("skier_data")
                .PutAsync(skierData);
        }

        /// <summary>
        /// Reads the skier snapshot from /sessions/{sessionId}/skier_data
        /// </summary>
        public async Task<SkierData> ReadSkierData(string sessionId)
        {
            return await _firebaseClient
                .Child("sessions")
                .Child(sessionId)
                .Child("skier_data")
                .OnceSingleAsync<SkierData>();
        }

        public async Task SavePairingGuidAsync(string guid, object metadata = null)
        {
            // Replace "pairings" with your actual Firebase path
            await _firebaseClient
                .Child("pairings")
                .Child(guid)
                .PutAsync(metadata ?? new { created = DateTime.UtcNow });
        }


        public async Task<bool> DoesPairingGuidExistAsync(string guid)
        {
            // Replace with your actual Firebase path and SDK usage
            var pairingNode = await _firebaseClient
                .Child("pairings")
                .Child(guid)
                .OnceSingleAsync<object>();
            return pairingNode != null;
        }

    }
}
