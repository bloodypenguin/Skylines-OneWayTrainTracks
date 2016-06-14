﻿using ICities;
using UnityEngine;

namespace SingleTrainTrack
{
    public class Mod : IUserMod
    {
        public const string TRAIN_STATION_TRACK = "Train Station Track";
        public const string TRAIN_TRACK = "Train Track";

        public string Name => "Single Train Tracks";

        public string Description => "Provides one-way single train tracks";
    }
}