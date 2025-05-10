using Firebase.Database;
using Firebase.Database.Query;
using SlopeGuard.Models;
using System.Threading.Tasks;

namespace SlopeGuard.Services
{
    public class FirebaseService
    {
        private readonly FirebaseClient _firebaseClient;

        public FirebaseService()
        {
            // Initialize Firebase client with your Firebase Database URL
            _firebaseClient = new FirebaseClient("https://your-firebase-database-url.firebaseio.com/");
        }

        // Method to write skier data to Firebase
        public async Task WriteSkierData(string sessionId, SkierData skierData)
        {
            await _firebaseClient
                .Child("sessions")
                .Child(sessionId)
                .Child("skier_data")
                .PutAsync(skierData);
        }

        // Method to read skier data from Firebase
        public async Task<SkierData> ReadSkierData(string sessionId)
        {
            var skierData = await _firebaseClient
                .Child("sessions")
                .Child(sessionId)
                .Child("skier_data")
                .OnceSingleAsync<SkierData>();
            return skierData;
        }
    }
}
