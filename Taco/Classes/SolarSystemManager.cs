using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using System.Linq;
using System.Windows.Forms;
using System.ComponentModel;

namespace Taco.Classes
{
    class SolarSystemManager
    {
        public SolarSystemManager ()
        {
            IsSystemVboDataDirty = true;
            IsSystemVaoDataDirty = true;
            IsSystemColorVaoDataDirty = true;

            IsSystemVboDirty = true;
            IsSystemVaoDirty = true;
            IsSystemColorVaoDirty = true;


            IsConnectionVboDataDirty = true;
            IsConnectionVaoDataDirty = true;
            IsConnectionColorVaoDataDirty = true;

            IsConnectionVboDirty = true;
            IsConnectionVaoDirty = true;
            IsConnectionColorVaoDirty = true;

            _pathFindingWorker.DoWork += _pathFindingWorker_DoWork;
            _pathFindingWorker.RunWorkerCompleted += _pathFindingWorker_RunWorkerCompleted;
        }

        public bool IncomingTick()
        {
            ProcessTick();

            if ((_alertSystems.Count == 0) && (_highlightSystems.Count == 0))
                return false;
            else
                return true;
        }

        #region Members
        private Dictionary<int, SolarSystem> m_dSolarSystems = new Dictionary<int, SolarSystem>();
        private Vector3[] _systemVboContent;
        private int[] _systemColorVaoContent;
        private int[] _systemVaoContent;

        private int _homeSystemId = - 1;

        public int HomeSystemId
        {
            get { return _homeSystemId; }
        }

        private int _MapRangeFromSystemId = -1;
        private int _CurrentCameraCenteredSystemId = -1;

        public void SetCurrentHomeSystem(int systemId)
        {
            if (_homeSystemId != -1)
                ClearHomeSystem();

            _homeSystemId = systemId;

            if (_homeSystemId == -1) return;

            AddGreenCrossHair(_homeSystemId);

            foreach (var tempPath in _redCrossHairIDs.Select(tempSystemId => new PathInfo
            {
                FromSystem = _homeSystemId,
                ToSystem = tempSystemId
            }))
            {
                _pathfindingQueue.Enqueue(tempPath);
            }

            if (_homeSystemId != -1 && _MapRangeFromSystemId != -1)
            {
                var tempPath = new PathInfo
                {
                    FromSystem = _MapRangeFromSystemId,
                    ToSystem = _homeSystemId
                };

                _pathfindingQueue.Enqueue(tempPath);
            }
        }

        public void Set_MapRangeFrom_SystemID(int systemId)
        {
            _MapRangeFromSystemId = systemId;

            if (_MapRangeFromSystemId == -1)
                return;

            foreach (var tempPath in _redCrossHairIDs.Select(tempSystemId => new PathInfo
            {
                FromSystem = systemId,
                ToSystem = tempSystemId
            }))
            {
                _pathfindingQueue.Enqueue(tempPath);
            }

            if (_homeSystemId != -1)
            {
                var tempPath = new PathInfo
                {
                    FromSystem = systemId,
                    ToSystem = _homeSystemId
                };

                _pathfindingQueue.Enqueue(tempPath);
            }
        }

        public void Set_MapRangeFrom_ToHomeSystem()
        {
            _MapRangeFromSystemId = _homeSystemId;
        }

        public void SetCameraCenteredSystemID(int systemID)
        {

            _CurrentCameraCenteredSystemId = systemID;
            IsConnectionColorVaoDataDirty = true;
        }

        public void ClearHomeSystem()
        {
            _homeSystemId = -1;
            _greenCrossHairIDs.Clear();
        }
                
        private Queue<int> _redCrossHairIDs = new Queue<int>(50);
        private Queue<int> _greenCrossHairIDs = new Queue<int>(50);
        #endregion Members

        #region Properties
        public void RemoveExpiredAlerts()
        {
            if (_maxAlertAge == 0) return;

            var olderThan = DateTime.Now.AddMinutes(_maxAlertAge * -1);

            var expiredSystems = _systemStats.Values.Where(system => !system.Expired && system.LastReport < olderThan).Select(system => system.SystemId).ToList();

            if (expiredSystems.Count > 0)
            {
                var tempQueue = new Queue<int>();

                while (_redCrossHairIDs.Count > 0)
                {
                    var tempSystem = _redCrossHairIDs.Dequeue();

                    if (!expiredSystems.Contains(tempSystem))
                        tempQueue.Enqueue(tempSystem);
                    else
                        _systemStats[tempSystem].Expired = true;
                }

                _redCrossHairIDs = tempQueue;
            }
        }


        public Queue<int> RedCrossHairIDs
        {
            get
            {
                return _redCrossHairIDs;
            }
        }

        public Queue<int> GreenCrossHairIDs
        {
            get
            {
                return _greenCrossHairIDs;
            }
        }

        public Dictionary<int, SolarSystem> SolarSystems
        {
            get
            {
                return m_dSolarSystems;
            }
        }

        public int SystemCount
        {
            get
            {
                return m_dSolarSystems.Count;
            }
        }

        public Vector3[] SystemVboContent
        {
            get
            {
                return _systemVboContent;
            }
        }

        public int[] SystemColorVaoContent
        {
            get
            {
                return _systemColorVaoContent;
            }
        }

        public int[] SystemVaoContent
        {
            get
            {
                return _systemVaoContent;
            }
        }

        public bool IsSystemVboDirty { get; set; }
        public bool IsSystemVaoDirty { get; set; }
        public bool IsSystemColorVaoDirty { get; set; }

        public bool AllVbOsClean
        {
            get
            {
                return !(IsSystemVboDirty || IsSystemVaoDirty || IsSystemColorVaoDirty || IsConnectionVboDirty || IsConnectionVaoDirty || IsConnectionColorVaoDirty);
            }
        }

        public bool IsSystemVboDataDirty { get; set; }
        public bool IsSystemVaoDataDirty { get; set; }
        public bool IsSystemColorVaoDataDirty { get; set; }

        public bool IsDataClean
        {
            get
            {
                return !(IsSystemVboDataDirty || IsSystemVaoDataDirty || IsSystemColorVaoDataDirty || IsConnectionVboDataDirty || IsConnectionVaoDataDirty || IsConnectionColorVaoDataDirty);
            }
        }
        #endregion Properties

        #region Methods

        List<int> _alertSystems = new List<int>();
        List<int> _highlightSystems = new List<int>();

        Dictionary<int, PathInfo> _pathCache = new Dictionary<int, PathInfo>();

        public Dictionary<int, PathInfo> PathCache
        {
            get
            {
                return _pathCache;
            }
        }

        #region Animation State Setting

        public void AddAlert(int systemId)
        {
            if (!_alertSystems.Contains(systemId))
            {
                foreach (var highlightSystem in _highlightSystems)
                {
                    m_dSolarSystems[highlightSystem].ResetHighlight();
                }

                _highlightSystems.Clear();

                _alertSystems.Add(systemId);
                m_dSolarSystems[systemId].StartAlert();
                if ((!_pathfindingCache.ContainsKey(GenerateUniquePathId(_homeSystemId, systemId))) && (_homeSystemId != -1))
                    FindAndCachePath(_homeSystemId, systemId);
                _areUniformsClean = false;
            }

            // Update system alert stats
            if (_systemStats.ContainsKey(systemId))
            {
                _systemStats[systemId].Update();
            }
            else
            {
                _systemStats.Add(systemId, new SystemStats(systemId));
            }

            // If the list of red crosshairs alread contains the
            // new alert system move it to the back of the queue
            if (_redCrossHairIDs.Contains(systemId) && _redCrossHairIDs.Count > 1)
            {
                var tempQueue = new Queue<int>();

                while (_redCrossHairIDs.Count > 0)
                {
                    var queueSystem = _redCrossHairIDs.Dequeue();

                    if (queueSystem != systemId)
                        tempQueue.Enqueue(queueSystem);
                }

                _redCrossHairIDs = tempQueue;
            }
               
            _redCrossHairIDs.Enqueue(systemId);

            while (_redCrossHairIDs.Count > _maxAlerts)
            {
                var tempSystem = _redCrossHairIDs.Dequeue();
                if (_systemStats.ContainsKey(tempSystem))
                    _systemStats[tempSystem].Expired = true;
            }
        }

        public int GenerateUniquePathId(int fromSystemId, int toSystemId)
        {
            return (fromSystemId * 10000) + toSystemId;
        }

        public SolarSystemPathFinder PathFinder { get; private set; }

        public void AddGreenCrossHair(int systemId)
        {
            _greenCrossHairIDs.Enqueue(systemId);

            while (_greenCrossHairIDs.Count > 10)
                _greenCrossHairIDs.Dequeue();
        }

        public void AddHighlight(int systemId, bool flash = false)
        {
            if ((!_highlightSystems.Contains(systemId)) && (!SolarSystems[systemId].IsAlerting))
            {
                _highlightSystems.Add(systemId);
                m_dSolarSystems[systemId].StartHighlight(flash);
                _areUniformsClean = false;
            }
        }

        public void RemoveHighlight(int systemId)
        {
            if (_highlightSystems.Contains(systemId))
            {
                m_dSolarSystems[systemId].HighlightState = AnimationState.Shrinking;
            }
        }

        private int[] _uniSystemIDs;
        private Color4[] _uniColors;
        private float[] _uniSizes;

        private bool _areUniformsClean;
        public bool AreUniformsClean
        {
            get
            {
                return _areUniformsClean;
            }
        }

        public int[] UniformSystems
        {
            get
            {
                return _uniSystemIDs;
            }
        }

        public Color4[] UniformColors
        {
            get
            {
                return _uniColors;
            }
        }

        public float[] UniformSizes
        {
            get
            {
                return _uniSizes;
            }
        }

        public void BuildUniforms()
        {
            var totalSystemCount = _alertSystems.Count + _highlightSystems.Count;

            var systemCount = totalSystemCount > 10 ? 10 : totalSystemCount;

            _uniSystemIDs = new int[10];
            _uniColors = new Color4[10];
            _uniSizes = new float[10];

            var i = 0;

            if (_alertSystems.Count <= 10)
            {
                foreach (var systemId in _alertSystems)
                {
                    _uniSystemIDs[i] = systemId;
                    _uniSizes[i] = m_dSolarSystems[systemId].DrawSize;
                    _uniColors[i] = m_dSolarSystems[systemId].DrawColorC4;
                    i++;
                }
            }
            else
            {
                var resetAt = _alertSystems.Count - 10;
                var start = false;

                i = 0;
                foreach (var systemId in _alertSystems)
                {
                    if ((i == resetAt) && !start)
                    {
                        i = 0;
                        start = true;
                    }

                    if (start)
                    {
                        _uniSystemIDs[i] = systemId;
                        _uniSizes[i] = m_dSolarSystems[systemId].DrawSize;
                        _uniColors[i] = m_dSolarSystems[systemId].DrawColorC4;
                    }
                    i++;
                }
            }
        
            if (i < systemCount)
            {
                foreach (var systemId in _highlightSystems.Where(systemId => !_alertSystems.Contains(systemId)))
                {
                    _uniSystemIDs[i] = systemId;
                    _uniSizes[i] = m_dSolarSystems[systemId].DrawSize;
                    _uniColors[i] = m_dSolarSystems[systemId].DrawColorC4;
                    i++;

                    if (i == systemCount)
                        break;
                }
            }

            while (i < 10)
            {
                _uniSystemIDs[i] = -1;
                _uniColors[i] = Color4.White;
                _uniSizes[i] = 0;
                i++;
            }

            _areUniformsClean = true;
        }


        private void ProcessTick()
        {
            var remove = (from tempSystem in _alertSystems let okToRemove = m_dSolarSystems[tempSystem].ProcessTick() where okToRemove select tempSystem).ToList();

            if (remove.Count > 0)
            {
                _areUniformsClean = false;
                foreach (var tempSystem in remove)
                {
                    m_dSolarSystems[tempSystem].ClearAlert();
                    _alertSystems.Remove(tempSystem);
                }
            }
            remove.Clear();

            remove.AddRange(from tempSystem in _highlightSystems let okToRemove = m_dSolarSystems[tempSystem].ProcessTick() where okToRemove select tempSystem);

            if (remove.Count > 0)
            {
                foreach (var tempSystem in remove)
                {
                    m_dSolarSystems[tempSystem].ClearHighlight();
                    _highlightSystems.Remove(tempSystem);
                }
            }
            _areUniformsClean = false;
            remove.Clear();
        }
        #endregion Animation State Setting

        private Dictionary<string, int> _names = new Dictionary<string, int>();

        public Dictionary<string, int> Names
        {
            get
            {
                return _names;
            }
        }

        #region Pathfinding
        private SolarSystemData[] _pathFindingData;

        public PathInfo FindPath(int fromSystemId, int toSystemId)
        {
            if ((fromSystemId >= 0) && (toSystemId >= 0))
                return PathFinder.FindPath(fromSystemId, toSystemId);

            return null;
        }
        #endregion Pathfinding

        private AutoCompleteStringCollection _nameStringCollection = new AutoCompleteStringCollection();

        public AutoCompleteStringCollection NameStringCollection
        {
            get
            {
                return _nameStringCollection;
            }
        }


        #region Data
        public bool LoadSystemData(SolarSystemData[] data)
        {
            _pathFindingData = data;

            PathFinder = new SolarSystemPathFinder(_pathFindingData);
            _okToProcessPaths = true;

            foreach (SolarSystemData temp in data)
            {
                var tempSystem = new SolarSystem(temp.NativeId, temp.Name, temp.X, temp.Y, temp.Z)
                {
                    ConnectedTo = new List<SolarSystemConnection>()
                };

                if (temp.ConnectedTo != null)
                {
                    foreach (SolarSystemConnectionData tempConnData in temp.ConnectedTo)
                    {
                        var tempConn = new SolarSystemConnection(tempConnData.ToSystemNativeId, tempConnData.IsRegional);

                        tempSystem.ConnectedTo.Add(tempConn);
                    }
                }

                _nameStringCollection.Add(tempSystem.Name);
                _names.Add(tempSystem.Name, temp.NativeId);
                m_dSolarSystems.Add(temp.NativeId, tempSystem);

            }

            return m_dSolarSystems.Count > 0;
        }
        #endregion Data

        #region VBO Initialisation

        #region Systems
        public bool InitVboData()
        {
            InitSystemVboContent();
            InitSystemVaoContent();
            InitSystemColorVaoContent();
            ExtractConnections();

            if ( AllVbOsClean )
                return true;
            else
                return false;
        }

        private void InitSystemVboContent()
        {
            _systemVboContent = new Vector3[SystemCount];

            var i = 0;
            foreach (var tempSystem in m_dSolarSystems.Values)
            {
                _systemVboContent[i++] = tempSystem.Xyz;
            }

            IsSystemVboDirty = _systemVboContent.Length != SystemCount;
        }

        private void InitSystemVaoContent()
        {
            _systemVaoContent = new int[SystemCount];

            for (var i = 0; i < SystemCount; i++ )
            {
                _systemVaoContent[i] = i;
            }

            IsSystemVaoDirty = _systemVaoContent.Length != SystemCount;
        }

        private void InitSystemColorVaoContent()
        {
            _systemColorVaoContent = new int[SystemCount];

            for (var i = 0; i < SystemCount; i++ )
            {
                _systemColorVaoContent[i] = Utility.ColorToRgba32(SolarSystem.DefaultDrawColor);
            }

            IsSystemColorVaoDirty = _systemColorVaoContent.Length != SystemCount;
        }
        #endregion Systems

        public bool IsConnectionVboDataDirty { get; set; }
        public bool IsConnectionVaoDataDirty { get; set; }
        public bool IsConnectionColorVaoDataDirty { get; set; }

        public bool IsConnectionVboDirty { get; set; }
        public bool IsConnectionVaoDirty { get; set; }
        public bool IsConnectionColorVaoDirty { get; set; }

        public int ConnectionVertexCount { get; private set; }

        #region Connections
        private void ExtractConnections()
        {
            var connectionCount = 0;
            foreach (var tempSystem in m_dSolarSystems)
            {
                var system = tempSystem.Value;
                connectionCount += system.ConnectedTo.Count();
            }

            ConnectionVertexCount = connectionCount * 2;

            if (ConnectionVboContent == null)
            {
                ConnectionVboContent = new Vector3[ConnectionVertexCount];
            }
            if (ConnectionVaoContent == null)
            {
                ConnectionVaoContent = new int[ConnectionVertexCount];
            }
            if (ConnectionColorVaoContent == null)
            {
                ConnectionColorVaoContent = new Vector4[ConnectionVertexCount];
            }
            Array.Clear(ConnectionVboContent, 0, ConnectionVboContent.Length);
            Array.Clear(ConnectionVaoContent, 0, ConnectionVaoContent.Length);
            Array.Clear(ConnectionColorVaoContent, 0, ConnectionColorVaoContent.Length);

            var i = 0;
            foreach (var tempSystem in m_dSolarSystems)
            {
                var system = tempSystem.Value;
                foreach (var connection in system.ConnectedTo)
                {
                    ConnectionVboContent[i] = new Vector3(
                        system.Xf,
                        system.Yf,
                        system.Zf);
                    ConnectionVaoContent[i] = i;

                    if (connection.IsRegional)
                    {
                        ConnectionColorVaoContent[i] = kRedVector;
                    }
                    else
                    {
                        ConnectionColorVaoContent[i] = kBlueVector;
                    }

                    i++;
                    ConnectionVboContent[i] = new Vector3(
                        m_dSolarSystems[connection.ToSystemNativeId].Xf,
                        m_dSolarSystems[connection.ToSystemNativeId].Yf,
                        m_dSolarSystems[connection.ToSystemNativeId].Zf);
                    ConnectionVaoContent[i] = i;
                    ConnectionColorVaoContent[i] = ConnectionColorVaoContent[i - 1];
                    i++;
                }
            }
            IsConnectionVboDataDirty = false;
            IsConnectionVaoDataDirty = false;
            IsConnectionColorVaoDataDirty = false;
        }

        public Vector3[] ConnectionVboContent { get; private set; }

        public Vector4[] ConnectionColorVaoContent { get; private set; }

        public int[] ConnectionVaoContent { get; private set; }

        #endregion Connections
        #endregion VBO Initialisation

        #region VBO Refresh
        public bool RefreshVboData()
        {
            RefreshSystemVboData();
            RefreshSystemVaoData();
            RefreshSystemColorVaoData();
            RefreshConnectionVboData();
            RefreshConnectionVaoData();
            RefreshConnectionColorVaoData();

            return IsDataClean;
        }

        public void RefreshSystemVboData()
        {
            if (!IsSystemVboDataDirty)
                return;
            for (var i = 0; i < SystemCount; i++)
            {
                _systemVboContent[i] = m_dSolarSystems.ElementAt(i).Value.Xyz;
            }

            IsSystemVboDataDirty = false;
            IsSystemVboDirty = true;
        }

        public void RefreshSystemVaoData()
        {
            if (!IsSystemVaoDataDirty)
                return;
            for (var i = 0; i < SystemCount; i++)
            {
                _systemVaoContent[i] = i;
            }

            IsSystemVaoDataDirty = false;
            IsSystemVaoDirty = true;
        }

        public void RefreshSystemColorVaoData()
        {
            if (!IsSystemColorVaoDataDirty)
                return;
            for (var i = 0; i < SystemCount; i++)
            {
                _systemColorVaoContent[i] = m_dSolarSystems.ElementAt(i).Value.DrawColorArgb32;
            }

            IsSystemColorVaoDataDirty = false;
            IsSystemColorVaoDirty = true;
        }

        public void RefreshConnectionVboData()
        {
            if (!IsConnectionVboDataDirty)
                return;
            if (ConnectionVboContent == null)
            {
                ConnectionVboContent = new Vector3[ConnectionVertexCount];
            }
            Array.Clear(ConnectionVboContent, 0, ConnectionVboContent.Length);

            var i = 0;
            foreach (var tempSystem in m_dSolarSystems)
            {
                var system = tempSystem.Value;
                foreach (var connection in system.ConnectedTo)
                {
                    ConnectionVboContent[i] = new Vector3(
                        system.Xf,
                        system.Yf,
                        system.Zf);
                    i++;
                    ConnectionVboContent[i] = new Vector3(
                        m_dSolarSystems[connection.ToSystemNativeId].Xf,
                        m_dSolarSystems[connection.ToSystemNativeId].Yf,
                        m_dSolarSystems[connection.ToSystemNativeId].Zf);
                    i++;
                }
            }
            
            IsConnectionVboDataDirty = false;
            IsConnectionVboDirty = true;
        }

        public void RefreshConnectionVaoData()
        {
            if (!IsConnectionVaoDataDirty)
                return;
            if (ConnectionVaoContent == null)
            {
                ConnectionVaoContent = new int[ConnectionVertexCount];
            }
            Array.Clear(ConnectionVaoContent, 0, ConnectionVaoContent.Length);
            for (var i = 0; i < ConnectionVertexCount; i++)
            {
                ConnectionVaoContent[i] = i;
            }

            IsConnectionVaoDataDirty = false;
            IsConnectionVaoDirty = true;
        }

        public void RefreshConnectionColorVaoData()
        {
            if (!IsConnectionColorVaoDataDirty)
                return;
            if (ConnectionColorVaoContent == null)
            {
                ConnectionColorVaoContent = new Vector4[ConnectionVertexCount];
            }
            Array.Clear(ConnectionColorVaoContent, 0, ConnectionColorVaoContent.Length);

            var i = 0;
            foreach (var tempSystem in m_dSolarSystems)
            {
                var system = tempSystem.Value;
                foreach (var connection in system.ConnectedTo)
                {
                    //todo - make this an option to highlight the nearest routes not the default
                    var nCurrentTrackedSystem = _CurrentCameraCenteredSystemId;
                    if (tempSystem.Key == nCurrentTrackedSystem || connection.ToSystemNativeId == nCurrentTrackedSystem)
                    {
                        ConnectionColorVaoContent[i] = kGreenVector;
                    }
                    else
                    {
                        if (connection.IsRegional)
                        {
                            ConnectionColorVaoContent[i] = kRedVector;
                        }
                        else
                        {
                            ConnectionColorVaoContent[i] = kBlueVector;
                        }
                    }
                    i++;
                    ConnectionColorVaoContent[i] = ConnectionColorVaoContent[i - 1];
                    i++;
                }
            }
            IsConnectionColorVaoDataDirty = false;
            IsConnectionColorVaoDirty = true;
        }
        //rgb
        private Vector4 kGreenVector = new Vector4(0.0f / 255.0f, 250.0f / 255.0f, 20.0f / 255.0f, 1f); //0,250,20
        private Vector4 kBrightRedVector = new Vector4(255.0f / 255.0f, 0.0f / 255.0f, 30.0f / 255.0f, 1f); //255,0,30
        private Vector4 kRedVector = new Vector4(85.0f / 255.0f, 0.0f / 255.0f, 20.0f / 255.0f, 1f); //85,0,20
        private Vector4 kBlueVector = new Vector4(10.0f / 255.0f, 0.0f / 255.0f, 120.0f / 255.0f, 1f); //10,0,120
        #endregion VBO Refresh
        #endregion Methods

        private PathInfo _workingPathInfo;
        private Queue<PathInfo> _pathfindingQueue = new Queue<PathInfo>();
        private BackgroundWorker _pathFindingWorker = new BackgroundWorker();
        private Dictionary<int, PathInfo> _pathfindingCache = new Dictionary<int, PathInfo>();
        private bool _processingPath, _okToProcessPaths;

        public Dictionary<int, PathInfo> PathFindingCache
        {
            get
            {
                return _pathfindingCache;
            }
        }

        public void FindAndCachePath(int fromSystemId, int toSystemId)
        {
            PathInfo tempPath = new PathInfo
            {
                FromSystem = fromSystemId,
                ToSystem = toSystemId
            };

            if (!_pathfindingCache.ContainsKey(tempPath.PathId))
                _pathfindingQueue.Enqueue(tempPath);
        }

        public bool IsProcessingPaths
        {
            get { return _pathfindingQueue.Count > 0; }
        }

        public void ProcessPathfindingQueue()
        {
            if (_okToProcessPaths)
            {
                if (!_processingPath)
                {
                    _processingPath = true;
                    if (_pathfindingQueue.Count > 0)
                    {
                        _workingPathInfo = _pathfindingQueue.Dequeue();
                        _pathFindingWorker.RunWorkerAsync(_workingPathInfo);
                    }
                    else
                    {
                        _processingPath = false;
                    }
                }
            }            
        }

        void _pathFindingWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var cacheId = GenerateUniquePathId(_workingPathInfo.FromSystem, _workingPathInfo.ToSystem);

            if (!_pathfindingCache.ContainsKey(cacheId))
                _pathfindingCache.Add(cacheId, _workingPathInfo);
            _processingPath = false;
        }

        void _pathFindingWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            PathInfo returnPath = (PathInfo)e.Argument;

            PathInfo tempPath = PathFinder.FindPath(returnPath.FromSystem, returnPath.ToSystem);

            returnPath.TotalJumps = tempPath.TotalJumps;
            returnPath.PathSystems = tempPath.PathSystems;

            e.Result = tempPath;
        }

        private Dictionary<int, SystemStats> _systemStats = new Dictionary<int, SystemStats>();

        public SystemStats GetSystemStats(int systemId)
        {
            if (_systemStats.ContainsKey(systemId))
            {
                return _systemStats[systemId];
            }

            return null;
        }

        private int _maxAlertAge = 15;
        private int _maxAlerts = 15;

        public int MaxAlertAge
        {
            get { return _maxAlertAge; }
            set { _maxAlertAge = value; }
        }

        public int MaxAlerts
        { 
            get { return _maxAlerts; }
            set { _maxAlerts = value; }
        }
    }
}
