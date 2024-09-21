using BallisticCalculator;
using Gehtsoft.Measurements;
using System.IO;

namespace RangeCard
{
  public class Placeholders
  {
    public const string Gun = "$GUN";
    public const string Distance = "$DIST";
    public const string Drop = "$DROP";
    public const string P800_Correction = "$P800";
    public const string P900_Correction = "$P900";
    public const string P950_Correction = "$P950";
    public const string P980_Correction = "$P980";
    public const string P1050_Correction = "$P1050";
    public const string T_15_Correction = "$T_15";
    public const string T_5_Correction = "$T_5";
    public const string T5_Correction = "$T5";
    public const string T25_Correction = "$T25";
    public const string T35_Correction = "$T35";
    public const string Spindrift_Correction = "$SP";
    public const string Wind_Prefix = "$W";
    public const string Drop_Prefix = "$D";
  }

  public class Constants
  {
    public const double MaxDistance = 1000;
    // ICAO standard athmospere
    public const double DefaultPressure = 1013.25; //mbar
    public const double DefaultTemperature = 15; //°C
    public const double DefaultHumidity = 0.0;
  }

  public class RangeCard
  {
    private Params param;

    public RangeCard(Params p)
    {
      param = p;
    }

    private Atmosphere PressureAtmosphere(double pressure)
    {
      return new Atmosphere(
        new Measurement<DistanceUnit>(0.0, DistanceUnit.Meter),
        new Measurement<PressureUnit>(pressure / 10.0, PressureUnit.KiloPascal),
        false,
        new Measurement<TemperatureUnit>(Constants.DefaultTemperature, TemperatureUnit.Celsius),
        Constants.DefaultHumidity);
    }

    private Atmosphere TemperatureAtmosphere(double temperature)
    {
      return new Atmosphere(
        new Measurement<DistanceUnit>(0.0, DistanceUnit.Meter),
        new Measurement<PressureUnit>(Constants.DefaultPressure / 10.0, PressureUnit.KiloPascal),
        false,
        new Measurement<TemperatureUnit>(temperature, TemperatureUnit.Celsius),
        Constants.DefaultHumidity);
    }

    private string FixSign(string number)
    {
      return (number == "-0" || number == "-0.0" || number == "-0,0") ? "0" : number;
    }

    public void CreateRangeCard()
    {
      var ammo = new Ammunition(
           weight: new Measurement<WeightUnit>(param.bulletWeight_grain, WeightUnit.Grain),
           muzzleVelocity: new Measurement<VelocityUnit>(param.muzzleVelocity_metersPerSecond, VelocityUnit.MetersPerSecond),
           ballisticCoefficient: new BallisticCoefficient(param.ballisticCoefficient, param.ballisticCoefficientType),
           bulletDiameter: new Measurement<DistanceUnit>(param.bulletDiameter_inch, DistanceUnit.Inch),
           bulletLength: new Measurement<DistanceUnit>(param.bulletLength_inch, DistanceUnit.Inch));

      var sight = new Sight(
          sightHeight: new Measurement<DistanceUnit>(param.sightHeight_millimeter, DistanceUnit.Millimeter),
          verticalClick: new Measurement<AngularUnit>(param.verticalMRadPerClick, AngularUnit.MRad),
          horizontalClick: new Measurement<AngularUnit>(param.horizontalMRadPerClick, AngularUnit.MRad)
          );

      var rifling = new Rifling(
          riflingStep: new Measurement<DistanceUnit>(param.riflingStep_inch, DistanceUnit.Inch),
          direction: TwistDirection.Right);

      var zero = new ZeroingParameters(
          distance: new Measurement<DistanceUnit>(param.zeroDistance_meter, DistanceUnit.Meter),
          ammunition: null,
          atmosphere: null
          );

      var rifle = new Rifle(sight: sight, zero: zero, rifling: rifling);

      var defaultAtmosphere = TemperatureAtmosphere(Constants.DefaultTemperature);

      var calc = new TrajectoryCalculator();

      var shot = new ShotParameters()
      {
        MaximumDistance = new Measurement<DistanceUnit>(Constants.MaxDistance, DistanceUnit.Meter),
        Step = new Measurement<DistanceUnit>(50, DistanceUnit.Meter),
        //calculate sight angle for the specified zero distance
        SightAngle = calc.SightAngle(ammo, rifle, defaultAtmosphere)
      };

      var trajectory0 = calc.Calculate(ammo, rifle, defaultAtmosphere, shot);

      object[,] trajectoriesForWindSpeed = new object[5, 12];

      for (int windSpeed = 0; windSpeed < 5; windSpeed++)
      {
        for (int angle = 0; angle < 12; angle++)
        {
          //define winds 
          //workaround the bug in BallisticCalculator 1.1.2 according wind direction
          Wind[] wind = new Wind[1]
          {
            new Wind()
            {
              Direction = new Measurement<AngularUnit>(180 - angle * 30, AngularUnit.Degree),
              Velocity = new Measurement<VelocityUnit>((windSpeed + 1) * 2, VelocityUnit.MetersPerSecond)
            }
          };

          //calculate trajectory
          trajectoriesForWindSpeed[windSpeed, angle] = calc.Calculate(ammo, rifle, defaultAtmosphere, shot, wind);
        }
      }

      var trajectoryP800 = calc.Calculate(ammo, rifle, PressureAtmosphere(800), shot);
      var trajectoryP900 = calc.Calculate(ammo, rifle, PressureAtmosphere(900), shot);
      var trajectoryP950 = calc.Calculate(ammo, rifle, PressureAtmosphere(950), shot);
      var trajectoryP980 = calc.Calculate(ammo, rifle, PressureAtmosphere(980), shot);
      var trajectoryP1050 = calc.Calculate(ammo, rifle, PressureAtmosphere(1050), shot);

      var trajectoryT_15 = calc.Calculate(ammo, rifle, TemperatureAtmosphere(-15), shot);
      var trajectoryT_5 = calc.Calculate(ammo, rifle, TemperatureAtmosphere(-5), shot);
      var trajectoryT5 = calc.Calculate(ammo, rifle, TemperatureAtmosphere(5), shot);
      var trajectoryT25 = calc.Calculate(ammo, rifle, TemperatureAtmosphere(25), shot);
      var trajectoryT35 = calc.Calculate(ammo, rifle, TemperatureAtmosphere(35), shot);

      Directory.CreateDirectory("output");

      int index = 0;
      foreach (var point0 in trajectory0)
      {
        string template = File.ReadAllText("template//" + param.template);

        template = template.Replace(Placeholders.Gun, param.name);

        double distance = point0.Distance.In(DistanceUnit.Meter);
        template = template.Replace(Placeholders.Distance, FixSign(distance.ToString("F0")));

        double drop = -point0.DropAdjustment.In(AngularUnit.MRad) / sight.VerticalClick.GetValueOrDefault().In(AngularUnit.MRad);
        template = template.Replace(Placeholders.Drop, FixSign(drop.ToString("F0")));

        double windage = point0.WindageAdjustment.In(AngularUnit.MRad) / sight.HorizontalClick.GetValueOrDefault().In(AngularUnit.MRad);
        template = template.Replace(Placeholders.Spindrift_Correction, FixSign(windage.ToString("F0")));

        double dropP = -(drop + trajectoryP800[index].DropAdjustment.In(AngularUnit.MRad) / sight.VerticalClick.GetValueOrDefault().In(AngularUnit.MRad));
        template = template.Replace(Placeholders.P800_Correction, FixSign(dropP.ToString("F1")));
        dropP = -(drop + trajectoryP900[index].DropAdjustment.In(AngularUnit.MRad) / sight.VerticalClick.GetValueOrDefault().In(AngularUnit.MRad));
        template = template.Replace(Placeholders.P900_Correction, FixSign(dropP.ToString("F1")));
        dropP = -(drop + trajectoryP950[index].DropAdjustment.In(AngularUnit.MRad) / sight.VerticalClick.GetValueOrDefault().In(AngularUnit.MRad));
        template = template.Replace(Placeholders.P950_Correction, FixSign(dropP.ToString("F1")));
        dropP = -(drop + trajectoryP980[index].DropAdjustment.In(AngularUnit.MRad) / sight.VerticalClick.GetValueOrDefault().In(AngularUnit.MRad));
        template = template.Replace(Placeholders.P980_Correction, FixSign(dropP.ToString("F1")));
        dropP = -(drop + trajectoryP1050[index].DropAdjustment.In(AngularUnit.MRad) / sight.VerticalClick.GetValueOrDefault().In(AngularUnit.MRad));
        template = template.Replace(Placeholders.P1050_Correction, FixSign(dropP.ToString("F1")));

        dropP = -(drop + trajectoryT_15[index].DropAdjustment.In(AngularUnit.MRad) / sight.VerticalClick.GetValueOrDefault().In(AngularUnit.MRad));
        template = template.Replace(Placeholders.T_15_Correction, FixSign(dropP.ToString("F1")));
        dropP = -(drop + trajectoryT_5[index].DropAdjustment.In(AngularUnit.MRad) / sight.VerticalClick.GetValueOrDefault().In(AngularUnit.MRad));
        template = template.Replace(Placeholders.T_5_Correction, FixSign(dropP.ToString("F1")));
        dropP = -(drop + trajectoryT5[index].DropAdjustment.In(AngularUnit.MRad) / sight.VerticalClick.GetValueOrDefault().In(AngularUnit.MRad));
        template = template.Replace(Placeholders.T5_Correction, FixSign(dropP.ToString("F1")));
        dropP = -(drop + trajectoryT25[index].DropAdjustment.In(AngularUnit.MRad) / sight.VerticalClick.GetValueOrDefault().In(AngularUnit.MRad));
        template = template.Replace(Placeholders.T25_Correction, FixSign(dropP.ToString("F1")));
        dropP = -(drop + trajectoryT35[index].DropAdjustment.In(AngularUnit.MRad) / sight.VerticalClick.GetValueOrDefault().In(AngularUnit.MRad));
        template = template.Replace(Placeholders.T35_Correction, FixSign(dropP.ToString("F1")));

        for (int windSpeed = 0; windSpeed < 5; windSpeed++)
        {
          for (int angle = 0; angle < 12; angle++)
          {
            TrajectoryPoint[] trajectoryW = (TrajectoryPoint[])trajectoriesForWindSpeed[windSpeed, angle];
            var pointW = trajectoryW[index];

            double windage2 = pointW.WindageAdjustment.In(AngularUnit.MRad) / sight.HorizontalClick.GetValueOrDefault().In(AngularUnit.MRad) - windage;

            string placeholderW = Placeholders.Wind_Prefix + windSpeed.ToString() + "_" + angle.ToString("D2");
            template = template.Replace(placeholderW, FixSign(windage2.ToString("F0")));

            double drop2 = -pointW.DropAdjustment.In(AngularUnit.MRad) / sight.HorizontalClick.GetValueOrDefault().In(AngularUnit.MRad) - drop;

            string placeholderD = Placeholders.Drop_Prefix + windSpeed.ToString() + "_" + angle.ToString("D2");
            template = template.Replace(placeholderD, FixSign(drop2.ToString("F0")));
          }
        }

        File.WriteAllText("output//ragecard_" + distance.ToString("F0") + ".svg", template);

        index++;
      }
    }
  }
}
