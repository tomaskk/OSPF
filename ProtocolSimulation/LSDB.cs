using System;
using System.Collections.Generic;
using System.Data;

namespace ProtocolSimulation
{
    class LSDB       // Link State DataBase - complete topology saved here - all destinations
    {
        public List<Router> Routers;
        public List<Host>   Hosts;
        public DataTable    AllNeighbourConnections;
        public Graph g;

        public bool PacketIndicator = true; // true if packet exists, false - nah
        public int PacketIndex = 0;
        public bool AllOK = true; // true if can send packet; false if topology is not well-connected;

        public LSDB()
        {
            Routers = new List<Router>();
            Hosts   = new List<Host>();

            AllNeighbourConnections = new DataTable("Connections");
            AllNeighbourConnections.Columns.Add("Source", typeof(String));
            AllNeighbourConnections.Columns.Add("Destination", typeof(String));
            AllNeighbourConnections.Columns.Add("Metric cost", typeof(int));
        }

        public void ShowNetworkTopology()
        {
            UpdateLSDB();

            Console.WriteLine("LSDB>> Routers: ");
            foreach (Router rt in Routers)
            {
                Console.WriteLine("Router>  {0}", rt.RouterName);
            }

            Console.WriteLine("\nLSDB>> Hosts: ");
            foreach (Host ht in Hosts)
            {
                Console.WriteLine("Host>  {0} <> Message \"{1}\"", ht.HostName, ht.PacketMessage);
            }

            Console.WriteLine("\nLSDB>> Routes: ");
            foreach (DataRow dr in AllNeighbourConnections.Rows)
            {
                Console.WriteLine("{0} --> {1} = {2}", dr["Source"], dr["Destination"], dr["Metric cost"]);
            }
        }

        public void UpdateLSDB()
        {
            AllNeighbourConnections.Clear();
            //update all neighbour list
            foreach (Router RouterX in Routers)
            {
                AllNeighbourConnections.Merge(RouterX.Neighbours);
            }

            foreach (Host HostX in Hosts)
            {
                AllNeighbourConnections.Merge(HostX.Neighbours);
            }
            //create graph
            g = new Graph();

            foreach (Host ht in Hosts)
            {
                String Name = ht.HostName;
                Dictionary<String, int> Edges = new Dictionary<String, int>();

                foreach (DataRow row in ht.Neighbours.Rows)
                {
                    Edges.Add(row["Destination"].ToString(), int.Parse(row["Metric cost"].ToString()));
                }
                g.Add_vertex(Name, Edges);
            }

            foreach (Router rt in Routers)
            {
                String Name = rt.RouterName;
                Dictionary<String, int> Edges = new Dictionary<String, int>();

                foreach (DataRow row in rt.Neighbours.Rows)
                {
                    Edges.Add(row["Destination"].ToString(), int.Parse(row["Metric cost"].ToString()));
                }
                g.Add_vertex(Name, Edges);
            }
            
            //update routing table
            foreach (Router RouterX in Routers)
            {
                RouterX.UpdateRoutingTable(this, false);
            }
        }

        public void RemoveRouter(String Name)
        {
            // Remove neighbour connections
            foreach (Router rt in Routers)
            {
                if (rt.RouterName == Name)
                {
                    rt.Neighbours.Clear();
                }
            }
            // Remove object
            for (int i = Routers.Count - 1; i >= 0; i--)
            {
                if (Routers[i].RouterName == Name)
                {
                    Routers.RemoveAt(i);
                    break;
                }
            }

            UpdateLSDB();
        }

        public void RemoveHost(String Name)
        {
            foreach (Host ht in Hosts)
            {
                if (ht.HostName == Name)
                {
                    ht.Neighbours.Clear();
                }
            }

            for (int i = Hosts.Count - 1; i >= 0; i--)
            {
                if (Hosts[i].HostName == Name)
                {
                    Hosts.RemoveAt(i);
                    break;
                }
            }

            UpdateLSDB();
        }

        public bool CheckIfExistsRouter(String Name)
        {
            foreach(Router rt in Routers)
            {
                if(rt.RouterName == Name)
                {
                    return true;
                }
            }
            return false;
        }

        public bool CheckIfExistsHost(String Name)
        {
            foreach(Host ht in Hosts)
            {
                if(ht.HostName == Name)
                {
                    return true;
                }
            }
            return false;
        }

        public void RemoveConnection(String Name1, String Name2)
        {
            foreach (Host ht in Hosts)
            {
                for (int i = ht.Neighbours.Rows.Count - 1; i >= 0; i--)
                {
                    DataRow dr = ht.Neighbours.Rows[i];

                    if ((dr["Source"].ToString() == Name1 && dr["Destination"].ToString() == Name2) || (dr["Source"].ToString() == Name2 && dr["Destination"].ToString() == Name1))
                    {
                        dr.Delete();
                    }
                }
            }

            foreach (Router rt in Routers)
            {
                for (int i = rt.Neighbours.Rows.Count - 1; i >= 0; i--)
                {
                    DataRow dr = rt.Neighbours.Rows[i];

                    if ((dr["Source"].ToString() == Name1 && dr["Destination"].ToString() == Name2) || (dr["Source"].ToString() == Name2 && dr["Destination"].ToString() == Name1))
                    {
                        dr.Delete();
                    }
                }
            }

            UpdateLSDB();
        }

        public void ShowRoutes()
        {
            Console.WriteLine("All Full Routes To All Hosts: ");
            foreach (Router RouterX in Routers)
            {
                RouterX.UpdateRoutingTable(this, true);
            }

            Console.WriteLine("All Routing Tables: ");
            foreach (Router rt in Routers)
            {
                foreach (DataRow row in rt.RoutingTable.Rows)
                {
                    Console.WriteLine("Dest> {0}  > Next Hop> {1}  > Distance> {2}   <<<<< Router \"{3}\"", row["Destination"], row["Next Hop"], row["Distance"], rt.RouterName);
                }
                Console.WriteLine();
            }
        }
    }
}
