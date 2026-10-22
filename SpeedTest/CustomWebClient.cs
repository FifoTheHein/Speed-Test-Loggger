#region Using
// Imported namespaces
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Cache;
#endregion
namespace SpeedTest
{
    /// <summary>Custom WebClient object that measures the download speed of a timed download.</summary>
    public class CustomWebClient : WebClient
    {
        #region Members
        // Private variables
        private long p_AllBytesDownloaded;
        private long p_CurrentSpeed;
        private long p_FirstChunkBytes;
        private TimeSpan p_FirstChunkTime;
        private bool p_Timing;
        private Stopwatch p_Stopwatch;
        #endregion
        #region Properties
        /// <summary>Gets the total number of bytes received by the timed download.</summary>
        public long AllBytesDownloaded
        {
            get { return p_AllBytesDownloaded; }
        }

        /// <summary>Gets the speed of the timed download, in bytes per second.</summary>
        public long CurrentSpeed
        {
            get { return p_CurrentSpeed; }
        }
        #endregion
        #region Constructor
        /// <summary>Initializes a new instance of the CustomWebClient class.</summary>
        public CustomWebClient() : base()
        {
            // Initialize private variables
            p_AllBytesDownloaded = 0;
            p_CurrentSpeed = 0;
            p_Stopwatch = new Stopwatch();

            // Always fetch from the server, so a cached copy can't report local disk speed
            CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
        }
        #endregion
        #region Public Methods
        /// <summary>Starts downloading the specified file and times it. Only this download counts towards CurrentSpeed.</summary>
        public void StartTimedDownload(Uri address, object userToken)
        {
            p_AllBytesDownloaded = 0;
            p_CurrentSpeed = 0;
            p_FirstChunkBytes = -1;
            p_Timing = true;
            p_Stopwatch.Restart();

            DownloadDataAsync(address, userToken);
        }
        #endregion
        #region Overrides
        /// <summary>Occurs when an asynchronous data download operation completes.</summary>
        protected override void OnDownloadDataCompleted(DownloadDataCompletedEventArgs e)
        {
            if (p_Timing)
            {
                // Count every byte of the file, including any that arrived after the last progress event
                if (e.Error == null && !e.Cancelled && e.Result != null) UpdateSpeed(e.Result.LongLength);

                p_Timing = false;
                p_Stopwatch.Stop();
            }

            // Continue processing the event as usual
            base.OnDownloadDataCompleted(e);
        }

        /// <summary>Occurs when an asynchronous download operation successfully transfers some or all of the data.</summary>
        protected override void OnDownloadProgressChanged(DownloadProgressChangedEventArgs e)
        {
            // Progress from the IP lookup isn't part of the speed test
            if (p_Timing) UpdateSpeed(e.BytesReceived);

            // Continue processing the event as usual
            base.OnDownloadProgressChanged(e);
        }
        #endregion
        #region Private Methods
        /// <summary>Updates the measured speed from the total bytes the timed download has received so far.</summary>
        private void UpdateSpeed(long bytesReceived)
        {
            TimeSpan elapsed = p_Stopwatch.Elapsed;
            p_AllBytesDownloaded = bytesReceived;

            // The first chunk arrives only after connecting and waiting for the server, so time from it when possible
            if (p_FirstChunkBytes < 0)
            {
                p_FirstChunkBytes = bytesReceived;
                p_FirstChunkTime = elapsed;
            }

            long bytes = bytesReceived - p_FirstChunkBytes;
            double seconds = (elapsed - p_FirstChunkTime).TotalSeconds;

            // A small file can arrive in one chunk; then the whole request time is all there is to go on
            if (bytes <= 0 || seconds <= 0)
            {
                bytes = bytesReceived;
                seconds = elapsed.TotalSeconds;
            }

            if (seconds > 0) p_CurrentSpeed = (long)(bytes / seconds);
        }
        #endregion
    }
}
