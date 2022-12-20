using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using AdvancedBot.client;
using AdvancedBot.client.Map;

namespace AdvancedBot
{
    public partial class Statistics : Form
    {
        private static long BytesSent = 0;
        private static long BytesRead = 0;
        private static long PacketsRead = 0;
        private static long PacketsSent = 0;

        private int sentBps, readBps;
        private int sentPktsPS, readPktsPS;

        private long prevBytesSent = BytesSent, prevBytesRead = BytesRead;
        private long prevPacketsRead, prevPacketsSent;

        public static void IncrementRead(int bytes)
        {
            BytesRead += bytes;
            PacketsRead++;
        }
        public static void IncrementReadBytesOnly(int bytes)
        {
            BytesRead += bytes;
        }
        public static void IncrementSent(int bytes)
        {
            BytesSent += bytes;
            PacketsSent++;
        }
        
        public Statistics()
        {
            InitializeComponent();

            var chart = ioSpeedChart.ChartAreas[0];

            chart.AxisX.MajorGrid.Enabled = false;
            chart.AxisX.MajorTickMark.Enabled = false;
            chart.AxisX.LabelStyle.Enabled = false;
            chart.AxisY.LabelStyle.IsStaggered = false;
            chart.AxisY.LabelStyle.Format = "{0:0.#} KB";

            chart.AxisY2.LabelStyle.Format = "{0:0}%";
            chart.AxisX2.LabelStyle.Enabled = false;
            chart.AxisY2.LabelStyle.IsStaggered = false;

            chart.Position.X = 0;
            chart.Position.Y = 0;
            chart.Position.Width = 100;
            chart.Position.Height = 100;

            ioSpeedChart.Series[2].YAxisType = AxisType.Secondary;

            for (int j = 0; j < 3; j++) {
                for (int k = 0; k < 60; k++) {
                    ioSpeedChart.Series[j].Points.Add(0.0);
                }
            }
        }

        private void Statistics_Load(object sender, EventArgs e)
        {
            UpdateDebug();
            Translation.setup(this);
            KeyPreview = true;
            KeyDown += (s, kv) => {
                if (kv.KeyCode == Keys.G && kv.Modifiers == Keys.Control)
                    GC.Collect();
            };
        }
        private void UpdateDebug()
        {
            int chunkCount = 0, nConnectedBots = 0, botCount = 0, sectionCount = 0;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < Program.FrmMain.Clients.Count; i++) {
                MinecraftClient c = Program.FrmMain.Clients[i];
                if (c != null) {
                    if (c.IsBeingTicked()) {
                        nConnectedBots++;
                    }
                    botCount++;

                    if (c.World != null) {
                        chunkCount += c.World.Chunks.Count;
                        foreach (var kv in c.World.Chunks) {
                            var chunk = kv.Value;
                            for (int y = 0; y < 16; y++) {
                                if (chunk.Sections[y] != null) {
                                    sectionCount++;
                                }
                            }
                        }
                    }
                }
            }

            using (Process proc = Process.GetCurrentProcess()) {
                sb.AppendFormat("Bytes enviados: {0} ({1}/s)\n", FormatBytes(BytesSent), FormatBytes(sentBps));
                sb.AppendFormat("Bytes recebidos: {0}, ({1}/s)\n", FormatBytes(BytesRead), FormatBytes(readBps));
                sb.AppendFormat("Packets recebidos: {0} ({1}/s)\n", PacketsRead, readPktsPS);
                sb.AppendFormat("Packets enviados: {0} ({1}/s)\n", PacketsSent, sentPktsPS);
                sb.AppendFormat("Chunks na memória: {0} ({1} seções, {2})\n", chunkCount, sectionCount, FormatBytes(sectionCount * 6144));
                sb.AppendFormat("Bots conectados: {0} de {1}\n", nConnectedBots, botCount);
                sb.AppendFormat("CPU: {0:0.00}%, Memória: {1}\n", cpuDelta, FormatBytes(proc.WorkingSet64));
                
                ThreadPool.GetAvailableThreads(out int aWorker, out int aIocp);
                ThreadPool.GetMaxThreads(out int maxWorker, out int maxIocp);
                sb.AppendFormat("Threads: {0} (Worker: {1}, IOCP: {2})\n", proc.Threads.Count, maxWorker - aWorker, maxIocp - aIocp);
            }
            lblInfo.Text = sb.ToString();
        }

        static string[] prefixes = new string[] { "B", "KB", "MB", "GB", "TB", "PB", "EB" };
        private static string FormatBytes(long bytes)
        {
            double b = bytes;
            int pIdx = 0;

            for (; b > 1024; pIdx++)
                b /= 1024;

            return FormatDouble(b) + prefixes[pIdx];
        }
        private static string FormatDouble(double d)
        {
            return (int)d == d ? ("" + (int)d) : d.ToString("#0.000", CultureInfo.InvariantCulture);
        }

        Stopwatch secCounter = Stopwatch.StartNew();
        double cpuDelta = 0.0, lastCpuTime = 0.0;

        private void timer1_Tick(object sender, EventArgs e)
        {
            UpdateDebug();
            if (secCounter.ElapsedMilliseconds >= 1000) {
                double d = secCounter.ElapsedTicks / (double)Stopwatch.Frequency;
                secCounter.Restart();

                sentBps = (int)((BytesSent - prevBytesSent) / d);
                readBps = (int)((BytesRead - prevBytesRead) / d);

                sentPktsPS = (int)((PacketsSent - prevPacketsSent) / d);
                readPktsPS = (int)((PacketsRead - prevPacketsRead) / d);

                prevBytesSent = BytesSent;
                prevBytesRead = BytesRead;
                prevPacketsSent = PacketsSent;
                prevPacketsRead = PacketsRead;

                using (Process p = Process.GetCurrentProcess()) {
                    double totalTime = p.TotalProcessorTime.TotalMilliseconds;
                    cpuDelta = (totalTime - lastCpuTime) / (10.0 * Environment.ProcessorCount) / d;
                    lastCpuTime = totalTime;
                }
                
                ioSpeedChart.Series[0].Points.Add(sentBps / 1024.0);
                ioSpeedChart.Series[1].Points.Add(readBps / 1024.0);
                var max = ioSpeedChart.ChartAreas[0].AxisY.ScaleView.ViewMaximum;
                ioSpeedChart.Series[2].Points.Add(cpuDelta);

                for (int j = 0; j < 3; j++) {
                    if (ioSpeedChart.Series[j].Points.Count > 60)
                        ioSpeedChart.Series[j].Points.RemoveAt(0);
                }
                ioSpeedChart.ChartAreas[0].RecalculateAxesScale();
            }
        }
    }
}
