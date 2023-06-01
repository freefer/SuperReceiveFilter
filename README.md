# SuperReceiveFilter
use System.IO.Pipelines parsing data. support serialPort udpClient tcpClient tcpListener socket,Built in common protocol templates.

The following code shows how to use listening to Socket data parsing and compose data packets according to protocol filters

var receiveHandler = new ByteReceiveHandler<StringPackageInfo>(new CommandLinePipelineFilter());
  
receiveHandler.PackageReceived += Re_PackageReceived2;
  
IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2013);
  
Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
  
receiveHandler.SetupReceive((bytes, ct) => socket.Receive(bytes, 0, bytes.Length, SocketFlags.None));
	
receiveHandler.StartReadDataAsync();
	
socket.Connect(endPoint);
	
receiveHandler.Begin();

Protocol Template
-------------------------------------------------------------
TerminatorPipelineFilter : For example, a protocol uses two characters' # # 'as the terminator. Data such as string1 # # string2##
	
LinePipelineFilter : Line feed protocol \r\n End character. Data columns such as: 123456\r\n 123789\r\n
	
CommandLinePipelineFilter : Command line protocol ends with \r\n command names and parameters separated by spaces Data example: MSG hello! Param1 param2\r\n
	
BeginEndMarkPipelineFilter : The protocol with a start stop character follows this format for all messages'! Xxxxxxxxxxxx $'. Therefore, in this case, "!" is the start tag and "$" is the end tag, so your acceptance filter can be defined as 0XBB 0X55 .........................0X7E 0X7E.
	
FixedSizePipelineFilter : Fixed request size protocol If each of your requests consists of fixed size characters, the following filters can be used.
	
FixedHeaderPipelineFilter : Protocol with fixed header and content length included
Key(1)+length(1)+body
	
