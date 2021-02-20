namespace Madamin.Unfollow
{
    partial class UpdateServerApi
    {
        public const string StatusOk = "ok";

#if TGBUILD || DEBUG
        public const string LanguageEnglish = "en";
        public const string LanguagePersian = "fa";
#endif
#if !TGBUILD || DEBUG
        public const string LanguageGithubChannel = "github";
#endif

        private const string NotAvailable = "Not Available";
        private const string JsonMimeType = "application/json";

        private const string MethodCheckUpdate = "check_update";
        private const string MethodBugReport = "bug_report";
#if TGBUILD || DEBUG
        private const string MethodDidLogin = "did_login";
#endif
    }
}
