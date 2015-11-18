using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Collections;

namespace MissionPlanner.Controls.PreFlight
{
    public partial class CheckListControl : UserControl
    {
        public List<CheckListItem> CheckListItems = new List<CheckListItem>();

        public string configfile = "checklist.xml";

        int rowcount = 0;

        public CheckListControl()
        {
            InitializeComponent();

            MissionPlanner.Controls.PreFlight.CheckListItem.defaultsrc = MainV2.comPort.MAV.cs;

            try
            {
                LoadConfig();
            }
            catch
            {
                Console.WriteLine("Failed to read CheckList config file " + configfile);
            }

            timer1.Start();
        }

        public void Draw()
        {
            if (rowcount == this.CheckListItems.Count)
                return;

            panel1.SuspendLayout();
            panel1.Controls.Clear();

            int y = 0;
            rowcount = 0;

            lock (this.CheckListItems)
            {
                foreach (var item in this.CheckListItems)
                {
                    var wrnctl = addwarningcontrol(5, y, item);

                    rowcount++;

                    y = wrnctl.Bottom;
                }
            }
            panel1.ResumeLayout(true);
        }

        void UpdateDisplay()
        {
            foreach (Control item in panel1.Controls)
            {
                if (item.Name.StartsWith("utext"))
                {
                    item.Text = ((CheckListItem)item.Tag).DisplayText();
                }
            }
        }

        Control addwarningcontrol(int x, int y, CheckListItem item, bool hideforchild = false)
        {
            var desctext = item.Description;
            var texttext = item.DisplayText();

            var height = 21;
            if (desctext.Length > 25 || texttext.Length > 25)
                height = 42;

            Label desc = new Label() { Text = desctext, Location = new Point(x, y), Size = new Size(150, height), Tag = item, Name = "udesc" + y };
            Label text = new Label() { Text = texttext, Location = new Point(desc.Right, y), Size = new Size(150, height), Tag = item, Name = "utext"+y };
            CheckBox tickbox = new CheckBox() { Checked = item.checkCond(item), Tag = item, Location = new Point(text.Right, y), Size = new Size(21, 21), Name = "utickbox" + y };

            if (tickbox.Checked)
            {
                text.ForeColor = item._TrueColor;
                desc.ForeColor = item._TrueColor;
            } else
            {
                text.ForeColor = item._FalseColor;
                desc.ForeColor = item._FalseColor;
            }

            panel1.Controls.Add(desc);
            panel1.Controls.Add(text);
            panel1.Controls.Add(tickbox);

            y = desc.Bottom;

            if (item.Child != null)
            {
                return addwarningcontrol(x += 5, y, item.Child, true);
            }

            return desc;
        }

        public void LoadConfig()
        {
            if (!File.Exists(configfile))
                return;

            System.Xml.Serialization.XmlSerializer reader =
                new System.Xml.Serialization.XmlSerializer(typeof(List<CheckListItem>),
                    new Type[] { typeof(CheckListItem) });

            using (StreamReader sr = new StreamReader(configfile))
            {
                CheckListItems = (List<CheckListItem>)reader.Deserialize(sr);
            }
        }

        public void SaveConfig()
        {
            // save config
            System.Xml.Serialization.XmlSerializer writer =
                new System.Xml.Serialization.XmlSerializer(typeof(List<CheckListItem>),
                    new Type[] { typeof(CheckListItem), typeof(Color) });

            using (StreamWriter sw = new StreamWriter(configfile))
            {
                lock (CheckListItems)
                {
                    writer.Serialize(sw, CheckListItems);
                }
            }
        }

        private void BUT_edit_Click(object sender, EventArgs e)
        {
            CheckListEditor form = new CheckListEditor(this);
            form.Show();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Draw();
            UpdateDisplay();
        }
    }
}
