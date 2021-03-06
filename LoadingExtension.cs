﻿using System;
using System.Collections.Generic;
using System.Linq;
using ColossalFramework.UI;
using DoubleTrainTrack.Rail2L1W;
using ICities;
using SingleTrainTrack.NEXT;
using UnityEngine;
using Object = UnityEngine.Object;
using Rail1LBuilder = SingleTrainTrack.Rail1L.Rail1L1W.Rail1LBuilder;
using Rail1L2WBuilder = SingleTrainTrack.Rail1L.Rail1L2W.Rail1L2WBuilder;
using Rail1L2SidedStationBuilder = SingleTrainTrack.Rail1LStation.Rail1L2SidedStation.Rail1L2SidedStationBuilder;
using Rail1LStationBuilder = SingleTrainTrack.Rail1LStation.Rail1L1SidedStation.Rail1LStationBuilder;

namespace SingleTrainTrack
{
    public class LoadingExtension : LoadingExtensionBase
    {
        public static Initializer Container;
        public static bool Done { get; private set; } // Only one Assets installation throughout the application

        public override void OnCreated(ILoading loading)
        {
            base.OnCreated(loading);
            InstallAssets();
            if (Container == null)
            {
                Container = new GameObject("Rail1L").AddComponent<Initializer>();
            }
            Initializer.Tracks1W = new List<KeyValuePair<NetInfo, NetInfoVersion>>();
            Initializer.Tracks2W = new List<KeyValuePair<NetInfo, NetInfoVersion>>();
            Initializer.Tracks2L1W = new List<KeyValuePair<NetInfo, NetInfoVersion>>();
            Initializer.Station2SidedTracks = new List<KeyValuePair<NetInfo, NetInfoVersion>>();
            Initializer.Station1SidedTracks = new List<KeyValuePair<NetInfo, NetInfoVersion>>();
            new object[]
            {
               new Rail2L1WBuilder(),
               new Rail1LBuilder(),
               new Rail1L2WBuilder(),
               new Rail1L2SidedStationBuilder(),
               new Rail1LStationBuilder()
            }.ForEach(trackBuilder =>
            {
                Util.AddLocale("NET", trackBuilder.GetPropery<string>("Name"), trackBuilder.GetPropery<string>("DisplayName"), trackBuilder.GetPropery<string>("Description"));
            });
        }

        private static void InstallAssets()
        {
            if (Done) // Only one Assets installation throughout the application
            {
                return;
            }
            var path = Util.AssemblyPath;
            foreach (var action in AssetManager.instance.CreateLoadingSequence(path))
            {
                var localAction = action;

                Loading.QueueLoadingAction(() =>
                {
                    try
                    {
                        localAction();
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogException(ex);
                    }
                });
            }
            Done = true;
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);
            if (Initializer.Tracks1W == null || Initializer.Tracks2W == null || Initializer.Station2SidedTracks == null || Initializer.Station1SidedTracks == null || Initializer.Tracks2L1W == null)
            {
                return; //that assures that following code gets executed only on the first loading
            }
            var railInfos = Resources.FindObjectsOfTypeAll<NetInfo>();
            foreach (var ri in railInfos.Where(ri => ri?.m_netAI is TrainTrackBaseAI && ri.m_class.m_subService == ItemClass.SubService.PublicTransportTrain))
            {
                if (Initializer.Tracks1W.Select(p => p.Key).Contains(ri) ||
                    Initializer.Tracks2W.Select(p => p.Key).Contains(ri) ||
                    Initializer.Station2SidedTracks.Select(p => p.Key).Contains(ri) ||
                    Initializer.Station1SidedTracks.Select(p => p.Key).Contains(ri) ||
                    Initializer.Tracks2L1W.Select(p => p.Key).Contains(ri))
                {
                    continue;
                }
                if (ri.name.Contains("Station") || ri.name.Contains("Cargo"))
                {
                    ri.m_nodeConnectGroups = ri.m_nodeConnectGroups | NetInfo.ConnectGroup.WideTram;
                    foreach (var node in ri.m_nodes)
                    {
                        if (node.m_connectGroup == (NetInfo.ConnectGroup.DoubleTrain | NetInfo.ConnectGroup.TrainStation))
                        {
                            node.m_connectGroup = node.m_connectGroup | NetInfo.ConnectGroup.WideTram;
                        }
                    }
                }
            }
            try
            {
                var track1WBuilder = new Rail1LBuilder();
                var track2WBuilder = new Rail1L2WBuilder();
                foreach (var pair in Initializer.Tracks1W)
                {
                    if (pair.Key.m_halfWidth < 4)
                        track1WBuilder.LateBuildUp(pair.Key, pair.Value);
                }
                foreach (var pair in Initializer.Tracks2W)
                {
                    if (pair.Key.m_halfWidth < 4)
                        track2WBuilder.LateBuildUp(pair.Key, pair.Value);
                }
                var stationTrackBuilder2 = new Rail1L2SidedStationBuilder();
                foreach (var pair in Initializer.Station2SidedTracks)
                {
                    stationTrackBuilder2.LateBuildUp(pair.Key, pair.Value);
                }
                var stationTrackBuilder1 = new Rail1LStationBuilder();
                foreach (var pair in Initializer.Station1SidedTracks)
                {
                    stationTrackBuilder1.LateBuildUp(pair.Key, pair.Value);
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogException(ex);
            }
            finally
            {
                Initializer.Station2SidedTracks = null;
                Initializer.Station1SidedTracks = null;
                Initializer.Tracks2L1W = null;
                Initializer.Tracks1W = null;
                Initializer.Tracks2W = null;
            }
        }

        public override void OnReleased()
        {
            base.OnReleased();
            if (Container == null)
            {
                return;
            }
            Object.Destroy(Container.gameObject);
            Container = null;
            ModifyExistingNetInfos.Reset();
        }
    }
}