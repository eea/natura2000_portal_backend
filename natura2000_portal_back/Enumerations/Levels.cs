﻿using System.Runtime.Serialization;

namespace natura2000_portal_back.Enumerations
{
    [DataContract]
    public enum Level
    {
        [DataMember]
        Info = 0,
        [DataMember]
        Warning = 1,
        [DataMember]
        Critical = 2
    }
}