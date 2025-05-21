using Firebase.Database;
using Firebase.Database.Query;
using Firebase.Database.Streaming;
using SlopeGuard.Models;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using System.Reactive.Linq;


namespace SlopeGuard.Services
{
    public class FirebaseService
    {
        private readonly FirebaseClient _firebaseClient;

        public FirebaseService()
        {
            var baseUrl = "https://slopeguard-8c766-default-rtdb.europe-west1.firebasedatabase.app/";
            _firebaseClient = new FirebaseClient(baseUrl);

            //_firebaseClient = new FirebaseClient(
            //    baseUrl,
            //    new FirebaseOptions
            //    {
            //        // each time the client needs a token, it'll pull the latest value
            //        AuthTokenAsyncFactory = () => Task.FromResult(AppConfig.FirebaseKey ?? string.Empty)
            //    });
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
            var observable = _firebaseClient
                .Child("sessions")
                .Child(guid)
                .Child("live")
                .AsObservable<LiveSessionData>();

            // Add inline debug for all received events for this GUID.
            return Observable.Create<FirebaseEvent<LiveSessionData>>(observer =>
            {
                return observable.Subscribe(evt =>
                {
                    // <--- Place your debug/comment line here!
                    Console.WriteLine($"[DEBUG][FirebaseService] [LIVE DATA] Raw event: {JsonConvert.SerializeObject(evt)}");

                    if (evt.Object == null)
                        Console.WriteLine($"[DEBUG][FirebaseService] [DESERIALIZATION FAILED] for {guid}, raw JSON: {JsonConvert.SerializeObject(evt)}");
                    else
                        Console.WriteLine($"[DEBUG][FirebaseService] [SUCCESS] Data: {JsonConvert.SerializeObject(evt.Object)}");

                    observer.OnNext(evt);
                },
                ex =>
                {
                    Console.WriteLine($"[DEBUG][FirebaseService] [LIVE DATA] Exception for {guid}: {ex}");
                    observer.OnError(ex);
                },
                () =>
                {
                    Console.WriteLine($"[DEBUG][FirebaseService] [LIVE DATA] Completed for {guid}");
                    observer.OnCompleted();
                });
            });
        }


        // Update session state (active/inactive)
        public async Task UpdateSessionStateAsync(string guid, bool isActive)
        {
            var state = isActive ? "active" : "inactive";
            var json = JsonConvert.SerializeObject(state); // will wrap in quotes!
            await _firebaseClient
                .Child("sessions")
                .Child(guid)
                .Child("state")
                .PutAsync(json);
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
