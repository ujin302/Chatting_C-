using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Chatting_Server
{
    public class ServerMain
    {
        List<String> msgList = new List<string>();
        TcpClient client = null;

        private void OutPut() // 화면에 출력하는 부분 
        {
            Console.Clear();
            Console.WriteLine("==========서버==========");
            Console.WriteLine("종료: /q 입력");
            Console.WriteLine("==============================");

            if (client != null)
            {
                if (client.Connected == true)
                {
                    Console.WriteLine("'수'님이 127.0.0.1에서 접속하셨습니다.");
                }
                else
                {
                    Console.WriteLine("Waiting Connection...");
                }
            }

            foreach (string st in msgList)
            {
                // 출력 
                Console.WriteLine(st);
            }
        }

        public void Start()
        {
            OutPut();
             
            Console.Title = "주 서버";

            // 로컬호스트 이 주소 사용
            IPEndPoint localAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8000);
            // 서버 소켓 클래스 생성 
            TcpListener server = new TcpListener(localAddress);

            // 서버 시작! 
            server.Start();

            while (true)
            {
                Console.WriteLine("Waiting Connection...");
                client = server.AcceptTcpClient(); // 클라이언트 접속 기달 

                ClientData clientData = new ClientData(client);
                Console.WriteLine("'수'님이 127.0.0.1에서 접속하셨습니다.");

                clientData.client.GetStream().BeginRead(clientData.readByteData, 0, clientData.readByteData.Length, new AsyncCallback(DataReceived), clientData);
                
                while (true)
                {
                    // 키 입력이 없을 경우 == _kbhit (c언어 함수)
                    while (Console.KeyAvailable == false)
                    {
                        // 클라 서버 종료 확인 
                        if (clientData.client.Connected == false) // 끊김 
                            break;
                    }
                    if (clientData.client.Connected == false)
                        break;

                    string msg = Console.ReadLine();

                    byte[] MsgByte = Encoding.Default.GetBytes(msg);
                    // 클라한테 보냄 
                    clientData.client.GetStream().Write(MsgByte, 0, MsgByte.Length);

                    if (msg == "/q")
                    {
                        // 종료
                        client.Close();
                        msgList.Clear();
                        break;
                    }

                    if (msgList.Count() == 10)
                    {
                        // 첫번째꺼 지움 
                        msgList.RemoveAt(0);
                    }
                    msgList.Add("[주]: " + msg);
                    OutPut();
                }
            }
        }

        private void DataReceived(IAsyncResult ar)
        {
            ClientData callbackClient = ar.AsyncState as ClientData;
            try
            {
                int bytesRead = callbackClient.client.GetStream().EndRead(ar);

                // 문자열로 넘어온 데이터를 파싱해서 출력해줍니다.
                string readString = Encoding.Default.GetString(callbackClient.readByteData, 0, bytesRead);

                Console.WriteLine(readString);

                if (readString == "/q")
                {
                    //  종료
                    callbackClient.client.Close();
                    msgList.Clear();
                    Console.Clear();
                    return;
                }

                if (msgList.Count() == 10)
                {
                    // 첫번째꺼 지움 
                    msgList.RemoveAt(0);
                }

                // 메세지를 리스트에 넣음 
                msgList.Add("[수]: " + readString);
                OutPut();

                // 비동기서버에서 가장중요한 핵심입니다. 
                // 비동기서버는 while문을 돌리지않고 콜백메서드에서 다시 읽으라고 비동기명령을 내립니다.
                // 계속 입력 받기 위해 
                callbackClient.client.GetStream().BeginRead(callbackClient.readByteData, 0, callbackClient.readByteData.Length, new AsyncCallback(DataReceived), callbackClient);
            }
            catch
            {
                if(callbackClient.client.Connected == false)
                {
                    Console.WriteLine("/q");
                }
            }
        }

        class ClientData
        {
            public TcpClient client { get; set; }
            public byte[] readByteData { get; set; }

            public ClientData(TcpClient client)
            {
                this.client = client;
                this.readByteData = new byte[2048];
            }
        }
    }
}
