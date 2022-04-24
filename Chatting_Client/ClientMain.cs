using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Chatting_Client
{
    class ClientMain
    {
        List<String> msgList = new List<string>();
        // 클라 소켓!! 
        TcpClient client = null;
        private void OutPut() // 화면에 출력하는 부분
        {
            Console.Clear();
            Console.WriteLine("==========클라이언트==========");
            Console.WriteLine("서버주소 입력: /c 127.0.0.1:8000");
            Console.WriteLine("종료: /q 입력");
            Console.WriteLine("==============================");

            if(client != null)
            {
                if (client.Connected == true)
                {
                    Console.WriteLine("주 서버에 연결되었습니다.");
                }
            }
            // 메세지 출력
            foreach(string st in msgList)
            {
                // 출력 
                Console.WriteLine(st);
            }
        }

        public void Run()
        {
            Console.Title = "수 클라이언트";

            while (true)
            {
                OutPut();

                // 한글자씩 읽음 
                char command1 = Convert.ToChar(Console.Read());
                char command2 = Convert.ToChar(Console.Read());
                if (command1 == '/' && command2 == 'c')
                {
                    Console.WriteLine("127.0.0.1:8000에 접속 시도 중...");
                    // 127.0.0.1:8000
                    string serveradd = Console.ReadLine();
                    if (!string.IsNullOrEmpty(serveradd.Trim()))
                    {
                        if (Connect(serveradd.Trim()))
                        {
                            Console.WriteLine("주 서버에 연결되었습니다.");
                            ClientData clientData = new ClientData(client);
                            // 서버가 보낸거 받는 곳 콜백함수로 등록 
                            clientData.client.GetStream().BeginRead(clientData.readByteData, 0, clientData.readByteData.Length, new AsyncCallback(DataReceived), clientData);
                            while (true)
                            {
                                // 키 입력이 없을 경우 
                                while (Console.KeyAvailable == false)
                                {
                                    // 서버 종료 확인 
                                    if (clientData.client.Connected == false) // 끊김 
                                        break;
                                }
                                if (clientData.client.Connected == false)
                                {
                                    msgList.Clear(); // 모든 메세지 지우기! 
                                    break;
                                }
                                    

                                string msg = Console.ReadLine();
                               
                                byte[] MsgByte = Encoding.Default.GetBytes(msg);
                                // 서버한테 보냄 
                                clientData.client.GetStream().Write(MsgByte, 0, MsgByte.Length);
                                if (msg == "/q")
                                {
                                    //  종료
                                    client.Close();
                                    msgList.Clear();
                                    break;
                                }

                                if (msgList.Count() == 10)
                                {
                                    // 첫번째꺼 지움 
                                    msgList.RemoveAt(0);
                                }
                                msgList.Add("[수]: " + msg);
                                OutPut();
                            }
                        }
                        else
                        {
                            Console.WriteLine("서버 연결 실패");
                        }
                    }
                }
            }


        }
        /// <summary>
        /// 서버가 보낸 데이터를 받을 콜백함수 
        /// </summary>
        /// <param name="ar"></param>
        private void DataReceived(IAsyncResult ar)
        {
            ClientData callbackClient = ar.AsyncState as ClientData;

            // 바이트 데이터 저장
            int bytesRead = callbackClient.client.GetStream().EndRead(ar);

            // 바이트배열 데이터 => 스트링 출력
            string readString = Encoding.Default.GetString(callbackClient.readByteData, 0, bytesRead);

            if (readString == "/q")
            {
                //  종료
                callbackClient.client.Close();
                msgList.Clear();
                return;
            }
            // 가져온 데이타 화면에 출력 

            if (msgList.Count() == 10)
            {
                // 첫번째꺼 지움 
                msgList.RemoveAt(0);
            }

            // 메세지를 리스트에 넣음 
            msgList.Add("[주]: "+readString);
            OutPut();
            // 비동기서버에서 가장중요한 핵심입니다. 
            // 비동기서버는 while문을 돌리지않고 콜백메서드에서 다시 읽으라고 비동기명령을 내립니다.
            // 계속 입력 받기 위해 
            callbackClient.client.GetStream().BeginRead(callbackClient.readByteData, 0, callbackClient.readByteData.Length, new AsyncCallback(DataReceived), callbackClient);
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

        // 만든 함수! 
        private bool Connect(string address)
        {
            try
            {
                string[] serverAddPort = address.Split(':');
                client = new TcpClient();
                // 원래 있는 함수 호출
                client.Connect(serverAddPort[0], int.Parse(serverAddPort[1]));
            }catch(Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
            return true;
        }
    }
}
