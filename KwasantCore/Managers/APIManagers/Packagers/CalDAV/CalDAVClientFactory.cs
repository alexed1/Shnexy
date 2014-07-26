using System;
using Data.Interfaces;
using KwasantCore.Managers.APIManagers.Authorizers;
using KwasantCore.Managers.APIManagers.Transmitters.Http;
using StructureMap;

namespace KwasantCore.Managers.APIManagers.Packagers.CalDAV
{
    public class CalDAVClientFactory : ICalDAVClientFactory
    {
        public ICalDAVClient Create(IRemoteCalendarAuthData authData)
        {
            if (authData == null)
                throw new ArgumentNullException("authData");

            var channel = new OAuthHttpChannel(ObjectFactory.GetNamedInstance<IOAuthAuthorizer>(authData.Provider.Name));
            return new CalDAVClient(authData.Provider.CalDAVEndPoint, channel);
        }
    }
}