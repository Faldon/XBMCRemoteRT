using System;
using Windows.System;

namespace XBMCRemoteRT.Helpers
{
    public static class FeedbackHelper
    {
        private static string recepient = "info@pulzer.it";
        private static string subject = "Kodi Assistant for Windows";

        public static async void SendFeedback(string feedbackMessage)
        {
            var uri = new Uri("mailto:?to=" + recepient + "&subject=" + subject + "&body=" + feedbackMessage);
            await Launcher.LaunchUriAsync(uri);
        }
    }
}
