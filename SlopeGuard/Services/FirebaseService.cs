using Firebase.Database;
using Firebase.Database.Query;
using Firebase.Database.Streaming;
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

        // Save live session data (called by skier device)
        public async Task SaveLiveSessionDataAsync(string guid, LiveSessionData data)
        {
            Console.WriteLine($"[DEBUG][FirebaseService] Saving LiveSessionData to /sessions/{guid}/live");
            await _firebaseClient
                .Child("sessions")
                .Child(guid)
                .Child("live")
                .PutAsync(data);
        }

        // Subscribe to live session data (called by viewer device)
        public IObservable<FirebaseEvent<LiveSessionData>> SubscribeToLiveSessionData(string guid)
        {
            Console.WriteLine($"[DEBUG][FirebaseService] Subscribing to /sessions/{guid}/live");
            return _firebaseClient
                .Child("sessions")
                .Child(guid)
                .Child("live")
                .AsObservable<LiveSessionData>();
        }

        // Update session state (active/inactive)
        public async Task UpdateSessionStateAsync(string guid, bool isActive)
        {
            await _firebaseClient
                .Child("sessions")
                .Child(guid)
                .Child("state")
                .PutAsync(isActive ? "active" : "inactive");
        }

        // Subscribe to session state (viewer listens)
        public IObservable<FirebaseEvent<string>> SubscribeToSessionState(string guid)
        {
            return _firebaseClient
                .Child("sessions")
                .Child(guid)
                .Child("state")
                .AsObservable<string>();
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
