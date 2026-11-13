using System;
using System.Collections.Generic;
using System.Linq;
using SmartX.Shared;

namespace SmartX.Api.Network
{
    public class TelemetryTreeService
    {
        // Each node stores readings that share the same timestamp.
        private class TreeNode
        {
            public DateTimeOffset Timestamp { get; set; }

            public List<HistoricalTelemetryRecord> Records { get; set; }

            public TreeNode? Left { get; set; }

            public TreeNode? Right { get; set; }

            public int Height { get; set; }

            public TreeNode(HistoricalTelemetryRecord record)
            {
                Timestamp = record.Timestamp;
                Records = new List<HistoricalTelemetryRecord> { record };
                Height = 1;
            }
        }

        private TreeNode? root;

        // Inserts a historical telemetry reading.
        public void AddReading(HistoricalTelemetryRecord record)
        {
            if (record == null)
                throw new ArgumentNullException(nameof(record));

            if (string.IsNullOrWhiteSpace(record.SensorId))
                throw new ArgumentException("Sensor ID is required.");

            if (string.IsNullOrWhiteSpace(record.MetricName))
                throw new ArgumentException("Metric name is required.");

            root = Insert(root, record);
        }

        // Recursively inserts a reading into the AVL tree.
        private TreeNode Insert(
            TreeNode? node,
            HistoricalTelemetryRecord record)
        {
            if (node == null)
                return new TreeNode(record);

            if (record.Timestamp < node.Timestamp)
            {
                node.Left = Insert(node.Left, record);
            }
            else if (record.Timestamp > node.Timestamp)
            {
                node.Right = Insert(node.Right, record);
            }
            else
            {
                // Multiple sensors may produce readings
                // with the same timestamp.
                node.Records.Add(record);
                return node;
            }

            UpdateHeight(node);

            int balance = GetBalance(node);

            // Left Left case.
            if (balance > 1 &&
                record.Timestamp < node.Left!.Timestamp)
            {
                return RotateRight(node);
            }

            // Right Right case.
            if (balance < -1 &&
                record.Timestamp > node.Right!.Timestamp)
            {
                return RotateLeft(node);
            }

            // Left Right case.
            if (balance > 1 &&
                record.Timestamp > node.Left!.Timestamp)
            {
                node.Left = RotateLeft(node.Left!);
                return RotateRight(node);
            }

            // Right Left case.
            if (balance < -1 &&
                record.Timestamp < node.Right!.Timestamp)
            {
                node.Right = RotateRight(node.Right!);
                return RotateLeft(node);
            }

            return node;
        }

        // Returns the height of a node.
        private int GetHeight(TreeNode? node)
        {
            return node?.Height ?? 0;
        }

        // Updates the height after insertion.
        private void UpdateHeight(TreeNode node)
        {
            node.Height = 1 + Math.Max(
                GetHeight(node.Left),
                GetHeight(node.Right));
        }

        // Calculates the AVL balance factor.
        private int GetBalance(TreeNode? node)
        {
            if (node == null)
                return 0;

            return GetHeight(node.Left) -
                   GetHeight(node.Right);
        }

        // Performs a right rotation.
        private TreeNode RotateRight(TreeNode y)
        {
            TreeNode x = y.Left!;
            TreeNode? middle = x.Right;

            x.Right = y;
            y.Left = middle;

            UpdateHeight(y);
            UpdateHeight(x);

            return x;
        }

        // Performs a left rotation.
        private TreeNode RotateLeft(TreeNode x)
        {
            TreeNode y = x.Right!;
            TreeNode? middle = y.Left;

            y.Left = x;
            x.Right = middle;

            UpdateHeight(x);
            UpdateHeight(y);

            return y;
        }

        // Retrieves readings within an inclusive timestamp range.
        public List<HistoricalTelemetryRecord> GetReadingsInRange(
            string sensorId,
            DateTimeOffset start,
            DateTimeOffset end)
        {
            if (string.IsNullOrWhiteSpace(sensorId))
                throw new ArgumentException("Sensor ID is required.");

            if (start > end)
                throw new ArgumentException("Start time must be before end time.");

            var results = new List<HistoricalTelemetryRecord>();

            SearchRange(root, sensorId, start, end, results);

            return results;
        }

        // Searches only the relevant timestamp branches.
        private void SearchRange(
            TreeNode? node,
            string sensorId,
            DateTimeOffset start,
            DateTimeOffset end,
            List<HistoricalTelemetryRecord> results)
        {
            if (node == null)
                return;

            if (start < node.Timestamp)
            {
                SearchRange(node.Left, sensorId, start, end, results);
            }

            if (node.Timestamp >= start &&
                node.Timestamp <= end)
            {
                results.AddRange(node.Records.Where(record =>
                    string.Equals(
                        record.SensorId,
                        sensorId,
                        StringComparison.OrdinalIgnoreCase)));
            }

            if (end > node.Timestamp)
            {
                SearchRange(node.Right, sensorId, start, end, results);
            }
        }

        // Returns all readings in timestamp order.
        public List<HistoricalTelemetryRecord> GetAllReadings()
        {
            var results = new List<HistoricalTelemetryRecord>();

            InOrderTraversal(root, results);

            return results;
        }

        // In-order traversal produces sorted timestamp results.
        private void InOrderTraversal(
            TreeNode? node,
            List<HistoricalTelemetryRecord> results)
        {
            if (node == null)
                return;

            InOrderTraversal(node.Left, results);

            results.AddRange(node.Records);

            InOrderTraversal(node.Right, results);
        }
    }
}
