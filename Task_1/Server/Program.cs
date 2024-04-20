using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class Server
{
    private int maxRequests; // Максимальное количество обрабатываемых запросов
    private int currentRequests; // Текущее количество активных запросов

    public Server(int maxRequests) // Конструктор 
    {
        this.maxRequests = maxRequests;
        this.currentRequests = 0;
    }

    public void Start()
    {
        TcpListener listener = new TcpListener(IPAddress.Any, 8888); // Создание TcpListener для прослушивания подключений на порту 8888
        listener.Start(); // Запуск прослушивания
        Console.WriteLine("Сервер запущен. Ожидание подключений... ");

        while (true)
        {
            TcpClient client = listener.AcceptTcpClient(); // Принятие входящего подключения
            Console.WriteLine($"Клиент {((IPEndPoint)client.Client.RemoteEndPoint).Address} подключен"); // Вывод информации о подключенном клиенте

            Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClient)); // Создание нового потока для обработки запроса клиента
            clientThread.Start(client);
        }
    }

    private void HandleClient(object obj)
    {
        TcpClient client = (TcpClient)obj;
        Interlocked.Increment(ref currentRequests); // Увеличение счетчика активных запросов

        if (currentRequests > maxRequests) // Если количество активных запросов превышает максимальное
        {
            SendResponse(client, "Ошибка: Сервер перегружен!"); // Отправка клиенту сообщения о перегрузке сервера
            Interlocked.Decrement(ref currentRequests); // Уменьшение счетчика активных запросов
            client.Close(); // Закрытие соединения с клиентом
            return;
        }

        NetworkStream stream = client.GetStream(); // Получение сетевого потока для обмена данными с клиентом
        byte[] buffer = new byte[1024];
        var bytesRead = stream.Read(buffer, 0, buffer.Length); // Чтение данных из потока
        string request = Encoding.UTF8.GetString(buffer, 0, bytesRead); // Декодирование данных в строку

        Thread.Sleep(2000); // Имитация длительной обработки запроса

        string response = IsPalindrome(request) ? "YES" : "NO"; // Проверка строки на палиндром и формирование ответа
        SendResponse(client, response); // Отправка ответа клиенту

        Interlocked.Decrement(ref currentRequests); // Уменьшение счетчика активных запросов
        client.Close(); // Закрытие соединения с клиентом
    }

    private void SendResponse(TcpClient client, string response)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(response); // Кодирование ответа в байтовый массив
        NetworkStream stream = client.GetStream(); // Получение сетевого потока для обмена данными с клиентом
        stream.Write(buffer, 0, buffer.Length); // Отправка данных клиенту
    }

    private bool IsPalindrome(string str) // Метод для определения является ли строка палиндромом
    {
        var left = 0;           // Инициализация левого указателя на начало строки
        var right = str.Length - 1; // Инициализация правого указателя на конец строки

        // Пока левый указатель меньше правого
        while (left < right)
        {
            if (str[left] != str[right]) // Проверка символов на равенство
                return false; // Если символы не равны, строка не является палиндромом

            left++;   // Перемещение левого указателя вправо
            right--;  // Перемещение правого указателя влево
        }

        return true; // Если все символы совпали, строка является палиндромом
    }

}

class Program
{
    static void Main(string[] args)
    {
        Console.Write("Введите максимальное количество запросов: ");
        string input = Console.ReadLine();

        if (!int.TryParse(input, out var maxRequests) || maxRequests <= 0) // Проверка корректности введенного значения
        {
            Console.WriteLine("Некорректное значение для максимального количества запросов!");
            return;
        }

        Server server = new Server(maxRequests); // Создание сервера с указанным максимальным количеством запросов
        server.Start(); // Запуск сервера
    }
}
