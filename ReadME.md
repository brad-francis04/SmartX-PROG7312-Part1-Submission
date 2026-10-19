# Smart-X IoT Mesh Ecosystem — PROG7312 Part 1

## Purpose
This is a .NET 10 Smart-X IoT simulation for Sensor Data Ingestion.

## Part 1 functionality
- Sensor registration using MAC/unique ID
- Deployment location and sensor category capture
- Mock IoT telemetry generation
- Dynamic dashboard counters
- File, photo, log, CSV, JSON and PDF upload support
- Recursive deployment validation
- Generic telemetry packets
- Operator overloading for total power calculation
- Jagged arrays, multidimensional arrays and generic lists

## Future pillars
- Real-Time Commands is locked for Part 2.
- Mesh Routing is locked for the Final POE.

## How to run
1. Open `SmartX.sln` in Visual Studio.
2. Confirm .NET 10 is installed.
3. Set `SmartX.Api` and `SmartX.Client` as multiple startup projects.
4. Press F5.
5. Register a sensor on the dashboard.
6. Generate mock telemetry.
7. Upload an attachment if required.

## Test data
MAC/Unique ID: ESP32-01-A1B2  
Location: Hydroponic Zone A  
Category: Environmental

youtube link: https://www.youtube.com/watch?v=WUrveX6BjMY

Smart-X IoT — Part 2: Real-Time Processing

Part 2 functionality

Real-time device commands, including TurnOn, TurnOff, StartPump, StopPump and Reset.

Command history and undo functionality.

FIFO processing of normal telemetry using Queue<T>.

Critical telemetry processing using PriorityQueue<TElement, TPriority>.

Command history management using Stack<T>.

Device lookup using dictionaries keyed by unique device IDs and MAC addresses.

Historical telemetry stored and retrieved using SortedDictionary<DateTime, List<TelemetryPacket<double>>>.

Active-alert tracking using HashSet<Guid>.

Device searching by MAC address or deployment location.

Filtering devices by category and active-alert status.

Automated Insights based on sensor readings, active alerts, repeated searches and repeated commands.

Historical telemetry dashboard and processing status.

Data structures and implementation

The application uses generic collections, arrays, operator overloading and recursion to support telemetry processing, device management and deployment validation.

Demo video

Smart-X IoT Part 2 demonstration: Watch the demo video

Testing

Run the API and client projects in Visual Studio.

Register sensors and generate mock telemetry on the Part 1 dashboard.

Open Real-Time Commands to issue commands and inspect command history.

Generate normal and critical telemetry and verify that critical telemetry is processed first.

Refresh Historical Telemetry to inspect stored readings.

Test device search, category filters and active-alert filters.

Repeat commands and refresh Automated Insights to view recommendations.

Undo the latest command and verify its status in command history.

Sensor readings and recommendation thresholds are simulated for demonstration purposes.
