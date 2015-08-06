﻿using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using TISFAT.Entities;
using TISFAT.Util;

namespace TISFAT
{
    public partial class MainForm : Form
    {
        public Project ActiveProject;
        private string ProjectFileName;
        private bool ProjectDirty;

        public CanvasForm Canvas;
        public Timeline MainTimeline;
        public ToolboxForm Toolbox;

        private static Random Why = new Random();

        public int HScrollVal
        {
            get { return scrl_HTimeline.Value; }
        }

        public bool HScrollVisible { get { return scrl_HTimeline.Visible; } }

        public int VScrollVal
        {
            get { return scrl_VTimeline.Value; }
        }

        public bool VScrollVisible { get { return scrl_VTimeline.Visible; } }

        public MainForm()
        {
            this.DoubleBuffered = true;
            
            InitializeComponent();

            GLContext.VSync = true;
            GLContext.KeyPress += MainForm_KeyPress;
            GLContext.MouseWheel += GLContext_MouseWheel;

            #region General Init
            // Create and show forms
            Canvas = new CanvasForm(sc_MainContainer.Panel2);
            Canvas.Show();

            MainTimeline = new Timeline(GLContext);

            Toolbox = new ToolboxForm(sc_MainContainer.Panel2);
            Toolbox.Show();

            ProjectNew();
            AddTestLayer();
            AddTestLayer();
            AddTestLayer();
            AddTestLayer();
            AddTestLayer();
            AddTestLayer();
            AddTestLayer();
            #endregion
        }

        private void GLContext_MouseWheel(object sender, MouseEventArgs e)
        {
            ScrollBar bar = scrl_VTimeline;

            if(ModifierKeys == Keys.Shift)
                bar = scrl_HTimeline;

            if (!bar.Visible)
                return;

            int scrollAmount = bar.SmallChange * -(e.Delta / 100);

            if (bar.Value + scrollAmount > 1 + bar.Maximum - bar.LargeChange)
                bar.Value = 1 + bar.Maximum - bar.LargeChange;
            else if (bar.Value + scrollAmount < bar.Minimum)
                bar.Value = bar.Minimum;
            else
                bar.Value += scrollAmount;

            MainTimeline.GLContext.Invalidate();
        }

        public void CalcScrollBars(int HContentSize, int VContentSize, int HViewSize, int VViewSize)
        {
            scrl_HTimeline.Visible = HViewSize < HContentSize;
            scrl_VTimeline.Visible = VViewSize < VContentSize;

            if (scrl_HTimeline.Visible)
            {
                scrl_HTimeline.Minimum = 0;

                scrl_HTimeline.SmallChange = HViewSize / 10;
                scrl_HTimeline.LargeChange = HViewSize / 5;

                scrl_HTimeline.Maximum = HContentSize - HViewSize;
                scrl_HTimeline.Maximum += scrl_HTimeline.LargeChange;
            }
            if(scrl_VTimeline.Visible)
            {
                scrl_VTimeline.Minimum = 0;

                scrl_VTimeline.SmallChange = VViewSize / 10;
                scrl_VTimeline.LargeChange = VViewSize / 5;

                scrl_VTimeline.Maximum = VContentSize - VViewSize;
                scrl_VTimeline.Maximum += scrl_VTimeline.LargeChange;
            }

            if (!scrl_HTimeline.Visible)
                scrl_HTimeline.Value = 0;
            if (!scrl_VTimeline.Visible)
                scrl_VTimeline.Value = 0;

            pnl_ScrollSquare.Visible = scrl_HTimeline.Visible || scrl_VTimeline.Visible;
        }

        private void AddTestLayer()
        {
            StickFigure figure = new StickFigure();

            var hip = new StickFigure.Joint();
            figure.Root = hip;
            var neck = new StickFigure.Joint(hip);
            hip.Children.Add(neck);
            var boop = new StickFigure.Joint(neck);
            neck.Children.Add(boop);

            Layer layer = new Layer(figure);
            layer.Framesets.Add(new Frameset());

            StickFigure.State state1 = new StickFigure.State();
            var ship = new StickFigure.Joint.State();
            ship.Location = new PointF(200f, 200f);
            state1.Root = ship;
            var sneck = new StickFigure.Joint.State(ship);
            sneck.Location = new PointF(200f, 147f);
            ship.Children.Add(sneck);
            var sboop = new StickFigure.Joint.State(sneck);
            sboop.Location = new PointF(300f, 225f);
            sneck.Children.Add(sboop);
            layer.Framesets[0].Keyframes.Add(new Keyframe(0, state1));

            StickFigure.State state2 = new StickFigure.State();
            var ship2 = new StickFigure.Joint.State();
            ship2.Location = new PointF(200f, 200f);
            state2.Root = ship2;
            var sneck2 = new StickFigure.Joint.State(ship2);
            sneck2.Location = new PointF(100f, 147f);
            ship2.Children.Add(sneck2);
            var sboop2 = new StickFigure.Joint.State(sneck2);
            sboop2.Location = new PointF(250f, 257f);
            sneck2.Children.Add(sboop2);
            layer.Framesets[0].Keyframes.Add(new Keyframe((uint)Why.Next(4, 40), state2));

            ActiveProject.Layers.Add(layer);
            MainTimeline.GLContext.Invalidate();
        }
        
        public void SetDirty(bool dirty)
        {
            ProjectDirty = dirty;
            Text = "TISFAT Zero - " + (Path.GetFileNameWithoutExtension(ProjectFileName) ?? "Untitled") + (dirty ? " *" : "");
        }

        private void SetFileName(string filename)
        {
            ProjectFileName = filename;
            SetDirty(filename == null);
        }

        #region MainForm Hooks
        private void MainForm_Load(object sender, EventArgs e)
        {
            MainTimeline.GLContext_Init();
            MainTimeline.Resize();
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            MainTimeline.Resize();
        }

        private void MainForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Q)
                MainTimeline.SeekStart();

            if (e.KeyChar == (char)Keys.Space)
                btn_PlayPause_Click(null, null);
        } 
        #endregion

        #region GLContext <-> Timeline hooks
        private void sc_MainContainer_SplitterMoved(object sender, SplitterEventArgs e)
        {
            MainTimeline.Resize();
        }
        
        private void GLContext_Paint(object sender, PaintEventArgs e)
        {
            MainTimeline.GLContext_Paint(sender, e);

            if (MainTimeline.IsPlaying())
                MainTimeline.GLContext.Invalidate();
        }

        private void GLContext_MouseMove(object sender, MouseEventArgs e)
        {
            MainTimeline.MouseMoved(e.Location);
        }

        private void GLContext_MouseLeave(object sender, EventArgs e)
        {
            MainTimeline.MouseLeft();
        }

        private void GLContext_MouseDown(object sender, MouseEventArgs e)
        {
            MainTimeline.MouseDown(e.Location);
        }

        private void GLContext_MouseUp(object sender, MouseEventArgs e)
        {
            MainTimeline.MouseUp();
        }
        #endregion

        #region File Saving / Loading
        public void ProjectNew()
        {
            ActiveProject = new Project();
            SetFileName(null);
            MainTimeline.GLContext.Invalidate();
        }

        public void ProjectOpen(string filename)
        {
            ActiveProject = new Project();

            using (var reader = new BinaryReader(new FileStream(filename, FileMode.Open)))
            {
                UInt16 version = reader.ReadUInt16();
                ActiveProject.Read(reader, version);
            }

            SetFileName(filename);
            MainTimeline.GLContext.Invalidate();
        }

        public void ProjectSave(string filename)
        {
            using (var writer = new BinaryWriter(new FileStream(filename, FileMode.Create)))
            {
                writer.Write(FileFormat.Version);
                ActiveProject.Write(writer);
            }

            SetFileName(filename);
            ProjectFileName = filename;
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ProjectNew();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.AddExtension = true;
            dialog.Filter = "TISFAT Zero Project|*.tzp";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                ProjectOpen(dialog.FileName);
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ProjectFileName != null)
                ProjectSave(ProjectFileName);
            else
                saveAsToolStripMenuItem_Click(sender, e);
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.AddExtension = true;
            dialog.Filter = "TISFAT Zero Project|*.tzp";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                ProjectSave(dialog.FileName);
            }
        } 
        #endregion

        #region Timeline Control Hooks
        private void btn_PlayPause_MouseEnter(object sender, EventArgs e)
        {
            btn_PlayPause.Image = MainTimeline.IsPlaying() ? Properties.Resources.pause_hover : Properties.Resources.play_hover;
        }

        private void btn_PlayPause_MouseLeave(object sender, EventArgs e)
        {
            btn_PlayPause.Image = MainTimeline.IsPlaying() ? Properties.Resources.pause_normal : Properties.Resources.play_normal;
        }

        private void btn_PlayPause_Click(object sender, EventArgs e)
        {
            MainTimeline.TogglePause();
            btn_PlayPause.Image = MainTimeline.IsPlaying() ? Properties.Resources.pause_hover : Properties.Resources.play_hover;
        }

        private void btn_Start_Click(object sender, EventArgs e)
        {
            MainTimeline.SeekStart();
        }

        private void btn_End_Click(object sender, EventArgs e)
        {
            MainTimeline.SeekLastFrame();
        }
        #endregion

        #region Live update view when splitter is moved
        private void sc_MainContainer_MouseDown(object sender, MouseEventArgs e)
        {
            // This disables the normal move behavior
            ((SplitContainer)sender).IsSplitterFixed = true;
        }

        private void sc_MainContainer_MouseUp(object sender, MouseEventArgs e)
        {
            // This allows the splitter to be moved normally again
            ((SplitContainer)sender).IsSplitterFixed = false;
        }

        private void sc_MainContainer_MouseMove(object sender, MouseEventArgs e)
        {
            // Check to make sure the splitter won't be updated by the
            // normal move behavior also
            if (((SplitContainer)sender).IsSplitterFixed)
            {
                // Make sure that the button used to move the splitter
                // is the left mouse button
                if (e.Button.Equals(MouseButtons.Left))
                {
                    // Checks to see if the splitter is aligned Vertically
                    if (((SplitContainer)sender).Orientation.Equals(Orientation.Vertical))
                    {
                        // Only move the splitter if the mouse is within
                        // the appropriate bounds
                        if (e.X > 0 && e.X < ((SplitContainer)sender).Width)
                        {
                            // Move the splitter
                            ((SplitContainer)sender).SplitterDistance = e.X;
                        }
                    }
                    // If it isn't aligned vertically then it must be
                    // horizontal
                    else
                    {
                        // Only move the splitter if the mouse is within
                        // the appropriate bounds
                        if (e.Y > 0 && e.Y < ((SplitContainer)sender).Height)
                        {
                            // Move the splitter
                            ((SplitContainer)sender).SplitterDistance = e.Y;
                        }
                    }
                }
                // If a button other than left is pressed or no button
                // at all
                else
                {
                    // This allows the splitter to be moved normally again
                    ((SplitContainer)sender).IsSplitterFixed = false;
                }
            }
        }
        #endregion

        private void scrl_Timeline_Scroll(object sender, ScrollEventArgs e)
        {
            MainTimeline.GLContext.Invalidate();
        }
    }
}