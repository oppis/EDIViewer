using Microsoft.Extensions.Logging;

namespace EDIViewer.Helper
{
    public class UserMessageHelper
    {
        private readonly ILogger<UserMessageHelper> _logger;

        public UserMessageHelper(ILogger<UserMessageHelper> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Log error message for web application
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static bool ShowErrorMessage(string title, string message)
        {
            // In web context, we just log the error
            Console.WriteLine($"ERROR - {title}: {message}");
            return true;
        }

        /// <summary>
        /// Log info message for web application
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static bool ShowInfoMessage(string title, string message)
        {
            // In web context, we just log the info
            Console.WriteLine($"INFO - {title}: {message}");
            return true;
        }
    }
}