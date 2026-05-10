using System.Globalization;
using System.Text;
using MDP_2.Models;

namespace MDP_2.Services;

public static class LevelingAdjustmentService
{
    public static string Compute(List<LPointClass> points, List<LineClass> lines)
    {
        if (points.Count == 0 || lines.Count == 0)
        {
            throw new InvalidOperationException("没有可计算的数据，请先加载示例或导入CSV。");
        }

        var knownPoints = points.Where(p => p.IsControlP && p.IsH0).ToList();
        if (knownPoints.Count < 2)
        {
            throw new InvalidOperationException("至少需要两个已知高程点才能进行计算。");
        }

        var startPoint = knownPoints.First();
        var endPoint = knownPoints.Last();

        var initialMap = EstimateInitialHeights(points, lines);
        foreach (var p in points)
        {
            if (initialMap.TryGetValue(p.PID, out var h0))
            {
                p.InitialH = h0;
            }
            else if (!p.IsControlP)
            {
                p.InitialH = 10000.0;
            }

            p.AdjustedH = p.InitialH;
        }

        var route = BuildRoute(lines, startPoint.PID, endPoint.PID);
        if (route.Count == 0)
        {
            throw new InvalidOperationException("无法从已知起点连接到已知终点，请检查边数据连通性。");
        }

        var routeLines = route.Select(r => r.Line).DistinctBy(l => l.LID).ToList();
        var sumDh = route.Sum(r => r.Sign * r.Line.dH); // ΣdH = Σ(sign_i * dH_i)
        var totalDistance = route.Sum(r => r.Line.Distance); // S = ΣSi
        var knownDelta = endPoint.H - startPoint.H; // ΔH_known = H_end - H_start
        var closure = sumDh - knownDelta; // f_h = ΣdH - ΔH_known
        var allow = 0.04 * Math.Sqrt(totalDistance); // f_allow = 40mm*sqrt(S) = 0.04*sqrt(S) (m)

        foreach (var line in lines)
        {
            line.Correction = 0.0;
            line.CorrectedDH = line.dH;
        }

        foreach (var step in route)
        {
            var v = totalDistance > 1e-10 ? -closure * (step.Line.Distance / totalDistance) : 0.0; // v_i = -f_h*(Si/S)
            step.Line.Correction = v;
            step.Line.CorrectedDH = step.Line.dH + v; // dH'_i = dH_i + v_i
        }

        var adjusted = new Dictionary<string, double>
        {
            [startPoint.PID] = startPoint.H
        };

        foreach (var step in route)
        {
            var current = step.Sign > 0 ? step.Line.SPID : step.Line.EPID;
            var next = step.Sign > 0 ? step.Line.EPID : step.Line.SPID;
            var nextH = adjusted[current] + step.Sign * step.Line.CorrectedDH; // H_next = H_current ± dH'_i
            adjusted[next] = nextH;
        }

        foreach (var p in points)
        {
            if (adjusted.TryGetValue(p.PID, out var ah))
            {
                p.AdjustedH = ah;
            }
        }

        return BuildResultText(points, lines, routeLines, startPoint, endPoint, knownDelta, sumDh, closure, allow, totalDistance);
    }

    private static Dictionary<string, double> EstimateInitialHeights(List<LPointClass> points, List<LineClass> lines)
    {
        var result = points.ToDictionary(p => p.PID, p => p.IsControlP && p.IsH0 ? p.H : 10000.0);
        var isKnown = points.ToDictionary(p => p.PID, p => p.IsControlP && p.IsH0);

        for (var i = 0; i < points.Count; i++)
        {
            var updated = false;
            foreach (var line in lines)
            {
                if (isKnown[line.SPID] && !isKnown[line.EPID])
                {
                    result[line.EPID] = result[line.SPID] + line.dH;
                    isKnown[line.EPID] = true;
                    updated = true;
                }
                else if (isKnown[line.EPID] && !isKnown[line.SPID])
                {
                    result[line.SPID] = result[line.EPID] - line.dH;
                    isKnown[line.SPID] = true;
                    updated = true;
                }
            }

            if (!updated)
            {
                break;
            }
        }

        return result;
    }

    private static List<RouteStep> BuildRoute(List<LineClass> lines, string startId, string endId)
    {
        var route = new List<RouteStep>();
        var current = startId;
        var used = new HashSet<string>();

        for (var i = 0; i < lines.Count + 2; i++)
        {
            if (current == endId)
            {
                return route;
            }

            var forward = lines.FirstOrDefault(l => !used.Contains(l.LID) && l.SPID == current);
            if (forward is not null)
            {
                route.Add(new RouteStep(forward, +1));
                used.Add(forward.LID);
                current = forward.EPID;
                continue;
            }

            var backward = lines.FirstOrDefault(l => !used.Contains(l.LID) && l.EPID == current);
            if (backward is not null)
            {
                route.Add(new RouteStep(backward, -1));
                used.Add(backward.LID);
                current = backward.SPID;
                continue;
            }

            return [];
        }

        return [];
    }

    private static string BuildResultText(
        List<LPointClass> points,
        List<LineClass> lines,
        List<LineClass> routeLines,
        LPointClass startPoint,
        LPointClass endPoint,
        double knownDelta,
        double sumDh,
        double closure,
        double allow,
        double totalDistance)
    {
        var sb = new StringBuilder();

        var n = Math.Max(routeLines.Count, 1); // n: 测段数
        var sumPDi2 = routeLines.Sum(l => l.PDi2Mm2PerKm); // Σ(p*di^2), p = 1/Si
        var sigmaPerKm = Math.Sqrt(sumPDi2 / (2.0 * n)); // m0 = sqrt(Σ(p*di^2)/(2n))
        var secondLine = routeLines.Count >= 2 ? routeLines[1] : routeLines.First();
        var sigmaSecond = sigmaPerKm * Math.Sqrt(secondLine.Distance); // m_L2 = m0*sqrt(S2)
        var sigmaSecondMean = sigmaSecond / Math.Sqrt(2.0); // m_L2_avg = m_L2/sqrt(2)
        var sigmaWhole = sigmaPerKm * Math.Sqrt(totalDistance); // m_h = m0*sqrt(ΣSi)
        var sigmaWholeMean = sigmaWhole / Math.Sqrt(2.0); // m_h_avg = m_h/sqrt(2)

        sb.AppendLine("水准测量数据精度评价");
        sb.AppendLine(new string('=', 64));

        sb.AppendLine("1) 未知点初始高程（按观测边自动传播）");
        foreach (var p in points.Where(p => !p.IsControlP))
        {
            sb.AppendLine(string.Create(CultureInfo.InvariantCulture, $"{p.PID,-6} H0 = {p.InitialH,9:F3} m"));
        }

        sb.AppendLine();
        sb.AppendLine("2) 四项精度指标");
        sb.AppendLine("测段   Si(km)   di(mm)    di^2(mm^2)   p*di^2(di^2/Si)");
        foreach (var line in routeLines)
        {
            sb.AppendLine(string.Create(CultureInfo.InvariantCulture,
                $"{line.LID,-6} {line.Distance,6:F1}   {line.DiMm,7:F1}   {line.Di2Mm2,10:F1}   {line.PDi2Mm2PerKm,10:F1}"));
        }

        sb.AppendLine(string.Create(CultureInfo.InvariantCulture, $"Σ(p*di^2) = {sumPDi2:F1}"));
        sb.AppendLine(string.Create(CultureInfo.InvariantCulture,
            $"(1) 每公里观测高差中误差 m0 = sqrt(Σ(p*di^2)/(2n)) = {sigmaPerKm:F1} mm"));
        sb.AppendLine(string.Create(CultureInfo.InvariantCulture,
            $"(2) 第二段观测高差中误差 m_L2 = m0*sqrt(S2) = {sigmaSecond:F1} mm"));
        sb.AppendLine(string.Create(CultureInfo.InvariantCulture,
            $"(3) 第二段高差平均值中误差 m_L2_avg = m_L2/sqrt(2) = {sigmaSecondMean:F1} mm"));
        sb.AppendLine(string.Create(CultureInfo.InvariantCulture,
            $"(4) 全长一次观测高差中误差 m_h = m0*sqrt(ΣSi) = {sigmaWhole:F1} mm"));
        sb.AppendLine(string.Create(CultureInfo.InvariantCulture,
            $"    全长高差平均值中误差 m_h_avg = m_h/sqrt(2) = {sigmaWholeMean:F1} mm"));

        sb.AppendLine();
        sb.AppendLine("3) 闭合差与限差检查");
        sb.AppendLine(string.Create(CultureInfo.InvariantCulture,
            $"已知高差({startPoint.PID}->{endPoint.PID}) = {knownDelta:F3} m"));
        sb.AppendLine(string.Create(CultureInfo.InvariantCulture, $"观测高差和                 = {sumDh:F3} m"));
        sb.AppendLine(string.Create(CultureInfo.InvariantCulture, $"高差闭合差 f_h             = {closure:F3} m"));
        sb.AppendLine(string.Create(CultureInfo.InvariantCulture, $"路线总长 S                 = {totalDistance:F3} km"));
        sb.AppendLine(string.Create(CultureInfo.InvariantCulture, $"容许闭合差 f_allow         = ±{allow:F3} m (40mm*sqrt(S))"));
        sb.AppendLine(Math.Abs(closure) <= allow ? "判定：闭合差满足限差。" : "判定：闭合差超限，请复核原始观测。");

        sb.AppendLine();
        sb.AppendLine("4) 按距离配赋改正并计算平差高程");
        sb.AppendLine("LID   dH均值(m)  v(m)      dH改正后(m)");
        foreach (var line in lines)
        {
            sb.AppendLine(string.Create(CultureInfo.InvariantCulture,
                $"{line.LID,-5} {line.dH,8:F3}  {line.Correction,8:F3}   {line.CorrectedDH,10:F3}"));
        }

        sb.AppendLine("平差后点高程：");
        foreach (var p in points)
        {
            sb.AppendLine(string.Create(CultureInfo.InvariantCulture,
                $"{p.PID,-6} H_adj = {p.AdjustedH,9:F3} m"));
        }

        return sb.ToString();
    }

    private sealed record RouteStep(LineClass Line, int Sign);
}

