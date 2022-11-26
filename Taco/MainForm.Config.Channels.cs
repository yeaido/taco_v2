using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Taco
{
    public partial class MainForm
    {
        private Dictionary<string, TabPage> _hiddenPages = new Dictionary<string, TabPage>();

        private void SetChannelTabSize()
        {  //play with this if the tab sizes end up being a problem for some reason
            //if (UITabControl.TabCount > 0)
            //{
            //    Size tempSize = UITabControl.ItemSize;
            //    tempSize.Width = (UITabControl.Width - 4) / UITabControl.TabCount;
            //    UITabControl.ItemSize = tempSize;
            //}
        }
        private void RemovePageFromMainTabGroup(string sPageName)
        {
            if (UITabControl.TabPages.ContainsKey(sPageName))
            {
                var TabToRemove = UITabControl.TabPages[sPageName];
                _hiddenPages.Add(sPageName, TabToRemove);
                UITabControl.TabPages.Remove(TabToRemove);
                SetChannelTabSize();
            }
        }
        private void AddPageToMainTabGroup(string sPageName)
        {
            if (_hiddenPages.ContainsKey(sPageName))
            {  //we put the new page right before the config unless its not at the end
                var insertionIndex = UITabControl.TabPages.IndexOfKey("ConfigPage");
                if (insertionIndex + 1 != UITabControl.TabPages.Count)
                {
                    insertionIndex = UITabControl.TabPages.Count;
                }
                UITabControl.TabPages.Insert(insertionIndex, _hiddenPages[sPageName]);
                _hiddenPages.Remove(sPageName);
                SetChannelTabSize();
            }
        }
        private void SetAllIntelTextBoxColors(Color cNew)
        {
            for (int iLog = 0; iLog < kTotalNumberOfLogs; ++iLog)
            {
                TextBox tempTextbox = GetIntelTextBoxFromLogIndex(iLog);
                if (tempTextbox != null)
                {
                    tempTextbox.BackColor = cNew;
                }
            }
        }

        private void MonitorBranch_CheckedChanged(object sender, EventArgs e)
        {
            if (_configLoaded)
                _conf.MonitorBranch = MonitorBranch.Checked;

            BranchIntelTextBox.Enabled = MonitorBranch.Checked;
            if (MonitorBranch.Checked)
            {
                AddPageToMainTabGroup("BranchPage");
                if (m_bWatchLogs)
                {
                    StartChatLog(kBranchLogIndex);
                }
            }
            else
            {
                if (m_bWatchLogs)
                {
                    StopChatLog(kBranchLogIndex);
                }
                RemovePageFromMainTabGroup("BranchPage");
            }
        }
        private void MonitorDeklein_CheckedChanged(object sender, EventArgs e)
        {
            if (_configLoaded)
                _conf.MonitorDeklein = MonitorDeklein.Checked;

            DekleinIntelTextBox.Enabled = MonitorDeklein.Checked;
            if (MonitorDeklein.Checked)
            {
                AddPageToMainTabGroup("DekleinPage");
                if (m_bWatchLogs)
                {
                    StartChatLog(kDekleinLogIndex);
                }
            }
            else
            {
                if (m_bWatchLogs)
                {
                    StopChatLog(kDekleinLogIndex);
                }
                RemovePageFromMainTabGroup("DekleinPage");
            }
        }
        private void MonitorTenal_CheckedChanged(object sender, EventArgs e)
        {
            if (_configLoaded)
                _conf.MonitorTenal = MonitorTenal.Checked;

            TenalIntelTextBox.Enabled = MonitorTenal.Checked;
            if (MonitorTenal.Checked)
            {
                AddPageToMainTabGroup("TenalPage");
                if (m_bWatchLogs)
                {
                    StartChatLog(kTenalLogIndex);
                }
            }
            else
            {
                if (m_bWatchLogs)
                {
                    StopChatLog(kTenalLogIndex);
                }
                RemovePageFromMainTabGroup("TenalPage");
            }
        }
        private void MonitorVenal_CheckedChanged(object sender, EventArgs e)
        {
            if (_configLoaded)
                _conf.MonitorVenal = MonitorVenal.Checked;

            VenalIntelTextBox.Enabled = MonitorVenal.Checked;
            if (MonitorVenal.Checked)
            {
                AddPageToMainTabGroup("VenalPage");
                if (m_bWatchLogs)
                {
                    StartChatLog(kVenalLogIndex);
                }
            }
            else
            {
                if (m_bWatchLogs)
                {
                    StopChatLog(kVenalLogIndex);
                }
                RemovePageFromMainTabGroup("VenalPage");
            }
        }
        private void MonitorFade_CheckedChanged(object sender, EventArgs e)
        {
            if (_configLoaded)
                _conf.MonitorFade = MonitorFade.Checked;

            FadeIntelTextBox.Enabled = MonitorFade.Checked;
            if (MonitorFade.Checked)
            {
                AddPageToMainTabGroup("FadePage");
                if (m_bWatchLogs)
                {
                    StartChatLog(kFadeLogIndex);
                }
            }
            else
            {
                if (m_bWatchLogs)
                {
                    StopChatLog(kFadeLogIndex);
                }
                RemovePageFromMainTabGroup("FadePage");
            }
        }
        private void MonitorPureBlind_CheckedChanged(object sender, EventArgs e)
        {
            if (_configLoaded)
                _conf.MonitorPureBlind = MonitorPureBlind.Checked;

            PureBlindIntelTextBox.Enabled = MonitorPureBlind.Checked;
            if (MonitorPureBlind.Checked)
            {
                AddPageToMainTabGroup("PureBlindPage");
                if (m_bWatchLogs)
                {
                    StartChatLog(kPureBlindLogIndex);
                }
            }
            else
            {
                if (m_bWatchLogs)
                {
                    StopChatLog(kPureBlindLogIndex);
                }
                RemovePageFromMainTabGroup("PureBlindPage");
            }
        }
        private void MonitorTribute_CheckedChanged(object sender, EventArgs e)
        {
            if (_configLoaded)
                _conf.MonitorTribute = MonitorTribute.Checked;

            TributeIntelTextBox.Enabled = MonitorTribute.Checked;
            if (MonitorTribute.Checked)
            {
                AddPageToMainTabGroup("TributePage");
                if (m_bWatchLogs)
                {
                    StartChatLog(kTributeLogIndex);
                }
            }
            else
            {
                if (m_bWatchLogs)
                {
                    StopChatLog(kTributeLogIndex);
                }
                RemovePageFromMainTabGroup("TributePage");
            }
        }
        private void MonitorVale_CheckedChanged(object sender, EventArgs e)
        {
            if (_configLoaded)
                _conf.MonitorVale = MonitorVale.Checked;

            ValeIntelTextBox.Enabled = MonitorVale.Checked;
            if (MonitorVale.Checked)
            {
                AddPageToMainTabGroup("ValePage");
                if (m_bWatchLogs)
                {
                    StartChatLog(kValeLogIndex);
                }
            }
            else
            {
                if (m_bWatchLogs)
                {
                    StopChatLog(kValeLogIndex);
                }
                RemovePageFromMainTabGroup("ValePage");
            }
        }
        private void MonitorProvidence_CheckedChanged(object sender, EventArgs e)
        {
            if (_configLoaded)
                _conf.MonitorProvidence = MonitorProvidence.Checked;

            ProvidenceIntelTextBox.Enabled = MonitorProvidence.Checked;
            if (MonitorProvidence.Checked)
            {
                AddPageToMainTabGroup("ProvidencePage");
                if (m_bWatchLogs)
                {
                    StartChatLog(kProvidenceLogIndex);
                }
            }
            else
            {
                if (m_bWatchLogs)
                {
                    StopChatLog(kProvidenceLogIndex);
                }
                RemovePageFromMainTabGroup("ProvidencePage");
            }
        }
        private void MonitorDelve_CheckedChanged(object sender, EventArgs e)
        {
            if (_configLoaded)
                _conf.MonitorDelve = MonitorDelve.Checked;

            DelveIntelTextBox.Enabled = MonitorDelve.Checked;
            if (MonitorDelve.Checked)
            {
                AddPageToMainTabGroup("DelvePage");
                if (m_bWatchLogs)
                {
                    StartChatLog(kDelveLogIndex);
                }
            }
            else
            {
                if (m_bWatchLogs)
                {
                    StopChatLog(kDelveLogIndex);
                }
                RemovePageFromMainTabGroup("DelvePage");
            }
        }
        private void MonitorQuerious_CheckedChanged(object sender, EventArgs e)
        {
            if (_configLoaded)
                _conf.MonitorQuerious = MonitorQuerious.Checked;

            QueriousIntelTextBox.Enabled = MonitorQuerious.Checked;
            if (MonitorQuerious.Checked)
            {
                AddPageToMainTabGroup("QueriousPage");
                if (m_bWatchLogs)
                {
                    StartChatLog(kQueriousLogIndex);
                }
            }
            else
            {
                if (m_bWatchLogs)
                {
                    StopChatLog(kQueriousLogIndex);
                }
                RemovePageFromMainTabGroup("QueriousPage");
            }
        }
        private void MonitorGameLog_CheckedChanged(object sender, EventArgs e)
        {
            if (_configLoaded)
                _conf.MonitorGameLog = MonitorGameLog.Checked;
        }
        private void BranchIntelTextBox_TextChanged(object sender, EventArgs e)
        {
            if (_configLoaded)
                _conf.BranchIntelChat = BranchIntelTextBox.Text;
            BranchIntelTextBox.BackColor = Color.Empty;
        }
        private void DekleinIntelTextBox_TextChanged(object sender, EventArgs e)
        {
            if (_configLoaded)
                _conf.DekleinIntelChat = DekleinIntelTextBox.Text;
            DekleinIntelTextBox.BackColor = Color.Empty;
        }
        private void TenalIntelTextBox_TextChanged(object sender, EventArgs e)
        {
            if (_configLoaded)
                _conf.TenalIntelChat = TenalIntelTextBox.Text;
            TenalIntelTextBox.BackColor = Color.Empty;
        }
        private void VenalIntelTextBox_TextChanged(object sender, EventArgs e)
        {
            if (_configLoaded)
                _conf.VenalIntelChat = VenalIntelTextBox.Text;
            VenalIntelTextBox.BackColor = Color.Empty;
        }
        private void FadeIntelTextBox_TextChanged(object sender, EventArgs e)
        {
            if (_configLoaded)
                _conf.FadeIntelChat = FadeIntelTextBox.Text;
            FadeIntelTextBox.BackColor = Color.Empty;
        }
        private void PureBlindIntelTextBox_TextChanged(object sender, EventArgs e)
        {
            if (_configLoaded)
                _conf.PureBlindIntelChat = PureBlindIntelTextBox.Text;
            PureBlindIntelTextBox.BackColor = Color.Empty;
        }
        private void TributeIntelTextBox_TextChanged(object sender, EventArgs e)
        {
            if (_configLoaded)
                _conf.TributeIntelChat = TributeIntelTextBox.Text;
            TributeIntelTextBox.BackColor = Color.Empty;
        }
        private void ValeIntelTextBox_TextChanged(object sender, EventArgs e)
        {
            if (_configLoaded)
                _conf.ValeIntelChat = ValeIntelTextBox.Text;
            ValeIntelTextBox.BackColor = Color.Empty;
        }
        private void ProvidenceIntelTextBox_TextChanged(object sender, EventArgs e)
        {
            if (_configLoaded)
                _conf.ProvidenceIntelChat = ProvidenceIntelTextBox.Text;
            ProvidenceIntelTextBox.BackColor = Color.Empty;
        }
        private void DelveIntelTextBox_TextChanged(object sender, EventArgs e)
        {
            if (_configLoaded)
                _conf.DelveIntelChat = DelveIntelTextBox.Text;
            DelveIntelTextBox.BackColor = Color.Empty;
        }
        private void QueriousIntelTextBox_TextChanged(object sender, EventArgs e)
        {
            if (_configLoaded)
                _conf.QueriousIntelChat = QueriousIntelTextBox.Text;
            QueriousIntelTextBox.BackColor = Color.Empty;
        }
        private void AlertBranch_CheckedChanged(object sender, EventArgs e)
        {
            if (_configLoaded)
                _conf.AlertBranch = AlertBranch.Checked;
        }
        private void AlertDeklein_CheckedChanged(object sender, EventArgs e)
        {
            if (_configLoaded)
                _conf.AlertDeklein = AlertDeklein.Checked;
        }
        private void AlertTenal_CheckedChanged(object sender, EventArgs e)
        {
            if (_configLoaded)
                _conf.AlertTenal = AlertTenal.Checked;
        }
        private void AlertVenal_CheckedChanged(object sender, EventArgs e)
        {
            if (_configLoaded)
                _conf.AlertVenal = AlertVenal.Checked;
        }
        private void AlertFade_CheckedChanged(object sender, EventArgs e)
        {
            if (_configLoaded)
                _conf.AlertFade = AlertFade.Checked;
        }
        private void AlertPureBlind_CheckedChanged(object sender, EventArgs e)
        {
            if (_configLoaded)
                _conf.AlertPureBlind = AlertPureBlind.Checked;
        }
        private void AlertTribute_CheckedChanged(object sender, EventArgs e)
        {
            if (_configLoaded)
                _conf.AlertTribute = AlertTribute.Checked;
        }
        private void AlertVale_CheckedChanged(object sender, EventArgs e)
        {
            if (_configLoaded)
                _conf.AlertVale = AlertVale.Checked;
        }
        private void AlertProvidence_CheckedChanged(object sender, EventArgs e)
        {
            if (_configLoaded)
                _conf.AlertProvidence = AlertProvidence.Checked;
        }
        private void AlertDelve_CheckedChanged(object sender, EventArgs e)
        {
            if (_configLoaded)
                _conf.AlertDelve = AlertDelve.Checked;
        }
        private void AlertQuerious_CheckedChanged(object sender, EventArgs e)
        {
            if (_configLoaded)
                _conf.AlertQuerious = AlertQuerious.Checked;
        }
    }
}
