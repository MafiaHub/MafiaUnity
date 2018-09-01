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
            buildTime = System.DateTime.Parse(BuildtimeInfo.DateTimeString());
#endif

            
            return buildTime;
        }
    }
}