﻿using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public class AuthorizationTokenDO
    {
        public AuthorizationTokenDO()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }

        [ForeignKey("UserDO")]
        public String UserID { get; set; }
        public virtual UserDO UserDO { get; set; }

        public String RedirectURL { get; set; }

        public DateTime ExpiresAt { get; set; }
    }
}