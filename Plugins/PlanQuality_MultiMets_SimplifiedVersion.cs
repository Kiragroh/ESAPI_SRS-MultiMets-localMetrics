using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

[assembly: ESAPIScript(IsWriteable = true)]
[assembly: AssemblyVersion("1.0.0.1")]

namespace VMS.TPS
{
  public class Script
  {
    public Script()
    {
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public void Execute(ScriptContext context /*, System.Windows.Window window, ScriptEnvironment environment*/)
    {
      // TODO : Add here the code that is called when the script is launched from Eclipse.

        // check if the plan has dose
        if (!context.PlanSetup.IsDoseValid)
        {
            MessageBox.Show("The opened plan has no valid dose.");
            return;
        }

        // get list of structures for loaded plan
        StructureSet ss = context.StructureSet;
        PlanSetup ps = context.PlanSetup;
        var listStructures = ss.Structures.OrderBy(x=>x.Id);

        // search for body
        Structure body = listStructures.Where(x => !x.IsEmpty && (x.DicomType.ToUpper().Equals("EXTERNAL") || x.Id.ToUpper().Equals("KÖRPER") || x.Id.ToUpper().Equals("BODY") || x.Id.ToUpper().Equals("OUTER CONTOUR"))).FirstOrDefault();
        if (body == null)
        {
            MessageBox.Show("Unbekannte Körper-Struktur-Bezeichnung. Verwende 'Körper', 'Body' oder 'Outer Contour' (oder DicomType 'External').");
            return;
        }
        // enable writing with this script.
        context.Patient.BeginModifications();

        ps.DoseValuePresentation = DoseValuePresentation.Absolute;
        DoseValue d12Gy = new DoseValue(12, DoseValue.DoseUnit.Gy);
        DoseValue dPrescGy = new DoseValue(ps.TotalDose.Dose, DoseValue.DoseUnit.Gy);

        //define rings, margins and dummy for local metric calculations
        Structure ptvRing_small_1 = ss.AddStructure("CONTROL", "zPTVring_small_1");
        ptvRing_small_1.ConvertToHighResolution();
        Structure ptvRing_small_2 = ss.AddStructure("CONTROL", "zPTVring_small_2");
        ptvRing_small_2.ConvertToHighResolution();
        Structure ptvRing_big_1 = ss.AddStructure("CONTROL", "zPTVring_big_1");
        ptvRing_big_1.ConvertToHighResolution();
        Structure ptvRing_big_2 = ss.AddStructure("CONTROL", "zPTVring_big_2");
        ptvRing_big_2.ConvertToHighResolution();
        Structure dummy = ss.AddStructure("CONTROL", "zDummy1");
        double margin_small_1 = 4;
        double margin_small_2 = 5;
        double margin_big_1 = 7.7;
        double margin_big_2 = 8.7;    
        
        //calculate TotalMU
        double TotalMU = 0;
        try
        {
            foreach (Beam b in ps.Beams.Where(x => !x.IsSetupField))
                TotalMU += b.Meterset.Value;
        }
        catch { }

        string msg = string.Format("Local SRS metrics for plan '{0}' with {1}MU:\n", ps.Id,Math.Round(TotalMU,0));


        //foreach (Structure ptv in listStructures.Where(x=> x.DicomType.ToUpper() == "PTV" && (x.Id == "PTV03") || (x.Id == "PTV04L M3 cerebellär")))
        foreach (Structure ptv in listStructures.Where(x=> x.DicomType.ToUpper() == "PTV" ))
        {
            dPrescGy = new DoseValue(ps.TotalDose.Dose, DoseValue.DoseUnit.Gy);
            // change prescription dose if a totaldose limit for a referencePoint with same ID exists (since Eclipse16 structure names can be longer than 16 chars but RP-Ids not -> therefore this simplification will not always work)
            foreach (ReferencePoint rp in ps.ReferencePoints.Where(x=> x.Id == ptv.Id && x.TotalDoseLimit.ToString() != "N/A"))
            {
                dPrescGy = new DoseValue(rp.TotalDoseLimit.Dose, DoseValue.DoseUnit.Gy);
                break;
            }

            // skip ptvs that have median less dose than prescription
            if (ps.GetDoseAtVolume(ptv, 50, VolumePresentation.Relative, DoseValuePresentation.Absolute).Dose < dPrescGy.Dose)
                continue;

            // expand PTV
            if (ptv.IsHighResolution)
            {
                ptvRing_small_1.SegmentVolume = ptv.Margin(margin_small_1);
                ptvRing_small_2.SegmentVolume = ptv.Margin(margin_small_2);
                ptvRing_small_2.SegmentVolume = ptvRing_small_1.Or(ptvRing_small_2);
                ptvRing_big_1.SegmentVolume = ptv.Margin(margin_big_1);
                ptvRing_big_2.SegmentVolume = ptv.Margin(margin_big_2);
                ptvRing_big_2.SegmentVolume = ptvRing_big_1.Or(ptvRing_big_2);
            }
            else
            {                
                dummy.SegmentVolume = ptv.SegmentVolume;
                if (dummy.CanConvertToHighResolution())
                    dummy.ConvertToHighResolution();
                
                ptvRing_small_1.SegmentVolume = dummy.Margin(margin_small_1);
                ptvRing_small_2.SegmentVolume = dummy.Margin(margin_small_2); 
                ptvRing_small_2.SegmentVolume = ptvRing_small_1.Or(ptvRing_small_2);
                ptvRing_big_1.SegmentVolume = dummy.Margin(margin_big_1);
                ptvRing_big_2.SegmentVolume = dummy.Margin(margin_big_2);
                ptvRing_big_2.SegmentVolume = ptvRing_big_1.Or(ptvRing_big_2);
            }

            double v50 = ps.GetVolumeAtDose(ptvRing_big_1, dPrescGy / 2, VolumePresentation.AbsoluteCm3);
            double v50_2 = ps.GetVolumeAtDose(ptvRing_big_2, dPrescGy / 2, VolumePresentation.AbsoluteCm3);
            double v100 = ps.GetVolumeAtDose(ptvRing_small_1, dPrescGy, VolumePresentation.AbsoluteCm3);
            double v100_2 = ps.GetVolumeAtDose(ptvRing_small_2, dPrescGy, VolumePresentation.AbsoluteCm3);

            double v12Gy_ = Math.Round(ps.GetVolumeAtDose(ptvRing_big_1, d12Gy, VolumePresentation.AbsoluteCm3) - ps.GetVolumeAtDose(ptv, d12Gy, VolumePresentation.AbsoluteCm3), 2);
            double GI_ptv = Math.Round(v50 / v100, 2);

            double ptv100 = ps.GetVolumeAtDose(ptv, dPrescGy, VolumePresentation.AbsoluteCm3);            
            double CI_ptv = Math.Round(v100 * ptv.Volume / (ptv100 * ptv100), 2);

            //check dor dose bridging. clear the metrics if dose metrics occur to prevent misreporting.
            //This method is not perfect but fast. to check whether dose pixel are between two rings would require the creation of isodose structures. Possible but would take additional time.
            if (Math.Round(v50,1) != Math.Round(v50_2,1))
            {
                GI_ptv = Double.NaN;
                v12Gy_ = Double.NaN;
            }
            if (Math.Round(v100,1) != Math.Round(v100_2,1))
                CI_ptv = Double.NaN;

            //calculate isocenter distance
            Beam firstbeam = ps.Beams.Where(b => b.IsSetupField == false).First();
            VVector isoc = new VVector(Double.NaN, Double.NaN, Double.NaN);
            isoc = firstbeam.IsocenterPosition;
            VVector targetCenter = ptv.CenterPoint;
            double dist = Math.Round((targetCenter - isoc).Length/10,1);
            
            msg += string.Format("\tId: {0}\tVolume: {1}cc\tDose: {6}Gy\nIsoDistance: {2}cm\tCI: {3}\tGI: {4}\tlocalV12: {5}cc\n", ptv.Id,Math.Round(ptv.Volume,2),dist,CI_ptv, GI_ptv, v12Gy_,Math.Round(dPrescGy.Dose,0));

        }
        //delete help structures
        ss.RemoveStructure(ptvRing_small_1);
        ss.RemoveStructure(ptvRing_small_2);
        ss.RemoveStructure(ptvRing_big_1);
        ss.RemoveStructure(ptvRing_big_2);
        ss.RemoveStructure(dummy);

        //show result
        MessageBox.Show(msg, "Simple Local SRS-PlanQuality Metric Script by MG (UKE)                                                                                                   .");
    }
  }
}
