using System;
using ProtoBuf;

namespace Taco.Classes
{
    [ProtoContract]
    class ConfigVer6
    {
        // ReSharper disable once RedundantArgumentDefaultValue
        public ConfigVer6() : this(false) { }

        public ConfigVer6(bool useDefaults = false)
        {
            if (!useDefaults) return;

            PreserveWindowPosition = true;
            PreserveWindowSize = true;
            WindowPositionX = 50;
            WindowPositionY = 50;
            WindowSizeX = 1253;
            WindowSizeY = 815;

            PreserveFullScreenStatus = true;
            IsFullScreen = false;

            PreserveHomeSystem = true;
            HomeSystemId = 30004759;

            MonitorBranch = false;
            MonitorDeklein = false;
            MonitorTenal = false;
            MonitorVenal = false;
            MonitorFade = false;
            MonitorPureBlind = false;
            MonitorTribute = false;
            MonitorVale = false;
            MonitorProvidence = false;
            MonitorDelve = true;
            MonitorGameLog = true;
            MonitorQuerious = true;

            AlertBranch = false;
            AlertDeklein = false;
            AlertTenal = false;
            AlertVenal = false;
            AlertFade = false;
            AlertPureBlind = false;
            AlertTribute = false;
            AlertVale = false;
            AlertProvidence = false;
            AlertDelve = true;
            AlertQuerious = true;

            PreserveCameraDistance = true;
            PreserveLookAt = true;

            CameraDistance = 700f;
            LookAtX = -4353;
            LookAtY = -2373;

            OverrideLogPath = false;
            LogPath = string.Empty;

            PreserveSelectedSystems = true;
            SelectedSystems = new[] { 30004735, 30004762, 30004730, 30004777, 30004768, 30004764, 30004738, 30004758, 30004712 };

            DisplayNewFileAlerts = true;
            DisplayOpenFileAlerts = true;

            AlertTriggers = new[]
            {
                new AlertTrigger()
                {
                    Enabled = false,
                    Type = AlertType.Ranged,
                    RangeTo = RangeAlertType.Character,
                    UpperLimitOperator = RangeAlertOperator.LessThanOrEqual,
                    UpperRange = 2,
                    LowerLimitOperator = RangeAlertOperator.GreaterThanOrEqual,
                    LowerRange = 0,
                    CharacterName = "CharName",
                    SoundId = 2,
                    SoundPath = "StarCoin",
                    SystemId = -1
                },
                new AlertTrigger()
                {
                    Enabled = true,
                    Type = AlertType.Ranged,
                    RangeTo = RangeAlertType.Home,
                    UpperLimitOperator = RangeAlertOperator.Equal,
                    UpperRange = 0,
                    LowerLimitOperator = RangeAlertOperator.Equal,
                    LowerRange = 0,
                    SoundId = 2,
                    SoundPath = "KamekLaugh",
                    SystemId = -1
                },
                new AlertTrigger()
                {
                    Enabled = true,
                    Type = AlertType.Ranged,
                    RangeTo = RangeAlertType.Home,
                    UpperLimitOperator = RangeAlertOperator.Equal,
                    UpperRange = 1,
                    LowerLimitOperator = RangeAlertOperator.Equal,
                    LowerRange = 1,
                    SoundId = 1,
                    SoundPath = "Boo2",
                    SystemId = -1
                },
                new AlertTrigger()
                {
                    Enabled = true,
                    Type = AlertType.Ranged,
                    RangeTo = RangeAlertType.Home,
                    UpperLimitOperator = RangeAlertOperator.LessThanOrEqual,
                    UpperRange = 3,
                    LowerLimitOperator = RangeAlertOperator.GreaterThanOrEqual,
                    LowerRange = 2,
                    SoundId = 4,
                    SoundPath = "RedCoin3",
                    SystemId = -1
                },
                new AlertTrigger()
                {
                    Enabled = true,
                    Type = AlertType.Ranged,
                    RangeTo = RangeAlertType.Home,
                    UpperLimitOperator = RangeAlertOperator.LessThanOrEqual,
                    UpperRange = 5,
                    LowerLimitOperator = RangeAlertOperator.GreaterThanOrEqual,
                    LowerRange = 4,
                    SoundId = 3,
                    SoundPath = "RedCoin2",
                    SystemId = -1
                },
                new AlertTrigger()
                {
                    Enabled = true,
                    Type = AlertType.Ranged,
                    RangeTo = RangeAlertType.System,
                    UpperLimitOperator = RangeAlertOperator.Equal,
                    UpperRange = 0,
                    LowerLimitOperator = RangeAlertOperator.Equal,
                    LowerRange = 0,
                    SoundId = 2,
                    SoundPath = "SuitSpin",
                    SystemId = 30004768
                },
                new AlertTrigger()
                {
                    Enabled = true,
                    Type = AlertType.Ranged,
                    RangeTo = RangeAlertType.System,
                    UpperLimitOperator = RangeAlertOperator.LessThanOrEqual,
                    UpperRange = 3,
                    LowerLimitOperator = RangeAlertOperator.GreaterThan,
                    LowerRange = 0,
                    SoundId = 2,
                    SoundPath = "Coin",
                    SystemId = 30004768
                },
                new AlertTrigger()
                {
                    Enabled = true,
                    Type = AlertType.Custom,
                    RepeatInterval = 60,
                    SoundId = 0,
                    SoundPath = "1up1",
                    Text = "Dread Guristas"
                }, 
                new AlertTrigger()
                {
                    Enabled = false,
                    Type = AlertType.Custom,
                    RepeatInterval = 0,
                    SoundId = 0,
                    SoundPath = "1up1",
                    Text = "Custom Alert 2"                    
                }
            };

            IgnoreStrings = new[]
            {
                "clr",
                "clear",
                "status"
            };

            IgnoreSystems = new[]
            {
                30003271
            };

            CharacterList = new string[0];

            DisplayCharacterNames = true;
            ShowCharacterLocations = true;
            
            CameraFollowCharacter = false;
            CentreOnCharacter = -1;

            MapRangeFrom = 0;

            AnomalyMonitorSoundId = -1;
            AnomalyMonitorSoundPath = string.Empty;

            ShowAlertAge = true;
            ShowAlertAgeSecs = true;
            ShowReportCount = false;
            MaxAlertAge = 15;
            MaxAlerts = 20;

            QueriousIntelChat = "querious";
            BranchIntelChat = "brn";
            DekleinIntelChat = "dek";
            TenalIntelChat = "tnl";
            VenalIntelChat = "vnl";
            FadeIntelChat = "fade";
            PureBlindIntelChat = "pb";
            TributeIntelChat = "tri";
            ValeIntelChat = "vale";
            ProvidenceIntelChat = "provi";
            DelveIntelChat = "delve";

            RenderWhileDragging = false;
        }

        [ProtoMember(1)]
        public bool PreserveWindowPosition { get; set; }
        [ProtoMember(2)]
        public bool PreserveWindowSize { get; set; }

        [ProtoMember(3)]
        public int WindowPositionX { get; set; }
        [ProtoMember(4)]
        public int WindowPositionY { get; set; }

        [ProtoMember(5)]
        public int WindowSizeX { get; set; }
        [ProtoMember(6)]
        public int WindowSizeY { get; set; }
    
        [ProtoMember(7)]
        public bool PreserveFullScreenStatus { get; set; }
        [ProtoMember(8)]
        public bool IsFullScreen { get; set; }

        [ProtoMember(9)]
        public bool PreserveHomeSystem { get; set; }
        [ProtoMember(10)]
        public int HomeSystemId { get; set; }

        [ProtoMember(11)]
        public bool MonitorBranch { get; set; }
        [ProtoMember(12)]
        public bool MonitorDeklein { get; set; }
        [ProtoMember(13)]
        public bool MonitorTenal { get; set; }
        [ProtoMember(14)]
        public bool MonitorVenal { get; set; }
        [ProtoMember(15)]
        public bool MonitorFade { get; set; }
        [ProtoMember(16)]
        public bool MonitorPureBlind { get; set; }
        [ProtoMember(17)]
        public bool MonitorTribute { get; set; }
        [ProtoMember(18)]
        public bool MonitorVale { get; set; }
        [ProtoMember(19)]
        public bool MonitorProvidence { get; set; }
        [ProtoMember(20)]
        public bool MonitorDelve { get; set; }
        [ProtoMember(21)]
        public bool MonitorGameLog { get; set; }

        [ProtoMember(22)]
        public bool AlertBranch { get; set; }
        [ProtoMember(23)]
        public bool AlertDeklein { get; set; }
        [ProtoMember(24)]
        public bool AlertTenal { get; set; }
        [ProtoMember(25)]
        public bool AlertVenal { get; set; }
        [ProtoMember(26)]
        public bool AlertFade { get; set; }
        [ProtoMember(27)]
        public bool AlertPureBlind { get; set; }
        [ProtoMember(28)]
        public bool AlertTribute { get; set; }
        [ProtoMember(29)]
        public bool AlertVale { get; set; }
        [ProtoMember(30)]
        public bool AlertProvidence { get; set; }
        [ProtoMember(31)]
        public bool AlertDelve { get; set; }

        [ProtoMember(32)]
        public bool PreserveCameraDistance { get; set; }
        [ProtoMember(33)]
        public bool PreserveLookAt { get; set; }

        [ProtoMember(34)]
        public float CameraDistance { get; set; }
        [ProtoMember(35)]
        public float LookAtX { get; set; }
        [ProtoMember(36)]
        public float LookAtY { get; set; }

        [ProtoMember(37)]
        public bool OverrideLogPath { get; set; }
        [ProtoMember(38)]
        public string LogPath { get; set; }

        [ProtoMember(39)]
        public bool PreserveSelectedSystems { get; set; }
        [ProtoMember(40)]
        public int[] SelectedSystems { get; set; }

        [ProtoMember(41)]
        public AlertTrigger[] AlertTriggers { get; set; }

        [ProtoMember(42)]
        public string[] IgnoreStrings { get; set; }
        [ProtoMember(43)]
        public int[] IgnoreSystems { get; set; }

        [ProtoMember(44)]
        public bool DisplayNewFileAlerts { get; set; }
        [ProtoMember(45)]
        public bool DisplayOpenFileAlerts { get; set; }

        [ProtoMember(46)]
        public string[] CharacterList { get; set; }

        [ProtoMember(47)]
        public bool DisplayCharacterNames { get; set; }
        [ProtoMember(48)]
        public bool ShowCharacterLocations { get; set; }

        [ProtoMember(49)]
        public bool CameraFollowCharacter { get; set; }
        [ProtoMember(50)]
        public int CentreOnCharacter { get; set; }

        [ProtoMember(51)]
        public int MapRangeFrom { get; set; }

        [ProtoMember(52)]
        public int AnomalyMonitorSoundId { get; set; }
        [ProtoMember(53)]
        public string AnomalyMonitorSoundPath { get; set; }

        [ProtoMember(54)]
        public bool ShowAlertAge { get; set; }
        [ProtoMember(55)]
        public bool ShowAlertAgeSecs { get; set; }
        [ProtoMember(56)]
        public bool ShowReportCount { get; set; }
        [ProtoMember(57)]
        public int MaxAlertAge { get; set; }
        [ProtoMember(58)]
        public int MaxAlerts { get; set; }
        [ProtoMember(59)]
        public bool MonitorQuerious { get; set; }
        [ProtoMember(60)]
        public bool AlertQuerious { get; set; }

        [ProtoMember(61)]
        public string QueriousIntelChat { get; set; }
        [ProtoMember(62)]
        public string BranchIntelChat { get; set; }
        [ProtoMember(63)]
        public string DekleinIntelChat { get; set; }
        [ProtoMember(64)]
        public string TenalIntelChat { get; set; }
        [ProtoMember(65)]
        public string VenalIntelChat { get; set; }
        [ProtoMember(66)]
        public string FadeIntelChat { get; set; }
        [ProtoMember(67)]
        public string PureBlindIntelChat { get; set; }
        [ProtoMember(68)]
        public string TributeIntelChat { get; set; }
        [ProtoMember(69)]
        public string ValeIntelChat { get; set; }
        [ProtoMember(70)]
        public string ProvidenceIntelChat { get; set; }
        [ProtoMember(71)]
        public string DelveIntelChat { get; set; }

        [ProtoMember(72)]
        public bool RenderWhileDragging { get; set; }
    }

}
