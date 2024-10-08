using Autodesk.Revit.UI;
using System;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace CurvedInsulation
{
    public class App : IExternalApplication
    {
        static void AddRibbonPanel(UIControlledApplication application)
        {
            RibbonPanel panel = application.CreateRibbonPanel("Insulation");

            string path = Assembly.GetExecutingAssembly().Location;

            PushButtonData data = new PushButtonData("Curved", "Curved", path, "CurvedInsulation.Draw");
            PushButton button = panel.AddItem(data) as PushButton;
            button.ToolTip = "Draw Curved Insulation. Pick a reference Insulation Batting and then the arc or line be applied to.";
            BitmapImage image = new BitmapImage(new Uri("pack://application:,,,/CurvedInsulation;component/Resources/CurvedInsulation.png"));
            button.LargeImage = image;
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
