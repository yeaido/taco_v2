using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Taco
{
    public partial class MainForm
    {
        private void PreserveWindowSize_CheckedChanged(object sender, EventArgs e)
        {
            if (_configLoaded)
                _conf.PreserveWindowSize = PreserveWindowSize.Checked;
        }
        private void PreserveWindowPosition_CheckedChanged(object sender, EventArgs e)
        {
            if (_configLoaded)
                _conf.PreserveWindowPosition = PreserveWindowPosition.Checked;
        }
        private void PreserveFullScreenStatus_CheckedChanged(object sender, EventArgs e)
        {
            if (_configLoaded)
                _conf.PreserveFullScreenStatus = PreserveFullScreenStatus.Checked;
        }
        private void PreserveLookAt_CheckedChanged(object sender, EventArgs e)
        {
            if (_configLoaded)
                _conf.PreserveLookAt = PreserveLookAt.Checked;
        }
        private void PreserveCameraDistance_CheckedChanged(object sender, EventArgs e)
        {
            if (_configLoaded)
                _conf.PreserveCameraDistance = PreserveCameraDistance.Checked;
        }
        private void PreserveSelectedSystems_CheckedChanged(object sender, EventArgs e)
        {
            if (_configLoaded)
                _conf.PreserveSelectedSystems = PreserveSelectedSystems.Checked;
        }
        private void PreserveHomeSystem_CheckedChanged(object sender, EventArgs e)
        {
            if (_configLoaded)
                _conf.PreserveHomeSystem = PreserveHomeSystem.Checked;
        }
        private void RenderWhileDragging_CheckedChanged(object sender, EventArgs e)
        {
            if (_configLoaded)
                _conf.RenderWhileDragging = RenderWhileDragging.Checked;
            
            EnableSplitContainerDragRender(RenderWhileDragging.Checked);
        }
        private void ShowCharacterLocations_CheckedChanged(object sender, EventArgs e)
        {
            if (_configLoaded)
                _conf.ShowCharacterLocations = ShowCharacterLocations.Checked;

            if (ShowCharacterLocations.Checked)
            {
                DisplayCharacterNames.Enabled = true;

                if (m_bWatchLogs && !_followingChars)
                    ToggleFollowChars();
            }
            else
            {
                DisplayCharacterNames.Enabled = false;
                DisplayCharacterNames.Checked = false;

                if (_followingChars)
                    ToggleFollowChars();
            }
        }
        private void DisplayCharacterNames_CheckedChanged(object sender, EventArgs e)
        {
            if (_configLoaded)
                _conf.DisplayCharacterNames = DisplayCharacterNames.Checked;
        }
        private void ShowAlertAge_CheckedChanged(object sender, EventArgs e)
        {
            if (_configLoaded)
                _conf.ShowAlertAge = ShowAlertAge.Checked;
        }
        private void ShowAlertAgeSecs_CheckedChanged(object sender, EventArgs e)
        {
            if (_configLoaded)
                _conf.ShowAlertAgeSecs = ShowAlertAgeSecs.Checked;
        }
        private void ShowReportCount_CheckedChanged(object sender, EventArgs e)
        {
            if (_configLoaded)
                _conf.ShowReportCount = ShowReportCount.Checked;
        }
        private void MaxAlertAge_ValueChanged(object sender, EventArgs e)
        {
            _solarSystems.MaxAlertAge = decimal.ToInt32(MaxAlertAge.Value);

            if (_configLoaded)
                _conf.MaxAlertAge = decimal.ToInt32(MaxAlertAge.Value);
        }
        private void MaxAlerts_ValueChanged(object sender, EventArgs e)
        {
            _solarSystems.MaxAlerts = decimal.ToInt32(MaxAlerts.Value);

            if (_configLoaded)
                _conf.MaxAlerts = decimal.ToInt32(MaxAlerts.Value);
        }
        private void CameraFollowCharacter_CheckedChanged(object sender, EventArgs e)
        {
            CentreOnCharacter.Enabled = CameraFollowCharacter.Checked;

            if (!CameraFollowCharacter.Checked)
            {
                CentreOnCharacter.SelectedIndex = -1;
                CentreOnCharacter.Text = string.Empty;
            }
            else
            {
                if (_conf.CharacterList.Count > 0)
                {
                    CentreOnCharacter.SelectedIndex = 0;
                }
                else
                {
                    CameraFollowCharacter.Checked = false;
                    MessageBox.Show("No characters discovered yet!", "PEBKAC", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            if (_configLoaded)
                _conf.CameraFollowCharacter = CameraFollowCharacter.Checked;
        }
        private void CentreOnCharacter_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_configLoaded)
                _conf.CentreOnCharacter = CentreOnCharacter.SelectedIndex;

            var sTargetCharacter = CentreOnCharacter.Text;
            foreach (ToolStripMenuItem dropDownItem in followMenuItem.DropDownItems)
            {
                if (_conf.CentreOnCharacter == -1 && dropDownItem.Text == "None")
                {
                    dropDownItem.Checked = true;
                }
                else if (dropDownItem.Text == sTargetCharacter)
                {
                    dropDownItem.Checked = true;
                }
                else
                {
                    dropDownItem.Checked = false;
                }
            }

            var nTargetSystemId = _conf.HomeSystemId;
            if (CameraFollowCharacter.Checked && _charLocations.ContainsKey(sTargetCharacter))
            {
                nTargetSystemId = _charLocations[sTargetCharacter];
                ZoomTo(nTargetSystemId);
            }
            else if (CameraFollowCharacter.Checked)
            {  // we wanted a character but couldn't find one - set to -1 to disable the highlighting altogether
                nTargetSystemId = -1;
            }
            _solarSystems.SetCameraCenteredSystemID(nTargetSystemId);
        }
        private void MapRangeFrom_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_configLoaded)
                _conf.MapRangeFrom = MapRangeFrom.SelectedIndex;

            foreach (ToolStripMenuItem dropDownItem in mapRangeFromMenuItem.DropDownItems)
            {
                dropDownItem.Checked = dropDownItem.Text == MapRangeFrom.Text;
            }

            if (_conf.MapRangeFrom != kHomeIndexMapRange)
            {
                var sMapRangeCharacter = _conf.CharacterList[(_conf.MapRangeFrom - 1)];
                if (_charLocations.ContainsKey(sMapRangeCharacter))
                {
                    _solarSystems.Set_MapRangeFrom_SystemID(_charLocations[sMapRangeCharacter]);
                }
                else
                {  // char location not saved (log watcher probably not running), clear MapRangeFrom value
                    _solarSystems.Set_MapRangeFrom_SystemID(-1);
                }
            }
            else
            {
                _solarSystems.Set_MapRangeFrom_ToHomeSystem();
            }
        }
        private void DisplayNewFileAlerts_CheckedChanged(object sender, EventArgs e)
        {
            if (_configLoaded)
                _conf.DisplayNewFileAlerts = DisplayNewFileAlerts.Checked;
        }
        private void DisplayOpenFileAlerts_CheckedChanged(object sender, EventArgs e)
        {
            if (_configLoaded)
                _conf.DisplayOpenFileAlerts = DisplayOpenFileAlerts.Checked;
        }
        private void OverrideLogPath_CheckedChanged(object sender, EventArgs e)
        {
            if (_configLoaded)
                _conf.OverrideLogPath = OverrideLogPath.Checked;

            ChooseLogPath.Enabled = OverrideLogPath.Checked;
            LogPath.Enabled = OverrideLogPath.Checked;
        }
        private void ChooseLogPath_Click(object sender, EventArgs e)
        {
            DialogResult result = LogBrowser.ShowDialog();

            if (result != DialogResult.OK) return;

            if (Directory.Exists(LogBrowser.SelectedPath + @"\Chatlogs") && Directory.Exists(LogBrowser.SelectedPath + @"\Gamelogs"))
            {
                LogPath.Text = LogBrowser.SelectedPath;
                if (_configLoaded)
                    _conf.LogPath = LogBrowser.SelectedPath;
            }
            else
            {
                MessageBox.Show("Invalid Log Path.  Chatlogs and/or Gamelogs sub-directories missing.",
                    "Invalid Log Path", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void PlayAnomalyWatcherSoundPreview_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            PlayAnomalyWatcherSound();
        }
        private void AnomalyWatcherSound_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_configLoaded) return;

            if (!_pickingFile && AnomalyWatcherSound.SelectedIndex == AnomalyWatcherSound.Items.Count - 1)
            {
                _pickingFile = true;

                if (CustomSoundPicker.ShowDialog() != DialogResult.OK) return;

                AnomalyWatcherSound.Items.RemoveAt(AnomalyWatcherSound.Items.Count - 1);
                AnomalyWatcherSound.Items.Add(CustomSoundPicker.FileName);
                AnomalyWatcherSound.SelectedIndex = AnomalyWatcherSound.Items.Count - 1;
                _anomalyWatcherSound = LoadSoundFromFile(CustomSoundPicker.FileName);
                _pickingFile = false;
            }

            if (_configLoaded)
            {
                _conf.AnomalyMonitorSoundId = AnomalyWatcherSound.SelectedIndex == AnomalyWatcherSound.Items.Count - 1
                    ? -1
                    : AnomalyWatcherSound.SelectedIndex;
                _conf.AnomalyMonitorSoundPath = AnomalyWatcherSound.Text;
            }
        }
        private void AnomalyWatcherSound_TextChanged(object sender, EventArgs e)
        {

        }
        private void ClearSelectedSystems_Click(object sender, EventArgs e)
        {
            _stickyHighlightSystems.Clear();
            SaveStickySystems();
        }
        private void QuitTaco_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        private void TestFlood_Click(object sender, EventArgs e)
        {
            Random random = new Random();

            for (int i = 0; i < 1; i++)
            {
                var randomNumber = random.Next(0, 5000);
                _solarSystems.AddAlert(randomNumber);
                ZoomTo(randomNumber);
            }

            _solarSystems.AddAlert(4322);
        }
        private void CrashException_Click(object sender, EventArgs e)
        {
#if DEBUG
            using (var testCrash = new StreamWriter(Application.StartupPath + @"\taco-test.txt"))
            {
                File.Delete(Application.StartupPath + @"\taco-test.txt");
            }
#endif
        }
        private void CrashRecursive_Click(object sender, EventArgs e)
        {
            CrashMeRecursive();
        }
    }
}
