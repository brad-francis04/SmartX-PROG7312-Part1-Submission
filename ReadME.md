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

## Smart-X IoT — Part 2: Real-Time Processing

### Features Implemented

- **Real-Time Commands**
  - Issue commands to registered devices.
  - Supported commands: `TurnOn`, `TurnOff`, `StartPump`, `StopPump` and `Reset`.
  - Record command descriptions, timestamps and critical status.
  - Undo the latest command.

- **Telemetry Processing**
  - Process normal telemetry using a FIFO queue (`Queue<T>`).
  - Prioritise critical telemetry using `PriorityQueue<TElement, TPriority>`.
  - Display queue counts and processed telemetry.
  - Process critical telemetry before normal telemetry.

- **Historical Telemetry**
  - Store telemetry readings by timestamp.
  - Display historical readings, device IDs, values, units and timestamps.
  - Use `SortedDictionary<DateTime, List<TelemetryPacket<double>>>` for historical storage.

- **Device Management**
  - Register sensors using MAC addresses and unique identifiers.
  - Search devices by MAC address or deployment location.
  - Filter devices by category.
  - Filter devices by active-alert status.
  - Track active alerts using `HashSet<Guid>`.

- **Command History**
  - Maintain command history using `Stack<T>`.
  - Display command details and timestamps.
  - Mark commands as undone.

- **Automated Insights**
  - Identify devices with active alerts.
  - Analyse sensor readings for configured thresholds.
  - Detect repeated device searches.
  - Recommend reviewing devices that receive repeated commands.
  - Identify devices receiving repeated critical commands.

### Data Structures and C# Concepts

The implementation demonstrates the following:

- `Queue<T>` — FIFO telemetry processing.
- `PriorityQueue<TElement, TPriority>` — critical telemetry prioritisation.
- `Stack<T>` — command history and undo operations.
- `Dictionary<TKey, TValue>` — device lookup by unique identifier and MAC address.
- `SortedDictionary<TKey, TValue>` — historical telemetry organised by timestamp.
- `HashSet<T>` — unique active-alert tracking.
- `List<T>` — collections of sensors, telemetry readings and commands.
- **Generic types** — reusable telemetry data models.
- **Jagged arrays** — historical reading batches.
- **Multidimensional arrays** — simulated connectivity data.
- **Operator overloading** — power meter calculations.
- **Recursion** — deployment hierarchy validation.

### Demo Video

- **Smart-X IoT Part 2 Demonstration:** (https://www.youtube.com/watch?v=GIKVnmvGYpo)

### How to Test Part 2

1. Open the Smart-X solution in Visual Studio.
2. Start the API and client projects.
3. Register a sensor on the Part 1 dashboard.
4. Generate mock telemetry readings.
5. Open the Real-Time Commands page.
6. Issue commands to registered devices.
7. Generate normal and critical telemetry and verify that critical telemetry is processed first.
8. Refresh Historical Telemetry to view stored readings.
9. Test device searching, category filtering and active-alert filtering.
10. Repeat commands and refresh Automated Insights to view recommendations.
11. Undo the latest command and verify its status in command history.

### Notes

- Sensor telemetry is simulated for demonstration purposes.
- Environmental and power-consumption thresholds are illustrative recommendation rules.
- The application uses in-memory storage; data may reset when the API restarts.
