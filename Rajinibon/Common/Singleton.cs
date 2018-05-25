using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rajinibon.Common
{
    public class Singleton
    {
        private static volatile Singleton _instance = null;
        private static Object _locker = new Object();
        public static Singleton GetValue()
        {
            if (_instance == null)
            {
                lock (_locker)
                {
                    if (_instance == null)
                    { _instance = new Singleton(); }
                }
            }
            return _instance;
        }
    }
}
