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

## Smart-X IoT — Part 3: Wireless Mesh Network and Historical Telemetry

### Features Implemented

## Network Topology Dashboard

- Display registered sensors as nodes in a wireless mesh network.

- Display wireless connections between sensors.

- View the network topology through an interactive dashboard.

- Configure a selected sensor as the network gateway.

## Graph Traversal

- Implement Breadth-First Search (BFS) to explore connected sensors.

- Implement Depth-First Search (DFS) to traverse the network.

- Identify connected devices starting from a selected sensor or gateway.

- Detect disconnected sensors within the network.

##Network Routing

- Find a route from a selected sensor to the configured gateway.

- Display the calculated route through the network.

- Use wireless connection information to represent the network structure.

## Minimum Spanning Tree (MST)

- Calculate a Minimum Spanning Tree for the wireless network.

- Use connection power costs when evaluating network links.

- Calculate the total power cost of the selected spanning network.

- Demonstrate a lower-cost network connection structure.

## Historical Telemetry Using an AVL Tree

- Store historical telemetry readings in an AVL tree.

- Organise readings using timestamps.

- Retrieve historical readings for a selected sensor.

- Search readings within a specified start and end date/time.

- Maintain organised historical telemetry for efficient range queries.

## Data Structures and Algorithms

### The Part 3 implementation demonstrates:

- Graph — represents sensors and wireless connections.

- Adjacency List — stores connections between network nodes.

- Breadth-First Search (BFS) — explores the network level by level.

- Depth-First Search (DFS) — explores connected nodes through traversal.

- Queue — supports BFS traversal.

- Dictionary — stores network nodes, connections and traversal information.

- Minimum Spanning Tree (MST) — identifies a spanning network using connection power costs.

- Prim's Algorithm — used for the MST calculation.

- AVL Tree — stores timestamped historical telemetry.

- Tree Rotations — maintains AVL tree balance during insertion.

- Date/Time Range Queries — retrieves historical readings within a selected period.

## Part 3 Demonstration

### The Part 3 demonstration uses three registered sensors:

- New York — configured as the gateway.

- Cape Town — connected sensor.

- Joburg — connected sensor.

The demonstration includes gateway configuration, BFS and DFS traversal, disconnected sensor detection, route finding, MST calculation and historical telemetry searches.

The MST demonstration used two wireless connections:

| Connection | Distance | Power Cost |
|---|---:|---:|
| First connection | 5 | 5 |
| Second connection | 10 | 7 |
| **Total power cost** | | **12** |

## How to Test Part 3

1. Open the Smart-X solution in Visual Studio.

2. Start the API and client projects.

3. Register the sensors required for the network demonstration.

4. Open the Mesh Routing dashboard.

5. Add wireless connections between the registered sensors.

6. Select New York as the network gateway.

7. Test BFS and DFS traversal.

8. Test disconnected sensor detection.

9. Select a sensor and find its route to the gateway.

10. Run the Minimum Spanning Tree calculation and view the total power cost.

11. Generate telemetry readings for a selected sensor.

12. Open historical telemetry and select a sensor.

13.Enter a start and end date/time to retrieve historical readings.

## Notes

- Sensor telemetry is simulated for demonstration purposes.

- Network nodes and wireless connections are maintained in application memory.

- Historical telemetry is stored using an AVL tree.

- In-memory data may reset when the API restarts.

## Demo Video

- Smart-X IoT Part 3 Demonstration: [Insert Part 3 YouTube link here]
