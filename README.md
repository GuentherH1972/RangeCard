# RangeCard
A simple generator for "swiss style" range cards for long range shooting.

This C# command line tool reads Config.json to define input parameters. 
Exterior ballistics then are calculated using https://github.com/gehtsoft-usa/BallisticCalculator1. 
Finally range cards similar to swiss style are created as SVG for every 50m up to 1000m. 
Sample configuration and resulting sample range cards can be found in Config.json and folder samples.

The idea is based on the book "Long Range Shooting" (https://www.vprojekte.com/p/buch-long-range-der-weg-zum-long-range-shooting-band-1-hardcover-3-auflage-2024) and related youtube video (https://www.youtube.com/watch?v=SyKkehRKG94) by Thomas Sadewasser.

In contrast to orignial concept the tool generates two wind roses per distance - one for drop and one for windage. For longer distances wind also has influence on drop which can be up to 1mrad (1m at 1000m). If wind rose for drop is not required template.svg can be modified using Inkskape to customize range card format. 

## Known limitations
- used library doesn't support multiple BCs (seen for VLD bullets to approximate real BC using G1 model) - use G7 BC in such cases
- used standard atmosphere for calculation is based on ICAO definitions (not ARMY)
- zeroing rifle (zero distance) is based on standard atmosphere condition - specifying zeroing weather is not yet supported
- range cards use metric units (meters for distance, m/s for wind, °C for temperature, mbar for pressure)
- correction tables for pressure and temperature do only contain drop corrections - windage conceptually cannot be temperature or pressure corrected using the range cards
- sight adjustment click is defined in mrad (convert MOA clicks into mrad for Config.json if required)




