﻿using System.Web.Mvc;
using Data.Infrastructure.StructureMap;
using KwasantCore.Security;
using StructureMap;

namespace KwasantWeb.Controllers
{
    public static class ControllerExtensions
    {
        public static string GetUserId(this Controller controller)
        {
            return ObjectFactory.GetInstance<ISecurityServices>().GetCurrentUser();
        }
        public static string GetUserName(this Controller controller)
        {
            return ObjectFactory.GetInstance<ISecurityServices>().GetUserName();
        }
        public static bool UserIsAuthenticated(this Controller controller)
        {
            return ObjectFactory.GetInstance<ISecurityServices>().IsAuthenticated();
        }
        public static void Logout(this Controller controller)
        {
            ObjectFactory.GetInstance<ISecurityServices>().Logout();
        }
    }
}