using MaterialSkin;
using MaterialSkin.Controls;
using PacketDotNet;
using SharpPcap;
using SharpPcap.WinPcap;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using SharpPcap.LibPcap;

namespace PacketSniffer
{
    public partial class PacketSniffer : MaterialForm
    {
        public string uy;
        private static CaptureFileWriterDevice captureFileWriter;
        // Array of devices
        CaptureDeviceList devices;

        //Global selected device
        ICaptureDevice selected_device;
        int packet_number = 0;//counter to count packet
        String source;         //source ip
        string Number;         //counter number but in string(convert packet_number to string)
        string Time;            //string to time
        String destination;     //destination ip
        string protocol;        //string for protocol type
        String length;          //string for packet length
        String type;            //type of packet(ethernet...)
        public string info;     //string contains all packet information
        string src_port;
        string dst_port;
        int[] flags = new int[] { 0, 0, 0, 0 };
        List<Packet> recieved_packets;
        List<byte []> recieved_data;
        public PacketSniffer()
        {
            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            materialSkinManager.ColorScheme = new ColorScheme(Primary.Teal600, Primary.Teal800, Primary.Blue200, Accent.Orange700, TextShade.WHITE);
            //materialSkinManager.ColorScheme = new ColorScheme(Primary.Blue600, Primary.Blue700, Primary.Blue200, Accent.Red100, TextShade.WHITE);
            InitializeComponent();
            captureFileWriter = new CaptureFileWriterDevice("ouzt_pcap.pcap");

            // Now list all the adapters
            ListAllDevices();

            //Event handlers
            SelectAdapter_Button.Click += StartCapture_Button_Click;
            startCapture_Button.Click += StartCaptureButton_Click;
            stopCaptureButton.Click += StopCaptureButton_Click;
            textBox1.TextChanged += TextBox1_TextChanged;
        }

        /// <summary>  
        ///  Repsonsible for updating the list of packets whenever the user
        ///  searchs for a specific protocol
        /// </summary>  
        private void TextBox1_TextChanged(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            int n = 0;
            foreach (var p in recieved_packets)
            {
                filter_packets(p,n.ToString());
                n++;
            }
        }

        private void StopCaptureButton_Click(object sender, EventArgs e)
        {
            try
            {
                selected_device.StopCapture();
                selected_device.Close();
            }
            catch (Exception)
            {

                //throw;
            }

        }


        /// <summary>  
        ///  Capture button event handler, mainly responsible for scanning
        ///  the selected adapter incoming packets and showing them
        /// </summary>  
        private void StartCaptureButton_Click(object sender, EventArgs e)
        {
            try
            {

                // Register the event handler function for the selected device
                selected_device.OnPacketArrival += Selected_device_OnPacketArrival;
                // Open the selected device
                // Note the arguments given tells the OS to forward all the packets to us
                // and a timeout is set to 1 second
                selected_device.Open(DeviceMode.Promiscuous, 100);
                selected_device.StartCapture();
                listView1.Items.Clear();
                //Every time we invoke the scan, dismiss the old items
                // Hopefully the garbage collector will handle this
                recieved_packets = new List<Packet>();
                recieved_data = new List<byte[]>();
            }
            catch (Exception ex)
            {

                MessageBox.Show("Make sure you already selected a valid device\n\n" + ex.ToString());
            }
        }

        /// <summary>  
        /// Packet arrival event handler, should store all the desired data into some global storage
        /// an array and show each arrived item info on the GUI, note we need to store incoming
        /// data to some array for further investigation
        /// </summary>  
        /// 
        private void Selected_device_OnPacketArrival(object sender, CaptureEventArgs e)
        {

            // Parse the packet
            if(materialCheckBox1.Checked)
                captureFileWriter.Write(e.Packet);
            Packet p =  PacketDotNet.Packet.ParsePacket(e.Packet.LinkLayerType, e.Packet.Data);
            //p.PayloadData = e.Packet.Data;
            info = p.ToString();
            recieved_data.Add(e.Packet.Data);
            recieved_packets.Add(p);
            //recieved_packets.Add(PacketDotNet.Packet.ParsePacket(e.Packet.LinkLayerType, e.Packet.Data));
            type = p.GetType().ToString();
            String y = p.ToString();//get all packet data and decompose it to get src ip,dst ip and protocol type
                                    //Console.WriteLine(y);
            
            //int src_port = ((TcpPacket)p).SourcePort;
            //if (((TcpPacket)p).SourcePort == 80 || ((TcpPacket)p).DestinationPort == 80)
            //    y = y + "x";
            //Append the recieved packet object to the array
            
            if (p != null)
            {
                int j = 0;
                string h = " ";
                int flag1 = 0;
                int flag2 = 0;
                int flag3 = 0;
                int flag4 = 0;
                int flag5 = 0;
                //this for loop ,loop in all packet string and search for "=" char and take string after this untill it founds "," for example ("sourceAaddress=192.168.7.1,...") we want to take 192.168.7.1 only
                //j is a variable to detect position of "=" of source ip and dst ip and protocol type in packet string(y)
                //you can continue in this loop to get more informatio that will help you in form2 like mac address by adding j=1,2,3...,print y to more understand
                for (int i = 0; i < y.Length - 1; i++)
                {

                    if (y[i] == '=' | j == 4 | j == 5 | j == 7 | j == 9)
                    {
                        if (j == 4 && flag1 == 0)
                        {
                            if (y[i + 1] != ',')
                            {
                                h = h + y[i];

                            }
                            else
                            {
                                h = h + y[i];
                                source = h;


                                h = " ";
                                flag1 = 1;
                            }


                        }
                        else if (j == 5 && flag2 == 0)
                        {
                            if (y[i + 1] != ',')
                            {
                                h = h + y[i];
                            }
                            else
                            {
                                h = h + y[i];
                                destination = h;


                                h = " ";
                                flag2 = 1;
                            }



                        }
                        else if (j == 7 && flag3 == 0)
                        {
                            if (y[i + 1] != ',')
                            {
                                h = h + y[i];
                            }
                            else
                            {
                                h = h + y[i];
                                protocol = h;

                                h = " ";
                                flag3 = 1;
                            }



                        }
                        else if (j == 9 && flag4 == 0)
                        {
                            if (y[i + 1] != ',')
                            {
                                h = h + y[i];
                            }
                            else
                            {
                                h = h + y[i];
                                src_port = h;

                                h = " ";
                                flag4 = 1;
                            }



                        }
                        else if (j == 10 && flag5 == 0)
                        {
                            if (y[i + 1] != ',')
                            {
                                h = h + y[i];
                            }
                            else
                            {
                                h = h + y[i];
                                dst_port = h;

                                h = " ";
                                flag5 = 1;
                            }



                        }
                        else if (y[i] == '=')
                        {
                            j++;
                        }


                    }

                }
                int len = e.Packet.Data.Length;
                length = len.ToString();
                //source = y.Substring(129, 13);
                // destination = y.Substring(161, 13);
                // protocol = y.Substring(199, 6);
                //info = p.ToString();
                DateTime time = e.Packet.Timeval.Date;
                // s = String.Format("{0}:{1}:{2},{3} Len={4} {5}:{6} -> {7}:{8}",
                // time.Hour, time.Minute, time.Second, time.Millisecond, len,
                // srcIp, srcPort, dstIp, dstPort);
                Number = packet_number.ToString();
                Time = time.Hour.ToString() + " : " + time.Minute.ToString() + " : " + time.Second.ToString() + " : " + time.Millisecond.ToString();
                packet_number++;
                //DataContainer.data = info;
                //public class its name datacontainer and has global attribute 
                //data to share it between form1 and form2
                if (listView1.InvokeRequired)//add items in coloumns in listview
                {
                    listView1.Invoke((MethodInvoker)delegate ()
                    {
                        ListViewItem item = new ListViewItem(Number);
                        item.SubItems.Add(Time);
                        item.SubItems.Add(source);
                        item.SubItems.Add(destination);
                        item.SubItems.Add(protocol);
                        item.SubItems.Add(length);
                        item.SubItems.Add(info);
                        if (src_port == "80" || src_port == "443" || src_port == "593")
                        {
                            item.SubItems.Add("HTTP");item.SubItems.Add("HTTP");
                        }
                        else if (src_port == "49152 " || src_port == "53")
                        {
                            item.SubItems.Add("DNS");
                        }
                        listView1.Items.Add(item);
                    });
                }
            }         
        }

        /// <summary>  
        ///  Select Adapter button event handler, mainly responsible for checking if the user
        ///  already selected a network adapter and attempts to sniff packets on this one
        ///  a proper message box should be shown in case no device was selected
        /// </summary>  
        private void StartCapture_Button_Click(object sender, EventArgs e)
        {
            //MessageBox.Show(DeviceListView.SelectedIndices[0].ToString());
            //materialTabControl1.SelectedIndex = 1;

            //Set the current selected device
            // Handle unselected device case
            try
            {
                // Select the device adapter
                selected_device = devices[DeviceListView.SelectedIndices[0]];
                // move to the next tab
                materialTabControl1.SelectedIndex = 1;
                dev_sel_lable.Text = "Current Selected Device: DEV" + DeviceListView.SelectedIndices[0].ToString();

            }
            catch (Exception ex)
            {
                // Print an error followed by the exception message
                MessageBox.Show("Make sure you already selected a valid device\n\n" + ex.ToString());
            }
        }

        /// <summary>  
        ///  The function used to show all the network adapter devices
        ///  takes no arguments, and shows all the devices @ the DeviceListView component
        /// </summary>  
        private void ListAllDevices()
        {
            devices = CaptureDeviceList.Instance;
            foreach (var dev in devices)
            {
                DeviceListView.Items.Add(dev.Description);
            }
        }

        private void filter_packets(Packet p, string Number)
        {
            String y = p.ToString();
            info = p.ToString();
            int j = 0;
            string h = " ";
            int flag1 = 0;
            int flag2 = 0;
            int flag3 = 0;
            int flag4 = 0;
            int flag5 = 0;
            
            type = p.GetType().ToString();
            for (int i = 0; i < y.Length - 1; i++)
            {
                if (y[i] == '=' | j == 4 | j == 5 | j == 7 | j == 9)
                {
                    if (j == 4 && flag1 == 0)
                    {
                        if (y[i + 1] != ',')
                        {
                            h = h + y[i];

                        }
                        else
                        {
                            h = h + y[i];
                            source = h;


                            h = " ";
                            flag1 = 1;
                        }

                    }
                    else if (j == 5 && flag2 == 0)
                    {
                        if (y[i + 1] != ',')
                        {
                            h = h + y[i];
                        }
                        else
                        {
                            h = h + y[i];
                            destination = h;


                            h = " ";
                            flag2 = 1;
                        }
                    }
                    else if (j == 7 && flag3 == 0)
                    {
                        if (y[i + 1] != ',')
                        {
                            h = h + y[i];
                        }
                        else
                        {
                            h = h + y[i];
                            protocol = h;

                            h = " ";
                            flag3 = 1;
                        }
                    }
                    else if (j == 9 && flag4 == 0)
                    {
                        if (y[i + 1] != ',')
                        {
                            h = h + y[i];
                        }
                        else
                        {
                            h = h + y[i];
                            src_port = h;

                            h = " ";
                            flag4 = 1;
                        }
                    }
                    else if (j == 10 && flag5 == 0)
                    {
                        if (y[i + 1] != ',')
                        {
                            h = h + y[i];
                        }
                        else
                        {
                            h = h + y[i];
                            dst_port = h;

                            h = " ";
                            flag5 = 1;
                        }

                    }
                    else if (y[i] == '=')
                    {
                        j++;
                    }
                }
            }
            string pro;
            pro = textBox1.Text.ToString();
            if (pro == "UDP")
            {
                flags[0] = 1;
                flags[1] = 0;
                flags[2] = 0;
                flags[3] = 0;
                //Console.WriteLine("hello");

                if (!listView1.InvokeRequired)//add items in coloumns in listview
                {
                    listView1.Invoke((MethodInvoker)delegate ()
                    {
                        foreach (ListViewItem item2 in listView1.Items)
                        {
                            // {
                            if (item2.SubItems[4].Text != " UDP")
                            {
                                item2.Remove();
                                //Console.WriteLine(item2.SubItems[4].Text);
                            }
                            //}                            
                        }
                        ListViewItem item = new ListViewItem(Number);
                        item.SubItems.Add(Time);
                        item.SubItems.Add(source);
                        item.SubItems.Add(destination);
                        item.SubItems.Add(protocol);
                        item.SubItems.Add(length);
                        item.SubItems.Add(info);
                        if (src_port == "80" || src_port == "443" || src_port == "593")
                        {
                            item.SubItems.Add("HTTP");item.SubItems.Add("HTTP");
                        }
                        else if (src_port == "49152 " || src_port == "53")
                        {
                            item.SubItems.Add("DNS");
                        }
                        listView1.Items.Add(item);
                    });
                }
            }
            else if (pro == "TCP")
            {
                flags[0] = 0;
                flags[1] = 1;
                flags[2] = 0;
                flags[3] = 0;
                if (!listView1.InvokeRequired)//add items in coloumns in listview
                {
                    listView1.Invoke((MethodInvoker)delegate ()
                    {
                        foreach (ListViewItem item2 in listView1.Items)
                        {
                            // {
                            if (item2.SubItems[4].Text != " TCP")
                            {
                                item2.Remove();
                                //Console.WriteLine(item2.SubItems[4].Text);
                            }
                            //}                            
                        }
                        ListViewItem item = new ListViewItem(Number);
                        item.SubItems.Add(Time);
                        item.SubItems.Add(source);
                        item.SubItems.Add(destination);
                        item.SubItems.Add(protocol);
                        item.SubItems.Add(length);
                        item.SubItems.Add(info);
                        if (src_port == "80" || src_port == "443" || src_port == "593")
                        {
                            item.SubItems.Add("HTTP");item.SubItems.Add("HTTP");
                        }
                        else if (src_port == "49152 " || src_port == "53")
                        {
                            item.SubItems.Add("DNS");
                        }
                        listView1.Items.Add(item);
                    });
                }
            }
            else if (pro == "HTTP")
            {
                flags[0] = 0;
                flags[1] = 0;
                flags[2] = 1;
                flags[3] = 0;
                if (!listView1.InvokeRequired)//add items in coloumns in listview
                {
                    listView1.Invoke((MethodInvoker)delegate ()
                    {
                        foreach (ListViewItem item2 in listView1.Items)
                        {
                            // {
                            if (item2.SubItems[4].Text != " HTTP")
                            {
                                item2.Remove();
                                //Console.WriteLine(item2.SubItems[4].Text);
                            }
                            //}                            
                        }
                        ListViewItem item = new ListViewItem(Number);
                        item.SubItems.Add(Time);
                        item.SubItems.Add(source);
                        item.SubItems.Add(destination);
                        item.SubItems.Add(protocol);
                        item.SubItems.Add(length);
                        item.SubItems.Add(info);
                        if (src_port == "80" || src_port == "443" || src_port == "593")
                        {
                            item.SubItems.Add("HTTP");item.SubItems.Add("HTTP");
                        }
                        else if (src_port == "49152 " || src_port == "53")
                        {
                            item.SubItems.Add("DNS");
                        }
                        listView1.Items.Add(item);
                    });
                }
            }
            else if (pro == "DNS")
            {
                flags[0] = 0;
                flags[1] = 0;
                flags[2] = 0;
                flags[3] = 1;
                if (!listView1.InvokeRequired)//add items in coloumns in listview
                {
                    listView1.Invoke((MethodInvoker)delegate ()
                    {
                        foreach (ListViewItem item2 in listView1.Items)
                        {

                            // {

                            if (item2.SubItems[4].Text != " DNS")
                            {
                                item2.Remove();
                                //Console.WriteLine(item2.SubItems[4].Text);
                            }
                            //}                            
                        }
                        ListViewItem item = new ListViewItem(Number);
                        item.SubItems.Add(Time);
                        item.SubItems.Add(source);
                        item.SubItems.Add(destination);
                        item.SubItems.Add(protocol);
                        item.SubItems.Add(length);
                        item.SubItems.Add(info);
                        if (src_port == "80" || src_port == "443" || src_port == "593")
                        {
                            item.SubItems.Add("HTTP");item.SubItems.Add("HTTP");
                        }
                        else if (src_port == "49152 " || src_port == "53")
                        {
                            item.SubItems.Add("DNS");
                        }
                        listView1.Items.Add(item);
                    });
                }
            }
            else if (pro == "")
            {
                if (!listView1.InvokeRequired)//add items in coloumns in listview
                {
                    listView1.Invoke((MethodInvoker)delegate ()
                    {
                        ListViewItem item = new ListViewItem(Number);
                        item.SubItems.Add(Time);
                        item.SubItems.Add(source);
                        item.SubItems.Add(destination);
                        item.SubItems.Add(protocol);
                        item.SubItems.Add(length);
                        item.SubItems.Add(info);
                        if (src_port == "80" || src_port == "443" || src_port == "593")
                        {
                            item.SubItems.Add("HTTP");item.SubItems.Add("HTTP");
                        }
                        else if (src_port == "49152 " || src_port == "53")
                        {
                            item.SubItems.Add("DNS");
                        }
                        listView1.Items.Add(item);
                    });
                }
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {


            // Show the settings form


        }

        private void DeviceListView_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void materialTabSelector1_Click(object sender, EventArgs e)
        {

        }

        private void materialFlatButton2_Click(object sender, EventArgs e)
        {

        }

        private void materialFlatButton3_Click(object sender, EventArgs e)
        {

        }


        private void listView1_DoubleClick_1(object sender, EventArgs e)
        {
            Form2 msg = new Form2();//when item from listview is clicked form2 will be shown
            DataContainer.p = recieved_packets[listView1.SelectedIndices[0]];
            DataContainer.b = recieved_data[listView1.SelectedIndices[0]];
            msg.Show();
            //MessageBox.Show(listView1.SelectedIndices[0].ToString());
        }
    }
}
