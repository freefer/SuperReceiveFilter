using SuperReceiveFilter;
using SuperReceiveFilter.Protocol;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Samples
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            UdpClientTest2();
        }

        public static void UdpClientTest2()
        {
            var re = new ByteReceiveHandler<BytePackage>(new MyFixedHeaderPipelineFilter());
            re.PackageReceived += Re_PackageReceived;
            UdpClient udpClient = new UdpClient(2012);
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 2013);

            re.SetupReceive((bytes, ct) =>
            {
                var data = udpClient.Receive(ref endPoint);
                data.CopyTo(bytes, 0);
                return data.Length;
            });
            re.StartReadDataAsync();
            udpClient.Connect("127.0.0.1", 2013);
            re.Begin();

            while (Console.ReadLine() != "q")
            {
                Console.WriteLine("input q exit");
            }
            udpClient.Close();
            re.End();
            re.Dispose();
        }


        public static void UdpClientTest()
        {
            var re = new ByteReceiveHandler<string>(new MyFixedSizePipelineFilter());
            re.PackageReceived += Re_PackageReceived3;
            UdpClient udpClient = new UdpClient(2012);
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 2013);

            re.SetupReceive((bytes, ct) =>
            {
                var data = udpClient.Receive(ref endPoint);
                data.CopyTo(bytes, 0);
                return data.Length;
            });
            re.StartReadDataAsync();
            udpClient.Connect("127.0.0.1", 2013);
            re.Begin();

            while (Console.ReadLine() != "q")
            {
                Console.WriteLine("input q exit");
            }
            udpClient.Close();
            re.End();
            re.Dispose();
        }

    
        public static void SocketTest()
        {
            var receiveHandler = new ByteReceiveHandler<StringPackageInfo>(new CommandLinePipelineFilter());
            receiveHandler.PackageReceived += Re_PackageReceived2;
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2013);
            Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            receiveHandler.SetupReceive((bytes, ct) => socket.Receive(bytes, 0, bytes.Length, SocketFlags.None));
            receiveHandler.StartReadDataAsync();
            socket.Connect(endPoint);
            receiveHandler.Begin();
            Console.WriteLine("Socket is connected byteReceiveHandler is begin");
            Thread.Sleep(1000);
            socket.Close();
            Console.WriteLine("Socket is closed byteReceiveHandler is end");
            receiveHandler.End();
             
            socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(endPoint);
            receiveHandler.Begin();
            Console.WriteLine("Socket is connected byteReceiveHandler is begin");

            while (Console.ReadLine() != "q")
            {
                Console.WriteLine("input q exit");
            }
            socket.Close();
            receiveHandler.Dispose();

        }
        

        public static void TcpServerTest()
        {

            TcpListener server = new TcpListener(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2012));
            server.Start();
            while (true)
            {
                Console.Write("Waiting for a connection... ");

                // Perform a blocking call to accept requests.
                // You could also use server.AcceptSocket() here.
                TcpClient client = server.AcceptTcpClient();
                Console.WriteLine("Connected!");



                // Get a stream object for reading and writing
                NetworkStream stream = client.GetStream();
                var re = new StreamReceiveHandler<BytePackage>(new MyBeginEndMarkPipelineFilter());
                re.PackageReceived += Re_PackageReceived;
                re.BindStream(stream);
                re.StartReadDataAsync();
                re.Begin();

            }
        }

        public static async Task TcpClientTest()
        {
            var re = new StreamReceiveHandler<TextPackage>(new TextTerminatorPipelineFilter());
            re.PackageReceived += Re_PackageReceived1;
            TcpClient tcpClient = new TcpClient();
            _ = re.StartReadDataAsync();
            await tcpClient.ConnectAsync("127.0.0.1", 2013);
            re.BindStream(tcpClient.GetStream());
            re.Begin();
            while (Console.ReadLine() != "q")
            {
                Console.WriteLine("input q exit");
            }

        }

        public static void SerialPortTest()
        {
            var serialPort = new SerialPort("COM1");
            var re = new StreamReceiveHandler<BytePackage>(new MyBeginEndMarkPipelineFilter());
            re.PackageReceived += Re_PackageReceived;
            _ = re.StartReadDataAsync();
            serialPort.Open();
            re.BindStream(serialPort.BaseStream);
            re.Begin();
            while (Console.ReadLine() != "q")
            {
                Console.WriteLine("input q exit");
            }


            serialPort.Close();
            re.Dispose();

        }
        private static void Re_PackageReceived3(object sender, PackageEventArgs<string> e)
        {
            var time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff");
            Console.WriteLine($"{time}>>{e.Package}");
        }

        private static void Re_PackageReceived2(object sender, PackageEventArgs<StringPackageInfo> e)
        {
            var time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff");
            Console.WriteLine($"{time}>>Key:{e.Package.Key} Body:{e.Package.Body}");
        }

        private static void Re_PackageReceived1(object sender, PackageEventArgs<TextPackage> e)
        {
            var time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff");
            Console.WriteLine($"{time}>>{e.Package.Text}");
        }
        private static void Re_PackageReceived(object sender, PackageEventArgs<BytePackage> e)
        {
            var time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff");
            Console.WriteLine($"{time}>>{BitConverter.ToString(e.Bytes.ToArray())}");
        }

    }
}
