﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using Shnexy.Utilities;

namespace Shnexy.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        
      

        private ShnexyDbContext db = new ShnexyDbContext();

        public User ()
        {
           

        }

        public User Get(int userId)
        {
            return db.Users.Find(userId);

        }

    }



}