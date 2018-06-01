using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;

namespace ProtocolSimulation
{
    class Router
    {
        public string RouterName;
        public DataTable Neighbours;
        public DataTable RoutingTable;

        public Router(string Name)
        {
            RouterName = Name;

            Neighbours = new DataTable("Neighbours");
            Neighbours.Columns.Add("Source", typeof(String));
            Neighbours.Columns.Add("Destination", typeof(String));
            Neighbours.Columns.Add("Metric cost", typeof(int));

            RoutingTable = new DataTable("RoutingTable");
            RoutingTable.Columns.Add("Destination", typeof(String));
            RoutingTable.Columns.Add("Next Hop", typeof(String));
            RoutingTable.Columns.Add("Distance", typeof(int));
        }

        public void AddNeighbours()
        {
            Console.WriteLine("Neighbour-Router's name: ");
            string NeighbourName = Console.ReadLine();
            Console.WriteLine("Metric Cost: ");
            Int32.TryParse(Console.ReadLine(), out int NeighbourDistance);

            Neighbours.Rows.Add(RouterName, NeighbourName, NeighbourDistance);
        }

        public void AddNeighbours(string Name, int Distance)
        {
            Neighbours.Rows.Add(RouterName, Name, Distance);
        }

        public void RemoveNeighbours()
        {
            Neighbours.Rows.Clear();
        }

        public void UpdateRoutingTable(LSDB LinkStateDB, bool InfoOnly)
        {
            if(!InfoOnly)
            {
                RoutingTable.Clear();
            }

            foreach(Host ht in LinkStateDB.Hosts)
            {
                try     
                {
                    List<String> Routes = new List<String>(LinkStateDB.g.Shortest_path(RouterName, ht.HostName));
                    Routes.Reverse();

                    if (InfoOnly)
                    {
                        Console.Write("Source>> {0} ", RouterName);
                        foreach (String hop in Routes)
                        {
                            Console.Write("> {0} ", hop);
                        }
                        Console.Write("  <<Destination\n");
                    }
                    else
                    {
                        foreach (DataRow row in Neighbours.Rows)
                        {
                            try     // Can't reach hosts  
                            {
                                if (Routes.First() == row["Destination"].ToString())
                                {
                                    RoutingTable.Rows.Add(ht.HostName, Routes.First(), Int32.Parse(row["Metric cost"].ToString()));
                                    break;
                                }
                            }
                            catch (Exception)
                            {
                                LinkStateDB.AllOK = false;
                            }
                        }
                    }
                }
                catch(Exception)
                {
                    LinkStateDB.AllOK = false;      // Host disappears while in mid-transaction
                }
            }
            if(InfoOnly)
            {
                Console.WriteLine();
            }
        }
    }
}
