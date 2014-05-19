﻿using System;
using Data.Infrastructure;
using Data.Interfaces;
using KwasantCore.StructureMap;
using log4net.Config;
using StructureMap;
using UtilitiesLib.Logging;

namespace Playground
{
    public class Program
    {
        /// <summary>
        /// This is a sandbox for devs to use. Useful for directly calling some library without needing to launch the main application
        /// </summary>
        /// <param name="args"></param>
        private static void Main(string[] args)
        {
            StructureMapBootStrapper.ConfigureDependencies("dev"); //set to either "test" or "dev"
            
            KwasantDbContext db = new KwasantDbContext();
            db.Database.Initialize(true);

            var first = ObjectFactory.GetInstance<IUnitOfWork>();
            var second = ObjectFactory.GetInstance<IUnitOfWork>();

            var fcc = first.GetHashCode();
            var scc = second.GetHashCode();
        }
    }
}
