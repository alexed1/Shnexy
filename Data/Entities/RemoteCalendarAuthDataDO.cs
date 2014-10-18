using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.Interfaces;

namespace Data.Entities
{
    /// <summary>
    /// This entity contains information about access to remote calendar providers granted by customer.
    /// The key field is AuthData that is JSON-string field with authorization data such as OAuth access and refresh token.
    /// </summary>
    public class RemoteCalendarAuthDataDO : IRemoteCalendarAuthData
    {
        [NotMapped]
        IRemoteCalendarProvider IRemoteCalendarAuthData.Provider
        {
            get { return Provider; }
            set { Provider = (RemoteCalendarProviderDO)value; }
        }

        [NotMapped]
        IUser IRemoteCalendarAuthData.User
        {
            get { return User; }
            set { User = (UserDO)value; }
        }

        [Key]
        public int Id { get; set; }
        public string AuthData { get; set; }

        [Required, ForeignKey("Provider")]
        public int? ProviderID { get; set; }
        public virtual RemoteCalendarProviderDO Provider { get; set; }
        
        [Required, ForeignKey("User")]
        public string UserID { get; set; }
        public virtual UserDO User { get; set; }        
        
        public bool HasAccessToken()
        {
            return !string.IsNullOrEmpty(AuthData) &&
                   AuthData.Contains("access_token");
        }
    }
}