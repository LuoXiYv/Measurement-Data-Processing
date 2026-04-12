using System.Globalization;

namespace MDP_2.Models;

public class LineClass
{
    public string LID { get; set; } = string.Empty;
    public string SPID { get; set; } = string.Empty;
    public string EPID { get; set; } = string.Empty;

    public double ForwardDH { get; set; }
    public double BackwardDH { get; set; }
    public double Distance { get; set; }

    public double dH => Math.Abs(BackwardDH) > 1e-10 ? (ForwardDH + BackwardDH) / 2.0 : ForwardDH;

    public double Di => ForwardDH - BackwardDH;
    public double DiMm => Di * 1000.0;
    public double Di2 => Di * Di;
    public double Di2Mm2 => DiMm * DiMm;
    public double PDi2 => Distance > 1e-10 ? Di2 / Distance : 0.0;
    public double PDi2Mm2PerKm => Distance > 1e-10 ? Di2Mm2 / Distance : 0.0;

    public double Correction { get; set; }
    public double CorrectedDH { get; set; }

    public override string ToString()
    {
        return string.Create(CultureInfo.InvariantCulture, $"{LID}: {SPID}->{EPID}, dH={dH:F3}, S={Distance:F1}km");
    }
}
