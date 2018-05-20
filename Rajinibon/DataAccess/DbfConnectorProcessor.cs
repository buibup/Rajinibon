﻿using Rajinibon.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rajinibon.DataAccess
{
    public static class DbfConnectorProcessor
    {
        public static List<StudentCheckTime> StudentCheckTimesFirstEntry(this List<StudentCheckTime> models)
        {
            var results = new List<StudentCheckTime>();

            results = models.GroupBy(x => x.EmpId).Select(y => y.FirstOrDefault()).ToList();

            return results;
        }
    }
}
