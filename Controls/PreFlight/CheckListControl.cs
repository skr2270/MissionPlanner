using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace MissionPlanner.Controls.PreFlight
{
    public partial class CheckListControl : UserControl
    {
        public List<CheckListItem> CheckListItems = new List<CheckListItem>();

        public string configfile = "checklist.xml";

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
            panel1.SuspendLayout();
            panel1.Controls.Clear();

            int y = 0;

            lock (this.CheckListItems)
            {
                foreach (var item in this.CheckListItems)
                {
                    var wrnctl = addwarningcontrol(5, y, item);

                    y = wrnctl.Bottom;
                }
            }
            panel1.ResumeLayout();
        }

        Control addwarningcontrol(int x, int y, CheckListItem item, bool hideforchild = false)
        {
            Label desc = new Label() { Text = item.Description, Location = new Point(x, y), Size = new Size(150, 42) };
            Label text = new Label() { Text = item.DisplayText(), Location = new Point(desc.Right, y), Size = new Size(150, 42) };
            CheckBox tickbox = new CheckBox() { Checked = item.checkCond(item), Tag = item, Location = new Point(text.Right, y) };

            if (tickbox.Checked)
            {
                text.ForeColor = item._TrueColor;
            } else
            {
                text.ForeColor = item._FalseColor;
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
        }
    }
}
