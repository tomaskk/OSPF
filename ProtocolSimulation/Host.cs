using System;
using System.Data;

namespace ProtocolSimulation
{
    class Host
    {
        public string HostName;
        public string PacketMessage;
        public DataTable Neighbours;

        public Host(string Name, string message)
        {
            HostName = Name;
            PacketMessage = message;

            Neighbours = new DataTable("Neighbours");
            Neighbours.Columns.Add("Source", typeof(String));
            Neighbours.Columns.Add("Destination", typeof(String));
            Neighbours.Columns.Add("Metric cost", typeof(int));
        }

        public void AddNeighbours()
        {
            Console.WriteLine("Neighbour-Router's name: ");
            string NeighbourName = Console.ReadLine();
            Console.WriteLine("Metric Cost: ");
            Int32.TryParse(Console.ReadLine(), out int NeighbourDistance);

            Neighbours.Rows.Add(HostName, NeighbourName, NeighbourDistance);
        }

        public void AddNeighbours(string Name, int Distance)
        {
            Neighbours.Rows.Add(HostName, Name, Distance);
        }

        public void RemoveNeighbours()
        {
            Neighbours.Rows.Clear();
        }

        public void EditPacketMessage(String msg)
        {
            PacketMessage = msg;
        }
    }
}
