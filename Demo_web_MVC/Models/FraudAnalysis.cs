using System;
using System.Collections.Generic;

namespace Demo_web_MVC.Models;

public partial class FraudAnalysis
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public decimal RiskScore { get; set; }

    public string RiskLevel { get; set; } = null!;

    public string RiskReasons { get; set; } = null!;

    public string InputSnapshot { get; set; } = null!;

    public string ModelName { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual Order Order { get; set; } = null!;
}
