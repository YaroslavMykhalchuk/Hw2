using StreetLibrary;
using System.Configuration;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Text;

string serverIP = ConfigurationManager.AppSettings["ServerIP"];
string[] addressParts = serverIP.Split(":");
IPAddress address = IPAddress.Parse(addressParts[0]);
int port = int.Parse(addressParts[1]);
IPEndPoint endPoint = new IPEndPoint(address, port);

using (Socket server_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP))
{
    try
    {
        server_socket.Bind(endPoint);
        server_socket.Listen(10);
        Console.WriteLine($"Server was started on {port} port. Wait for connection...");

        while (true)
        {
            Socket client_socket = await server_socket.AcceptAsync();
            
            Task.Run(async() => await ProccessClientAsync(client_socket));
        }
    }
    catch (SocketException ex)
    {
        Console.WriteLine(ex.Message);
    }
}

async Task ProccessClientAsync(Socket client_socket)
{
    Console.WriteLine($"Client {client_socket.RemoteEndPoint} has connected.");
    int zipCodeChecking;
    List<Street> listStreets = new List<Street>();
    byte[] buffer = new byte[1024];

    try
    {
        while (true)
        {
            int receivedBytes = await client_socket.ReceiveAsync(buffer, SocketFlags.None);
            zipCodeChecking = Convert.ToInt32(Encoding.Default.GetString(buffer, 0, receivedBytes));

            listStreets = CheckZipCode(zipCodeChecking);
            byte[] buffer1 = Encoding.Default.GetBytes(DataPackaging(listStreets));
            await client_socket.SendAsync(new ArraySegment<byte>(buffer1, 0, buffer1.Length), SocketFlags.None);
            Console.WriteLine($"Data was sent to {client_socket.RemoteEndPoint}");
        }

    }
    catch (SocketException ex)
    {
        Console.WriteLine(ex.Message);
    }
    finally
    {
        client_socket.Shutdown(SocketShutdown.Both);
        client_socket.Close();
    }
}

//Task.Run(async () =>
//{
//    int zipCodeChecking;
//    List<Street> listStreets = new List<Street>();
//    using (Socket server_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP))
//    {
//        server_socket.Bind(endPoint);
//        server_socket.Listen(10);

//        Console.WriteLine($"Server was started on {port} port.");

//        try
//        {
//            while (true)
//            {
//                Socket client_socket = await server_socket.AcceptAsync();
//                Console.WriteLine($"Client {client_socket.RemoteEndPoint} has connected.");


//            }
//		}
//		catch (SocketException ex)
//		{
//            Console.WriteLine(ex.ToString());
//        }
//    }
//});


List<Street> CheckZipCode(int zipCode)
{
    List<Street> streetsCheck = new List<Street>();
    using (StreamReader reader = new StreamReader("street.json", Encoding.Default))
    {
        string data = reader.ReadToEnd();
        List<Street> streets = JsonSerializer.Deserialize<List<Street>>(data);
        foreach (Street street in streets)
        {
            if(street.ZipCode == zipCode)
            {
                streetsCheck.Add(street);
            }
        }
    }
    return streetsCheck;
}

string DataPackaging(List<Street> streets)
{
    string tmp = JsonSerializer.Serialize<List<Street>>(streets);
    return tmp;
}