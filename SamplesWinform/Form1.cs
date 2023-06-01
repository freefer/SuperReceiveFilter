using System;
using System.Buffers;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SuperReceiveFilter;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2013);
        ByteReceiveHandler<TextPackage> re;
        Socket socket;
        public Form1()
        {
            InitializeComponent();
            socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            re = new ByteReceiveHandler<TextPackage>(new TextTerminatorPipelineFilter());
            re.PackageReceived += Re_PackageReceived;
            re.SetupReceive((bytes, ct) => socket.Receive(bytes, 0, bytes.Length, SocketFlags.None));
            re.StartReadDataAsync();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            re.Dispose();
            base.OnClosing(e);
        }



        private void Re_PackageReceived(object sender, PackageEventArgs<TextPackage> e)
        {
            Invoke(new Action(() =>
            {
                rtx.AppendText($"\r{BitConverter.ToString(e.Bytes.ToArray())}");

                rtx.ScrollToCaret();
            }));
        }

        private async void btnConn_Click(object sender, EventArgs e)
        {
            if (socket == null || !socket.Connected)
            {
                socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                socket.Connect(endPoint);
                re.Begin();
                btnConn.Text = "断开";

            }
            else
            {

                socket.Close();
                re.End();
                btnConn.Text = "连接";
            }



        }
    }
}
