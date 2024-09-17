using BallisticCalculator;
using Gehtsoft.Measurements;
using System;
using System.Text.Json.Serialization;

namespace RangeCard
{
  public class Params
  {
    public Params()
    {
      bulletWeight_grain = 168;
      muzzleVelocity_metersPerSecond = 780;
      ballisticCoefficientType = DragTableId.G7;
      ballisticCoefficient = 0.218;
      bulletDiameter_inch = 0.308;
      bulletLength_inch = 1.22;
      sightHeight_millimeter = 68;
      verticalMRadPerClick = 0.1;
      horizontalMRadPerClick = 0.1;
      riflingStep_inch = 11;
      zeroDistance_meter = 100;
    }

    public double bulletWeight_grain { get; set; }
    public double muzzleVelocity_metersPerSecond { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter<DragTableId>))]
    public DragTableId ballisticCoefficientType { get; set; }
    public double ballisticCoefficient { get; set; }
    public double bulletDiameter_inch { get; set; }
    public double bulletLength_inch { get; set; }
    public double sightHeight_millimeter { get; set; }
    public double verticalMRadPerClick { get; set; }
    public double horizontalMRadPerClick { get; set; }
    public double riflingStep_inch { get; set; }
    public double zeroDistance_meter { get; set; }
  }
}