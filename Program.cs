using BallisticCalculator;
using System;
using System.Text.Json;
using Gehtsoft.Measurements;
using System.IO;

namespace RangeCard
{
  internal class Program
  {
    static void Main(string[] args)
    {
//      {
//        Params param1 = new Params();
//        string jsonString1 = JsonSerializer.Serialize(param1);
//        File.WriteAllText("Config.json", jsonString1);
//      }
      string jsonString = File.ReadAllText("Config.json");
      Params param = JsonSerializer.Deserialize<Params>(jsonString)!;

      RangeCard rangeCard = new RangeCard(param);
      rangeCard.CreateRangeCard();
    }
  }
}