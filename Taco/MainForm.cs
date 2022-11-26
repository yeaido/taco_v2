using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Reflection;
using System.Windows.Forms;
using OpenTK;
using ProtoBuf;
using Taco.Classes;

namespace Taco
{
    public partial class MainForm : Form
    {
        #region Variables
        TacoConfig _conf = new TacoConfig(Application.StartupPath);
        TacoCharacters _characters = new TacoCharacters(Application.StartupPath);        

        private bool _isHighlighting, _zooming, m_bWatchLogs, _isFullScreen, _dragging;
        private bool _hasRendered, _configLoaded, _muteSound;
        private int _zoomTick, _highlightTick;

        private float _cameraDistance = 12000.0f;

        private int _currentHighlight = -1;
        private int _maxHighlighTick = 30;
        private int _maxZoomTick = 100;
        private int _zoomToSystemId = -1;

        private int lineCountCombinesIntel = 0;

        private Vector3 _eye, _lookAt, _zoomStart, _zoomEnd, _zoomDiff;

        private Size _oldUiContainerSize, _oldSize, _oldGlOutSize;

        private Point _oldUiContainerPosition, _oldPosition, _oldGlOutPosition;        
        
        private Dictionary<int, LogWatcher> m_dLogWatchers = new Dictionary<int, LogWatcher>();

        private Dictionary<string, SoundPlayer> _sounds = new Dictionary<string, SoundPlayer>();        
        private SoundPlayer _anomalyWatcherSound = new SoundPlayer();

        private Queue<LogEntry> _intelProcessingQueue = new Queue<LogEntry>();
        private Queue<LogEntry> _newIntelQueue = new Queue<LogEntry>();

        private SolarSystemManager _solarSystems;
        
        private HashSet<int> _stickyHighlightSystems = new HashSet<int>();
        private LocalWatcher _localWatcher = new LocalWatcher();
        private Dictionary<string, int> _charLocations = new Dictionary<string, int>();
        private bool _followingChars;
        #endregion Variables
        #region Constants
        private static int kDelveLogIndex { get { return 0; } }
        private static int kQueriousLogIndex { get { return 1; } }
        private static int kProvidenceLogIndex { get { return 2; } }
        private static int kDekleinLogIndex { get { return 3; } }
        private static int kBranchLogIndex { get { return 4; } }
        private static int kValeLogIndex { get { return 5; } }
        private static int kPureBlindLogIndex { get { return 6; } }
        private static int kFadeLogIndex { get { return 7; } }
        private static int kTenalLogIndex { get { return 8; } }
        private static int kVenalLogIndex { get { return 9; } }
        private static int kTributeLogIndex { get { return 10; } }
        private static int kGameLogIndex { get { return 11; } }
        private static int kTotalNumberOfLogs { get { return 12; } }
        private static int kHomeIndexMapRange { get { return 0; } }
        #endregion Constants        
        public MainForm()
        {
            InitializeComponent();

            _solarSystems = new SolarSystemManager();

            LoadSystemData();
            LoadSounds();
            SetupAutoComplete();
            PopulateSoundCombos();
            LoadConfig();

            glOut.MouseWheel += glOut_MouseWheel;
        }

        #region Sound
        private SoundPlayer LoadSoundFromResource(string resourceName, ref Assembly assembly)
        {
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                try
                {
                    var soundPlayer = new SoundPlayer(stream);
                    soundPlayer.Load();

                    return soundPlayer;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }
        private SoundPlayer LoadSoundFromFile(string fileName)
        {
            using (var stream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                try
                {
                    var soundPlayer = new SoundPlayer(stream);
                    soundPlayer.Load();

                    return soundPlayer;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }
        #endregion Sound

        #region Utility
        #region Configuration
        private void LoadConfig()
        {
            MonitorBranch.Checked = _conf.MonitorBranch;
            MonitorDeklein.Checked = _conf.MonitorDeklein;
            MonitorTenal.Checked = _conf.MonitorTenal;
            MonitorVenal.Checked = _conf.MonitorVenal;
            MonitorFade.Checked = _conf.MonitorFade;
            MonitorPureBlind.Checked = _conf.MonitorPureBlind;
            MonitorTribute.Checked = _conf.MonitorTribute;
            MonitorVale.Checked = _conf.MonitorVale;
            MonitorProvidence.Checked = _conf.MonitorProvidence;
            MonitorDelve.Checked = _conf.MonitorDelve;
            MonitorQuerious.Checked = _conf.MonitorQuerious;
            MonitorGameLog.Checked = _conf.MonitorGameLog;

            AlertBranch.Checked = _conf.AlertBranch;
            AlertDeklein.Checked = _conf.AlertDeklein;
            AlertTenal.Checked = _conf.AlertTenal;
            AlertVenal.Checked = _conf.AlertVenal;
            AlertFade.Checked = _conf.AlertFade;
            AlertPureBlind.Checked = _conf.AlertPureBlind;
            AlertTribute.Checked = _conf.AlertTribute;
            AlertVale.Checked = _conf.AlertVale;
            AlertProvidence.Checked = _conf.AlertProvidence;
            AlertDelve.Checked = _conf.AlertDelve;
            AlertQuerious.Checked = _conf.AlertQuerious;

            PreserveCameraDistance.Checked = _conf.PreserveCameraDistance;
            PreserveLookAt.Checked = _conf.PreserveLookAt;
            PreserveSelectedSystems.Checked = _conf.PreserveSelectedSystems;

            DisplayNewFileAlerts.Checked = _conf.DisplayNewFileAlerts;
            DisplayOpenFileAlerts.Checked = _conf.DisplayOpenFileAlerts;

            ShowCharacterLocations.Checked = _conf.ShowCharacterLocations;
            DisplayCharacterNames.Checked = _conf.DisplayCharacterNames;

            OverrideLogPath.Checked = _conf.OverrideLogPath;
            if (OverrideLogPath.Checked)
                LogPath.Text = _conf.LogPath;

            MonitorGameLog.Checked = _conf.MonitorGameLog;

            LoadAlertConfig();
            LoadIgnoreLists();
            PopulateCharacterNameCombos();

            CameraFollowCharacter.Checked = _conf.CameraFollowCharacter;
            CentreOnCharacter.SelectedIndex = _conf.CentreOnCharacter;
            MapRangeFrom.SelectedIndex = _conf.MapRangeFrom;

            ShowAlertAge.Checked = _conf.ShowAlertAge;
            ShowAlertAgeSecs.Checked = _conf.ShowAlertAgeSecs;
            ShowReportCount.Checked = _conf.ShowReportCount;
            MaxAlertAge.Value = _conf.MaxAlertAge;
            MaxAlerts.Value = _conf.MaxAlerts;

            if ((_conf.AnomalyMonitorSoundId == -1) && (_conf.AnomalyMonitorSoundPath == string.Empty))
            {
                AnomalyWatcherSound.SelectedIndex = -1;
            }
            else if ((_conf.AnomalyMonitorSoundId == -1) && (_conf.AnomalyMonitorSoundPath != string.Empty))
            {
                AnomalyWatcherSound.Items.RemoveAt(AnomalyWatcherSound.Items.Count - 1);
                AnomalyWatcherSound.Items.Add(_conf.AnomalyMonitorSoundPath);
                AnomalyWatcherSound.SelectedIndex = AnomalyWatcherSound.Items.Count - 1;
                _anomalyWatcherSound = LoadSoundFromFile(_conf.AnomalyMonitorSoundPath);
            }
            else if (_conf.AnomalyMonitorSoundId > -1)
            {
                AnomalyWatcherSound.SelectedIndex = _conf.AnomalyMonitorSoundId;
            }

            if (_conf.SelectedSystems != null)
            {
                int[] loadedSelectedSystems = _conf.SelectedSystems;

                foreach (var loadedSystem in loadedSelectedSystems)
                {
                    _stickyHighlightSystems.Add(loadedSystem);
                }
            }

            PreserveHomeSystem.Checked = _conf.PreserveHomeSystem;
            if (PreserveHomeSystem.Checked)
            {
                if (_conf.HomeSystemId != -1)
                {
                    if (_solarSystems.SolarSystems.ContainsKey(_conf.HomeSystemId))
                    {
                        _solarSystems.SetCurrentHomeSystem(_conf.HomeSystemId);
                    }
                }
            }

            EnableSplitContainerDragRender(false);
            RenderWhileDragging.Checked = _conf.RenderWhileDragging;

            PopulateCharacterList();

            //load the channel names to monitor
            QueriousIntelTextBox.Text = _conf.QueriousIntelChat;
            BranchIntelTextBox.Text = _conf.BranchIntelChat;
            DekleinIntelTextBox.Text = _conf.DekleinIntelChat;
            TenalIntelTextBox.Text = _conf.TenalIntelChat;
            VenalIntelTextBox.Text = _conf.VenalIntelChat;
            FadeIntelTextBox.Text = _conf.FadeIntelChat;
            PureBlindIntelTextBox.Text = _conf.PureBlindIntelChat;
            TributeIntelTextBox.Text = _conf.TributeIntelChat;
            ValeIntelTextBox.Text = _conf.ValeIntelChat;
            ProvidenceIntelTextBox.Text = _conf.ProvidenceIntelChat;
            DelveIntelTextBox.Text = _conf.DelveIntelChat;
            //set channel text boxes disabled or enabled
            QueriousIntelTextBox.Enabled = MonitorQuerious.Checked;
            BranchIntelTextBox.Enabled = MonitorBranch.Checked;
            DekleinIntelTextBox.Enabled = MonitorDeklein.Checked;
            TenalIntelTextBox.Enabled = MonitorTenal.Checked;
            VenalIntelTextBox.Enabled = MonitorVenal.Checked;
            FadeIntelTextBox.Enabled = MonitorFade.Checked;
            PureBlindIntelTextBox.Enabled = MonitorPureBlind.Checked;
            TributeIntelTextBox.Enabled = MonitorTribute.Checked;
            ValeIntelTextBox.Enabled = MonitorVale.Checked;
            ProvidenceIntelTextBox.Enabled = MonitorProvidence.Checked;
            DelveIntelTextBox.Enabled = MonitorDelve.Checked;

            _configLoaded = true;
        }
        private void PopulateCharacterList()
        {
            foreach (var name in _characters.Names)
            {
                CharacterList.Items.Add(name);
            }
        }
        private void FinaliseConfig()
        {
            _conf.IsFullScreen = _isFullScreen;

            if (!_conf.IsFullScreen)
            {
                _conf.WindowPositionX = Left;
                _conf.WindowPositionY = Top;
                _conf.WindowSizeX = Width;
                _conf.WindowSizeY = Height;
            }
            else
            {
                _conf.WindowPositionX = _oldPosition.X;
                _conf.WindowPositionY = _oldPosition.Y;
                _conf.WindowSizeX = _oldSize.Width;
                _conf.WindowSizeY = _oldSize.Height;
            }

            if (_conf.PreserveHomeSystem)
                _conf.HomeSystemId = _solarSystems.HomeSystemId;

            if (_conf.PreserveCameraDistance)
                _conf.CameraDistance = _cameraDistance;

            if (_conf.PreserveLookAt)
            {
                _conf.LookAtX = _lookAt.X;
                _conf.LookAtY = _lookAt.Y;
            }

            if (_conf.OverrideLogPath)
            {
                _conf.LogPath = LogPath.Text;
            }
        }
        private void SaveStickySystems()
        {
            int[] temp = new int[_stickyHighlightSystems.Count];
            _stickyHighlightSystems.CopyTo(temp);
            _conf.SelectedSystems = temp;
        }        
        private void PopulateCharacterNameCombos()
        {
            if (_conf.CharacterList.Count > 0)
            {
                RangeAlertCharacter.Items.AddRange(_conf.CharacterList.ToArray());
                RangeAlertCharacter.Invalidate();

                CentreOnCharacter.Items.AddRange(_conf.CharacterList.ToArray());
                CentreOnCharacter.Invalidate();

                MapRangeFrom.Items.AddRange(_conf.CharacterList.ToArray());
                MapRangeFrom.Invalidate();

                foreach (var character in _conf.CharacterList)
                {
                    followMenuItem.DropDownItems.Add(new ToolStripMenuItem(character));
                    mapRangeFromMenuItem.DropDownItems.Add(new ToolStripMenuItem(character));
                    anomalyMonitorMenuItem.DropDownItems.Add(new ToolStripMenuItem(character));
                }
            }
        }
        private void AddNewCharacter(string characterName)
        {
            if (characterName.Trim().Length == 0) return;

            _conf.AddCharacter(characterName);

            RangeAlertCharacter.Items.Add(characterName);
            RangeAlertCharacter.Invalidate();

            CentreOnCharacter.Items.Add(characterName);
            CentreOnCharacter.Invalidate();

            MapRangeFrom.Items.Add(characterName);
            MapRangeFrom.Invalidate();

            followMenuItem.DropDownItems.Add(new ToolStripMenuItem(characterName));
            mapRangeFromMenuItem.DropDownItems.Add(new ToolStripMenuItem(characterName));
            anomalyMonitorMenuItem.DropDownItems.Add(new ToolStripMenuItem(characterName));

            WriteIntel("sys", " > New character found: " + characterName);
        }
        #endregion Configuration

        #region Log Processing
        void logFile_ProcessNewData(object sender, ProcessNewDataEventArgs e)
        {
            if (!_hasRendered)
                return;

            LogEntry entry = e.LogEntry;

            if (entry == null || entry.PlayerName == "EVE System") return;


            if (entry.EntryType != LogEntryType.ChatEvent && entry.EntryType != LogEntryType.UnknownChatLogEvent && entry.EntryType != LogEntryType.UnknownGameLogEvent)
            {
                if (!_conf.CharacterList.Contains(entry.CharacterName))
                {
                    AddNewCharacter(entry.CharacterName);
                }

                switch (entry.EntryType)
                {
                    case LogEntryType.OpenChatLogEvent:
                        if (_conf.DisplayOpenFileAlerts)
                            WriteIntel(e.LogPrefix, " > Chat log opened: (" + entry.CharacterName + ") " + entry.FileName);
                        return;
                    case LogEntryType.NewChatLogEvent:
                        if (_conf.DisplayNewFileAlerts)
                            WriteIntel(e.LogPrefix, " > New chat log detected: (" + entry.CharacterName + ") " + entry.FileName);
                        return;
                    case LogEntryType.OpenGameLogEvent:
                        if (_conf.DisplayOpenFileAlerts)
                            WriteIntel(e.LogPrefix, " > Game log opened: (" + entry.CharacterName + ") " + entry.FileName);
                        return;
                    case LogEntryType.NewGameLogEvent:
                        if (_conf.DisplayNewFileAlerts)
                            WriteIntel(e.LogPrefix, " > New game log detected: (" + entry.CharacterName + ") " + entry.FileName);
                        return;
                }
                return;
            }

            if (!entry.ParseSuccess) return;

            var matchIds = new HashSet<int>();

            if (!_ignoreStrings.Any(ignoreString => ignoreString.IsMatch(entry.LineContent)))
            {
                if (entry.LogType == LogFileType.Chat)
                {
                    foreach (
                        var tempSystem in
                            _solarSystems.SolarSystems.Values.Where(
                                tempSystem => tempSystem.MatchNameRegex(entry.LineContent))
                                .Where(tempSystem => !_ignoreSystems.Contains(_solarSystems.Names[tempSystem.Name])))
                    {
                        matchIds.Add(_solarSystems.Names[tempSystem.Name]);
                    }

                    foreach (var matchId in matchIds)
                    {
                        // Home system path
                        var fromSystem = _solarSystems.HomeSystemId;
                        var toSystem = matchId;
                        var pathId = _solarSystems.GenerateUniquePathId(fromSystem, toSystem);

                        // Queue the path from home system for pathfinding if needed
                        if (!_solarSystems.PathCache.ContainsKey(pathId))
                            _solarSystems.FindAndCachePath(fromSystem, toSystem);

                        // Character paths
                        var tempLocations = BuildCharacterLocationIndex();

                        foreach (var locationId in tempLocations.Keys)
                        {
                            pathId = _solarSystems.GenerateUniquePathId(locationId, toSystem);

                            // Queue for pathfinding if needed
                            if (!_solarSystems.PathCache.ContainsKey(pathId))
                                _solarSystems.FindAndCachePath(locationId, toSystem);
                        }

                        foreach (
                            var tempTrigger in
                                _alertTriggers.Where(
                                    tempTrigger =>
                                        tempTrigger.Enabled &&
                                        (tempTrigger.RangeTo == RangeAlertType.System ||
                                         tempTrigger.RangeTo == RangeAlertType.Character)))
                        {
                            // Range from system alert
                            if (tempTrigger.RangeTo == RangeAlertType.System)
                            {
                                fromSystem = tempTrigger.SystemId;
                            }
                            else // Character alert
                            {
                                continue;
                            }


                            pathId = _solarSystems.GenerateUniquePathId(fromSystem, toSystem);

                            // Queue the path for pathfinding if needed
                            if (!_solarSystems.PathCache.ContainsKey(pathId))
                                _solarSystems.FindAndCachePath(fromSystem, toSystem);
                        }
                    }

                    // Process ZoomTo and log non-triggering intel if needed
                    if (matchIds.Count > 0)
                    {
                        var zoomToSystem = -1;

                        // Check for any systems in the log entry
                        foreach (var matchId in matchIds)
                        {
                            _solarSystems.AddAlert(matchId);

                            // Set the zoom to system to the final system
                            // in an entry if multiple systems are found
                            zoomToSystem = matchId;
                        }

                        // Check if we need to zoom to a found system based 
                        // on which intel channel the log entry comes from
                        if (CheckZoomTo(entry.LogPrefix))
                            ZoomTo(zoomToSystem);
                    }
                }
            }

            entry.MatchedIds = matchIds;
            _newIntelQueue.Enqueue(entry);
        }
        void logFile_ProcessCombat(object sender, ProcessCombatEventArgs e)
        {
            if (e.CombatEvent == CombatEventType.Start)
            {
                foreach (
                    var dropDownItem in
                        anomalyMonitorMenuItem.DropDownItems.Cast<ToolStripMenuItem>()
                            .Where(dropDownItem => dropDownItem.Text == e.CharacterName && dropDownItem.Checked))
                {
                    WriteIntel("sys", " > Anomaly Monitor | Combat started: " + dropDownItem.Text);
                }
            }
            else if (e.CombatEvent == CombatEventType.Stop)
            {
                foreach (
                    var dropDownItem in
                        anomalyMonitorMenuItem.DropDownItems.Cast<ToolStripMenuItem>()
                            .Where(dropDownItem => dropDownItem.Text == e.CharacterName && dropDownItem.Checked))
                {
                    WriteIntel("sys", " > Anomaly Monitor | Combat finished: " + dropDownItem.Text);
                    PlayAnomalyWatcherSound();
                }
            }
        }                
        private LogWatcher StartNewLogWatcher(string prefix)
        {
            LogFileType logFileType = LogFileType.Chat;
            if ("gme" == prefix)
            {
                logFileType = LogFileType.Game;
            }

            string logFilePath = "";
            if (OverrideLogPath.Checked)
            {
                logFilePath = LogPath.Text;
            }
            var tempWatcher = new LogWatcher(prefix, logFileType, logFilePath);
            tempWatcher.ProcessNewData += logFile_ProcessNewData;
            tempWatcher.ProcessCombat += logFile_ProcessCombat;
            bool bStarted = tempWatcher.StartWatch();
            if (bStarted)
            {
                return tempWatcher;
            }
            return null;
        }
        private bool StartChatLog(int iLog)
        {
            if (GetIntelCheckboxFromLogIndex(iLog).Checked)
            {
                TextBox tempTextbox = GetIntelTextBoxFromLogIndex(iLog);
                LogWatcher tempWatcher = StartNewLogWatcher(GetIntelChannelFromLogIndex(iLog));
                if (null != tempWatcher)
                {  //intel file opened successfully, add to dictionary
                    m_dLogWatchers.Add(iLog, tempWatcher);
                    if (tempTextbox != null)
                    {  //now set the textbox to green so the user knows a file has been found
                        tempTextbox.BackColor = Color.LightGreen;
                    }
                    return true;
                }
                else
                {  //otherwise, set the checkbox back to false
                    if (tempTextbox != null)
                    {  //and let the user know that we tried and failed to find a file
                        tempTextbox.BackColor = Color.IndianRed;
                    }
                }
            }
            return false;
        }
        private void StopChatLog(int iLog)
        {
            if (m_dLogWatchers.ContainsKey(iLog))
            {
                m_dLogWatchers[iLog].ProcessNewData -= logFile_ProcessNewData;
                m_dLogWatchers[iLog].ProcessCombat -= logFile_ProcessCombat;
                m_dLogWatchers[iLog].StopWatch();
                if (m_dLogWatchers.Remove(iLog))
                {
                    TextBox tempTextbox = GetIntelTextBoxFromLogIndex(iLog);
                    if (tempTextbox != null)
                    {
                        tempTextbox.BackColor = Color.Empty;
                    }
                }
            }
        }
        private CheckBox GetIntelCheckboxFromLogIndex(int index)
        {
            if (kDelveLogIndex == index)
            {
                return MonitorDelve;
            }
            else if (kQueriousLogIndex == index)
            {
                return MonitorQuerious;
            }
            else if (kProvidenceLogIndex == index)
            {
                return MonitorProvidence;
            }
            else if (kDekleinLogIndex == index)
            {
                return MonitorDeklein;
            }
            else if (kBranchLogIndex == index)
            {
                return MonitorBranch;
            }
            else if (kValeLogIndex == index)
            {
                return MonitorVale;
            }
            else if (kPureBlindLogIndex == index)
            {
                return MonitorPureBlind;
            }
            else if (kFadeLogIndex == index)
            {
                return MonitorFade;
            }
            else if (kTenalLogIndex == index)
            {
                return MonitorTenal;
            }
            else if (kVenalLogIndex == index)
            {
                return MonitorVenal;
            }
            else if (kTributeLogIndex == index)
            {
                return MonitorTribute;
            }
            else if (kGameLogIndex == index)
            {
                return MonitorGameLog;
            }
            return null;
        }
        private string GetIntelChannelFromLogIndex(int index)
        {
            if (kDelveLogIndex == index)
            {
                return _conf.DelveIntelChat;
            }
            else if (kQueriousLogIndex == index)
            {
                return _conf.QueriousIntelChat;
            }
            else if (kProvidenceLogIndex == index)
            {
                return _conf.ProvidenceIntelChat;
            }
            else if (kDekleinLogIndex == index)
            {
                return _conf.DekleinIntelChat;
            }
            else if (kBranchLogIndex == index)
            {
                return _conf.BranchIntelChat;
            }
            else if (kValeLogIndex == index)
            {
                return _conf.ValeIntelChat;
            }
            else if (kPureBlindLogIndex == index)
            {
                return _conf.PureBlindIntelChat;
            }
            else if (kFadeLogIndex == index)
            {
                return _conf.FadeIntelChat;
            }
            else if (kTenalLogIndex == index)
            {
                return _conf.TenalIntelChat;
            }
            else if (kVenalLogIndex == index)
            {
                return _conf.VenalIntelChat;
            }
            else if (kTributeLogIndex == index)
            {
                return _conf.TributeIntelChat;
            }
            else if (kGameLogIndex == index)
            {
                return "gme";
            }
            return null;
        }
        private TextBox GetIntelTextBoxFromLogIndex(int index)
        {
            if (kDelveLogIndex == index)
            {
                return DelveIntelTextBox;
            }
            else if (kQueriousLogIndex == index)
            {
                return QueriousIntelTextBox;
            }
            else if (kProvidenceLogIndex == index)
            {
                return ProvidenceIntelTextBox;
            }
            else if (kDekleinLogIndex == index)
            {
                return DekleinIntelTextBox;
            }
            else if (kBranchLogIndex == index)
            {
                return BranchIntelTextBox;
            }
            else if (kValeLogIndex == index)
            {
                return ValeIntelTextBox;
            }
            else if (kPureBlindLogIndex == index)
            {
                return PureBlindIntelTextBox;
            }
            else if (kFadeLogIndex == index)
            {
                return FadeIntelTextBox;
            }
            else if (kTenalLogIndex == index)
            {
                return TenalIntelTextBox;
            }
            else if (kVenalLogIndex == index)
            {
                return VenalIntelTextBox;
            }
            else if (kTributeLogIndex == index)
            {
                return TributeIntelTextBox;
            }
            return null;
        }
        private bool CheckZoomTo(string logPrefix)
        {
            if (!_conf.CameraFollowCharacter)
            {
                if (logPrefix == _conf.QueriousIntelChat)
                {
                    if (_conf.AlertQuerious)
                    {
                        return true;
                    }
                }
                else if (logPrefix == _conf.BranchIntelChat)
                {
                    if (_conf.AlertBranch)
                    {
                        return true;
                    }
                }
                else if (logPrefix == _conf.DekleinIntelChat)
                {
                    if (_conf.AlertDeklein)
                    {
                        return true;
                    }
                }
                else if (logPrefix == _conf.TenalIntelChat)
                {
                    if (_conf.AlertTenal)
                    {
                        return true;
                    }
                }
                else if (logPrefix == _conf.VenalIntelChat)
                {
                    if (_conf.AlertVenal)
                    {
                        return true;
                    }
                }
                else if (logPrefix == _conf.FadeIntelChat)
                {
                    if (_conf.AlertFade)
                    {
                        return true;
                    }
                }
                else if (logPrefix == _conf.PureBlindIntelChat)
                {
                    if (_conf.AlertPureBlind)
                    {
                        return true;
                    }
                }
                else if (logPrefix == _conf.TributeIntelChat)
                {
                    if (_conf.AlertTribute)
                    {
                        return true;
                    }
                }
                else if (logPrefix == _conf.ValeIntelChat)
                {
                    if (_conf.AlertVale)
                    {
                        return true;
                    }
                }
                else if (logPrefix == _conf.ProvidenceIntelChat)
                {
                    if (_conf.AlertProvidence)
                    {
                        return true;
                    }
                }
                else if (logPrefix == _conf.DelveIntelChat)
                {
                    if (_conf.AlertDelve)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        private void PlayTriggerSound(AlertTrigger tempTrigger)
        {
            if (_muteSound) return;

            // If the trigger sound is a custom sound
            if (tempTrigger.SoundId == -1)
            {
                // Load and play the sound
                SoundPlayer temp = LoadSoundFromFile(tempTrigger.SoundPath);
                temp.Play();
                temp.Dispose();
            }
            else // If the trigger sound is a built in sound
            {
                // Play the built in sound
                _sounds[tempTrigger.SoundPath].Play();
            }
        }
        public string ShortPrefix(string prefix)
        {
            switch (prefix)
            {
                case "fade":
                    return "fde";
                case "pb":
                    return "pbd";
                case "vale":
                    return "vle";
                case "provi":
                    return "prv";
                case "delve":
                    return "del";
                default:
                    return prefix;
            }
        }
        public void WriteIntel(string prefix, string logLine, bool parseForSystemLinks = false, bool writingBuffer = false)
        {
            if (_bufferIntel && !writingBuffer)
            {
                var tempIntelBuffer = new IntelBuffer
                {
                    Prefix = prefix,
                    LogLine = logLine,
                    ParseForSystemLinks = parseForSystemLinks
                };

                _bufferedIntel.Enqueue(tempIntelBuffer);

                BufferingIndicator.Text = "Buffered Intel: " + _bufferedIntel.Count + " line";
                if (_bufferedIntel.Count > 1) BufferingIndicator.Text += "s";

                return;
            }

            const int logLineMaxLength = 100;

            // Trim the displayed line to the maximum line length if needed
            if (logLine.Length > logLineMaxLength)
                logLine = logLine.Substring(0, logLineMaxLength) + "...";

            var outputLine = ShortPrefix(prefix) + logLine + Environment.NewLine;

            // Write the line to the combined intel pane
            CombinedIntel.DeselectAll();
            CombinedIntel.SelectionStart = 0;
            CombinedIntel.SelectedText = outputLine;
            lineCountCombinesIntel++;

            // Link any systems in the new log line if needed
            if (parseForSystemLinks)
            {
                foreach (
                    var tempSystem in
                        _solarSystems.SolarSystems.Values.Where(
                            tempSystem => tempSystem.MatchNameRegex(logLine))
                            .Where(tempSystem => !_ignoreSystems.Contains(_solarSystems.Names[tempSystem.Name])))
                {
                    var linkPos = CombinedIntel.Find(tempSystem.Name);
                    CombinedIntel.InsertLink(tempSystem.Name, linkPos);
                }
            }

            // Link any names we know about
            foreach (var name in _characters.Names)
            {
                var nameStart = CombinedIntel.Text.IndexOf(name, 0, outputLine.Length - 1);
                if (nameStart >= 0)
                    CombinedIntel.InsertLink(name, nameStart);
            }

            // Write the line to the require channel pane
            if (prefix == _conf.QueriousIntelChat)
            {
                QueriousIntel.Text = outputLine + QueriousIntel.Text;
            }
            else if (prefix == _conf.BranchIntelChat)
            {
                BranchIntel.Text = outputLine + BranchIntel.Text;
            }
            else if (prefix == _conf.DekleinIntelChat)
            {
                DekleinIntel.Text = outputLine + DekleinIntel.Text;
            }
            else if (prefix == _conf.TenalIntelChat)
            {
                TenalIntel.Text = outputLine + TenalIntel.Text;
            }
            else if (prefix == _conf.VenalIntelChat)
            {
                VenalIntel.Text = outputLine + VenalIntel.Text;
            }
            else if (prefix == _conf.FadeIntelChat)
            {
                FadeIntel.Text = outputLine + FadeIntel.Text;
            }
            else if (prefix == _conf.PureBlindIntelChat)
            {
                PureBlindIntel.Text = outputLine + PureBlindIntel.Text;
            }
            else if (prefix == _conf.TributeIntelChat)
            {
                TributeIntel.Text = outputLine + TributeIntel.Text;
            }
            else if (prefix == _conf.ValeIntelChat)
            {
                ValeIntel.Text = outputLine + ValeIntel.Text;
            }
            else if (prefix == _conf.ProvidenceIntelChat)
            {
                ProvidenceIntel.Text = outputLine + ProvidenceIntel.Text;
            }
            else if (prefix == _conf.DelveIntelChat)
            {
                DelveIntel.Text = outputLine + DelveIntel.Text;
            }

            // Trim all intel panes to a maximum length
            const int maxTextBoxLength = 10000;
            const int maxIntelLines = 100;

            while (lineCountCombinesIntel > maxIntelLines)
            {
                var lastLineStart = CombinedIntel.Text.LastIndexOf("\n", CombinedIntel.Text.Length - 2) + 1;

                // Select the last line and delete it
                CombinedIntel.Select(lastLineStart, CombinedIntel.Text.Length - lastLineStart);
                CombinedIntel.ReadOnly = false;
                CombinedIntel.SelectedText = string.Empty;
                CombinedIntel.ReadOnly = true;
                CombinedIntel.SelectionStart = 0;
                lineCountCombinesIntel--;
            }
            if (QueriousIntel.Text.Length > maxTextBoxLength)
                QueriousIntel.Text = QueriousIntel.Text.Substring(0, maxTextBoxLength);

            if (BranchIntel.Text.Length > maxTextBoxLength)
                BranchIntel.Text = BranchIntel.Text.Substring(0, maxTextBoxLength);

            if (DekleinIntel.Text.Length > maxTextBoxLength)
                DekleinIntel.Text = DekleinIntel.Text.Substring(0, maxTextBoxLength);

            if (TenalIntel.Text.Length > maxTextBoxLength)
                TenalIntel.Text = TenalIntel.Text.Substring(0, maxTextBoxLength);

            if (VenalIntel.Text.Length > maxTextBoxLength)
                VenalIntel.Text = VenalIntel.Text.Substring(0, maxTextBoxLength);

            if (FadeIntel.Text.Length > maxTextBoxLength)
                FadeIntel.Text = FadeIntel.Text.Substring(0, maxTextBoxLength);

            if (PureBlindIntel.Text.Length > maxTextBoxLength)
                PureBlindIntel.Text = PureBlindIntel.Text.Substring(0, maxTextBoxLength);

            if (TributeIntel.Text.Length > maxTextBoxLength)
                TributeIntel.Text = TributeIntel.Text.Substring(0, maxTextBoxLength);

            if (ValeIntel.Text.Length > maxTextBoxLength)
                ValeIntel.Text = ValeIntel.Text.Substring(0, maxTextBoxLength);

            if (ProvidenceIntel.Text.Length > maxTextBoxLength)
                ProvidenceIntel.Text = ProvidenceIntel.Text.Substring(0, maxTextBoxLength);

            if (DelveIntel.Text.Length > maxTextBoxLength)
                DelveIntel.Text = DelveIntel.Text.Substring(0, maxTextBoxLength);

            CombinedIntel.SelectionStart = 0;
        }
        private void IntelUpdateTicker_Tick(object sender, EventArgs e)
        {
            // If there's nothing in the queue or we're waiting on paths to be found,
            // add any new log entries to the processing queue and stop further processing
            if ((_intelProcessingQueue.Count <= 0) || (_solarSystems.IsProcessingPaths))
            {
                while (_newIntelQueue.Count > 0)
                {
                    _intelProcessingQueue.Enqueue(_newIntelQueue.Dequeue());
                }

                return;
            }

            // Stop the update timer so it doesn't tick while we're processing
            IntelUpdateTicker.Enabled = false;

            while (_intelProcessingQueue.Count > 0)
            {
                // Dequeue the first item to process
                var entry = _intelProcessingQueue.Dequeue();

                // Write the intel to the text panels if chatlog entry
                if (entry.LogType == LogFileType.Chat)
                {
                    var jumpDisplay = "--";
                    var containsSystem = false;

                    // Get the jump range from home system to first matched system id
                    if (entry.MatchedIds.Count > 0)
                    {
                        var pathId = _solarSystems.GenerateUniquePathId(_solarSystems.HomeSystemId, entry.MatchedIds.First());
                        jumpDisplay = (_solarSystems.PathFindingCache[pathId].TotalJumps - 1).ToString().PadLeft(2, '0');
                        containsSystem = true;
                    }

                    // Write the intel to the text panels
                    WriteIntel(entry.LogPrefix, "-" + entry.LogTime + "|" + jumpDisplay + "| " + entry.PlayerName + " > " + entry.LineContent, containsSystem);
                }

                // Skip any further processing of the line if it contains an ignore string
                if (_ignoreStrings.Any(ignoreString => ignoreString.IsMatch(entry.LineContent))) continue;

                // Loop through each alert trigger
                foreach (var alertTrigger in _alertTriggers.Where(alertTrigger => alertTrigger.Enabled))
                {
                    var alertTriggered = false;

                    if (alertTrigger.Type == AlertType.Ranged)
                    {
                        // Loop through each matched system
                        foreach (var matchedId in entry.MatchedIds)
                        {
                            // Setup necessary variables for trigger processing
                            var fromSystem = new[] { -1 };
                            var toSystem = matchedId;

                            if (alertTrigger.RangeTo == RangeAlertType.Home)
                            {
                                fromSystem = new[] { _solarSystems.HomeSystemId };
                            }
                            else if (alertTrigger.RangeTo == RangeAlertType.System)
                            {
                                fromSystem = new[] { alertTrigger.SystemId };
                            }
                            else if (alertTrigger.RangeTo == RangeAlertType.Character)
                            {
                                if (_followingChars && _charLocations.ContainsKey(alertTrigger.CharacterName))
                                {
                                    fromSystem = new[] { _charLocations[alertTrigger.CharacterName] };
                                }
                                else
                                {
                                    continue;
                                }
                            }
                            else if (alertTrigger.RangeTo == RangeAlertType.AnyCharacter && _followingChars)
                            {
                                fromSystem = _charLocations.Values.Distinct().ToArray();
                            }

                            foreach (var system in fromSystem)
                            {
                                var pathId = _solarSystems.GenerateUniquePathId(system, toSystem);
                                if (_solarSystems.PathFindingCache.ContainsKey(pathId))
                                {
                                    var jumpCount = _solarSystems.PathFindingCache[pathId].TotalJumps - 1;

                                    // Process the trigger and break if alert has been triggered
                                    alertTriggered = ProcessRangeAlertTrigger(alertTrigger, entry, jumpCount);
                                }
                                else
                                {
                                    WriteIntel("sys", "-" + "|!!| Path not found: " + pathId + " Alert: " + alertTrigger);
                                }

                                if (alertTriggered)
                                    break;
                            }
                        }

                        // Stop further alert processing if an alert was triggered
                        if (alertTriggered)
                            break;
                    }
                    else // Check custom text alerts
                    {
                        if (entry.LineContent.Contains(alertTrigger.Text) && (DateTime.Now - alertTrigger.TriggerTime).TotalSeconds > alertTrigger.RepeatInterval)
                        {
                            WriteIntel(ShortPrefix(entry.LogPrefix), "-" + entry.LogTime + "> Custom Alert Match: " + alertTrigger.Text);
                            PlayTriggerSound(alertTrigger);
                            alertTrigger.TriggerTime = DateTime.Now;
                            break;
                        }
                    }
                }
            }

            // Restart the update timer
            IntelUpdateTicker.Enabled = true;
        }
        private bool ProcessRangeAlertTrigger(AlertTrigger alertTrigger, LogEntry entry, int jumpCount)
        {
            if (alertTrigger.UpperLimitOperator == RangeAlertOperator.Equal)
            {
                if (alertTrigger.UpperRange != jumpCount) return false;

                PlayTriggerSound(alertTrigger);
                WriteIntel(entry.LogPrefix, "-" + entry.LogTime + "|♦♦| " + alertTrigger);
                return true;
            }

            if (jumpCount > alertTrigger.UpperRange) return false;

            if (alertTrigger.LowerLimitOperator == RangeAlertOperator.GreaterThanOrEqual)
            {
                if (jumpCount < alertTrigger.LowerRange) return false;

                PlayTriggerSound(alertTrigger);
                WriteIntel(entry.LogPrefix, "-" + entry.LogTime + "|♦♦| " + alertTrigger);
                return true;
            }

            if (jumpCount <= alertTrigger.LowerRange) return false;

            PlayTriggerSound(alertTrigger);
            WriteIntel(entry.LogPrefix, "-" + entry.LogTime + "|♦♦| " + alertTrigger);
            return true;
        }
        #endregion Log Processing

        #region Initialize
        private void LoadSystemData()
        {
            var assembly = Assembly.GetExecutingAssembly();

            using (var file = assembly.GetManifestResourceStream("Taco.Resources.Data.systemdata.bin"))
            {
                try
                {
                    var sa = Serializer.Deserialize<SolarSystemData[]>(file);
                    _solarSystems.LoadSystemData(sa);
                    _conf.AdjustAlertSystemIDs(sa);
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }
        private void LoadSounds()
        {
            var assembly = Assembly.GetExecutingAssembly();

            _sounds.Add("1up1", LoadSoundFromResource("Taco.Resources.Sounds.1up1.wav", ref assembly));
            _sounds.Add("Boo2", LoadSoundFromResource("Taco.Resources.Sounds.Boo2.wav", ref assembly));
            _sounds.Add("KamekLaugh", LoadSoundFromResource("Taco.Resources.Sounds.KamekLaugh.wav", ref assembly));
            _sounds.Add("RedCoin2", LoadSoundFromResource("Taco.Resources.Sounds.RedCoin2.wav", ref assembly));
            _sounds.Add("RedCoin3", LoadSoundFromResource("Taco.Resources.Sounds.RedCoin3.wav", ref assembly));
            _sounds.Add("Coin", LoadSoundFromResource("Taco.Resources.Sounds.Coin.wav", ref assembly));
            _sounds.Add("Powerup", LoadSoundFromResource("Taco.Resources.Sounds.Powerup.wav", ref assembly));
            _sounds.Add("StarCoin", LoadSoundFromResource("Taco.Resources.Sounds.StarCoin.wav", ref assembly));
            _sounds.Add("SuitFly", LoadSoundFromResource("Taco.Resources.Sounds.SuitFly.wav", ref assembly));
            _sounds.Add("SuitSpin", LoadSoundFromResource("Taco.Resources.Sounds.SuitSpin.wav", ref assembly));
            _sounds.Add("Whistle", LoadSoundFromResource("Taco.Resources.Sounds.Whistle.wav", ref assembly));
        }
        private void SetupAutoComplete()
        {
            SearchSystem.AutoCompleteCustomSource = _solarSystems.NameStringCollection;
            SearchSystem.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            SearchSystem.AutoCompleteSource = AutoCompleteSource.CustomSource;
            SearchSystem.Invalidate();

            RangeAlertSystem.AutoCompleteCustomSource = _solarSystems.NameStringCollection;
            RangeAlertSystem.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            RangeAlertSystem.AutoCompleteSource = AutoCompleteSource.CustomSource;
            RangeAlertSystem.Invalidate();

            NewIgnoreSystem.AutoCompleteCustomSource = _solarSystems.NameStringCollection;
            NewIgnoreSystem.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            NewIgnoreSystem.AutoCompleteSource = AutoCompleteSource.CustomSource;
            NewIgnoreSystem.Invalidate();
        }
        private void PopulateSoundCombos()
        {
            foreach (string sound in _sounds.Keys)
            {
                RangeAlertSound.Items.Add(sound);
                CustomTextAlertSound.Items.Add(sound);
                AnomalyWatcherSound.Items.Add(sound);
            }

            RangeAlertSound.Items.Add("Custom...");
            CustomTextAlertSound.Items.Add("Custom...");
            AnomalyWatcherSound.Items.Add("Custom...");
        }
        #endregion Initialize

        #region Zoom
        private void ZoomTo(int systemId)
        {
            _solarSystems.SetCameraCenteredSystemID(systemId);

            if (_dragging)
                return;

            if (_zooming)
                StopZoom();

            _zoomStart = _lookAt;
            _zoomEnd = _solarSystems.SolarSystems[systemId].Xyz;

            _zoomDiff = _zoomEnd - _zoomStart;
            _zoomToSystemId = systemId;

            _zooming = true;
        }
        private void StopZoom()
        {
            _currentHighlight = _zoomToSystemId;
            _isHighlighting = true;
            _zooming = false;
            _zoomTick = 0;
            _zoomToSystemId = -1;
        }
        #endregion Zoom

        #region Click On Control
        private void EnableSplitContainerDragRender(bool bRender)
        {
            splitContainer1.EnableDragRender(bRender);
        }        
        private void ToggleLogWatch()
        {
            m_bWatchLogs = !m_bWatchLogs;

            if (m_bWatchLogs)
            {
                bool bOneOrMoreLogs = false;
                for(int iLog = 0; iLog < kTotalNumberOfLogs; ++iLog)
                {
                    bOneOrMoreLogs |= StartChatLog(iLog);
                }

                if (!bOneOrMoreLogs)
                {  //we're not monitoring anything, give a warning
                    MessageBox.Show("Pick channel(s) to monitor.", "Pick Channels", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    m_bWatchLogs = !m_bWatchLogs;
                    return;
                }

                LogWatchToggle.Text = "Stop Watching Logs";

                //set the manual log path text box to the actual log path once we start watching logs
                LogPath.Text = LogWatcher.RootLogsPath;

                if (_conf.ShowCharacterLocations && !_followingChars)
                    ToggleFollowChars();
            }
            else
            {
                SetAllIntelTextBoxColors(Color.Empty);
                LogWatchToggle.Text = "Start Watching Logs";

                for (int iLog = 0; iLog < kTotalNumberOfLogs; ++iLog)
                {
                    StopChatLog(iLog);
                }
                m_dLogWatchers.Clear();

                if (_followingChars)
                    ToggleFollowChars();
            }
        }
        private void ToggleFullScreen()
        {
            if (!_isFullScreen)
            {
                _oldSize = Size;
                _oldPosition = new Point(Left, Top);

                _oldGlOutSize = glOut.Size;
                _oldGlOutPosition = new Point(glOut.Left, glOut.Top);

                _oldUiContainerSize = UIContainer.Size;
                _oldUiContainerPosition = new Point(UIContainer.Left, UIContainer.Top);

                FormBorderStyle = FormBorderStyle.None;

                Left = Screen.FromControl(this).Bounds.Left;
                Top = Screen.FromControl(this).Bounds.Top;
                Width = Screen.FromControl(this).Bounds.Width;
                Height = Screen.FromControl(this).Bounds.Height;

                glOut.Top = 0;
                glOut.Left = 0;
                glOut.Width = Width - 450;
                glOut.Height = Height;

                UIContainer.Top = 0;
                UIContainer.Left = Width - 450;
                UIContainer.SplitterDistance = 65;

                TopMost = true;

                FullscreenToggle.Text = "Windowed";
                _isFullScreen = true;
            }
            else
            {
                FormBorderStyle = FormBorderStyle.Sizable;
                Size = _oldSize;
                Left = _oldPosition.X;
                Top = _oldPosition.Y;

                glOut.Size = _oldGlOutSize;
                glOut.Left = _oldGlOutPosition.X;
                glOut.Top = _oldGlOutPosition.Y;

                UIContainer.Size = _oldUiContainerSize;
                UIContainer.Left = _oldUiContainerPosition.X;
                UIContainer.Top = _oldUiContainerPosition.Y;
                UIContainer.SplitterDistance = 65;

                TopMost = false;

                FullscreenToggle.Text = "Fullscreen";
                _isFullScreen = false;
            }
        }
        private void ToggleFollowChars()
        {
            _followingChars = !_followingChars;

            if (_followingChars)
            {
                _localWatcher = new LocalWatcher();
                _localWatcher.SystemChange += _localWatcher_SystemChange;
                _localWatcher.StartWatch();
            }
            else
            {
                _localWatcher.SystemChange -= _localWatcher_SystemChange;
                _localWatcher.StopWatch();
                _localWatcher = null;
                _charLocations.Clear();
                var nHighlightPathSystemID = _conf.HomeSystemId;
                if (CameraFollowCharacter.Checked)
                {
                    nHighlightPathSystemID = -1;
                }
                _solarSystems.SetCameraCenteredSystemID(nHighlightPathSystemID);
            }
        }
        private void CrashMeRecursive()
        {
#if DEBUG
            CrashMeRecursive();
#endif
        }
        public void PlayAnomalyWatcherSound()
        {
            if (_muteSound) return;

            if (AnomalyWatcherSound.SelectedIndex == -1)
                MessageBox.Show("Pick a sound first.", "PEBKAC", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else if (AnomalyWatcherSound.SelectedIndex < AnomalyWatcherSound.Items.Count - 1)
                _sounds[(string)AnomalyWatcherSound.SelectedItem].Play();
            else if (AnomalyWatcherSound.SelectedIndex == AnomalyWatcherSound.Items.Count - 1)
                _anomalyWatcherSound.Play();
        }        
        #endregion Click On Control

        #region Window Helpers
        private void SetWindowState()
        {
            var temp = new Rectangle(_conf.WindowPositionX, _conf.WindowPositionY, _conf.WindowSizeX, _conf.WindowSizeY);

            if (!SystemInformation.VirtualScreen.IntersectsWith(temp))
            {
                _conf.WindowPositionX = 50;
                _conf.WindowPositionY = 50;
                _conf.WindowSizeX = 1253;
                _conf.WindowSizeY = 815;
                _conf.IsFullScreen = false;
            }

            PreserveWindowPosition.Checked = _conf.PreserveWindowPosition;
            if (PreserveWindowPosition.Checked)
            {
                Left = _conf.WindowPositionX;
                Top = _conf.WindowPositionY;
            }

            PreserveWindowSize.Checked = _conf.PreserveWindowSize;
            if (PreserveWindowSize.Checked)
            {
                Width = _conf.WindowSizeX;
                Height = _conf.WindowSizeY;
            }

            PreserveFullScreenStatus.Checked = _conf.PreserveFullScreenStatus;
            if (PreserveFullScreenStatus.Checked)
                if (_conf.IsFullScreen)
                    ToggleFullScreen();
        }
        private bool InputFocused()
        {
            return SearchSystem.Focused ||
                   RangeAlertSystem.Focused ||
                   RangeAlertSound.Focused ||
                   RangeAlertCharacter.Focused ||
                   CentreOnCharacter.Focused ||
                   MapRangeFrom.Focused ||
                   NewCustomAlertText.Focused ||
                   CustomTextAlertSound.Focused ||
                   NewIgnoreText.Focused ||
                   NewIgnoreSystem.Focused ||
                   BranchIntelTextBox.Focused ||
                   DekleinIntelTextBox.Focused ||
                   TenalIntelTextBox.Focused ||
                   VenalIntelTextBox.Focused ||
                   FadeIntelTextBox.Focused ||
                   PureBlindIntelTextBox.Focused ||
                   TributeIntelTextBox.Focused ||
                   ValeIntelTextBox.Focused ||
                   ProvidenceIntelTextBox.Focused ||
                   DelveIntelTextBox.Focused ||
                   QueriousIntelTextBox.Focused;
        }
        #endregion Window Helpers
        #endregion Utility        

        #region Other Window Events
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case (Keys.Q):
                    if (!InputFocused())
                    {
                        Application.Exit();
                        return true;
                    }
                    break;
                case (Keys.Escape):
                    if (_isFullScreen && !AlertList.Focused)
                        ToggleFullScreen();
                    break;
                case (Keys.H):
                    if (_isFullScreen && !InputFocused())
                    {
                        if (glOut.Width == Width - 450)
                            glOut.Width = Width;
                        else
                            glOut.Width = Width - 450;
                        glOut.Height = Height;
                    }
                    break;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
        private void MainForm_Load(object sender, EventArgs e)
        {
            SetWindowState();
        }
        private void MainForm_Deactivate(object sender, EventArgs e)
        {
            glOut.Focus();
        }
        private void UIContainer_Panel1_Resize(object sender, EventArgs e)
        {
            UIContainer.SplitterDistance = 65;
            SetChannelTabSize();
        }
        private void Ticker_Tick(object sender, EventArgs e)
        {
            if (!_hasRendered)
                return;


            if (_isHighlighting)
                _highlightTick++;
            else
                _highlightTick = 0;

            if (_isHighlighting && (_highlightTick > _maxHighlighTick))
            {
                _solarSystems.AddHighlight(_currentHighlight);
            }

            if (_zooming)
            {
                _zoomTick++;

                if (_zoomTick <= _maxZoomTick)
                {
                    var tempLookat = new Vector3
                    {
                        X = (float)PennerDoubleAnimation.QuintEaseInOut(_zoomTick, _zoomStart.X, _zoomDiff.X, 100),
                        Y = (float)PennerDoubleAnimation.QuintEaseInOut(_zoomTick, _zoomStart.Y, _zoomDiff.Y, 100),
                        Z = 0.0f
                    };

                    _lookAt = tempLookat;
                    _eye = new Vector3(tempLookat.X, tempLookat.Y, _cameraDistance);
                }
                else
                {
                    _lookAt = _zoomEnd;
                    _eye = new Vector3(_lookAt.X, _lookAt.Y, _cameraDistance);
                    _solarSystems.AddHighlight(_zoomToSystemId, true);

                    if (_currentHighlight > 0)
                    {
                        _solarSystems.RemoveHighlight(_currentHighlight);
                    }

                    StopZoom();
                }
            }

            _solarSystems.IncomingTick();
            glOut.Invalidate();
        }
        private void PathFindingTicker_Tick(object sender, EventArgs e)
        {
            _solarSystems.ProcessPathfindingQueue();
        }
        void _localWatcher_SystemChange(object sender, ProcessSystemChangeEventArgs e)
        {
            int nativeId;
            int systemId = -1;
            if (int.TryParse(e.SystemName, out nativeId))
            {
                foreach (KeyValuePair<int, SolarSystem> tempSolarSystem in from tempSolarSystem in _solarSystems.SolarSystems let tempSystem = tempSolarSystem.Value where tempSystem.NativeId == nativeId select tempSolarSystem)
                {
                    systemId = tempSolarSystem.Key;
                    break;
                }
            }
            else
            {
                foreach (KeyValuePair<int, SolarSystem> tempSolarSystem in from tempSolarSystem in _solarSystems.SolarSystems let tempSystem = tempSolarSystem.Value where tempSystem.MatchNameRegex(e.SystemName) select tempSolarSystem)
                {
                    systemId = tempSolarSystem.Key;
                    break;
                }
            }

            var charName = e.CharName.Trim();

            if (charName.Length > 0 && !_charLocations.ContainsKey(charName) && systemId != -1)
            {
                _charLocations.Add(charName, systemId);

                if (_conf.CameraFollowCharacter && CentreOnCharacter.Text == charName)
                {
                    ZoomTo(systemId);                    
                }

                if (!_conf.CharacterList.Contains(charName))
                {
                    AddNewCharacter(e.CharName);
                }
            }
            else if (systemId != -1 && charName.Length > 0)
            {
                _charLocations[charName] = systemId;

                if (CameraFollowCharacter.Checked && CentreOnCharacter.Text == charName)
                {
                    ZoomTo(systemId);
                }
            }

            var charDisplayName = charName.Length == 0 ? "Unknown" : charName;

            if (systemId != -1)
            {
                WriteIntel("sys", " > System Change: " + charDisplayName + " (" + _solarSystems.SolarSystems[systemId].Name + ")", true);

                // Update ranges if needed
                if (charDisplayName.Length > 0 && _conf.MapRangeFrom != kHomeIndexMapRange)
                { // have a character and don't have home system selected as map range option
                    var charId = _conf.CharacterId(charDisplayName);

                    if (charId != -1 && (_conf.MapRangeFrom - 1) == charId)
                    {
                        _solarSystems.Set_MapRangeFrom_SystemID(systemId);
                    }
                }
            }
        }
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            FinaliseConfig();

            // Cleanup hidden tab pages
            foreach (var hiddenPage in _hiddenPages)
            {
                hiddenPage.Value.Dispose();
            }
        }
        #endregion Other Window Events        

        #region Custom Controls
        #region Main Buttons
        private void FindSystem_Click(object sender, EventArgs e)
        {
            foreach (KeyValuePair<int, SolarSystem> tempSolarSystem in _solarSystems.SolarSystems)
            {
                SolarSystem tempSystem = tempSolarSystem.Value;
                if (tempSystem.MatchNameRegex(SearchSystem.Text))
                {
                    ZoomTo(tempSolarSystem.Key);
                    return;
                }
            }

            MessageBox.Show("System \"" + SearchSystem.Text + "\" not found, try again.", "System Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        private void FullscreenToggle_Click(object sender, EventArgs e)
        {
            ToggleFullScreen();
        }
        private void LogWatchToggle_Click(object sender, EventArgs e)
        {
            ToggleLogWatch();
        }
        #endregion Main Buttons

        #region All Tab
        private bool _bufferIntel;
        private Queue<IntelBuffer> _bufferedIntel = new Queue<IntelBuffer>();
        private void CombinedIntel_Enter(object sender, EventArgs e)
        {
            _bufferIntel = true;
            BufferingIndicator.Text = "Buffering Intel";
            BufferingIndicator.Visible = true;
        }
        private void CombinedIntel_Leave(object sender, EventArgs e)
        {
            while (_bufferedIntel.Count > 0)
            {
                var tempIntelBuffer = _bufferedIntel.Dequeue();

                WriteIntel(tempIntelBuffer.Prefix, tempIntelBuffer.LogLine, tempIntelBuffer.ParseForSystemLinks, true);
            }

            BufferingIndicator.Visible = false;
            _bufferIntel = false;
        }
        private void CombinedIntel_KeyDown(object sender, KeyEventArgs e)
        {
            e.SuppressKeyPress = true;

            if (e.KeyCode == Keys.Z || e.KeyCode == Keys.Enter)
            {
                var selectedText = CombinedIntel.SelectedText.Trim();

                if (selectedText == string.Empty) return;

                if (!_characters.Names.Contains(selectedText))
                {
                    _characters.AddName(selectedText);
                    CharacterList.Items.Add(selectedText);
                }

                CombinedIntel.DeselectAll();

                var textEnd = false;
                var findStart = 0;

                while (!textEnd)
                {
                    var charNameStart = CombinedIntel.Text.IndexOf(selectedText, findStart);

                    if (charNameStart == -1)
                    {
                        textEnd = true;
                    }
                    else
                    {
                        // Insert the name as a link
                        CombinedIntel.InsertLink(selectedText, charNameStart);

                        // Set the start for the next search as the end of the currnet one
                        findStart = charNameStart + selectedText.Length;
                    }
                }

                if (selectedText.Length > 0)
                    Process.Start("http://zkillboard.com/search/" + selectedText);
            }
        }
        private void CombinedIntel_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            if (_characters.Names.Contains(e.LinkText))
                Process.Start("http://zkillboard.com/search/" + e.LinkText);
            else if (_solarSystems.Names.ContainsKey(e.LinkText))
                ZoomTo(_solarSystems.Names[e.LinkText]);
        }
        #endregion All Tab

        #region Right Click Menu
        private void followMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem.Text == "None")
            {
                CameraFollowCharacter.Checked = false;
            }
            else
            {
                CameraFollowCharacter.Checked = true;
                CentreOnCharacter.SelectedIndex = _conf.CharacterId(e.ClickedItem.Text);
            }
        }
        private void mapRangeFromMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            var selectionIndex = 0;
            if (e.ClickedItem.Text != "Home System")
            {
                selectionIndex = _conf.CharacterId(e.ClickedItem.Text) + 1;
            }
            MapRangeFrom.SelectedIndex = selectionIndex; // This will trigger MapRangeFrom_SelectedIndexChanged and update the conf file
        }
        private void anomalyMonitorMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            foreach (ToolStripMenuItem dropDownItem in anomalyMonitorMenuItem.DropDownItems)
            {
                if (dropDownItem.Text == e.ClickedItem.Text)
                {
                    dropDownItem.Checked = !dropDownItem.Checked;

                    if (dropDownItem.Checked)
                    {
                        if (AnomalyWatcherSound.SelectedIndex == -1)
                        {
                            MessageBox.Show("Select an \"Anomaloy Monitor\" sound first. (Config -> Misc Settings)", "PEBKAC",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                            dropDownItem.Checked = false;
                            return;
                        }
                        WriteIntel("sys", " > Anomaly Monitor | Enabled: " + dropDownItem.Text);
                    }
                    else
                    {
                        WriteIntel("sys", " > Anomaly Monitor | Disabled: " + dropDownItem.Text);
                    }
                }
            }
        }
        private void muteSoundMenuItem_Click(object sender, EventArgs e)
        {
            _muteSound = muteSoundMenuItem.Checked;
        }
        private void clearSelectedSystemsMenuItem_Click(object sender, EventArgs e)
        {
            _stickyHighlightSystems.Clear();
            SaveStickySystems();
        }
        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        #endregion Right Click Menu
        #endregion Custom Controls
    }
}
