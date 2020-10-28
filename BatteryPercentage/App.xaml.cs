using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Threading;

// dependencies for notification icon
using System.Windows.Forms;
using System.Drawing;


using System.Diagnostics;
using System.Drawing.Text;
using System.Windows.Media.Media3D;
using System.Diagnostics.Tracing;

namespace BatteryPercentage
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private static int defaultIconDimension = 16;
        private static Font defaultFont = new Font("Segoe UI", 8);

        public static System.Windows.Forms.NotifyIcon nIcon = new System.Windows.Forms.NotifyIcon();
        private System.Windows.Forms.ContextMenu iconMenu;
        private System.Windows.Forms.MenuItem menuItem1;

        /// <summary>
        /// Copies the binary to the users startup folder only if its name isn't already taken
        /// </summary>
        void InstallMeOnStartUp()
        {
            try
            {
                System.Reflection.Assembly curAssembly = System.Reflection.Assembly.GetExecutingAssembly();
                Debug.WriteLine(Environment.GetFolderPath(Environment.SpecialFolder.Startup));
                System.IO.File.Copy(curAssembly.Location, Environment.GetFolderPath(Environment.SpecialFolder.Startup) 
                    + "\\BatteryPercentage.exe");
            }
            catch { }
        }

        public App()
        {
            InstallMeOnStartUp();
            this.iconMenu = new System.Windows.Forms.ContextMenu();
            this.menuItem1 = new System.Windows.Forms.MenuItem();

            // Initialize contextMenu1
            this.iconMenu.MenuItems.AddRange(
                        new System.Windows.Forms.MenuItem[] { this.menuItem1 });

            // Initialize menuItem1
            this.menuItem1.Index = 0;
            this.menuItem1.Text = "E&xit";
            this.menuItem1.Click += new System.EventHandler(this.menuItem1_Click);

            nIcon.ContextMenu = this.iconMenu;

            display_Icon(this);
            System.Threading.Timer t = new System.Threading.Timer(display_Icon, null, 0, 60000);

            nIcon.Click += nIcon_Click;   
        }

        private static void display_Icon(Object stateInfo)
        {
            // read battery percentage
            PowerStatus status = SystemInformation.PowerStatus;
            float percent = status.BatteryLifePercent * 100;

            //avoid displaying "100" since there is no space for the third digit
            if (percent > 99)
            {
                percent = 99;
            }
            // Get the bitmap.
            Bitmap bm = text_to_bitmap(percent.ToString());

            // Convert to an icon and use for the form's icon.
            nIcon.Icon = Icon.FromHandle(bm.GetHicon());
            nIcon.Visible = true;
        }

        private void menuItem1_Click(object Sender, EventArgs e)
        {
            // shutdown application
            System.Windows.Application.Current.Shutdown();
        }

        void nIcon_Click(object sender, EventArgs e)
        {
            MainWindow.Visibility = Visibility.Visible;
        }

        public static double GetWindowsScaling()
        {
            return Screen.PrimaryScreen.Bounds.Width / SystemParameters.PrimaryScreenWidth;
        }

        /// <summary>
        /// Draws text into bitmap of the right size for the taskbar.
        /// Tested on 1080p pannel with various scaling factors and resolution settings.
        /// </summary>
        public static Bitmap text_to_bitmap(String text)
        {
            int scaledIconDim = (int)(GetWindowsScaling() * defaultIconDimension);

            Bitmap bmp = new Bitmap(scaledIconDim, scaledIconDim);

            // construct a rectangle with y offset of 1/16 of the icon so that the text will be centerd vertically
            RectangleF rectf = new RectangleF(0, scaledIconDim/defaultIconDimension, scaledIconDim, scaledIconDim);

            Graphics g = Graphics.FromImage(bmp);

            // AntiAliasing didn't do me any good. code snippet left, because it was recommended
            // on the interwebs. I don't want so search it up, if nedded at some point in the future.
            //g.TextRenderingHint = TextRenderingHint.AntiAlias;

            // todo: select brush according to taskbar theme
            g.DrawString(text, defaultFont, System.Drawing.Brushes.White, rectf);

            g.Flush();

            return (bmp);
        }
    }
}
