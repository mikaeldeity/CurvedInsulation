using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace CurvedInsulation
{
    public class App : IExternalApplication
    {
        static void AddRibbonPanel(UIControlledApplication application)
        {
            RibbonPanel ribbonPanel = application.CreateRibbonPanel("Insulation");

            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;

            PushButtonData b1Data = new PushButtonData("Curved", "Curved", thisAssemblyPath, "CurvedInsulation.Draw");
            PushButton pb1 = ribbonPanel.AddItem(b1Data) as PushButton;
            pb1.ToolTip = "Draw Curved Insulation. Pick a reference Insulation Batting and then the arc or line be applied to.";
            Uri addinImage =
                new Uri("pack://application:,,,/CurvedInsulation;component/Resources/CurvedInsulation.png");
            BitmapImage pb1Image = new BitmapImage(addinImage);
            pb1.LargeImage = pb1Image;
        }
        public Result OnStartup(UIControlledApplication application)
        {
            AddRibbonPanel(application);

            return Result.Succeeded;
        }
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }        
    }
}
