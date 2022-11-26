using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Taco.Classes;

namespace Taco
{
    public partial class MainForm
    {
        private bool _pickingFile;
        private bool _loadingEdit, _loadingAlerts;
        private List<AlertTrigger> _alertTriggers = new List<AlertTrigger>();
        private SoundPlayer _customSound = new SoundPlayer();
        private HashSet<int> _triggerSystems = new HashSet<int>();

        private void WriteAlertConfig()
        {
            AlertTrigger[] temp = new AlertTrigger[_alertTriggers.Count];

            _alertTriggers.CopyTo(temp);
            _conf.AlertTriggers = temp;
        }
        private void LoadAlertConfig(bool clearAlertList = true)
        {
            _loadingAlerts = true;

            if (clearAlertList)
                AlertList.Items.Clear();

            _alertTriggers.Clear();
            _triggerSystems.Clear();

            if (_conf.AlertTriggers == null)
            {
                _loadingAlerts = false;
                return;
            }

            foreach (var temp in _conf.AlertTriggers)
            {
                if (temp.SystemId > -1 && temp.RangeTo != RangeAlertType.Character)
                {
                    temp.SystemName = _solarSystems.SolarSystems[temp.SystemId].Name;
                    _triggerSystems.Add(temp.SystemId);
                }

                _alertTriggers.Add(temp);

                if (clearAlertList)
                    AlertList.Items.Add(temp.ToString(), temp.Enabled);
            }

            _loadingAlerts = false;
        }

        private void AlertList_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (_loadingAlerts) return;

            int selectedIndex = e.Index;

            _alertTriggers[e.Index].Enabled = !_alertTriggers[e.Index].Enabled;
            WriteAlertConfig();
            LoadAlertConfig();

            AlertList.SelectedIndex = selectedIndex;
        }
        private void AlertList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                AlertList.SelectedIndex = -1;
                glOut.Focus();
            }

        }
        private void AlertList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (AlertList.SelectedIndex >= 0)
            {
                RemoveSelectedItem.Enabled = true;
                PlayAlertSound.Enabled = true;
                EditSelectedItem.Enabled = true;
            }
            else
            {
                RemoveSelectedItem.Enabled = false;
                PlayAlertSound.Enabled = false;
                EditSelectedItem.Enabled = false;
            }
        }
        private void MoveAlertUp_Click(object sender, EventArgs e)
        {
            if (AlertList.SelectedIndex == -1) return;

            var selectedIndex = AlertList.SelectedIndex;

            _alertTriggers.Move(AlertList.SelectedIndex, MoveDirection.Up);
            WriteAlertConfig();
            LoadAlertConfig();

            selectedIndex--;
            if (selectedIndex == -1)
                selectedIndex = 0;

            AlertList.SelectedIndex = selectedIndex;
        }
        private void MoveAlertDown_Click(object sender, EventArgs e)
        {
            if (AlertList.SelectedIndex == -1) return;

            var selectedIndex = AlertList.SelectedIndex;

            _alertTriggers.Move(AlertList.SelectedIndex, MoveDirection.Down);
            WriteAlertConfig();
            LoadAlertConfig();

            selectedIndex++;
            if (selectedIndex == AlertList.Items.Count)
                selectedIndex = AlertList.Items.Count - 1;

            AlertList.SelectedIndex = selectedIndex;
        }
        private void PlayAlertSound_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (_muteSound) return;

            if (_alertTriggers[AlertList.SelectedIndex].SoundId == -1)
            {
                var temp = LoadSoundFromFile(_alertTriggers[AlertList.SelectedIndex].SoundPath);
                temp.Play();
                temp.Dispose();
            }
            else if (RangeAlertSound.SelectedIndex < RangeAlertSound.Items.Count - 1)
            {
                _sounds[_alertTriggers[AlertList.SelectedIndex].SoundPath].Play();
            }
        }
        private void EditSelectedItem_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            _loadingEdit = true;

            if (_alertTriggers[AlertList.SelectedIndex].Type == AlertType.Ranged)
            {
                AlertList.Enabled = false;
                RemoveSelectedItem.Enabled = false;
                PlayAlertSound.Enabled = false;
                MoveAlertUp.Enabled = false;
                MoveAlertDown.Enabled = false;
                AddCustomAlertGroup.Enabled = false;
                EditSelectedItem.Visible = false;
                CancelEditSelectedItem.Visible = true;
                AddNewRangeAlert.Visible = false;
                SaveRangeAlert.Visible = true;

                switch (_alertTriggers[AlertList.SelectedIndex].UpperLimitOperator)
                {
                    case RangeAlertOperator.LessThanOrEqual:
                        UpperLimitOperator.SelectedItem = "<=";
                        break;
                    case RangeAlertOperator.Equal:
                        UpperLimitOperator.SelectedItem = "=";
                        break;
                    default:
                        UpperLimitOperator.SelectedItem = "=";
                        break;
                }

                UpperAlertRange.Value = _alertTriggers[AlertList.SelectedIndex].UpperRange;

                switch (_alertTriggers[AlertList.SelectedIndex].LowerLimitOperator)
                {
                    case RangeAlertOperator.GreaterThanOrEqual:
                        LowerLimitOperator.SelectedItem = ">=";
                        break;
                    case RangeAlertOperator.GreaterThan:
                        LowerLimitOperator.SelectedItem = "=";
                        break;
                    default:
                        LowerLimitOperator.SelectedItem = "=";
                        break;
                }

                LowerAlertRange.Value = _alertTriggers[AlertList.SelectedIndex].UpperLimitOperator == RangeAlertOperator.Equal
                    ? 0
                    : _alertTriggers[AlertList.SelectedIndex].LowerRange;

                switch (_alertTriggers[AlertList.SelectedIndex].RangeTo)
                {
                    case RangeAlertType.Home:
                        NewRangeAlertType.SelectedIndex = 0;
                        break;
                    case RangeAlertType.AnyCharacter:
                        NewRangeAlertType.SelectedIndex = 1;
                        break;
                    case RangeAlertType.System:
                        NewRangeAlertType.SelectedIndex = 2;
                        RangeAlertSystem.Text =
                            _solarSystems.SolarSystems[_alertTriggers[AlertList.SelectedIndex].SystemId].Name;
                        break;
                    case RangeAlertType.Character:
                        NewRangeAlertType.SelectedIndex = 3;
                        RangeAlertCharacter.SelectedIndex =
                            _conf.CharacterId(_alertTriggers[AlertList.SelectedIndex].CharacterName);
                        break;
                }

                RangeAlertSystem.Text = _alertTriggers[AlertList.SelectedIndex].SystemName;

                if (_alertTriggers[AlertList.SelectedIndex].SoundId == -1)
                {
                    RangeAlertSound.Items.RemoveAt(RangeAlertSound.Items.Count - 1);
                    RangeAlertSound.Items.Add(_alertTriggers[AlertList.SelectedIndex].SoundPath);
                    RangeAlertSound.SelectedIndex = RangeAlertSound.Items.Count - 1;
                    _customSound = LoadSoundFromFile(_alertTriggers[AlertList.SelectedIndex].SoundPath);
                }
                else
                {
                    RangeAlertSound.SelectedIndex = _alertTriggers[AlertList.SelectedIndex].SoundId;
                }
            }
            else if (_alertTriggers[AlertList.SelectedIndex].Type == AlertType.Custom)
            {
                AlertList.Enabled = false;
                RemoveSelectedItem.Enabled = false;
                PlayAlertSound.Enabled = false;
                MoveAlertUp.Enabled = false;
                MoveAlertDown.Enabled = false;
                AddRangeAlertGroup.Enabled = false;
                EditSelectedItem.Visible = false;
                CancelEditSelectedItem.Visible = true;
                AddNewCustomAlert.Visible = false;
                SaveCustomAlert.Visible = true;

                NewCustomAlertText.Text = _alertTriggers[AlertList.SelectedIndex].Text;
                CustomAlertRepeatInterval.Value = _alertTriggers[AlertList.SelectedIndex].RepeatInterval;

                if (_alertTriggers[AlertList.SelectedIndex].SoundId == -1)
                {
                    CustomTextAlertSound.Items.RemoveAt(CustomTextAlertSound.Items.Count - 1);
                    CustomTextAlertSound.Items.Add(_alertTriggers[AlertList.SelectedIndex].SoundPath);
                    CustomTextAlertSound.SelectedIndex = RangeAlertSound.Items.Count - 1;
                    _customSound = LoadSoundFromFile(_alertTriggers[AlertList.SelectedIndex].SoundPath);
                }
                else
                {
                    CustomTextAlertSound.SelectedIndex = _alertTriggers[AlertList.SelectedIndex].SoundId;
                }
            }

            _loadingEdit = false;
        }
        private void CancelEditSelectedItem_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (_alertTriggers[AlertList.SelectedIndex].Type == AlertType.Ranged)
            {
                AlertList.Enabled = true;
                RemoveSelectedItem.Enabled = true;
                PlayAlertSound.Enabled = true;
                MoveAlertUp.Enabled = true;
                MoveAlertDown.Enabled = true;
                AddCustomAlertGroup.Enabled = true;
                EditSelectedItem.Visible = true;
                CancelEditSelectedItem.Visible = false;
                AddNewRangeAlert.Visible = true;
                SaveRangeAlert.Visible = false;

                UpperLimitOperator.SelectedIndex = -1;
                UpperAlertRange.Value = 0;
                LowerLimitOperator.SelectedIndex = -1;
                LowerAlertRange.Value = 0;
                RangeAlertSystem.SelectedIndex = -1;
                RangeAlertSystem.Text = string.Empty;

                RangeAlertCharacter.Text = string.Empty;

                NewRangeAlertType.SelectedIndex = -1;

                RangeAlertSound.SelectedIndex = -1;
                RangeAlertSound.Items.RemoveAt(RangeAlertSound.Items.Count - 1);
                RangeAlertSound.Items.Add("Custom...");
            }
            else if (_alertTriggers[AlertList.SelectedIndex].Type == AlertType.Custom)
            {
                AlertList.Enabled = true;
                RemoveSelectedItem.Enabled = true;
                PlayAlertSound.Enabled = true;
                MoveAlertUp.Enabled = true;
                MoveAlertDown.Enabled = true;
                AddRangeAlertGroup.Enabled = true;
                EditSelectedItem.Visible = true;
                CancelEditSelectedItem.Visible = false;
                AddNewCustomAlert.Visible = true;
                SaveCustomAlert.Visible = false;

                NewCustomAlertText.Text = string.Empty;
                CustomAlertRepeatInterval.Value = 0;

                CustomTextAlertSound.SelectedIndex = -1;
                CustomTextAlertSound.Items.RemoveAt(RangeAlertSound.Items.Count - 1);
                CustomTextAlertSound.Items.Add("Custom...");
            }
        }
        private void RemoveSelectedItem_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            _alertTriggers.RemoveAt(AlertList.SelectedIndex);
            AlertList.Items.RemoveAt(AlertList.SelectedIndex);

            WriteAlertConfig();
            LoadAlertConfig();
        }
        private void NewRangeAlertType_TextChanged(object sender, EventArgs e)
        {
            if (NewRangeAlertType.SelectedIndex == 2)
            {
                RangeAlertSystem.Enabled = true;
                RangeAlertSystem.Visible = true;
                RangeAlertCharacter.Enabled = false;
                RangeAlertCharacter.Visible = false;
            }
            else if (NewRangeAlertType.SelectedIndex == 3)
            {
                RangeAlertCharacter.Enabled = true;
                RangeAlertCharacter.Visible = true;
                RangeAlertSystem.Enabled = false;
                RangeAlertSystem.Visible = false;
            }
            else
            {
                RangeAlertCharacter.Enabled = false;
                RangeAlertCharacter.Visible = false;
                RangeAlertSystem.Enabled = false;
                RangeAlertSystem.Visible = false;
            }
        }
        private void RangeAlertSystem_Leave(object sender, EventArgs e)
        {
            if (RangeAlertSystem.Text.Trim().Length == 0) return;

            foreach (var tempSystem in _solarSystems.SolarSystems.Select(tempSolarSystem => tempSolarSystem.Value).Where(tempSystem => tempSystem.MatchNameRegex(RangeAlertSystem.Text)))
            {
                RangeAlertSystem.Text = tempSystem.Name;
                return;
            }

            MessageBox.Show("System \"" + RangeAlertSystem.Text + "\" not found, try again.", "System Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
            RangeAlertSystem.Text = string.Empty;
        }
        private void AddNewRangeAlert_Click(object sender, EventArgs e)
        {
            if (UpperLimitOperator.SelectedIndex == -1)
            {
                MessageBox.Show("Select an upper limit operator (the first drop down box).", "PEBKAC", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (UpperLimitOperator.SelectedIndex > 0 && LowerLimitOperator.SelectedIndex == -1)
            {
                MessageBox.Show("Select a lower limit operator (the second drop down box).", "PEBKAC", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (NewRangeAlertType.SelectedIndex == -1)
            {
                MessageBox.Show("Select a range alert type.", "PEBKAC", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (RangeAlertSystem.Text.Trim().Length == 0 && NewRangeAlertType.SelectedIndex == 2)
            {
                MessageBox.Show("Select a system", "PEBKAC", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (RangeAlertCharacter.Text.Trim().Length == 0 && NewRangeAlertType.SelectedIndex == 3)
            {
                MessageBox.Show("Select a character", "PEBKAC", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (RangeAlertSound.SelectedIndex == -1)
            {
                MessageBox.Show("Select an alert sound", "PEBKAC", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var temp = new AlertTrigger { Type = AlertType.Ranged };


            switch ((string)UpperLimitOperator.SelectedItem)
            {
                case "<=":
                    temp.UpperLimitOperator = RangeAlertOperator.LessThanOrEqual;
                    break;
                case "=":
                    temp.UpperLimitOperator = RangeAlertOperator.Equal;
                    break;
                default:
                    temp.UpperLimitOperator = RangeAlertOperator.Equal;
                    break;
            }

            temp.UpperRange = Convert.ToInt32(UpperAlertRange.Value);

            switch ((string)LowerLimitOperator.SelectedItem)
            {
                case ">=":
                    temp.LowerLimitOperator = RangeAlertOperator.GreaterThanOrEqual;
                    break;
                case ">":
                    temp.LowerLimitOperator = RangeAlertOperator.GreaterThan;
                    break;
                default:
                    temp.LowerLimitOperator = RangeAlertOperator.GreaterThanOrEqual;
                    break;
            }

            temp.LowerRange = temp.UpperLimitOperator == RangeAlertOperator.Equal ? 0 : Convert.ToInt32(LowerAlertRange.Value);

            switch (NewRangeAlertType.SelectedIndex)
            {
                case 0:
                    temp.SystemId = -1;
                    temp.RangeTo = RangeAlertType.Home;
                    break;
                case 1:
                    temp.SystemId = -1;
                    temp.RangeTo = RangeAlertType.AnyCharacter;
                    break;
                case 2:
                    temp.SystemId = _solarSystems.Names[RangeAlertSystem.Text];
                    temp.RangeTo = RangeAlertType.System;
                    break;
                case 3:
                    temp.CharacterName = RangeAlertCharacter.Text.Trim();
                    temp.RangeTo = RangeAlertType.Character;
                    break;
            }

            temp.SystemName = RangeAlertSystem.Text;
            temp.SoundId = RangeAlertSound.SelectedIndex == RangeAlertSound.Items.Count - 1 ? -1 : RangeAlertSound.SelectedIndex;
            temp.SoundPath = (string)RangeAlertSound.SelectedItem;

            temp.Enabled = false;

            AlertList.Items.Add(temp.ToString());
            _alertTriggers.Add(temp);

            UpperLimitOperator.SelectedIndex = -1;
            UpperAlertRange.Value = 0;
            LowerLimitOperator.SelectedIndex = -1;
            LowerAlertRange.Value = 0;
            RangeAlertSystem.SelectedIndex = -1;
            RangeAlertSystem.Text = string.Empty;

            RangeAlertCharacter.Text = string.Empty;

            NewRangeAlertType.SelectedIndex = -1;

            RangeAlertSound.SelectedIndex = -1;
            RangeAlertSound.Items.RemoveAt(RangeAlertSound.Items.Count - 1);
            RangeAlertSound.Items.Add("Custom...");

            WriteAlertConfig();
            LoadAlertConfig();
        }
        private void SaveRangeAlert_Click(object sender, EventArgs e)
        {
            if (UpperLimitOperator.SelectedIndex == -1)
            {
                MessageBox.Show("Select an upper limit operator (the first drop down box).", "PEBKAC", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (UpperLimitOperator.SelectedIndex > 0 && LowerLimitOperator.SelectedIndex == -1)
            {
                MessageBox.Show("Select a lower limit operator (the second drop down box).", "PEBKAC", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (NewRangeAlertType.SelectedIndex == -1)
            {
                MessageBox.Show("Select a range alert type.", "PEBKAC", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (RangeAlertSystem.Text.Trim().Length == 0 && NewRangeAlertType.SelectedIndex == 2)
            {
                MessageBox.Show("Select a system", "PEBKAC", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (RangeAlertCharacter.Text.Trim().Length == 0 && NewRangeAlertType.SelectedIndex == 3)
            {
                MessageBox.Show("Select a character", "PEBKAC", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (RangeAlertSound.SelectedIndex == -1)
            {
                MessageBox.Show("Select an alert sound", "PEBKAC", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var tempIndex = AlertList.SelectedIndex;

            switch ((string)UpperLimitOperator.SelectedItem)
            {
                case "<=":
                    _alertTriggers[tempIndex].UpperLimitOperator = RangeAlertOperator.LessThanOrEqual;
                    break;
                case "=":
                    _alertTriggers[tempIndex].UpperLimitOperator = RangeAlertOperator.Equal;
                    break;
                default:
                    _alertTriggers[tempIndex].UpperLimitOperator = RangeAlertOperator.Equal;
                    break;
            }

            _alertTriggers[tempIndex].UpperRange = Convert.ToInt32(UpperAlertRange.Value);

            switch ((string)LowerLimitOperator.SelectedItem)
            {
                case ">=":
                    _alertTriggers[tempIndex].LowerLimitOperator = RangeAlertOperator.GreaterThanOrEqual;
                    break;
                case ">":
                    _alertTriggers[tempIndex].LowerLimitOperator = RangeAlertOperator.GreaterThan;
                    break;
                default:
                    _alertTriggers[tempIndex].LowerLimitOperator = RangeAlertOperator.GreaterThanOrEqual;
                    break;
            }

            _alertTriggers[tempIndex].LowerRange =
                _alertTriggers[tempIndex].UpperLimitOperator == RangeAlertOperator.Equal
                    ? 0
                    : Convert.ToInt32(LowerAlertRange.Value);

            switch (NewRangeAlertType.SelectedIndex)
            {
                case 0:
                    _alertTriggers[tempIndex].SystemId = -1;
                    _alertTriggers[tempIndex].RangeTo = RangeAlertType.Home;
                    break;
                case 1:
                    _alertTriggers[tempIndex].SystemId = -1;
                    _alertTriggers[tempIndex].RangeTo = RangeAlertType.AnyCharacter;
                    break;
                case 2:
                    _alertTriggers[tempIndex].SystemId = _solarSystems.Names[RangeAlertSystem.Text];
                    _alertTriggers[tempIndex].RangeTo = RangeAlertType.System;
                    break;
                case 3:
                    _alertTriggers[tempIndex].CharacterName = RangeAlertCharacter.Text.Trim();
                    _alertTriggers[tempIndex].RangeTo = RangeAlertType.Character;
                    break;
            }

            _alertTriggers[tempIndex].SystemName = RangeAlertSystem.Text;
            _alertTriggers[tempIndex].SoundId = RangeAlertSound.SelectedIndex ==
                                                              RangeAlertSound.Items.Count - 1
                ? -1
                : RangeAlertSound.SelectedIndex;
            _alertTriggers[tempIndex].SoundPath = (string)RangeAlertSound.SelectedItem;


            AlertList.Enabled = true;
            RemoveSelectedItem.Enabled = true;
            PlayAlertSound.Enabled = true;
            MoveAlertUp.Enabled = true;
            MoveAlertDown.Enabled = true;
            AddCustomAlertGroup.Enabled = true;
            EditSelectedItem.Visible = true;
            CancelEditSelectedItem.Visible = false;
            AddNewRangeAlert.Visible = true;
            SaveRangeAlert.Visible = false;

            UpperLimitOperator.SelectedIndex = -1;
            UpperAlertRange.Value = 0;
            LowerLimitOperator.SelectedIndex = -1;
            LowerAlertRange.Value = 0;
            RangeAlertSystem.SelectedIndex = -1;
            RangeAlertSystem.Text = string.Empty;

            RangeAlertCharacter.Text = string.Empty;

            NewRangeAlertType.SelectedIndex = -1;

            RangeAlertSound.SelectedIndex = -1;
            RangeAlertSound.Items.RemoveAt(RangeAlertSound.Items.Count - 1);
            RangeAlertSound.Items.Add("Custom...");

            WriteAlertConfig();
            LoadAlertConfig();

            AlertList.SelectedIndex = tempIndex;
        }
        private void PlayRangeAlertSound_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (_muteSound) return;

            if (RangeAlertSound.SelectedIndex == -1)
                MessageBox.Show("Pick a sound first.", "PEBKAC", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else if (RangeAlertSound.SelectedIndex < RangeAlertSound.Items.Count - 1)
                _sounds[(string)RangeAlertSound.SelectedItem].Play();
            else if (RangeAlertSound.SelectedIndex == RangeAlertSound.Items.Count - 1)
                _customSound.Play();
        }
        private void UpperLimitOperator_SelectedIndexChanged(object sender, EventArgs e)
        {
            // If not nothing and not >=
            if (UpperLimitOperator.SelectedIndex != -1 && UpperLimitOperator.SelectedIndex == 1)
            {
                LowerLimitOperator.Enabled = true;
                LowerAlertRange.Enabled = true;
            }
            else
            {
                LowerLimitOperator.Enabled = false;
                LowerAlertRange.Enabled = false;
                LowerLimitOperator.SelectedIndex = -1;
                LowerAlertRange.Value = 0;
            }
        }
        private void RangeAlertSound_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_pickingFile || RangeAlertSound.SelectedIndex != RangeAlertSound.Items.Count - 1 || _loadingEdit) return;

            _pickingFile = true;

            if (CustomSoundPicker.ShowDialog() != DialogResult.OK) return;

            RangeAlertSound.Items.RemoveAt(RangeAlertSound.SelectedIndex);
            RangeAlertSound.Items.Add(CustomSoundPicker.FileName);
            RangeAlertSound.SelectedIndex = RangeAlertSound.Items.Count - 1;
            _customSound = LoadSoundFromFile(CustomSoundPicker.FileName);
            _pickingFile = false;
        }
        private void PlayCustomTextAlertSound_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (_muteSound) return;

            if (CustomTextAlertSound.SelectedIndex == -1)
                MessageBox.Show("Pick a sound first.", "PEBKAC", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else if (CustomTextAlertSound.SelectedIndex < CustomTextAlertSound.Items.Count - 1)
                _sounds[(string)CustomTextAlertSound.SelectedItem].Play();
            else if (CustomTextAlertSound.SelectedIndex == CustomTextAlertSound.Items.Count - 1)
                _customSound.Play();
        }
        private void CustomTextAlertSound_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_pickingFile || CustomTextAlertSound.SelectedIndex != CustomTextAlertSound.Items.Count - 1) return;

            _pickingFile = true;

            if (CustomSoundPicker.ShowDialog() != DialogResult.OK) return;

            CustomTextAlertSound.Items.RemoveAt(CustomTextAlertSound.SelectedIndex);
            CustomTextAlertSound.Items.Add(CustomSoundPicker.FileName);
            CustomTextAlertSound.SelectedIndex = CustomTextAlertSound.Items.Count - 1;
            _customSound = LoadSoundFromFile(CustomSoundPicker.FileName);
            _pickingFile = false;
        }
        private void AddNewCustomAlert_Click(object sender, EventArgs e)
        {
            if (NewCustomAlertText.Text.Trim().Length == 0)
            {
                MessageBox.Show("At least enter some text to alert on...", "PEBKAC", MessageBoxButtons.OK, MessageBoxIcon.Error);
                NewCustomAlertText.Text = string.Empty;
                return;
            }

            if (CustomTextAlertSound.SelectedIndex == -1)
            {
                MessageBox.Show("Select an alert sound", "PEBKAC", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var temp = new AlertTrigger
            {
                Type = AlertType.Custom,
                Enabled = false,
                Text = NewCustomAlertText.Text,
                RepeatInterval = Convert.ToInt32(CustomAlertRepeatInterval.Value),
                SoundId =
                    CustomTextAlertSound.SelectedIndex == CustomTextAlertSound.Items.Count - 1
                        ? -1
                        : CustomTextAlertSound.SelectedIndex,
                SoundPath = (string)CustomTextAlertSound.SelectedItem
            };

            AlertList.Items.Add(temp.ToString());
            _alertTriggers.Add(temp);

            NewCustomAlertText.Text = string.Empty;
            CustomAlertRepeatInterval.Value = 0;

            CustomTextAlertSound.SelectedIndex = -1;
            CustomTextAlertSound.Items.RemoveAt(RangeAlertSound.Items.Count - 1);
            CustomTextAlertSound.Items.Add("Custom...");

            WriteAlertConfig();
            LoadAlertConfig();
        }
        private void SaveCustomAlert_Click(object sender, EventArgs e)
        {
            if (NewCustomAlertText.Text.Trim().Length == 0)
            {
                MessageBox.Show("At least enter some text to alert on...", "PEBKAC", MessageBoxButtons.OK, MessageBoxIcon.Error);
                NewCustomAlertText.Text = string.Empty;
                return;
            }

            if (CustomTextAlertSound.SelectedIndex == -1)
            {
                MessageBox.Show("Select an alert sound", "PEBKAC", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var tempIndex = AlertList.SelectedIndex;

            _alertTriggers[tempIndex].Type = AlertType.Custom;
            _alertTriggers[tempIndex].Text = NewCustomAlertText.Text;
            _alertTriggers[tempIndex].RepeatInterval = Convert.ToInt32(CustomAlertRepeatInterval.Value);
            _alertTriggers[tempIndex].SoundId =
                CustomTextAlertSound.SelectedIndex == CustomTextAlertSound.Items.Count - 1
                    ? -1
                    : CustomTextAlertSound.SelectedIndex;
            _alertTriggers[tempIndex].SoundPath = (string)CustomTextAlertSound.SelectedItem;

            AlertList.Enabled = true;
            RemoveSelectedItem.Enabled = true;
            PlayAlertSound.Enabled = true;
            MoveAlertUp.Enabled = true;
            MoveAlertDown.Enabled = true;
            AddRangeAlertGroup.Enabled = true;
            EditSelectedItem.Visible = true;
            CancelEditSelectedItem.Visible = false;
            AddNewCustomAlert.Visible = true;
            SaveCustomAlert.Visible = false;

            NewCustomAlertText.Text = string.Empty;
            CustomAlertRepeatInterval.Value = 0;

            CustomTextAlertSound.SelectedIndex = -1;
            CustomTextAlertSound.Items.RemoveAt(RangeAlertSound.Items.Count - 1);
            CustomTextAlertSound.Items.Add("Custom...");

            WriteAlertConfig();
            LoadAlertConfig();

            AlertList.SelectedIndex = tempIndex;
        }
    }
}
