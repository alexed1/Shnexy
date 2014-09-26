﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using KwasantCore.Interfaces;
using KwasantCore.Services;
using Newtonsoft.Json;
using Segment;
using Segment.Model;
using StructureMap;

namespace KwasantWeb.Controllers
{
    public class TokenAuthController : Controller
    {
        public ActionResult Index(String token)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var validToken = uow.AuthorizationTokenRepository.GetQuery().FirstOrDefault(t => t.Id.ToString() == token);
                if (validToken == null)
                    throw new HttpException(404, "Authorization token not found.");
                if (validToken.ExpiresAt < DateTime.Now)
                    throw new HttpException(404, "Authorization token expired.");

                ObjectFactory.GetInstance<ISecurityServices>().Login(uow, validToken.UserDO);

                if (!String.IsNullOrEmpty(validToken.SegmentTrackingEventName))
                {
                    Properties segmentProps = null;
                    if (!String.IsNullOrEmpty(validToken.SegmentTrackingProperties))
                    {
                        segmentProps = new Properties();
                        var trackingProperties = JsonConvert.DeserializeObject<Dictionary<String, Object>>(validToken.SegmentTrackingProperties);
                        foreach (var prop in trackingProperties)
                            segmentProps.Add(prop.Key, prop.Value);
                    }

                    ObjectFactory.GetInstance<ISegmentIO>().Track(validToken.UserDO, validToken.SegmentTrackingEventName, segmentProps);
                }

                return Redirect(validToken.RedirectURL);
            }
        }
	}
}