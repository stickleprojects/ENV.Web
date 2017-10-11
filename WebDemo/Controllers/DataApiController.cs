﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ENV.Web;

namespace WebDemo.Controllers
{
    public class DataApiController : Controller
    {
        static DataApiHelper _dataApi = new DataApiHelper();
        static DataApiController()
        {
            _dataApi.Register(typeof(Northwind.Models.Categories),true);
            _dataApi.Register(typeof(Northwind.Models.Orders));
            _dataApi.Register(typeof(Northwind.Models.Customers));
            _dataApi.Register(typeof(Northwind.Models.Shippers));
            
        }
        // GET: DataApi
        public void Index(string name, string id = null)
        {
            _dataApi.ProcessRequest(name, id);
        }
    }
}