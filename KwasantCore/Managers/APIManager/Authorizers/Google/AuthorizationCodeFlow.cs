using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Requests;

namespace KwasantCore.Managers.APIManager.Authorizers.Google
{
    class AuthorizationCodeFlow : GoogleAuthorizationCodeFlow
    {
        private readonly string _email;

        public AuthorizationCodeFlow(Initializer initializer, string email)
            : base(initializer)
        {
            _email = email;
        }

        public override AuthorizationCodeRequestUrl CreateAuthorizationCodeRequest(string redirectUri)
        {
            var url = base.CreateAuthorizationCodeRequest(redirectUri);
            var googleUrl = url as GoogleAuthorizationCodeRequestUrl;
            if (googleUrl != null)
            {
                googleUrl.AccessType = "offline";
                googleUrl.ApprovalPrompt = "force";
                googleUrl.LoginHint = _email;
            }
            return url;
        }
    }
}