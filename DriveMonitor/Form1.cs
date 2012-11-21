using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DriveMonitor
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Motors = new Drive();
            Pult = new Client(Motors);
        }
        Drive Motors;
        Client Pult;

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            //  MessageBox.Show(e.KeyCode.ToString());
            this.timer1.Enabled = false;

            switch (e.KeyCode)
            {

                case Keys.NumPad8:
                case Keys.Up: { Motors.Forward(); break; }
                case Keys.NumPad2:
                case Keys.Down: { Motors.Back(); break; }
                case Keys.NumPad4:
                case Keys.Left: { Motors.Left(); break; }
                case Keys.NumPad6:
                case Keys.Right: { Motors.Right(); break; }
                case Keys.NumPad5:
                case Keys.Z: { Motors.IncPower(); break; }
                case Keys.NumPad0:
                case Keys.X: { Motors.DecPower(); break; }
                case Keys.NumPad7: { Motors.Forward(); Motors.Left(); break; }
                case Keys.NumPad9: { Motors.Forward(); Motors.Right(); break; }
                case Keys.NumPad1: { Motors.Back(); Motors.Left(); break; }
                case Keys.NumPad3: { Motors.Back(); Motors.Right(); break; }

            }
            progressBar1.Value = Convert.ToInt32(Motors.RecvPower[0]);
            progressBar2.Value = Convert.ToInt32(Motors.RecvPower[1]);
            progressBar3.Value = Convert.ToInt32(Motors.RecvPower[2]);
            progressBar4.Value = Convert.ToInt32(Motors.RecvPower[3]);
            foreach (Control s in this.Controls) s.Refresh();
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            //Graphics g = e.Graphics;
            System.Drawing.Pen pen1 = new System.Drawing.Pen(Color.SteelBlue, 2F);
            int m = (progressBar1.Value + progressBar3.Value - progressBar2.Value - progressBar4.Value);
            e.Graphics.DrawLine(pen1, 0, pictureBox1.Size.Height / 2 - m, pictureBox1.Size.Width, pictureBox1.Size.Height / 2 + m);
            //this.pictureBox1.Refresh();
        }

        private void pictureBox2_Paint(object sender, PaintEventArgs e)
        {
            //Graphics g = e.Graphics;
            System.Drawing.Pen pen2 = new System.Drawing.Pen(Color.Red, 2F);
            int m = (progressBar1.Value + progressBar2.Value - progressBar3.Value - progressBar4.Value);
            e.Graphics.DrawLine(pen2, 0, pictureBox2.Size.Height / 2 + m, pictureBox2.Size.Width, pictureBox2.Size.Height / 2 - m);
            // this.pictureBox2.Refresh();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Motors.Smooth();
            StatusString.Text = Pult.Status;
            progressBar1.Value = Convert.ToInt32(Motors.RecvPower[0]);
            progressBar2.Value = Convert.ToInt32(Motors.RecvPower[1]);
            progressBar3.Value = Convert.ToInt32(Motors.RecvPower[2]);
            progressBar4.Value = Convert.ToInt32(Motors.RecvPower[3]);
            foreach (Control s in this.Controls) s.Refresh();
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            this.timer1.Enabled = true;
        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Pult.m_formClosed = true;
        }
    }
}
