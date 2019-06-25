using UnityEngine;
using System;

namespace MafiaUnity
{
    public static class BuildInfo
    {
        public static DateTime BuildTime()
        {
            DateTime buildTime;
#if UNITY_EDITOR
            buildTime = System.DateTime.Now;
#else
            return System.DateTime.Now;
            //buildTime = System.DateTime.Parse(BuildtimeInfo.DateTimeString());
#endif


            return buildTime;
        }
    }
}