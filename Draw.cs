using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurvedInsulation
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    public class Draw : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            if (doc.IsFamilyDocument)
            {
                TaskDialog.Show("Error", "This tool only works in Project environment.");

                return Result.Cancelled;
            }

            Transaction t1 = new Transaction(doc, "Draw Insulation");

            try
            {
                Reference refinsulation = uidoc.Selection.PickObject(ObjectType.Element, new SelectionFilterAnnotation(), "Select Insulation Batting");
                
                Reference refcurve = uidoc.Selection.PickObject(ObjectType.Element, new SelectionFilterCurve(), "Select Curve");

                DetailCurve insulationdetailcurve = doc.GetElement(refinsulation) as DetailCurve;

                DetailCurve detailcurve = doc.GetElement(refcurve) as DetailCurve;

                double width = insulationdetailcurve.get_Parameter(BuiltInParameter.INSULATION_WIDTH).AsDouble();//swapped to built in parameter for cross-language compatibility

                double ratio = insulationdetailcurve.get_Parameter(BuiltInParameter.INSULATION_SCALE).AsDouble();//swapped to built in parameter for cross-language compatibility

                Curve curve = detailcurve.GeometryCurve;                

                t1.Start();

                if (curve is Arc && curve.IsBound)
                {
                    List<Line> lines = SplitArc(curve, width , ratio);

                    foreach (Line l in lines)
                    {
                        DetailCurve newcurve = doc.GetElement(ElementTransformUtils.CopyElement(doc, insulationdetailcurve.Id, new XYZ()).First()) as DetailCurve;
                        newcurve.GeometryCurve = l;
                    }
                }
                if(curve is Line && curve.IsBound)
                {
                    Line l = curve as Line;
                    DetailCurve newcurve = doc.GetElement(ElementTransformUtils.CopyElement(doc, insulationdetailcurve.Id, new XYZ()).First()) as DetailCurve;
                    newcurve.GeometryCurve = l;
                }

                t1.Commit();

                return Result.Succeeded;
            }
            catch (Exception e)
            {
                if (t1.HasStarted()) // check to see if the transaction even started before trying to roll it back
                {
                    t1.RollBack();
                }
                return Result.Cancelled;
            }    
        }
        private List<Line> SplitArc(Curve curve, double width, double ratio)
        {
            List <Line> Lines = new List<Line>();

            Arc arc = curve as Arc;

            Arc offsetarc = curve.CreateOffset(width / 2, arc.Normal) as Arc;
                        
            double radius = offsetarc.Radius;

            double chord = 1.1 * (width / ratio);

            double angle = 2 * Math.Asin(chord / (2 * radius));

            double arcangle = offsetarc.Length / offsetarc.Radius;

            int divisions = (int)Math.Floor(arcangle / angle) - 1;

            List<XYZ> points = new List<XYZ>();

            for (int i = 0; i < divisions + 1; i++)
            {
                double param = i * (1.0 / divisions);
                XYZ point = offsetarc.Evaluate(param, true);
                points.Add(point);
            }

            for (int i = 0; i< points.Count - 1; i++)
            {
                Line l = Line.CreateBound(points[i], points[i + 1]);
                Line nl = l.CreateOffset(width/2, arc.Normal.Negate()) as Line;
                Lines.Add(nl);
            }

            return Lines;            
        }
    }
    internal sealed class SelectionFilterCurve : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            if (elem is DetailCurve)
            {
                DetailCurve cv = elem as DetailCurve;

                if (cv.CurveElementType == CurveElementType.DetailCurve)
                {
                    return true;
                }                
            }

            return false;
        }
        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
    internal sealed class SelectionFilterAnnotation : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            if (elem is DetailCurve)
            {
                DetailCurve cv = elem as DetailCurve;

                if (cv.CurveElementType == CurveElementType.Insulation)
                {
                    return true;
                }
            }
            return false;
        }
        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
}
