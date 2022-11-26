using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Taco
{
    public partial class MainForm
    {
        private List<int> _ignoreSystems = new List<int>();
        private List<Regex> _ignoreStrings = new List<Regex>();

        private void LoadIgnoreLists()
        {
            if (_conf.IgnoreSystems != null)
            {
                _ignoreSystems = new List<int>(_conf.IgnoreSystems);

                foreach (var tempSystemId in _ignoreSystems)
                {
                    IgnoreSystemList.Items.Add(_solarSystems.SolarSystems[tempSystemId].Name);
                }
            }

            if (_conf.IgnoreStrings != null)
            {
                foreach (var tempString in _conf.IgnoreStrings)
                {
                    _ignoreStrings.Add(new Regex(@"\b" + tempString + @"\b", RegexOptions.Compiled | RegexOptions.IgnoreCase));
                    IgnoreTextList.Items.Add(tempString);
                }
            }
        }
        private void WriteIgnoreLists()
        {
            if (_ignoreSystems.Count > 0)
            {
                int[] tempIgnoreSystems = new int[_ignoreSystems.Count];
                _ignoreSystems.CopyTo(tempIgnoreSystems);
                _conf.IgnoreSystems = tempIgnoreSystems;
            }
            else
            {
                _conf.IgnoreSystems = null;
            }

            if (IgnoreTextList.Items.Count > 0)
            {
                string[] tempIgnoreStrings = new string[IgnoreTextList.Items.Count];
                IgnoreTextList.Items.CopyTo(tempIgnoreStrings, 0);
                _conf.IgnoreStrings = tempIgnoreStrings;
            }
            else
            {
                _conf.IgnoreStrings = null;
            }
        }

        private void AddIgnoreText_Click(object sender, EventArgs e)
        {
            if (NewIgnoreText.Text.Length <= 0) return;

            _ignoreStrings.Add(new Regex(@"\b" + NewIgnoreText.Text + @"\b", RegexOptions.Compiled | RegexOptions.IgnoreCase));
            IgnoreTextList.Items.Add(NewIgnoreText.Text);
            NewIgnoreText.Clear();
            WriteIgnoreLists();
        }
        private void RemoveIgnoreText_Click(object sender, EventArgs e)
        {
            if (IgnoreTextList.SelectedIndex < 0) return;

            IgnoreTextList.Items.RemoveAt(IgnoreTextList.SelectedIndex);

            _ignoreStrings.Clear();
            foreach (var tempString in IgnoreTextList.Items)
            {
                _ignoreStrings.Add(new Regex(@"\b" + tempString + @"\b", RegexOptions.Compiled | RegexOptions.IgnoreCase));
            }
            WriteIgnoreLists();
        }
        private void AddIgnoreSystem_Click(object sender, EventArgs e)
        {
            if (NewIgnoreSystem.Text.Length <= 0) return;

            IgnoreSystemList.Items.Add(NewIgnoreSystem.Text);
            _ignoreSystems.Add(_solarSystems.Names[NewIgnoreSystem.Text]);
            NewIgnoreSystem.Text = "";
            WriteIgnoreLists();
        }
        private void RemoveIgnoreSystem_Click(object sender, EventArgs e)
        {
            if (IgnoreSystemList.SelectedIndex < 0) return;

            _ignoreSystems.Remove(_solarSystems.Names[(string)IgnoreSystemList.SelectedItem]);
            IgnoreSystemList.Items.RemoveAt(IgnoreSystemList.SelectedIndex);
            WriteIgnoreLists();
        }
        private void NewIgnoreSystem_Leave(object sender, EventArgs e)
        {
            if (NewIgnoreSystem.Text.Length == 0) return;

            foreach (var tempSystem in _solarSystems.SolarSystems.Select(tempSolarSystem => tempSolarSystem.Value).Where(tempSystem => tempSystem.MatchNameRegex(NewIgnoreSystem.Text)))
            {
                NewIgnoreSystem.Text = tempSystem.Name;
                return;
            }

            MessageBox.Show("System \"" + NewIgnoreSystem.Text + "\" not found, try again.", "System Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
            NewIgnoreSystem.Text = string.Empty;
        }
        private void AddLinkedCharacter_Click(object sender, EventArgs e)
        {
            if (NewLinkedCharacter.Text.Trim().Length <= 0) return;

            _characters.AddName(NewLinkedCharacter.Text.Trim());
            CharacterList.Items.Add(NewLinkedCharacter.Text.Trim());

            NewLinkedCharacter.Text = string.Empty;
        }
        private void RemoveLinkedCharacter_Click(object sender, EventArgs e)
        {
            if (CharacterList.SelectedIndex < 0) return;

            _characters.RemoveName(CharacterList.Items[CharacterList.SelectedIndex].ToString());
            CharacterList.Items.RemoveAt(CharacterList.SelectedIndex);
        }
    }
}
