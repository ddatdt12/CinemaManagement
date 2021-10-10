﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CinemaManagement.Models
{
    class DataProvider
    {
        private static DataProvider _ins;
        public static DataProvider Ins
        {
            get { if (_ins == null) _ins = new DataProvider(); return _ins; }
            set
            {
                _ins = value;
            }
        }
        public CinemaManagementEntities DB { get; set; }
        private DataProvider()
        {
            try
            {
                DB = new CinemaManagementEntities();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}