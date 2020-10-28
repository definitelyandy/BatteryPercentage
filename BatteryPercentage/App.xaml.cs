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
using System.Runtime.CompilerServices;

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
        public static int displayed_percent = 0;
        public static bool systemLightMode = false;
        private System.Windows.Forms.ContextMenu iconMenu;
        private System.Windows.Forms.MenuItem menuItem1;
        private System.Threading.Timer t;

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
            this.menuItem1.Text = "Exit";
            this.menuItem1.Click += new System.EventHandler(this.menuItem1_Click);

            nIcon.ContextMenu = this.iconMenu;

            update_Icon(this);
            t = new System.Threading.Timer(update_Icon, null, 0, 2000);

            nIcon.Click += nIcon_Click;   
        }

        private static void update_Icon(Object stateInfo)
        {
            // read battery percentage
            PowerStatus status = SystemInformation.PowerStatus;
            int percent = Convert.ToInt32(status.BatteryLifePercent * 100);

            //avoid displaying "100" since there is no space for the third digit
            if (percent > 99){
                percent = 99;
            }
            // Get System theme
            Microsoft.Win32.RegistryKey registryKey =
                    Microsoft.Win32.Registry.CurrentUser.OpenSubKey
                        ("Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize");

            bool lightMode = Convert.ToBoolean(registryKey.GetValue("SystemUsesLightTheme"));

            if (percent != App.displayed_percent | lightMode != App.systemLightMode){
                // Get the bitmap.
                Bitmap bm = text_to_bitmap(percent.ToString(), lightMode);

                // Convert to an icon and use for the form's icon.
                nIcon.Icon = Icon.FromHandle(bm.GetHicon());
                nIcon.Visible = true;
                App.displayed_percent = percent;
                App.systemLightMode = lightMode;
            }
            
        }

        private void menuItem1_Click(object Sender, EventArgs e)
        {
            // shutdown application
            System.Windows.Application.Current.Shutdown();
        }

        void nIcon_Click(object sender, EventArgs e)
        {
            MouseEventArgs me = (MouseEventArgs) e;
            if (me.Button == MouseButtons.Left)
            {
                MainWindow.Visibility = Visibility.Visible;
            }
            
        }

        public static double GetWindowsScaling()
        {
            return Screen.PrimaryScreen.Bounds.Width / SystemParameters.PrimaryScreenWidth;
        }

        /// <summary>
        /// Draws text into bitmap of the right size for the taskbar.
        /// Tested on 1080p pannel with various scaling factors and resolution settings.
        /// </summary>
        public static Bitmap text_to_bitmap(String text, bool lightMode)
        {
            int scaledIconDim = (int)(GetWindowsScaling() * defaultIconDimension);

            Bitmap bmp = new Bitmap(scaledIconDim, scaledIconDim);

            // construct a rectangle with y offset of 1/16 of the icon so that the text will be centerd vertically
            RectangleF rectf = new RectangleF(0, scaledIconDim/defaultIconDimension, scaledIconDim, scaledIconDim);

            Graphics g = Graphics.FromImage(bmp);

            if (lightMode)
            {  
                // AntiAliasing only yields good results with light mode
                g.TextRenderingHint = TextRenderingHint.AntiAlias; 
                g.DrawString(text, defaultFont, System.Drawing.Brushes.Black, rectf); 
            }
            else
            {
                g.TextRenderingHint = TextRenderingHint.SystemDefault; 
                g.DrawString(text, defaultFont, System.Drawing.Brushes.White, rectf); 
            }
            

            g.Flush();

            return (bmp);
        }
    }
}
