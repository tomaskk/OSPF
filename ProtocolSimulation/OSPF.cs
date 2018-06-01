using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;

namespace ProtocolSimulation
{
    class OSPF
    {
        private static LSDB LinkStateDB;
        private static int Command = 0, Command2 = 0, Metric;
        private static String Name, PacketMessage, NName, Sender, Receiver;
        private static String PacketLocation = "";

        static void Main(string[] args)
        {
            LinkStateDB = new LSDB();
            AddTestTopologyData();

            for(;;)
            {
                if(Command == -1)
                {
                    break;
                }

                ShowMenu();
                bool valid = Int32.TryParse(Console.ReadLine(), out Command);

                if (valid)
                {
                    switch (Command)
                    {
                        case 0:     // Exit
                            Command = -1;
                            break;

                        case 1:     // Add Router
                            Console.WriteLine("Router's name: ");
                            Name = Console.ReadLine();
                            if (LinkStateDB.CheckIfExistsRouter(Name))
                            {
                                Console.WriteLine("ERR> Name already taken!");
                            }
                            else
                            {
                                LinkStateDB.Routers.Add(new Router(Name));
                                Console.WriteLine("Router Added");
                            }
                            break;

                        case 2:     // Remove Router
                            Console.WriteLine("Router's name: "); 
                            Name = Console.ReadLine();
                            LinkStateDB.RemoveRouter(Name);
                            Console.WriteLine("Router Removed");
                            break;

                        case 3:     // Add Host
                            Console.WriteLine("Host's name: ");
                            Name = Console.ReadLine();
                            Console.WriteLine("Host's default message: ");
                            PacketMessage = Console.ReadLine();
                            if (LinkStateDB.CheckIfExistsHost(Name))
                            {
                                Console.WriteLine("ERR> Name already taken!");
                            }
                            else
                            {
                                LinkStateDB.Hosts.Add(new Host(Name, PacketMessage));
                                Console.WriteLine("Host Added");
                            }
                            break;

                        case 4:     // Remove Host
                            Console.WriteLine("Host's name: ");
                            Name = Console.ReadLine();
                            LinkStateDB.RemoveHost(Name);
                            Console.WriteLine("Host Removed");
                            break;

                        case 5:     // Change Host's Message
                            Console.WriteLine("Host's name: ");
                            Name = Console.ReadLine();
                            Console.WriteLine("Host's new default message: ");
                            PacketMessage = Console.ReadLine();

                            foreach (Host obj in LinkStateDB.Hosts)
                            {
                                if (obj.HostName == Name)
                                {
                                    obj.EditPacketMessage(PacketMessage);
                                    Console.WriteLine("Message changed");
                                }
                            }
                            break;

                        case 6:     // Add Router's Neighbour ROUTERS ONLY

                            Console.Write("Router's name: ");
                            Name = Console.ReadLine();
                            Console.Write("Neighbour's name: ");
                            NName = Console.ReadLine();
                            Console.Write("Metric cost: ");

                            if(Name == NName)
                            {
                                Console.WriteLine("ERR> already connected!");
                                break;
                            }

                            if(!LinkStateDB.CheckIfExistsRouter(Name))
                            {
                                Console.WriteLine("ERR> Router {0} does not exist!", Name);
                                break;
                            }
                            else
                            {
                                if(!LinkStateDB.CheckIfExistsRouter(NName))
                                {
                                    Console.WriteLine("ERR> Router {0} does not exist!", NName);
                                    break;
                                }
                                else
                                {
                                    if (Int32.TryParse(Console.ReadLine(), out Metric)) 
                                    {
                                        foreach (Router obj in LinkStateDB.Routers)
                                        {
                                            if (obj.RouterName == Name)
                                            {
                                                obj.AddNeighbours(NName, Metric);
                                            }
                                            if (obj.RouterName == NName)
                                            {
                                                obj.AddNeighbours(Name, Metric);
                                            }
                                        }

                                        LinkStateDB.UpdateLSDB();
                                        Console.WriteLine("Routers Connected");
                                        break;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }


                        case 7:     // Add Host's Neighbour ROUTER-HOST ONLY
                            Console.Write("Host's name: ");
                            Name = Console.ReadLine();
                            Console.Write("Router's name: ");
                            NName = Console.ReadLine();
                            Console.Write("Metric cost: ");

                            if (Name == NName) // foreach check if exists such connection
                            {
                                Console.WriteLine("ERR> already connected!");
                                break;
                            }

                            if (!LinkStateDB.CheckIfExistsHost(Name))
                            {
                                Console.WriteLine("ERR> Host {0} does not exist!", Name);
                                break;
                            }
                            else
                            {
                                if (!LinkStateDB.CheckIfExistsRouter(NName))
                                {
                                    Console.WriteLine("ERR> Router {0} does not exist!", NName);
                                    break;
                                }
                                else
                                {
                                    if (Int32.TryParse(Console.ReadLine(), out Metric))
                                    {
                                        foreach (Host obj in LinkStateDB.Hosts)
                                        {
                                            if (obj.HostName == Name)
                                            {
                                                obj.AddNeighbours(NName, Metric);
                                            }
                                        }

                                        foreach (Router obj in LinkStateDB.Routers)
                                        {
                                            if (obj.RouterName == NName)
                                            {
                                                obj.AddNeighbours(Name, Metric);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }

                            LinkStateDB.UpdateLSDB();
                            Console.WriteLine("Host <---> Router Connected");
                            break;
                            

                        case 8:     // Remove Neighbour's Connection
                            Console.Write("Name #1: ");
                            Name = Console.ReadLine();
                            Console.Write("Name #2: ");
                            NName = Console.ReadLine();
                            LinkStateDB.RemoveConnection(Name, NName);
                            Console.WriteLine("Connection Removed");
                            break;

                        case 9:     // Show Network Topology
                            LinkStateDB.ShowNetworkTopology();
                            break;

                        case 10:    // Send Packet From 1 Host to Another

                            LinkStateDB.AllOK = true;
                            LinkStateDB.UpdateLSDB();
                            if(!LinkStateDB.AllOK)
                            {
                                Console.WriteLine("ERR> Connect all Hosts to a network or delete them!");
                                break;
                            }

                            Console.Write("Sender: ");  // must be hosts
                            Sender = Console.ReadLine();
                            Console.Write("Receiver: ");
                            Receiver = Console.ReadLine();

                            if(!LinkStateDB.CheckIfExistsHost(Sender) || !LinkStateDB.CheckIfExistsHost(Receiver))
                            {
                                Console.WriteLine("ERR> Sender and Receiver must be hosts!");
                                break;
                            }

                            PacketLocation = Sender;
                            bool PacketArrived = false;
                            String NextHop = "";

                            if (PacketLocation == Receiver)
                            {
                                PacketArrived = true;
                            }
                            
                            while (!PacketArrived)
                            {
                                if (PacketLocation == Receiver)
                                {
                                    PacketArrived = true;
                                }
                                else
                                {
                                    //---------------------------------Existing Route
                                    NextHop = "";
                                    Console.WriteLine("in-proccess> Packet located in {0}", PacketLocation);

                                    if (LinkStateDB.CheckIfExistsHost(PacketLocation)) //show next hop
                                    {
                                        List<String> Route = new List<String>(LinkStateDB.g.Shortest_path(PacketLocation, Receiver));
                                        Route.Reverse();
                                        try
                                        {
                                            NextHop = Route.First();
                                        }
                                        catch (System.InvalidOperationException)
                                        {
                                            Console.WriteLine("ERR> Can't reach one of the hosts!");
                                            PacketArrived = false;
                                            break;
                                        }
                                        Console.WriteLine("in-proccess> Host does not have Routing table. Closest Router --> {0}", NextHop);
                                    }
                                    else if (LinkStateDB.CheckIfExistsRouter(PacketLocation))   // show routing table + next hop
                                    {
                                        foreach(Router rt in LinkStateDB.Routers)
                                        {
                                            if(rt.RouterName == PacketLocation)
                                            {
                                                Console.WriteLine("in-proccess> From {0} Routing Table: ", rt.RouterName);
                                                foreach (DataRow row in rt.RoutingTable.Rows)
                                                {
                                                    if (row["Destination"].ToString() == Receiver)
                                                    {
                                                        Console.WriteLine("Dest> {0}  > Next Hop> {1}  > Distance> {2}   <<<<< Router \"{3}\"", row["Destination"], row["Next Hop"], row["Distance"], rt.RouterName);
                                                        NextHop = row["Next Hop"].ToString();
                                                        Console.WriteLine("in-proccess> Expected Next Hop = {0}.", NextHop);
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    //------------------------------------Change Route?
                                    Console.WriteLine("in-proccess> Change Network topology? [Y/<anything else>]");
                                    if (Console.ReadKey().KeyChar == 'y')
                                    {
                                        Console.WriteLine("\nin-proccess> Edit Network Topology Before Sending Out The Packet:");

                                        bool StillWantToChange = true;
                                        while (StillWantToChange)
                                        {
                                            ShowChangeMenu();
                                            bool ChoiceValid = Int32.TryParse(Console.ReadLine(), out Command2);

                                            if (ChoiceValid)
                                            {
                                                switch (Command2)
                                                {
                                                    case 0:     // Exit
                                                        StillWantToChange = false;
                                                        break;

                                                    case 1:     // Add Router
                                                        Console.WriteLine("Router's name: ");
                                                        Name = Console.ReadLine();
                                                        if (LinkStateDB.CheckIfExistsRouter(Name))
                                                        {
                                                            Console.WriteLine("ERR> Name already taken!");
                                                        }
                                                        else
                                                        {
                                                            LinkStateDB.Routers.Add(new Router(Name));
                                                            Console.WriteLine("Router Added");
                                                        }
                                                        break;

                                                    case 2:     // Add Host
                                                        Console.WriteLine("Host's name: ");
                                                        Name = Console.ReadLine();
                                                        Console.WriteLine("Host's default message: ");
                                                        PacketMessage = Console.ReadLine();
                                                        if (LinkStateDB.CheckIfExistsHost(Name))
                                                        {
                                                            Console.WriteLine("ERR> Name already taken!");
                                                        }
                                                        else
                                                        {
                                                            LinkStateDB.Hosts.Add(new Host(Name, PacketMessage));
                                                            Console.WriteLine("Host Added");
                                                        }
                                                        break;

                                                    case 3:     // Add Router's Neighbour ROUTERS ONLY

                                                        Console.Write("in-proccess> Router's name: ");
                                                        Name = Console.ReadLine();
                                                        Console.Write("in-proccess> Neighbour's name: ");
                                                        NName = Console.ReadLine();
                                                        Console.Write("in-proccess> Metric cost: ");

                                                        if (Name == NName)
                                                        {
                                                            Console.WriteLine("in-proccess> ERR> already connected!");
                                                            break;
                                                        }

                                                        if (!LinkStateDB.CheckIfExistsRouter(Name))
                                                        {
                                                            Console.WriteLine("in-proccess> ERR> Router {0} does not exist!", Name);
                                                            break;
                                                        }
                                                        else
                                                        {
                                                            if (!LinkStateDB.CheckIfExistsRouter(NName))
                                                            {
                                                                Console.WriteLine("in-proccess> ERR> Router {0} does not exist!", NName);
                                                                break;
                                                            }
                                                            else
                                                            {
                                                                if (Int32.TryParse(Console.ReadLine(), out Metric))
                                                                {
                                                                    foreach (Router obj in LinkStateDB.Routers)
                                                                    {
                                                                        if (obj.RouterName == Name)
                                                                        {
                                                                            obj.AddNeighbours(NName, Metric);
                                                                        }
                                                                        if (obj.RouterName == NName)
                                                                        {
                                                                            obj.AddNeighbours(Name, Metric);
                                                                        }
                                                                    }

                                                                    LinkStateDB.UpdateLSDB();
                                                                    Console.WriteLine("Routers Connected");
                                                                    break;
                                                                }
                                                                else
                                                                {
                                                                    break;
                                                                }
                                                            }
                                                        }


                                                    case 4:     // Add Host's Neighbour ROUTER-HOST ONLY
                                                        Console.Write("in-proccess> Host's name: ");
                                                        Name = Console.ReadLine();
                                                        Console.Write("in-proccess> Router's name: ");
                                                        NName = Console.ReadLine();
                                                        Console.Write("in-proccess> Metric cost: ");

                                                        if (Name == NName) // foreach check if exists such connection
                                                        {
                                                            Console.WriteLine("ERR> already connected!");
                                                            break;
                                                        }

                                                        if (!LinkStateDB.CheckIfExistsHost(Name))
                                                        {
                                                            Console.WriteLine("in-proccess> ERR> Host {0} does not exist!", Name);
                                                            break;
                                                        }
                                                        else
                                                        {
                                                            if (!LinkStateDB.CheckIfExistsRouter(NName))
                                                            {
                                                                Console.WriteLine("in-proccess> ERR> Router {0} does not exist!", NName);
                                                                break;
                                                            }
                                                            else
                                                            {
                                                                if (Int32.TryParse(Console.ReadLine(), out Metric))
                                                                {
                                                                    foreach (Host obj in LinkStateDB.Hosts)
                                                                    {
                                                                        if (obj.HostName == Name)
                                                                        {
                                                                            obj.AddNeighbours(NName, Metric);
                                                                        }
                                                                    }

                                                                    foreach (Router obj in LinkStateDB.Routers)
                                                                    {
                                                                        if (obj.RouterName == NName)
                                                                        {
                                                                            obj.AddNeighbours(Name, Metric);
                                                                        }
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    break;
                                                                }
                                                            }
                                                        }

                                                        LinkStateDB.UpdateLSDB();
                                                        Console.WriteLine("Host <---> Router Connected");
                                                        break;


                                                    case 5:     // Remove Neighbour's Connection
                                                        Console.Write("in-proccess> Name #1: ");
                                                        Name = Console.ReadLine();
                                                        Console.Write("in-proccess> Name #2: ");
                                                        NName = Console.ReadLine();
                                                        LinkStateDB.RemoveConnection(Name, NName);
                                                        Console.WriteLine("Connection Removed");
                                                        break;

                                                    case 6:     // Show Network Topology
                                                        LinkStateDB.ShowNetworkTopology();
                                                        break;

                                                    case 7:     //Show Routing Tables
                                                        LinkStateDB.ShowRoutes();
                                                        break;

                                                    default:
                                                        Console.WriteLine("in-proccess> ERR >> Wrong input!");
                                                        break;

                                                }
                                            }
                                            else
                                            {
                                                Console.WriteLine("in-proccess> ERR >> Wrong input!");
                                            }
                                        }
                                        //------------------------------Edit if Route was Changed
                                        if (LinkStateDB.CheckIfExistsHost(PacketLocation))
                                        {
                                            List<String> Route = new List<String>(LinkStateDB.g.Shortest_path(PacketLocation, Receiver));
                                            Route.Reverse();
                                            try
                                            {
                                                NextHop = Route.First();
                                            }
                                            catch(System.InvalidOperationException)
                                            {
                                                Console.WriteLine("ERR> Can't reach one of the hosts!");
                                                PacketArrived = false;
                                                break;
                                            }
                                            Console.WriteLine("in-proccess> Host does not have Routing table. Closest Router --> {0}", NextHop);
                                        }
                                        else if (LinkStateDB.CheckIfExistsRouter(PacketLocation))
                                        {
                                            foreach (Router rt in LinkStateDB.Routers)
                                            {
                                                if (rt.RouterName == PacketLocation)
                                                {
                                                    Console.WriteLine("in-proccess> From {0} Routing Table: ", rt.RouterName);
                                                    foreach (DataRow row in rt.RoutingTable.Rows)
                                                    {
                                                        if (row["Destination"].ToString() == Receiver)
                                                        {
                                                            Console.WriteLine("\n|| Dest> {0}  || Next Hop> {1} || Distance> {2} ||  <<<<< Router \"{3}\"", row["Destination"], row["Next Hop"], row["Distance"], rt.RouterName);
                                                            NextHop = row["Next Hop"].ToString();
                                                            Console.WriteLine("in-proccess> Expected Next Hop = {0}.", NextHop);
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                Console.WriteLine("\n[HOP COMPLETED]");
                                Console.WriteLine("in-proccess> Packet Hop'ed successfully: {0} --> {1}. Route: From {2} to {3}.", PacketLocation, NextHop, Sender, Receiver);
                                PacketLocation = NextHop;
                            }

                            //------------------PacketArrived

                            if (PacketArrived)
                            {
                                Console.WriteLine("in-proccess> Packet arrived successfully: {0} --> {1}.", Sender, Receiver);

                                List<String> Route = new List<String>(LinkStateDB.g.Shortest_path(Sender, Receiver));
                                Console.Write("Route Travelled  {0}", Sender);
                                Route.Reverse();
                                foreach (String str in Route)
                                {
                                    Console.Write(" > {0} ", str);
                                }

                                foreach (Host ht in LinkStateDB.Hosts)
                                {
                                    if (ht.HostName == Sender)
                                    {
                                        Console.WriteLine("\nMessage Arrived: {0}", ht.PacketMessage);
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                Console.WriteLine("in-proccess>  Packet was dropped at {0}.", PacketLocation);
                            }
                            break;

                        case 11:
                            LinkStateDB.ShowRoutes();
                            break;

                        default:
                            Console.WriteLine("ERR >> Wrong input!");
                            break;
                    }
                }
                else
                {
                    Console.WriteLine("ERR >> Wrong input!");
                }
            }
        }

        static void AddTestTopologyData()
        {
            LinkStateDB.Routers.Add(new Router("R1")); // 0
            LinkStateDB.Routers.Add(new Router("R2")); // 1
            LinkStateDB.Routers.Add(new Router("R3")); // 2
            LinkStateDB.Routers.Add(new Router("R4")); // 3
            LinkStateDB.Routers.Add(new Router("R5")); // 4
            LinkStateDB.Routers.Add(new Router("R6")); // 5
            LinkStateDB.Routers.Add(new Router("R7")); // 6

            LinkStateDB.Hosts.Add(new Host("H1", "Hello from Host 1")); // 0
            LinkStateDB.Hosts.Add(new Host("H2", "Hello from Host 2")); // 1
            LinkStateDB.Hosts.Add(new Host("H3", "Hello from Host 3")); // 2

            LinkStateDB.Routers[0].AddNeighbours(LinkStateDB.Routers[1].RouterName, 20); // R1 <--> R2
            LinkStateDB.Routers[1].AddNeighbours(LinkStateDB.Routers[0].RouterName, 20);

            LinkStateDB.Routers[0].AddNeighbours(LinkStateDB.Routers[2].RouterName, 10); // R1 <--> R3
            LinkStateDB.Routers[2].AddNeighbours(LinkStateDB.Routers[0].RouterName, 10);

            LinkStateDB.Routers[1].AddNeighbours(LinkStateDB.Routers[3].RouterName, 10); // R2 <--> R4
            LinkStateDB.Routers[3].AddNeighbours(LinkStateDB.Routers[1].RouterName, 10);

            LinkStateDB.Routers[2].AddNeighbours(LinkStateDB.Routers[3].RouterName, 10); // R3 <--> R4
            LinkStateDB.Routers[3].AddNeighbours(LinkStateDB.Routers[2].RouterName, 10);

            LinkStateDB.Routers[4].AddNeighbours(LinkStateDB.Routers[3].RouterName, 30); // R4 <--> R5
            LinkStateDB.Routers[3].AddNeighbours(LinkStateDB.Routers[4].RouterName, 30);

            LinkStateDB.Routers[5].AddNeighbours(LinkStateDB.Routers[2].RouterName, 10); // R3 <--> R6
            LinkStateDB.Routers[2].AddNeighbours(LinkStateDB.Routers[5].RouterName, 10);

            LinkStateDB.Routers[4].AddNeighbours(LinkStateDB.Routers[6].RouterName, 5); // R5 <--> R7
            LinkStateDB.Routers[6].AddNeighbours(LinkStateDB.Routers[4].RouterName, 5);

            LinkStateDB.Routers[5].AddNeighbours(LinkStateDB.Routers[6].RouterName, 20); // R6 <--> R7
            LinkStateDB.Routers[6].AddNeighbours(LinkStateDB.Routers[5].RouterName, 20);


            LinkStateDB.Routers[0].AddNeighbours(LinkStateDB.Hosts[0].HostName, 5);  // H1 <--> R1
            LinkStateDB.Hosts[0].AddNeighbours(LinkStateDB.Routers[0].RouterName, 5);

            LinkStateDB.Routers[1].AddNeighbours(LinkStateDB.Hosts[1].HostName, 5);  // H2 <--> R2
            LinkStateDB.Hosts[1].AddNeighbours(LinkStateDB.Routers[1].RouterName, 5);

            LinkStateDB.Routers[5].AddNeighbours(LinkStateDB.Hosts[2].HostName, 30);  // H3 <--> R6
            LinkStateDB.Hosts[2].AddNeighbours(LinkStateDB.Routers[5].RouterName, 30);

            LinkStateDB.Routers[6].AddNeighbours(LinkStateDB.Hosts[2].HostName, 5);  // H3 <--> R7
            LinkStateDB.Hosts[2].AddNeighbours(LinkStateDB.Routers[6].RouterName, 5);

            LinkStateDB.UpdateLSDB();
        }

        static void ShowMenu()
        {
            Console.WriteLine("\n>> Routing Protocol Simulation: OSPF  <<");
            Console.WriteLine("1. Add Router                          *");
            Console.WriteLine("2. Remove Router                       *");
            Console.WriteLine("3. Add Host                            *");
            Console.WriteLine("4. Remove Host                         *");
            Console.WriteLine("5. Change Host's Message               *");
            Console.WriteLine("6. Connect Router <-> Router ONLY      *");
            Console.WriteLine("7. Connect Host <-> Router ONLY        *");
            Console.WriteLine("8. Remove Connection                   *");
            Console.WriteLine("9. Show Network Topology               *");
            Console.WriteLine("10. Send Packet From 1 Host to Another *");
            Console.WriteLine("11. All Routes and Routing Tables      *");
            Console.WriteLine("0. Exit Simulation                     *");
            Console.WriteLine("________________________________________");
            Console.Write(":Command> ");
        }

        static void ShowChangeMenu()
        {
            Console.WriteLine("\n>>  Change Network Topology     <<");
            Console.WriteLine("1. Add Router                   *");
            Console.WriteLine("2. Add Host                     *");
            Console.WriteLine("3. Add Router's Neighbour       *");
            Console.WriteLine("4. Add Host's Neighbour         *");
            Console.WriteLine("5. Remove Connection            *");
            Console.WriteLine("6. Show Network Topology        *");
            Console.WriteLine("7. All Routes and Routing Tables*");
            Console.WriteLine("0. No More Changes              *");
            Console.WriteLine("_________________________________");
            Console.Write("in-proccess>:Command> ");
        }
    }
}
